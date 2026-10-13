import { PrismaClient } from '@prisma/client';
import bcrypt from 'bcryptjs';

// Prisma client instance
export const prisma = new PrismaClient({
  log: process.env.NODE_ENV === 'development' ? ['warn', 'error'] : ['error'],
});

// In-Memory / Standalone Mock Store for zero-setup execution & test fallback
export interface MockStore {
  users: any[];
  businesses: any[];
  memberships: any[];
  categories: any[];
  products: any[];
  stockMovements: any[];
  customers: any[];
  suppliers: any[];
  invoices: any[];
  invoiceItems: any[];
  purchases: any[];
  purchaseItems: any[];
  transactions: any[];
  auditLogs: any[];
}

export const mockStore: MockStore = {
  users: [],
  businesses: [],
  memberships: [],
  categories: [],
  products: [],
  stockMovements: [],
  customers: [],
  suppliers: [],
  invoices: [],
  invoiceItems: [],
  purchases: [],
  purchaseItems: [],
  transactions: [],
  auditLogs: [],
};

let isConnectedToPostgres = false;

export async function initDatabase(): Promise<{ isPostgres: boolean }> {
  try {
    // Attempt connecting to PostgreSQL
    await prisma.$connect();
    isConnectedToPostgres = true;
    console.log('✅ [Database] Successfully connected to PostgreSQL via Prisma.');
    return { isPostgres: true };
  } catch (err: any) {
    console.warn('⚠️ [Database] PostgreSQL connection failed or not running locally:', err.message);
    console.log('🔄 [Database] Falling back to high-performance in-memory transactional data store with demo seed.');
    seedMockStore();
    isConnectedToPostgres = false;
    return { isPostgres: false };
  }
}

export function isPostgres(): boolean {
  return isConnectedToPostgres;
}

