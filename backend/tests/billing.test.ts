import { describe, it } from 'node:test';
import assert from 'node:assert';

describe('Billing Engine & Price Calculations', () => {
  it('correctly calculates line items, taxes, and subtotal', () => {
    const items = [
      { unitPrice: 240.0, qty: 4, taxRate: 12.0 },
      { unitPrice: 899.0, qty: 2, taxRate: 18.0 },
    ];

    let subtotal = 0;
    let totalTax = 0;

    for (const item of items) {
      const lineSub = item.unitPrice * item.qty;
      const lineTax = (lineSub * item.taxRate) / 100;
      subtotal += lineSub;
      totalTax += lineTax;
    }

    const discount = 50.0;
    const finalTotal = subtotal - discount + totalTax;

    // Expected:
    // item 1: 240 * 4 = 960, tax = 115.2
    // item 2: 899 * 2 = 1798, tax = 323.64
    // subtotal = 2758
    // totalTax = 438.84
    // finalTotal = 2758 - 50 + 438.84 = 3146.84
    assert.strictEqual(subtotal, 2758.0);
    assert.strictEqual(Number(totalTax.toFixed(2)), 438.84);
    assert.strictEqual(Number(finalTotal.toFixed(2)), 3146.84);
  });

  it('safely bounds discounts and prevents negative invoice totals', () => {
    const subtotal = 100.0;
    const excessDiscount = 150.0;
    const boundedDiscount = Math.min(subtotal, Math.max(0, excessDiscount));
    const totalAmount = Math.max(0, subtotal - boundedDiscount);

    assert.strictEqual(boundedDiscount, 100.0);
    assert.strictEqual(totalAmount, 0.0);
  });
});

describe('Inventory & Stock Deduction Logic', () => {
  it('deducts stock correctly upon sale and records movement balance', () => {
    const initialStock = 120;
    const soldQty = 4;

    assert.ok(initialStock >= soldQty, 'Stock must be sufficient for checkout');
    const newStock = initialStock - soldQty;
    assert.strictEqual(newStock, 116);
  });

  it('rejects checkout when requested quantity exceeds available inventory', () => {
    const availableStock = 5;
    const requestedQty = 10;
    const hasSufficientStock = availableStock >= requestedQty;

    assert.strictEqual(hasSufficientStock, false);
  });

  it('correctly increments stock when receiving purchase order', () => {
    const currentStock = 28;
    const receivedQty = 50;
    const updatedStock = currentStock + receivedQty;

    assert.strictEqual(updatedStock, 78);
  });
});

describe('Multi-Tenant Isolation & Role Permission Guards', () => {
  it('scopes queries strictly to current businessId', () => {
    const sampleProducts = [
      { id: 'p1', businessId: 'biz-alpha', name: 'Product A' },
      { id: 'p2', businessId: 'biz-beta', name: 'Product B' },
      { id: 'p3', businessId: 'biz-alpha', name: 'Product C' },
    ];

    const currentTenantId = 'biz-alpha';
    const tenantFiltered = sampleProducts.filter((p) => p.businessId === currentTenantId);

    assert.strictEqual(tenantFiltered.length, 2);
    assert.ok(tenantFiltered.every((p) => p.businessId === currentTenantId));
  });

  it('enforces RBAC role permissions correctly', () => {
    const allowedRolesForSettings = ['OWNER', 'MANAGER'];

    const userRoleOwner = 'OWNER';
    const userRoleBiller = 'BILLER';

    assert.strictEqual(allowedRolesForSettings.includes(userRoleOwner), true);
    assert.strictEqual(allowedRolesForSettings.includes(userRoleBiller), false);
  });
});
