import { Router, Response } from 'express';
import { prisma, isPostgres, mockStore } from '../../database/db';
import { AuthenticatedRequest } from '../../types';
import { authenticate } from '../../middleware/auth';
import { resolveTenant } from '../../middleware/tenant';
import { requireRole } from '../../middleware/roleGuard';
import { logAuditEvent } from '../../utils/audit';

export const businessesRouter = Router();

// Get active business details
businessesRouter.get('/current', authenticate, resolveTenant, async (req: AuthenticatedRequest, res: Response): Promise<void> => {
  try {
    const bizId = req.tenant!.businessId;
    let biz: any;

    if (isPostgres()) {
      biz = await prisma.business.findUnique({
        where: { id: bizId },
        include: {
          memberships: {
            include: { user: { select: { id: true, name: true, email: true } } },
          },
        },
      });
    } else {
      biz = mockStore.businesses.find((b) => b.id === bizId);
      if (biz) {
        const mems = mockStore.memberships
          .filter((m) => m.businessId === bizId)
          .map((m) => {
            const u = mockStore.users.find((user) => user.id === m.userId);
            return {
              ...m,
              user: u ? { id: u.id, name: u.name, email: u.email } : null,
            };
          });
        biz = { ...biz, memberships: mems };
      }
    }

    if (!biz) {
      res.status(404).json({ success: false, message: 'Business not found.' });
      return;
    }

    res.json({ success: true, business: biz, userRole: req.tenant!.role });
  } catch (err: any) {
    res.status(500).json({ success: false, message: 'Failed to fetch business: ' + err.message });
  }
});

// Update active business settings (Owner & Manager only)
businessesRouter.put(
  '/current',
  authenticate,
  resolveTenant,
  requireRole(['OWNER', 'MANAGER']),
  async (req: AuthenticatedRequest, res: Response): Promise<void> => {
    try {
      const bizId = req.tenant!.businessId;
      const { name, legalName, taxNumber, phone, email, address, currency, currencySymbol } = req.body;

      let updatedBiz: any;
      if (isPostgres()) {
        updatedBiz = await prisma.business.update({
          where: { id: bizId },
          data: {
            name: name?.trim() || undefined,
            legalName: legalName?.trim() || undefined,
            taxNumber: taxNumber?.trim() || undefined,
            phone: phone?.trim() || undefined,
            email: email?.trim() || undefined,
            address: address?.trim() || undefined,
            currency: currency?.trim() || undefined,
            currencySymbol: currencySymbol?.trim() || undefined,
          },
        });
      } else {
        const idx = mockStore.businesses.findIndex((b) => b.id === bizId);
        if (idx >= 0) {
          mockStore.businesses[idx] = {
            ...mockStore.businesses[idx],
            name: name ?? mockStore.businesses[idx].name,
            legalName: legalName ?? mockStore.businesses[idx].legalName,
            taxNumber: taxNumber ?? mockStore.businesses[idx].taxNumber,
            phone: phone ?? mockStore.businesses[idx].phone,
            email: email ?? mockStore.businesses[idx].email,
            address: address ?? mockStore.businesses[idx].address,
            currency: currency ?? mockStore.businesses[idx].currency,
            currencySymbol: currencySymbol ?? mockStore.businesses[idx].currencySymbol,
            updatedAt: new Date(),
          };
          updatedBiz = mockStore.businesses[idx];
        }
      }

      await logAuditEvent({
        businessId: bizId,
        userId: req.user!.id,
        action: 'UPDATE_BUSINESS_SETTINGS',
        entity: 'BUSINESS',
        entityId: bizId,
        payload: req.body,
      });

      res.json({ success: true, message: 'Business details updated successfully.', business: updatedBiz });
    } catch (err: any) {
      res.status(500).json({ success: false, message: 'Failed to update business: ' + err.message });
    }
  }
);
