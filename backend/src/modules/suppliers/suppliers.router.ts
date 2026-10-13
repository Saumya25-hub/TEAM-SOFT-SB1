import { Router, Response } from 'express';
import { prisma, isPostgres, mockStore } from '../../database/db';
import { AuthenticatedRequest } from '../../types';
import { authenticate } from '../../middleware/auth';
import { resolveTenant } from '../../middleware/tenant';
import { logAuditEvent } from '../../utils/audit';

export const suppliersRouter = Router();

// List suppliers
suppliersRouter.get('/', authenticate, resolveTenant, async (req: AuthenticatedRequest, res: Response): Promise<void> => {
  try {
    const bizId = req.tenant!.businessId;
    const query = (req.query.q as string)?.toLowerCase().trim();

    if (isPostgres()) {
      let whereClause: any = { businessId: bizId };
      if (query) {
        whereClause.OR = [
          { name: { contains: query, mode: 'insensitive' } },
          { phone: { contains: query, mode: 'insensitive' } },
          { gstNumber: { contains: query, mode: 'insensitive' } },
        ];
      }

      const suppliers = await prisma.supplier.findMany({
        where: whereClause,
        include: { purchases: { select: { id: true, totalAmount: true } } },
        orderBy: { name: 'asc' },
      });

      res.json({
        success: true,
        suppliers: suppliers.map((s) => ({
          id: s.id,
          name: s.name,
          phone: s.phone || '',
          email: s.email || '',
          address: s.address || '',
          gstNumber: s.gstNumber || '',
          outstandingBalance: Number(s.outstandingBalance),
          purchasesCount: s.purchases.length,
          totalPurchased: s.purchases.reduce((acc, p) => acc + Number(p.totalAmount), 0),
        })),
      });
    } else {
      let list = mockStore.suppliers.filter((s) => s.businessId === bizId);
      if (query) {
        list = list.filter(
          (s) =>
            s.name.toLowerCase().includes(query) ||
            (s.phone && s.phone.includes(query)) ||
            (s.gstNumber && s.gstNumber.toLowerCase().includes(query))
        );
      }

      res.json({
        success: true,
        suppliers: list.map((s) => {
          const purs = mockStore.purchases.filter((p) => p.supplierId === s.id);
          return {
            id: s.id,
            name: s.name,
            phone: s.phone || '',
            email: s.email || '',
            address: s.address || '',
            gstNumber: s.gstNumber || '',
            outstandingBalance: Number(s.outstandingBalance),
            purchasesCount: purs.length,
            totalPurchased: purs.reduce((acc, p) => acc + Number(p.totalAmount), 0),
          };
        }),
      });
    }
  } catch (err: any) {
    res.status(500).json({ success: false, message: 'Failed to fetch suppliers: ' + err.message });
  }
});

// Add supplier
suppliersRouter.post('/', authenticate, resolveTenant, async (req: AuthenticatedRequest, res: Response): Promise<void> => {
  try {
    const bizId = req.tenant!.businessId;
    const { name, phone, email, address, gstNumber } = req.body;

    if (!name?.trim()) {
      res.status(400).json({ success: false, message: 'Supplier name is required.' });
      return;
    }

    let supplier: any;
    if (isPostgres()) {
      supplier = await prisma.supplier.create({
        data: {
          businessId: bizId,
          name: name.trim(),
          phone: phone?.trim() || null,
          email: email?.trim() || null,
          address: address?.trim() || null,
          gstNumber: gstNumber?.trim() || null,
        },
      });
    } else {
      supplier = {
        id: 'sup-' + Date.now(),
        businessId: bizId,
        name: name.trim(),
        phone: phone?.trim() || null,
        email: email?.trim() || null,
        address: address?.trim() || null,
        gstNumber: gstNumber?.trim() || null,
        outstandingBalance: 0.0,
        createdAt: new Date(),
      };
      mockStore.suppliers.push(supplier);
    }

    await logAuditEvent({
      businessId: bizId,
      userId: req.user!.id,
      action: 'CREATE_SUPPLIER',
      entity: 'SUPPLIER',
      entityId: supplier.id,
      payload: { name, gstNumber },
    });

    res.status(201).json({ success: true, message: 'Supplier added successfully.', supplier });
  } catch (err: any) {
    res.status(500).json({ success: false, message: 'Failed to add supplier: ' + err.message });
  }
});
