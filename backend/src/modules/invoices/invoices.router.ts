import { Router, Response } from 'express';
import { prisma, isPostgres, mockStore } from '../../database/db';
import { AuthenticatedRequest, PaymentMethod, PaymentStatus } from '../../types';
import { authenticate } from '../../middleware/auth';
import { resolveTenant } from '../../middleware/tenant';
import { logAuditEvent } from '../../utils/audit';

export const invoicesRouter = Router();

// List Invoices
invoicesRouter.get('/', authenticate, resolveTenant, async (req: AuthenticatedRequest, res: Response): Promise<void> => {
  try {
    const bizId = req.tenant!.businessId;
    const query = (req.query.q as string)?.toLowerCase().trim();
    const status = req.query.status as string | undefined;

    if (isPostgres()) {
      let whereClause: any = { businessId: bizId };
      if (status) whereClause.paymentStatus = status;
      if (query) {
        whereClause.OR = [
          { invoiceNumber: { contains: query, mode: 'insensitive' } },
          { customer: { name: { contains: query, mode: 'insensitive' } } },
          { customer: { phone: { contains: query, mode: 'insensitive' } } },
        ];
      }

      const invoices = await prisma.invoice.findMany({
        where: whereClause,
        include: {
          customer: true,
          items: { include: { product: true } },
          transactions: true,
        },
        orderBy: { createdAt: 'desc' },
      });

      res.json({
        success: true,
        invoices: invoices.map((inv) => ({
          id: inv.id,
          invoiceNumber: inv.invoiceNumber,
          customerName: inv.customer?.name || 'Walk-in Customer',
          customerPhone: inv.customer?.phone || '',
          itemCount: inv.items.length,
          subtotal: Number(inv.subtotal),
          discountAmount: Number(inv.discountAmount),
          taxAmount: Number(inv.taxAmount),
          totalAmount: Number(inv.totalAmount),
          paidAmount: Number(inv.paidAmount),
          paymentStatus: inv.paymentStatus,
          createdAt: inv.createdAt,
          notes: inv.notes,
        })),
      });
    } else {
      let list = mockStore.invoices.filter((i) => i.businessId === bizId);
      if (status) list = list.filter((i) => i.paymentStatus === status);
      if (query) {
        list = list.filter((i) => {
          const cust = mockStore.customers.find((c) => c.id === i.customerId);
          return (
            i.invoiceNumber.toLowerCase().includes(query) ||
            (cust && cust.name.toLowerCase().includes(query)) ||
            (cust && cust.phone && cust.phone.includes(query))
          );
        });
      }

      res.json({
        success: true,
        invoices: list.map((inv) => {
          const cust = mockStore.customers.find((c) => c.id === inv.customerId);
          const items = mockStore.invoiceItems.filter((ii) => ii.invoiceId === inv.id);
          return {
            id: inv.id,
            invoiceNumber: inv.invoiceNumber,
            customerName: cust?.name || 'Walk-in Customer',
            customerPhone: cust?.phone || '',
            itemCount: items.length,
            subtotal: Number(inv.subtotal),
            discountAmount: Number(inv.discountAmount),
            taxAmount: Number(inv.taxAmount),
            totalAmount: Number(inv.totalAmount),
            paidAmount: Number(inv.paidAmount),
            paymentStatus: inv.paymentStatus,
            createdAt: inv.createdAt,
            notes: inv.notes,
          };
        }),
      });
    }
  } catch (err: any) {
    res.status(500).json({ success: false, message: 'Failed to fetch invoices: ' + err.message });
  }
});

