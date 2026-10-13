import { Response, NextFunction } from 'express';
import { AuthenticatedRequest, UserRole } from '../types';

export function requireRole(allowedRoles: UserRole[]) {
  return (req: AuthenticatedRequest, res: Response, next: NextFunction): void => {
    const userRole = req.tenant?.role;
    if (!userRole || !allowedRoles.includes(userRole)) {
      res.status(403).json({
        success: false,
        message: `Forbidden. Action requires one of the following permissions: ${allowedRoles.join(', ')}. Your current role is: ${userRole || 'UNKNOWN'}.`,
      });
      return;
    }
    next();
  };
}
