import { Router, Response } from 'express';
import { prisma, isPostgres, mockStore } from '../../database/db';
import { AuthenticatedRequest } from '../../types';
import { authenticate } from '../../middleware/auth';
import { resolveTenant } from '../../middleware/tenant';
import { requireRole } from '../../middleware/roleGuard';
import { logAuditEvent } from '../../utils/audit';

export const productsRouter = Router();

// List products
productsRouter.get('/', authenticate, resolveTenant, async (req: AuthenticatedRequest, res: Response): Promise<void> => {
  try {
    const bizId = req.tenant!.businessId;
    const query = (req.query.q as string)?.toLowerCase().trim();
    const barcode = req.query.barcode as string | undefined;

    if (isPostgres()) {
      let whereClause: any = { businessId: bizId, isActive: true };
      if (barcode) {
        whereClause.barcode = barcode;
      } else if (query) {
        whereClause.OR = [
          { name: { contains: query, mode: 'insensitive' } },
          { sku: { contains: query, mode: 'insensitive' } },
          { barcode: { contains: query, mode: 'insensitive' } },
        ];
      }

      const products = await prisma.product.findMany({
        where: whereClause,
        include: { category: true },
        orderBy: { name: 'asc' },
      });

      res.json({
        success: true,
        products: products.map((p) => ({
          id: p.id,
          sku: p.sku,
          barcode: p.barcode,
          name: p.name,
          category: p.category?.name || 'General',
          categoryId: p.categoryId,
          unit: p.unit,
          purchasePrice: Number(p.purchasePrice),
          sellingPrice: Number(p.sellingPrice),
          stockQuantity: p.stockQuantity,
          minStockAlert: p.minStockAlert,
          taxRatePercent: Number(p.taxRatePercent),
          isLowStock: p.stockQuantity <= p.minStockAlert,
        })),
      });
    } else {
      let list = mockStore.products.filter((p) => p.businessId === bizId && p.isActive !== false);
      if (barcode) {
        list = list.filter((p) => p.barcode === barcode);
      } else if (query) {
        list = list.filter(
          (p) =>
            p.name.toLowerCase().includes(query) ||
            p.sku.toLowerCase().includes(query) ||
            (p.barcode && p.barcode.includes(query))
        );
      }

      res.json({
        success: true,
        products: list.map((p) => {
          const cat = mockStore.categories.find((c) => c.id === p.categoryId);
          return {
            id: p.id,
            sku: p.sku,
            barcode: p.barcode,
            name: p.name,
            category: cat?.name || 'General',
            categoryId: p.categoryId,
            unit: p.unit,
            purchasePrice: Number(p.purchasePrice),
            sellingPrice: Number(p.sellingPrice),
            stockQuantity: p.stockQuantity,
            minStockAlert: p.minStockAlert,
            taxRatePercent: Number(p.taxRatePercent),
            isLowStock: p.stockQuantity <= p.minStockAlert,
          };
        }),
      });
    }
  } catch (err: any) {
    res.status(500).json({ success: false, message: 'Failed to fetch products: ' + err.message });
  }
});

