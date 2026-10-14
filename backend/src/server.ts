import express from 'express';
import cors from 'cors';
import dotenv from 'dotenv';
import { initDatabase } from './database/db';
import { authRouter } from './modules/auth/auth.router';
import { businessesRouter } from './modules/businesses/businesses.router';
import { dashboardRouter } from './modules/dashboard/dashboard.router';
import { productsRouter } from './modules/products/products.router';
import { customersRouter } from './modules/customers/customers.router';
import { suppliersRouter } from './modules/suppliers/suppliers.router';
import { invoicesRouter } from './modules/invoices/invoices.router';
import { purchasesRouter } from './modules/purchases/purchases.router';
import { transactionsRouter } from './modules/transactions/transactions.router';
import { reportsRouter } from './modules/reports/reports.router';
import { auditRouter } from './modules/audit/audit.router';

dotenv.config();

const app = express();
const PORT = process.env.PORT || 5000;

// Security & Middlewares
app.use(cors({ origin: true, credentials: true }));
app.use(express.json());

// Request logger
app.use((req, res, next) => {
  const start = Date.now();
  res.on('finish', () => {
    const duration = Date.now() - start;
    if (process.env.NODE_ENV !== 'test') {
      console.log(`[HTTP] ${req.method} ${req.originalUrl} -> ${res.statusCode} (${duration}ms)`);
    }
  });
  next();
});

// Health check
app.get('/api/health', (req, res) => {
  res.json({
    status: 'ONLINE',
    service: 'SimpleBilling Backend API',
    version: '1.0.0',
    timestamp: new Date().toISOString(),
  });
});

// Mount Domain Routers
app.use('/api/auth', authRouter);
app.use('/api/businesses', businessesRouter);
app.use('/api/dashboard', dashboardRouter);
app.use('/api/products', productsRouter);
app.use('/api/customers', customersRouter);
app.use('/api/suppliers', suppliersRouter);
app.use('/api/invoices', invoicesRouter);
app.use('/api/purchases', purchasesRouter);
app.use('/api/transactions', transactionsRouter);
app.use('/api/reports', reportsRouter);
app.use('/api/audit-logs', auditRouter);

// Global Error Handler
app.use((err: any, req: express.Request, res: express.Response, next: express.NextFunction) => {
  console.error('Unhandled Application Error:', err);
  res.status(500).json({
    success: false,
    message: err.message || 'An internal server error occurred.',
  });
});

// Start Server
if (process.env.NODE_ENV !== 'test') {
  initDatabase().then(() => {
    app.listen(PORT, () => {
      console.log(`🚀 [Backend] SimpleBilling Server listening on http://localhost:${PORT}`);
    });
  });
}

export default app;
