import { Response, NextFunction } from 'express';
import { AuthenticatedRequest, UserRole } from '../types';
import { prisma, isPostgres, mockStore } from '../database/db';

export async function resolveTenant(req: AuthenticatedRequest, res: Response, next: NextFunction): Promise<void> {
  const userId = req.user?.id;
  if (!userId) {
    res.status(401).json({ success: false, message: 'User not authenticated.' });
    return;
  }

  // Check if header x-business-id is supplied, else lookup user's active/first membership
  const requestedBusinessId = req.headers['x-business-id'] as string | undefined;

  if (isPostgres()) {
    try {
      let membership = requestedBusinessId
        ? await prisma.membership.findFirst({
            where: { userId, businessId: requestedBusinessId },
            include: { business: true },
          })
        : await prisma.membership.findFirst({
            where: { userId },
            include: { business: true },
          });

      if (!membership) {
        res.status(403).json({
          success: false,
          message: 'Access denied. You do not have an authorized membership for this business.',
        });
        return;
      }

      req.tenant = {
        businessId: membership.businessId,
        role: membership.role as UserRole,
        businessName: membership.business.name,
        currency: membership.business.currency,
        currencySymbol: membership.business.currencySymbol,
      };
      next();
    } catch (err: any) {
      res.status(500).json({ success: false, message: 'Failed to resolve tenant membership: ' + err.message });
    }
  } else {
    // In-Memory store fallback
    const memberships = mockStore.memberships.filter((m) => m.userId === userId);
    let mem = requestedBusinessId
      ? memberships.find((m) => m.businessId === requestedBusinessId)
      : memberships[0];

    if (!mem) {
      res.status(403).json({
        success: false,
        message: 'Access denied. You do not have an authorized membership for this business.',
      });
      return;
    }

    const biz = mockStore.businesses.find((b) => b.id === mem.businessId);
    req.tenant = {
      businessId: mem.businessId,
      role: mem.role as UserRole,
      businessName: biz?.name || 'Simple Billing',
      currency: biz?.currency || 'INR',
      currencySymbol: biz?.currencySymbol || '₹',
    };
    next();
  }
}
