import { Router, Response } from 'express';
import { prisma, isPostgres, mockStore } from '../../database/db';
import { AuthenticatedRequest } from '../../types';
import { authenticate } from '../../middleware/auth';
import { resolveTenant } from '../../middleware/tenant';
import { logAuditEvent } from '../../utils/audit';

export const purchasesRouter = Router();

// List Purchases
purchasesRouter.get('/', authenticate, resolveTenant, async (req: AuthenticatedRequest, res: Response): Promise<void> => {
  try {
    const bizId = req.tenant!.businessId;
    if (isPostgres()) {
      const purchases = await prisma.purchase.findMany({
        where: { businessId: bizId },
        include: { supplier: true, items: { include: { product: true } } },
        orderBy: { createdAt: 'desc' },
      });
      res.json({ success: true, purchases });
    } else {
      const list = mockStore.purchases
        .filter((p) => p.businessId === bizId)
        .map((p) => ({
          ...p,
          supplier: mockStore.suppliers.find((s) => s.id === p.supplierId),
          items: mockStore.purchaseItems
            .filter((pi) => pi.purchaseId === p.id)
            .map((pi) => ({
              ...pi,
              product: mockStore.products.find((prod) => prod.id === pi.productId),
            })),
        }));
      res.json({ success: true, purchases: list });
    }
  } catch (err: any) {
    res.status(500).json({ success: false, message: 'Failed to fetch purchases: ' + err.message });
  }
});

// Create Purchase Entry & Auto-Increment Stock
purchasesRouter.post('/', authenticate, resolveTenant, async (req: AuthenticatedRequest, res: Response): Promise<void> => {
  try {
    const bizId = req.tenant!.businessId;
    const { supplierId, items, notes } = req.body;

    if (!items || !Array.isArray(items) || items.length === 0) {
      res.status(400).json({ success: false, message: 'Items array is required for purchase entry.' });
      return;
    }

    const purchaseNumber = `PO-${Date.now().toString().slice(-6)}`;

    if (isPostgres()) {
      const result = await prisma.$transaction(async (tx) => {
        let totalAmount = 0;
        const purchaseItems: any[] = [];

        for (const item of items) {
          const product = await tx.product.findFirst({
            where: { id: item.productId, businessId: bizId },
          });
          if (!product) throw new Error(`Product "${item.productId}" not found.`);

          const qty = parseInt(item.quantity, 10);
          const unitPrice = parseFloat(item.unitPrice) || Number(product.purchasePrice);
          const lineTotal = qty * unitPrice;
          totalAmount += lineTotal;

          const newStock = product.stockQuantity + qty;
          await tx.product.update({
            where: { id: product.id },
            data: { stockQuantity: newStock, purchasePrice: unitPrice },
          });

          await tx.stockMovement.create({
            data: {
              businessId: bizId,
              productId: product.id,
              type: 'PURCHASE',
              quantity: qty,
              balanceAfter: newStock,
              referenceId: purchaseNumber,
              notes: `Stock in via ${purchaseNumber}`,
            },
          });

          purchaseItems.push({
            productId: product.id,
            quantity: qty,
            unitPrice,
            totalPrice: lineTotal,
          });
        }

        const purchase = await tx.purchase.create({
          data: {
            businessId: bizId,
            supplierId: supplierId || null,
            purchaseNumber,
            totalAmount,
            notes: notes || null,
            items: { create: purchaseItems },
          },
          include: { items: { include: { product: true } }, supplier: true },
        });

        return purchase;
      });

      await logAuditEvent({
        businessId: bizId,
        userId: req.user!.id,
        action: 'CREATE_PURCHASE',
        entity: 'PURCHASE',
        entityId: result.id,
        payload: { purchaseNumber, total: result.totalAmount },
      });

      res.status(201).json({ success: true, message: `Purchase order ${purchaseNumber} recorded.`, purchase: result });
    } else {
      let totalAmount = 0;
      const pid = 'po-' + Date.now();
      const pItems: any[] = [];

      for (const item of items) {
        const prod = mockStore.products.find((p) => p.id === item.productId && p.businessId === bizId);
        if (!prod) {
          res.status(404).json({ success: false, message: `Product "${item.productId}" not found.` });
          return;
        }

        const qty = parseInt(item.quantity, 10);
        const unitPrice = parseFloat(item.unitPrice) || Number(prod.purchasePrice);
        const lineTotal = qty * unitPrice;
        totalAmount += lineTotal;

        prod.stockQuantity += qty;
        prod.purchasePrice = unitPrice;

        mockStore.stockMovements.push({
          id: 'sm-' + Date.now() + Math.random(),
          businessId: bizId,
          productId: prod.id,
          type: 'PURCHASE',
          quantity: qty,
          balanceAfter: prod.stockQuantity,
          referenceId: purchaseNumber,
          notes: `Stock in via ${purchaseNumber}`,
          createdAt: new Date(),
        });

        const pItemObj = {
          id: 'pi-' + Date.now() + Math.random(),
          purchaseId: pid,
          productId: prod.id,
          quantity: qty,
          unitPrice,
          totalPrice: lineTotal,
          product: prod,
        };
        pItems.push(pItemObj);
        mockStore.purchaseItems.push(pItemObj);
      }

      const newPurchase = {
        id: pid,
        businessId: bizId,
        supplierId: supplierId || null,
        purchaseNumber,
        totalAmount,
        notes: notes || null,
        createdAt: new Date(),
        items: pItems,
        supplier: mockStore.suppliers.find((s) => s.id === supplierId),
      };

      mockStore.purchases.unshift(newPurchase);

      await logAuditEvent({
        businessId: bizId,
        userId: req.user!.id,
        action: 'CREATE_PURCHASE',
        entity: 'PURCHASE',
        entityId: pid,
        payload: { purchaseNumber, total: totalAmount },
      });

      res.status(201).json({ success: true, message: `Purchase order ${purchaseNumber} recorded.`, purchase: newPurchase });
    }
  } catch (err: any) {
    res.status(500).json({ success: false, message: 'Failed to record purchase: ' + err.message });
  }
});
