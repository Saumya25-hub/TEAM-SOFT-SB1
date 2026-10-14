import { prisma, isPostgres, mockStore } from '../database/db';

export async function logAuditEvent(params: {
  businessId: string;
  userId?: string;
  action: string;
  entity: string;
  entityId?: string;
  payload?: any;
}) {
  try {
    const payloadStr = params.payload ? JSON.stringify(params.payload) : null;
    if (isPostgres()) {
      await prisma.auditLog.create({
        data: {
          businessId: params.businessId,
          userId: params.userId,
          action: params.action,
          entity: params.entity,
          entityId: params.entityId,
          payload: payloadStr,
        },
      });
    } else {
      mockStore.auditLogs.unshift({
        id: 'aud-' + Date.now() + '-' + Math.random().toString(36).substring(2, 6),
        businessId: params.businessId,
        userId: params.userId,
        action: params.action,
        entity: params.entity,
        entityId: params.entityId,
        payload: payloadStr,
        timestamp: new Date(),
      });
    }
  } catch (err: any) {
    console.error('⚠️ [Audit] Failed to record audit log:', err.message);
  }
}
