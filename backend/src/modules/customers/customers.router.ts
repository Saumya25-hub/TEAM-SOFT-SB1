import { Router, Response } from 'express';
import { prisma, isPostgres, mockStore } from '../../database/db';
import { AuthenticatedRequest } from '../../types';
import { authenticate } from '../../middleware/auth';
import { resolveTenant } from '../../middleware/tenant';
import { logAuditEvent } from '../../utils/audit';

export const customersRouter = Router();

// List customers
customersRouter.get('/', authenticate, resolveTenant, async (req: AuthenticatedRequest, res: Response): Promise<void> => {
  try {
    const bizId = req.tenant!.businessId;
    const query = (req.query.q as string)?.toLowerCase().trim();

    if (isPostgres()) {
      let whereClause: any = { businessId: bizId };
      if (query) {
        whereClause.OR = [
          { name: { contains: query, mode: 'insensitive' } },
          { phone: { contains: query, mode: 'insensitive' } },
          { email: { contains: query, mode: 'insensitive' } },
        ];
      }

      const customers = await prisma.customer.findMany({
        where: whereClause,
        include: { invoices: { select: { id: true, totalAmount: true } } },
        orderBy: { name: 'asc' },
      });

      res.json({
        success: true,
        customers: customers.map((c) => ({
          id: c.id,
          name: c.name,
          phone: c.phone || '',
          email: c.email || '',
          address: c.address || '',
          outstandingBalance: Number(c.outstandingBalance),
          invoicesCount: c.invoices.length,
          totalSpent: c.invoices.reduce((acc, i) => acc + Number(i.totalAmount), 0),
        })),
      });
    } else {
      let list = mockStore.customers.filter((c) => c.businessId === bizId);
      if (query) {
        list = list.filter(
          (c) =>
            c.name.toLowerCase().includes(query) ||
            (c.phone && c.phone.includes(query)) ||
            (c.email && c.email.toLowerCase().includes(query))
        );
      }

      res.json({
        success: true,
        customers: list.map((c) => {
          const invs = mockStore.invoices.filter((i) => i.customerId === c.id);
          return {
            id: c.id,
            name: c.name,
            phone: c.phone || '',
            email: c.email || '',
            address: c.address || '',
            outstandingBalance: Number(c.outstandingBalance),
            invoicesCount: invs.length,
            totalSpent: invs.reduce((acc, i) => acc + Number(i.totalAmount), 0),
          };
        }),
      });
    }
  } catch (err: any) {
    res.status(500).json({ success: false, message: 'Failed to fetch customers: ' + err.message });
  }
});

// Add customer
customersRouter.post('/', authenticate, resolveTenant, async (req: AuthenticatedRequest, res: Response): Promise<void> => {
  try {
    const bizId = req.tenant!.businessId;
    const { name, phone, email, address } = req.body;

    if (!name?.trim()) {
      res.status(400).json({ success: false, message: 'Customer name is required.' });
      return;
    }

    let customer: any;
    if (isPostgres()) {
      customer = await prisma.customer.create({
        data: {
          businessId: bizId,
          name: name.trim(),
          phone: phone?.trim() || null,
          email: email?.trim() || null,
          address: address?.trim() || null,
        },
      });
    } else {
      customer = {
        id: 'cst-' + Date.now(),
        businessId: bizId,
        name: name.trim(),
        phone: phone?.trim() || null,
        email: email?.trim() || null,
        address: address?.trim() || null,
        outstandingBalance: 0.0,
        createdAt: new Date(),
      };
      mockStore.customers.push(customer);
    }

    await logAuditEvent({
      businessId: bizId,
      userId: req.user!.id,
      action: 'CREATE_CUSTOMER',
      entity: 'CUSTOMER',
      entityId: customer.id,
      payload: { name, phone },
    });

    res.status(201).json({ success: true, message: 'Customer added successfully.', customer });
  } catch (err: any) {
    res.status(500).json({ success: false, message: 'Failed to add customer: ' + err.message });
  }
});
