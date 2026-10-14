import { Router, Response } from 'express';
import { prisma, isPostgres, mockStore } from '../../database/db';
import { AuthenticatedRequest } from '../../types';
import { authenticate } from '../../middleware/auth';
import { resolveTenant } from '../../middleware/tenant';

export const reportsRouter = Router();

// Sales & Revenue Report
reportsRouter.get('/sales', authenticate, resolveTenant, async (req: AuthenticatedRequest, res: Response): Promise<void> => {
  try {
    const bizId = req.tenant!.businessId;
    if (isPostgres()) {
      const invoices = await prisma.invoice.findMany({
        where: { businessId: bizId },
        include: { items: { include: { product: true } } },
      });

      const totalRevenue = invoices.reduce((acc, i) => acc + Number(i.totalAmount), 0);
      const totalTax = invoices.reduce((acc, i) => acc + Number(i.taxAmount), 0);
      const totalDiscounts = invoices.reduce((acc, i) => acc + Number(i.discountAmount), 0);

      res.json({
        success: true,
        report: {
          totalInvoices: invoices.length,
          totalRevenue,
          totalTaxCollected: totalTax,
          totalDiscountsGiven: totalDiscounts,
          averageOrderValue: invoices.length > 0 ? totalRevenue / invoices.length : 0,
        },
      });
    } else {
      const invoices = mockStore.invoices.filter((i) => i.businessId === bizId);
      const totalRevenue = invoices.reduce((acc, i) => acc + Number(i.totalAmount), 0);
      const totalTax = invoices.reduce((acc, i) => acc + Number(i.taxAmount), 0);
      const totalDiscounts = invoices.reduce((acc, i) => acc + Number(i.discountAmount), 0);

      res.json({
        success: true,
        report: {
          totalInvoices: invoices.length,
          totalRevenue,
          totalTaxCollected: totalTax,
          totalDiscountsGiven: totalDiscounts,
          averageOrderValue: invoices.length > 0 ? totalRevenue / invoices.length : 0,
        },
      });
    }
  } catch (err: any) {
    res.status(500).json({ success: false, message: 'Failed to generate sales report: ' + err.message });
  }
});

// Inventory Valuation Report
reportsRouter.get('/inventory', authenticate, resolveTenant, async (req: AuthenticatedRequest, res: Response): Promise<void> => {
  try {
    const bizId = req.tenant!.businessId;
    if (isPostgres()) {
      const products = await prisma.product.findMany({ where: { businessId: bizId, isActive: true } });
      const totalItemsInStock = products.reduce((acc, p) => acc + p.stockQuantity, 0);
      const purchaseValue = products.reduce((acc, p) => acc + p.stockQuantity * Number(p.purchasePrice), 0);
      const retailValue = products.reduce((acc, p) => acc + p.stockQuantity * Number(p.sellingPrice), 0);

      res.json({
        success: true,
        report: {
          uniqueProductCount: products.length,
          totalUnitsInStock: totalItemsInStock,
          totalPurchaseCostValuation: purchaseValue,
          totalRetailSellingValuation: retailValue,
          projectedGrossMargin: retailValue - purchaseValue,
        },
      });
    } else {
      const products = mockStore.products.filter((p) => p.businessId === bizId && p.isActive !== false);
      const totalItemsInStock = products.reduce((acc, p) => acc + p.stockQuantity, 0);
      const purchaseValue = products.reduce((acc, p) => acc + p.stockQuantity * Number(p.purchasePrice), 0);
      const retailValue = products.reduce((acc, p) => acc + p.stockQuantity * Number(p.sellingPrice), 0);

      res.json({
        success: true,
        report: {
          uniqueProductCount: products.length,
          totalUnitsInStock: totalItemsInStock,
          totalPurchaseCostValuation: purchaseValue,
          totalRetailSellingValuation: retailValue,
          projectedGrossMargin: retailValue - purchaseValue,
        },
      });
    }
  } catch (err: any) {
    res.status(500).json({ success: false, message: 'Failed to generate inventory report: ' + err.message });
  }
});