function seedMockStore() {
  const adminId = 'usr-admin-01';
  const bizId = 'biz-default-01';

  // Seed default admin user (Password: admin123)
  const salt = bcrypt.genSaltSync(10);
  const passwordHash = bcrypt.hashSync('admin123', salt);

  mockStore.users = [
    {
      id: adminId,
      email: 'admin@simplebilling.com',
      name: 'Jenil Rana',
      passwordHash,
      isActive: true,
      createdAt: new Date(),
    },
    {
      id: 'usr-biller-02',
      email: 'biller@simplebilling.com',
      name: 'Cashier Staff',
      passwordHash: bcrypt.hashSync('biller123', salt),
      isActive: true,
      createdAt: new Date(),
    },
  ];

  // Seed default business
  mockStore.businesses = [
    {
      id: bizId,
      name: 'Simple Billing SuperStore',
      legalName: 'Simple Billing Retail Private Limited',
      taxNumber: '24AAACT1234F1Z5',
      phone: '+91 9876543210',
      email: 'contact@simplebilling.com',
      address: '101 Commercial Hub, Ring Road, Surat, Gujarat 395007',
      currency: 'INR',
      currencySymbol: '₹',
      createdAt: new Date(),
    },
  ];

  // Seed memberships
  mockStore.memberships = [
    {
      id: 'mem-01',
      userId: adminId,
      businessId: bizId,
      role: 'OWNER',
      createdAt: new Date(),
    },
    {
      id: 'mem-02',
      userId: 'usr-biller-02',
      businessId: bizId,
      role: 'BILLER',
      createdAt: new Date(),
    },
  ];

  // Seed categories
  mockStore.categories = [
    { id: 'cat-01', businessId: bizId, name: 'Stationery' },
    { id: 'cat-02', businessId: bizId, name: 'Electronics' },
    { id: 'cat-03', businessId: bizId, name: 'Office Supplies' },
  ];

  // Seed products
  mockStore.products = [
    {
      id: 'prd-01',
      businessId: bizId,
      categoryId: 'cat-01',
      sku: 'ST-A4-PAP',
      barcode: '890123456701',
      name: 'A4 Copier Paper (75 GSM, 500 Sheets)',
      unit: 'Ream',
      purchasePrice: 180.0,
      sellingPrice: 240.0,
      stockQuantity: 120,
      minStockAlert: 15,
      taxRatePercent: 12.0,
      isActive: true,
      createdAt: new Date(),
    },
    {
      id: 'prd-02',
      businessId: bizId,
      categoryId: 'cat-01',
      sku: 'ST-PEN-BLU',
      barcode: '890123456702',
      name: 'Gel Pen Smooth Ink (Blue)',
      unit: 'Piece',
      purchasePrice: 6.5,
      sellingPrice: 10.0,
      stockQuantity: 450,
      minStockAlert: 50,
      taxRatePercent: 18.0,
      isActive: true,
      createdAt: new Date(),
    },
    {
      id: 'prd-03',
      businessId: bizId,
      categoryId: 'cat-02',
      sku: 'EL-LPT-STN',
      barcode: '890123456703',
      name: 'Ergonomic Aluminium Laptop Stand',
      unit: 'Piece',
      purchasePrice: 620.0,
      sellingPrice: 899.0,
      stockQuantity: 28,
      minStockAlert: 5,
      taxRatePercent: 18.0,
      isActive: true,
      createdAt: new Date(),
    },
    {
      id: 'prd-04',
      businessId: bizId,
      categoryId: 'cat-02',
      sku: 'EL-MOU-PAD',
      barcode: '890123456704',
      name: 'Extended Gaming Desk Mouse Pad',
      unit: 'Piece',
      purchasePrice: 110.0,
      sellingPrice: 199.0,
      stockQuantity: 8, // Low stock trigger
      minStockAlert: 10,
      taxRatePercent: 18.0,
      isActive: true,
      createdAt: new Date(),
    },
    {
      id: 'prd-05',
      businessId: bizId,
      categoryId: 'cat-03',
      sku: 'OF-STP-HVY',
      barcode: '890123456705',
      name: 'Heavy Duty Metal Stapler (No. 10)',
      unit: 'Piece',
      purchasePrice: 130.0,
      sellingPrice: 185.0,
      stockQuantity: 42,
      minStockAlert: 10,
      taxRatePercent: 12.0,
      isActive: true,
      createdAt: new Date(),
    },
  ];

  // Seed customers
  mockStore.customers = [
    {
      id: 'cst-01',
      businessId: bizId,
      name: 'Amit Sharma',
      phone: '9876543210',
      email: 'amit.sharma@example.com',
      address: 'Plot 42, Civil Lines, Jaipur',
      outstandingBalance: 0.0,
      createdAt: new Date(),
    },
    {
      id: 'cst-02',
      businessId: bizId,
      name: 'Priya Patel',
      phone: '9123456780',
      email: 'priya.patel@example.com',
      address: '24 Green Avenue, Ahmedabad',
      outstandingBalance: 1250.0,
      createdAt: new Date(),
    },
    {
      id: 'cst-03',
      businessId: bizId,
      name: 'Rahul Verma',
      phone: '9988776655',
      email: 'rahul.verma@example.com',
      address: 'Flat 302, Sunrise Towers, Pune',
      outstandingBalance: 0.0,
      createdAt: new Date(),
    },
    {
      id: 'cst-04',
      businessId: bizId,
      name: 'Sneha Gupta',
      phone: '9001122334',
      email: 'sneha.gupta@example.com',
      address: '15 Mall Road, Delhi',
      outstandingBalance: 340.0,
      createdAt: new Date(),
    },
  ];

  // Seed suppliers
  mockStore.suppliers = [
    {
      id: 'sup-01',
      businessId: bizId,
      name: 'Navneet Stationery Wholesale',
      phone: '022-28491000',
      email: 'orders@navneetwholesale.com',
      address: 'Dadar West, Mumbai',
      gstNumber: '27AAACN1234E1Z8',
      outstandingBalance: 15400.0,
      createdAt: new Date(),
    },
    {
      id: 'sup-02',
      businessId: bizId,
      name: 'Quantum Tech Distributors',
      phone: '080-41223344',
      email: 'sales@quantumdist.in',
      address: 'Electronic City, Bengaluru',
      gstNumber: '29AAACQ5678K1Z2',
      outstandingBalance: 0.0,
      createdAt: new Date(),
    },
  ];

  // Seed sample invoices & transactions
  const inv1 = {
    id: 'inv-01',
    businessId: bizId,
    customerId: 'cst-01',
    invoiceNumber: 'INV-2026-0001',
    subtotal: 1000.0,
    discountAmount: 50.0,
    taxAmount: 114.0,
    totalAmount: 1064.0,
    paidAmount: 1064.0,
    paymentStatus: 'PAID',
    notes: 'Counter sales payment via UPI',
    createdAt: new Date(Date.now() - 86400000 * 2),
  };

  const inv2 = {
    id: 'inv-02',
    businessId: bizId,
    customerId: 'cst-02',
    invoiceNumber: 'INV-2026-0002',
    subtotal: 2400.0,
    discountAmount: 0.0,
    taxAmount: 360.0,
    totalAmount: 2760.0,
    paidAmount: 1500.0,
    paymentStatus: 'PARTIAL',
    notes: 'Part cash payment, balance on credit',
    createdAt: new Date(Date.now() - 86400000),
  };

  mockStore.invoices = [inv1, inv2];

  mockStore.invoiceItems = [
    {
      id: 'ii-01',
      invoiceId: 'inv-01',
      productId: 'prd-01',
      quantity: 4,
      unitPrice: 240.0,
      taxRate: 12.0,
      taxAmount: 114.0,
      totalPrice: 1064.0,
    },
    {
      id: 'ii-02',
      invoiceId: 'inv-02',
      productId: 'prd-03',
      quantity: 2,
      unitPrice: 899.0,
      taxRate: 18.0,
      taxAmount: 323.64,
      totalPrice: 2121.64,
    },
  ];

  mockStore.transactions = [
    {
      id: 'txn-01',
      businessId: bizId,
      invoiceId: 'inv-01',
      receiptNumber: 'RCP-261011-001',
      paymentMethod: 'UPI',
      amount: 1064.0,
      notes: 'GPay transaction #984210',
      transactionDate: new Date(Date.now() - 86400000 * 2),
    },
    {
      id: 'txn-02',
      businessId: bizId,
      invoiceId: 'inv-02',
      receiptNumber: 'RCP-261012-002',
      paymentMethod: 'CASH',
      amount: 1500.0,
      notes: 'Initial cash deposit',
      transactionDate: new Date(Date.now() - 86400000),
    },
  ];

  mockStore.auditLogs = [
    {
      id: 'aud-01',
      businessId: bizId,
      userId: adminId,
      action: 'SYSTEM_BOOTSTRAP',
      entity: 'BUSINESS',
      entityId: bizId,
      payload: JSON.stringify({ message: 'Initial business setup and demo seed' }),
      timestamp: new Date(),
    },
  ];
}