// Create product
productsRouter.post(
  '/',
  authenticate,
  resolveTenant,
  requireRole(['OWNER', 'MANAGER', 'BILLER']),
  async (req: AuthenticatedRequest, res: Response): Promise<void> => {
    try {
      const bizId = req.tenant!.businessId;
      const { sku, barcode, name, categoryId, unit, purchasePrice, sellingPrice, stockQuantity, minStockAlert, taxRatePercent } =
        req.body;

      if (!name || !sku) {
        res.status(400).json({ success: false, message: 'Product name and SKU are required.' });
        return;
      }

      const cleanSku = sku.trim().toUpperCase();
      const initialStock = parseInt(stockQuantity, 10) || 0;

      let newProduct: any;
      if (isPostgres()) {
        newProduct = await prisma.product.create({
          data: {
            businessId: bizId,
            sku: cleanSku,
            barcode: barcode?.trim() || null,
            name: name.trim(),
            categoryId: categoryId || null,
            unit: unit?.trim() || 'Piece',
            purchasePrice: parseFloat(purchasePrice) || 0,
            sellingPrice: parseFloat(sellingPrice) || 0,
            stockQuantity: initialStock,
            minStockAlert: parseInt(minStockAlert, 10) || 5,
            taxRatePercent: parseFloat(taxRatePercent) || 0,
          },
          include: { category: true },
        });

        if (initialStock > 0) {
          await prisma.stockMovement.create({
            data: {
              businessId: bizId,
              productId: newProduct.id,
              type: 'INITIAL',
              quantity: initialStock,
              balanceAfter: initialStock,
              notes: 'Initial inventory setup',
            },
          });
        }
      } else {
        const prodId = 'prd-' + Date.now();
        newProduct = {
          id: prodId,
          businessId: bizId,
          sku: cleanSku,
          barcode: barcode?.trim() || null,
          name: name.trim(),
          categoryId: categoryId || null,
          unit: unit?.trim() || 'Piece',
          purchasePrice: parseFloat(purchasePrice) || 0,
          sellingPrice: parseFloat(sellingPrice) || 0,
          stockQuantity: initialStock,
          minStockAlert: parseInt(minStockAlert, 10) || 5,
          taxRatePercent: parseFloat(taxRatePercent) || 0,
          isActive: true,
          createdAt: new Date(),
        };
        mockStore.products.push(newProduct);

        if (initialStock > 0) {
          mockStore.stockMovements.push({
            id: 'sm-' + Date.now(),
            businessId: bizId,
            productId: prodId,
            type: 'INITIAL',
            quantity: initialStock,
            balanceAfter: initialStock,
            notes: 'Initial inventory setup',
            createdAt: new Date(),
          });
        }
      }

      await logAuditEvent({
        businessId: bizId,
        userId: req.user!.id,
        action: 'CREATE_PRODUCT',
        entity: 'PRODUCT',
        entityId: newProduct.id,
        payload: { sku: cleanSku, name, initialStock },
      });

      res.status(201).json({ success: true, message: 'Product created successfully.', product: newProduct });
    } catch (err: any) {
      res.status(500).json({ success: false, message: 'Failed to create product: ' + err.message });
    }
  }
);

// Adjust stock quantity with movement log
productsRouter.post(
  '/:id/adjust-stock',
  authenticate,
  resolveTenant,
  requireRole(['OWNER', 'MANAGER']),
  async (req: AuthenticatedRequest, res: Response): Promise<void> => {
    try {
      const bizId = req.tenant!.businessId;
      const { id } = req.params;
      const { deltaQuantity, reason } = req.body;

      const delta = parseInt(deltaQuantity, 10);
      if (isNaN(delta) || delta === 0) {
        res.status(400).json({ success: false, message: 'Valid positive or negative adjustment delta required.' });
        return;
      }

      let updatedProduct: any;
      if (isPostgres()) {
        updatedProduct = await prisma.$transaction(async (tx) => {
          const prod = await tx.product.findFirst({ where: { id, businessId: bizId } });
          if (!prod) throw new Error('Product not found.');

          const newStock = Math.max(0, prod.stockQuantity + delta);
          const updated = await tx.product.update({
            where: { id },
            data: { stockQuantity: newStock },
          });

          await tx.stockMovement.create({
            data: {
              businessId: bizId,
              productId: id,
              type: delta > 0 ? 'PURCHASE' : 'ADJUSTMENT',
              quantity: Math.abs(delta),
              balanceAfter: newStock,
              notes: reason || 'Manual stock adjustment',
            },
          });

          return updated;
        });
      } else {
        const prod = mockStore.products.find((p) => p.id === id && p.businessId === bizId);
        if (!prod) {
          res.status(404).json({ success: false, message: 'Product not found.' });
          return;
        }

        const newStock = Math.max(0, prod.stockQuantity + delta);
        prod.stockQuantity = newStock;
        mockStore.stockMovements.push({
          id: 'sm-' + Date.now(),
          businessId: bizId,
          productId: id,
          type: delta > 0 ? 'PURCHASE' : 'ADJUSTMENT',
          quantity: Math.abs(delta),
          balanceAfter: newStock,
          notes: reason || 'Manual stock adjustment',
          createdAt: new Date(),
        });
        updatedProduct = prod;
      }

      await logAuditEvent({
        businessId: bizId,
        userId: req.user!.id,
        action: 'ADJUST_STOCK',
        entity: 'PRODUCT',
        entityId: id,
        payload: { delta, reason },
      });

      res.json({ success: true, message: 'Stock adjusted successfully.', product: updatedProduct });
    } catch (err: any) {
      res.status(500).json({ success: false, message: 'Stock adjustment failed: ' + err.message });
    }
  }
);