// POS Checkout & Generate Invoice
invoicesRouter.post('/', authenticate, resolveTenant, async (req: AuthenticatedRequest, res: Response): Promise<void> => {
  try {
    const bizId = req.tenant!.businessId;
    const { customerId, customerName, customerPhone, items, discountAmount = 0, paymentMethod = 'CASH', notes } = req.body;

    if (!items || !Array.isArray(items) || items.length === 0) {
      res.status(400).json({ success: false, message: 'At least one line item is required to generate an invoice.' });
      return;
    }

    const invoiceNumber = `INV-${Date.now().toString().slice(-6)}`;
    const receiptNumber = `RCP-${Date.now().toString().slice(-6)}`;

    if (isPostgres()) {
      // Execute within atomic database transaction with concurrency safeguards
      const result = await prisma.$transaction(async (tx) => {
        // 1. Resolve or create customer if walk-in with name/phone
        let resolvedCustomerId = customerId || null;
        if (!resolvedCustomerId && customerName?.trim()) {
          const newCust = await tx.customer.create({
            data: {
              businessId: bizId,
              name: customerName.trim(),
              phone: customerPhone?.trim() || null,
            },
          });
          resolvedCustomerId = newCust.id;
        }

        // 2. Validate products and calculate line totals
        let subtotal = 0;
        let totalTax = 0;
        const processedItems: any[] = [];

        for (const item of items) {
          const product = await tx.product.findFirst({
            where: { id: item.productId, businessId: bizId },
          });

          if (!product) {
            throw new Error(`Product with ID "${item.productId}" not found.`);
          }

          const qty = parseInt(item.quantity, 10);
          if (qty <= 0) throw new Error(`Invalid quantity for ${product.name}.`);

          // Stock Check
          if (product.stockQuantity < qty) {
            throw new Error(`Insufficient stock for "${product.name}". Available: ${product.stockQuantity}, Requested: ${qty}`);
          }

          const unitPrice = parseFloat(item.unitPrice) || Number(product.sellingPrice);
          const taxRate = parseFloat(item.taxRate) || Number(product.taxRatePercent) || 0;
          const lineSub = unitPrice * qty;
          const lineTax = (lineSub * taxRate) / 100;
          const lineTotal = lineSub + lineTax;

          subtotal += lineSub;
          totalTax += lineTax;

          // Deduct stock atomically
          const newStock = product.stockQuantity - qty;
          await tx.product.update({
            where: { id: product.id },
            data: { stockQuantity: newStock },
          });

          // Stock movement audit
          await tx.stockMovement.create({
            data: {
              businessId: bizId,
              productId: product.id,
              type: 'SALE',
              quantity: qty,
              balanceAfter: newStock,
              referenceId: invoiceNumber,
              notes: `Billed in invoice ${invoiceNumber}`,
            },
          });

          processedItems.push({
            productId: product.id,
            quantity: qty,
            unitPrice,
            taxRate,
            taxAmount: lineTax,
            totalPrice: lineTotal,
          });
        }

        const disc = Math.min(subtotal, Math.max(0, parseFloat(discountAmount) || 0));
        const totalAmount = Math.max(0, subtotal - disc + totalTax);

        // 3. Create Invoice
        const invoice = await tx.invoice.create({
          data: {
            businessId: bizId,
            customerId: resolvedCustomerId,
            invoiceNumber,
            subtotal,
            discountAmount: disc,
            taxAmount: totalTax,
            totalAmount,
            paidAmount: totalAmount, // Paid upfront in POS
            paymentStatus: 'PAID' as PaymentStatus,
            notes: notes?.trim() || null,
            items: {
              create: processedItems,
            },
          },
          include: { items: { include: { product: true } }, customer: true },
        });

        // 4. Record Transaction Receipt
        await tx.transaction.create({
          data: {
            businessId: bizId,
            invoiceId: invoice.id,
            receiptNumber,
            paymentMethod: paymentMethod as PaymentMethod,
            amount: totalAmount,
            notes: `Payment for ${invoiceNumber} via ${paymentMethod}`,
          },
        });

        return invoice;
      });

      await logAuditEvent({
        businessId: bizId,
        userId: req.user!.id,
        action: 'CREATE_INVOICE',
        entity: 'INVOICE',
        entityId: result.id,
        payload: { invoiceNumber, total: result.totalAmount, paymentMethod },
      });

      res.status(201).json({
        success: true,
        message: `Invoice ${invoiceNumber} generated successfully.`,
        invoice: result,
      });
    } else {
      // In-Memory Fallback
      let resolvedCustomerId = customerId || null;
      if (!resolvedCustomerId && customerName?.trim()) {
        const newCustId = 'cst-' + Date.now();
        mockStore.customers.push({
          id: newCustId,
          businessId: bizId,
          name: customerName.trim(),
          phone: customerPhone?.trim() || null,
          outstandingBalance: 0.0,
          createdAt: new Date(),
        });
        resolvedCustomerId = newCustId;
      }

      let subtotal = 0;
      let totalTax = 0;
      const invId = 'inv-' + Date.now();
      const processedItems: any[] = [];

      for (const item of items) {
        const prod = mockStore.products.find((p) => p.id === item.productId && p.businessId === bizId);
        if (!prod) {
          res.status(404).json({ success: false, message: `Product "${item.productId}" not found.` });
          return;
        }

        const qty = parseInt(item.quantity, 10);
        if (prod.stockQuantity < qty) {
          res.status(400).json({
            success: false,
            message: `Insufficient stock for "${prod.name}". Available: ${prod.stockQuantity}, Requested: ${qty}`,
          });
          return;
        }

        const unitPrice = parseFloat(item.unitPrice) || Number(prod.sellingPrice);
        const taxRate = parseFloat(item.taxRate) || Number(prod.taxRatePercent) || 0;
        const lineSub = unitPrice * qty;
        const lineTax = (lineSub * taxRate) / 100;
        const lineTotal = lineSub + lineTax;

        subtotal += lineSub;
        totalTax += lineTax;

        // Deduct stock
        prod.stockQuantity -= qty;

        mockStore.stockMovements.push({
          id: 'sm-' + Date.now() + Math.random(),
          businessId: bizId,
          productId: prod.id,
          type: 'SALE',
          quantity: qty,
          balanceAfter: prod.stockQuantity,
          referenceId: invoiceNumber,
          notes: `Billed in invoice ${invoiceNumber}`,
          createdAt: new Date(),
        });

        const invoiceItemObj = {
          id: 'ii-' + Date.now() + Math.random(),
          invoiceId: invId,
          productId: prod.id,
          quantity: qty,
          unitPrice,
          taxRate,
          taxAmount: lineTax,
          totalPrice: lineTotal,
          product: prod,
        };
        processedItems.push(invoiceItemObj);
        mockStore.invoiceItems.push(invoiceItemObj);
      }

      const disc = Math.min(subtotal, Math.max(0, parseFloat(discountAmount) || 0));
      const totalAmount = Math.max(0, subtotal - disc + totalTax);

      const newInv = {
        id: invId,
        businessId: bizId,
        customerId: resolvedCustomerId,
        invoiceNumber,
        subtotal,
        discountAmount: disc,
        taxAmount: totalTax,
        totalAmount,
        paidAmount: totalAmount,
        paymentStatus: 'PAID' as PaymentStatus,
        notes: notes?.trim() || null,
        createdAt: new Date(),
        items: processedItems,
        customer: mockStore.customers.find((c) => c.id === resolvedCustomerId),
      };

      mockStore.invoices.unshift(newInv);

      mockStore.transactions.unshift({
        id: 'txn-' + Date.now(),
        businessId: bizId,
        invoiceId: invId,
        receiptNumber,
        paymentMethod: paymentMethod as PaymentMethod,
        amount: totalAmount,
        notes: `Payment for ${invoiceNumber} via ${paymentMethod}`,
        transactionDate: new Date(),
      });

      await logAuditEvent({
        businessId: bizId,
        userId: req.user!.id,
        action: 'CREATE_INVOICE',
        entity: 'INVOICE',
        entityId: invId,
        payload: { invoiceNumber, total: totalAmount, paymentMethod },
      });

      res.status(201).json({
        success: true,
        message: `Invoice ${invoiceNumber} generated successfully.`,
        invoice: newInv,
      });
    }
  } catch (err: any) {
    res.status(500).json({ success: false, message: 'Invoice checkout failed: ' + err.message });
  }
});
