import { Router, Response } from 'express';
import bcrypt from 'bcryptjs';
import jwt from 'jsonwebtoken';
import { prisma, isPostgres, mockStore } from '../../database/db';
import { AuthenticatedRequest, AuthUser } from '../../types';
import { authenticate } from '../../middleware/auth';
import { logAuditEvent } from '../../utils/audit';

export const authRouter = Router();
const JWT_SECRET = process.env.JWT_SECRET || 'super-secret-billing-jwt-key-change-in-production-2026';
const signOptions: jwt.SignOptions = { expiresIn: '7d' };

// Register a new user & default business
authRouter.post('/register', async (req, res): Promise<void> => {
  try {
    const { name, email, password, businessName } = req.body;
    if (!name || !email || !password) {
      res.status(400).json({ success: false, message: 'Name, email, and password are required.' });
      return;
    }

    const salt = bcrypt.genSaltSync(10);
    const passwordHash = bcrypt.hashSync(password, salt);
    const bizName = businessName?.trim() || `${name}'s Business`;

    if (isPostgres()) {
      const existingUser = await prisma.user.findUnique({ where: { email } });
      if (existingUser) {
        res.status(400).json({ success: false, message: 'A user with this email already exists.' });
        return;
      }

      const result = await prisma.$transaction(async (tx) => {
        const user = await tx.user.create({
          data: { name, email, passwordHash },
        });

        const business = await tx.business.create({
          data: {
            name: bizName,
            email,
          },
        });

        await tx.membership.create({
          data: {
            userId: user.id,
            businessId: business.id,
            role: 'OWNER',
          },
        });

        return { user, business };
      });

      const tokenPayload: AuthUser = { id: result.user.id, email: result.user.email, name: result.user.name };
      const token = jwt.sign(tokenPayload, JWT_SECRET, signOptions);

      res.status(201).json({
        success: true,
        token,
        user: tokenPayload,
        business: result.business,
      });
    } else {
      // Mock store
      if (mockStore.users.some((u) => u.email.toLowerCase() === email.toLowerCase())) {
        res.status(400).json({ success: false, message: 'A user with this email already exists.' });
        return;
      }

      const userId = 'usr-' + Date.now();
      const bizId = 'biz-' + Date.now();

      const newUser = { id: userId, email, name, passwordHash, isActive: true, createdAt: new Date() };
      const newBiz = {
        id: bizId,
        name: bizName,
        email,
        currency: 'INR',
        currencySymbol: '₹',
        createdAt: new Date(),
      };
      const newMem = { id: 'mem-' + Date.now(), userId, businessId: bizId, role: 'OWNER', createdAt: new Date() };

      mockStore.users.push(newUser);
      mockStore.businesses.push(newBiz);
      mockStore.memberships.push(newMem);

      const tokenPayload: AuthUser = { id: userId, email, name };
      const token = jwt.sign(tokenPayload, JWT_SECRET, signOptions);

      res.status(201).json({
        success: true,
        token,
        user: tokenPayload,
        business: newBiz,
      });
    }
  } catch (err: any) {
    res.status(500).json({ success: false, message: 'Registration failed: ' + err.message });
  }
});

// Login
authRouter.post('/login', async (req, res): Promise<void> => {
  try {
    const { email, password } = req.body;
    if (!email || !password) {
      res.status(400).json({ success: false, message: 'Email and password are required.' });
      return;
    }

    let user: any;
    let memberships: any[] = [];

    if (isPostgres()) {
      user = await prisma.user.findUnique({
        where: { email },
        include: {
          memberships: {
            include: { business: true },
          },
        },
      });
      memberships = user?.memberships || [];
    } else {
      user = mockStore.users.find((u) => u.email.toLowerCase() === email.toLowerCase());
      if (user) {
        memberships = mockStore.memberships
          .filter((m) => m.userId === user.id)
          .map((m) => ({
            ...m,
            business: mockStore.businesses.find((b) => b.id === m.businessId),
          }));
      }
    }

    if (!user || !bcrypt.compareSync(password, user.passwordHash)) {
      res.status(401).json({ success: false, message: 'Invalid email or password.' });
      return;
    }

    const tokenPayload: AuthUser = { id: user.id, email: user.email, name: user.name };
    const token = jwt.sign(tokenPayload, JWT_SECRET, signOptions);

    const businesses = memberships.map((m) => ({
      businessId: m.businessId,
      businessName: m.business?.name || 'Business',
      role: m.role,
      currency: m.business?.currency || 'INR',
      currencySymbol: m.business?.currencySymbol || '₹',
    }));

    if (businesses.length > 0) {
      await logAuditEvent({
        businessId: businesses[0].businessId,
        userId: user.id,
        action: 'USER_LOGIN',
        entity: 'USER',
        entityId: user.id,
      });
    }

    res.json({
      success: true,
      token,
      user: tokenPayload,
      businesses,
      activeBusiness: businesses[0] || null,
    });
  } catch (err: any) {
    res.status(500).json({ success: false, message: 'Login failed: ' + err.message });
  }
});

// Get Current User Profile
authRouter.get('/me', authenticate, async (req: AuthenticatedRequest, res: Response): Promise<void> => {
  try {
    const userId = req.user!.id;
    let businesses: any[] = [];

    if (isPostgres()) {
      const mems = await prisma.membership.findMany({
        where: { userId },
        include: { business: true },
      });
      businesses = mems.map((m) => ({
        businessId: m.businessId,
        businessName: m.business.name,
        role: m.role,
        currency: m.business.currency,
        currencySymbol: m.business.currencySymbol,
      }));
    } else {
      businesses = mockStore.memberships
        .filter((m) => m.userId === userId)
        .map((m) => {
          const biz = mockStore.businesses.find((b) => b.id === m.businessId);
          return {
            businessId: m.businessId,
            businessName: biz?.name || 'Business',
            role: m.role,
            currency: biz?.currency || 'INR',
            currencySymbol: biz?.currencySymbol || '₹',
          };
        });
    }

    res.json({
      success: true,
      user: req.user,
      businesses,
    });
  } catch (err: any) {
    res.status(500).json({ success: false, message: 'Failed to fetch user profile: ' + err.message });
  }
});
