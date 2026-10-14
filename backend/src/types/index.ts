import { Request } from 'express';

export type UserRole = 'OWNER' | 'MANAGER' | 'BILLER' | 'VIEWER';
export type PaymentStatus = 'PAID' | 'PARTIAL' | 'UNPAID';
export type PaymentMethod = 'CASH' | 'BANK_TRANSFER' | 'UPI' | 'CARD';
export type StockMovementType = 'SALE' | 'PURCHASE' | 'RETURN' | 'ADJUSTMENT' | 'INITIAL';

export interface AuthUser {
  id: string;
  email: string;
  name: string;
}

export interface TenantContext {
  businessId: string;
  role: UserRole;
  businessName: string;
  currency: string;
  currencySymbol: string;
}

export interface AuthenticatedRequest extends Request {
  user?: AuthUser;
  tenant?: TenantContext;
}
