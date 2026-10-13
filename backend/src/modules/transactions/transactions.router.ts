import { Router, Response } from 'express';
import { prisma, isPostgres, mockStore } from '../../database/db';
import { AuthenticatedRequest, PaymentMethod } from '../../types';
import { authenticate } from '../../middleware/auth';
import { resolveTenant } from '../../middleware/tenant';
import { logAuditEvent } from '../../utils/audit';

export const transactionsRouter = Router();

// List all transactions
transactionsRouter.get('/', authenticate, resolveTenant, async (req: AuthenticatedRequest, res: Response): Promise<void> => {
  try {
    const bizId = req.tenant!.businessId;
    const method = req.query.method as string | undefined;

    if (isPostgres()) {
      let whereClause: any = { businessId: bizId };
      if (method) whereClause.paymentMethod = method;

      const txns = await prisma.transaction.findMany({
        where: whereClause,
        include: {
          invoice: { include: { customer: true } },
          purchase: { include: { supplier: true } },
        },
        orderBy: { transactionDate: 'desc' },
      });

      res.json({
        success: true,
        transactions: txns.map((t) => ({
          id: t.id,
          receiptNumber: t.receiptNumber,
          amount: Number(t.amount),
          paymentMethod: t.paymentMethod,
          date: t.transactionDate,
          notes: t.notes || '',
          invoiceNumber: t.invoice?.invoiceNumber,
          customerName: t.invoice?.customer?.name,
          purchaseNumber: t.purchase?.purchaseNumber,
          supplierName: t.purchase?.supplier?.name,
        })),
      });
    } else {
      let list = mockStore.transactions.filter((t) => t.businessId === bizId);
      if (method) list = list.filter((t) => t.paymentMethod === method);

      res.json({
        success: true,
        transactions: list.map((t) => {
          const inv = mockStore.invoices.find((i) => i.id === t.invoiceId);
          const cust = inv ? mockStore.customers.find((c) => c.id === inv.customerId) : null;
          const pur = mockStore.purchases.find((p) => p.id === t.purchaseId);
          const sup = pur ? mockStore.suppliers.find((s) => s.id === pur.supplierId) : null;

          return {
            id: t.id,
            receiptNumber: t.receiptNumber,
            amount: Number(t.amount),
            paymentMethod: t.paymentMethod,
            date: t.transactionDate,
            notes: t.notes || '',
            invoiceNumber: inv?.invoiceNumber,
            customerName: cust?.name,
            purchaseNumber: pur?.purchaseNumber,
            supplierName: sup?.name,
          };
        }),
      });
    }
  } catch (err: any) {
    res.status(500).json({ success: false, message: 'Failed to fetch transactions: ' + err.message });
  }
});

// Record Standalone Payment Receipt
transactionsRouter.post('/', authenticate, resolveTenant, async (req: AuthenticatedRequest, res: Response): Promise<void> => {
  try {
    const bizId = req.tenant!.businessId;
    const { amount, paymentMethod = 'CASH', notes, invoiceId } = req.body;

    const parsedAmount = parseFloat(amount);
    if (isNaN(parsedAmount) || parsedAmount <= 0) {
      res.status(400).json({ success: false, message: 'A valid payment amount is required.' });
      return;
    }

    const receiptNumber = `RCP-${Date.now().toString().slice(-6)}`;

    let txn: any;
    if (isPostgres()) {
      txn = await prisma.transaction.create({
        data: {
          businessId: bizId,
          invoiceId: invoiceId || null,
          receiptNumber,
          paymentMethod: paymentMethod as PaymentMethod,
          amount: parsedAmount,
          notes: notes?.trim() || null,
        },
      });
    } else {
      txn = {
        id: 'txn-' + Date.now(),
        businessId: bizId,
        invoiceId: invoiceId || null,
        receiptNumber,
        paymentMethod: paymentMethod as PaymentMethod,
        amount: parsedAmount,
        notes: notes?.trim() || null,
        transactionDate: new Date(),
      };
      mockStore.transactions.unshift(txn);
    }

    await logAuditEvent({
      businessId: bizId,
      userId: req.user!.id,
      action: 'RECORD_TRANSACTION',
      entity: 'TRANSACTION',
      entityId: txn.id,
      payload: { receiptNumber, amount: parsedAmount, paymentMethod },
    });

    res.status(201).json({ success: true, message: `Receipt ${receiptNumber} recorded.`, transaction: txn });
  } catch (err: any) {
    res.status(500).json({ success: false, message: 'Failed to record transaction: ' + err.message });
  }
});
