import { Router, Response } from 'express';
import { prisma, isPostgres, mockStore } from '../../database/db';
import { AuthenticatedRequest } from '../../types';
import { authenticate } from '../../middleware/auth';
import { resolveTenant } from '../../middleware/tenant';
import { requireRole } from '../../middleware/roleGuard';

export const auditRouter = Router();

auditRouter.get(
  '/',
  authenticate,
  resolveTenant,
  requireRole(['OWNER', 'MANAGER']),
  async (req: AuthenticatedRequest, res: Response): Promise<void> => {
    try {
      const bizId = req.tenant!.businessId;
      if (isPostgres()) {
        const logs = await prisma.auditLog.findMany({
          where: { businessId: bizId },
          include: { user: { select: { id: true, name: true, email: true } } },
          orderBy: { timestamp: 'desc' },
          take: 100,
        });

        res.json({
          success: true,
          auditLogs: logs.map((l) => ({
            id: l.id,
            action: l.action,
            entity: l.entity,
            entityId: l.entityId,
            userName: l.user?.name || 'System',
            userEmail: l.user?.email || '',
            payload: l.payload ? JSON.parse(l.payload) : null,
            timestamp: l.timestamp,
          })),
        });
      } else {
        const logs = mockStore.auditLogs
          .filter((l) => l.businessId === bizId)
          .slice(0, 100)
          .map((l) => {
            const u = mockStore.users.find((user) => user.id === l.userId);
            return {
              id: l.id,
              action: l.action,
              entity: l.entity,
              entityId: l.entityId,
              userName: u?.name || 'System',
              userEmail: u?.email || '',
              payload: l.payload ? JSON.parse(l.payload) : null,
              timestamp: l.timestamp,
            };
          });

        res.json({ success: true, auditLogs: logs });
      }
    } catch (err: any) {
      res.status(500).json({ success: false, message: 'Failed to fetch audit logs: ' + err.message });
    }
  }
);
