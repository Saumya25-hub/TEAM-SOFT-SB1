import React, { useState, useEffect } from 'react';
import { Sidebar, NavModuleId } from './components/Sidebar';
import { Header } from './components/Header';
import { DashboardModule } from './modules/DashboardModule';
import { BillingPosModule } from './modules/BillingPosModule';
import { ProductsModule } from './modules/ProductsModule';
import { CustomersModule } from './modules/CustomersModule';
import { SuppliersModule } from './modules/SuppliersModule';
import { PurchasesModule } from './modules/PurchasesModule';
import { TransactionsModule } from './modules/TransactionsModule';
import { StatisticsModule } from './modules/StatisticsModule';
import { NotificationsModule } from './modules/NotificationsModule';
import { SettingsModule } from './modules/SettingsModule';
import { api } from './services/api';

export const App: React.FC = () => {
  const [activeModule, setActiveModule] = useState<NavModuleId>('overview');
  const [isDarkMode, setIsDarkMode] = useState<boolean>(() => {
    return localStorage.getItem('sb_theme') === 'dark' || true; // Default to sleek dark mode
  });
  const [user, setUser] = useState<any>({ name: 'Jenil Rana', email: 'admin@simplebilling.com' });
  const [business, setBusiness] = useState<any>(null);
  const [role, setRole] = useState('OWNER');
  const [searchQuery, setSearchQuery] = useState('');

  // Synchronize dark mode class on <html> element
  useEffect(() => {
    if (isDarkMode) {
      document.documentElement.classList.add('dark');
      localStorage.setItem('sb_theme', 'dark');
    } else {
      document.documentElement.classList.remove('dark');
      localStorage.setItem('sb_theme', 'light');
    }
  }, [isDarkMode]);

  useEffect(() => {
    bootstrapSession();
  }, []);

  const bootstrapSession = async () => {
    try {
      // Auto-authenticate default session if not logged in
      const loginRes = await api.login('admin@simplebilling.com', 'admin123');
      setUser(loginRes.user);
      if (loginRes.activeBusiness) {
        setRole(loginRes.activeBusiness.role);
      }
      loadBusinessDetails();
    } catch (err) {
      console.warn('Backend login fallback or offline mode:', err);
    }
  };

  const loadBusinessDetails = async () => {
    try {
      const res = await api.getCurrentBusiness();
      setBusiness(res.business);
      if (res.userRole) setRole(res.userRole);
    } catch (err) {
      console.warn('Failed to fetch business details:', err);
    }
  };

  const getModuleMeta = () => {
    switch (activeModule) {
      case 'overview':
        return {
          title: 'Analytics',
          subtitle: 'Real-time performance and financial dynamics.',
        };
      case 'statistics':
        return {
          title: 'Statistics & Reports',
          subtitle: 'View your sales revenue statistics and inventory performance metrics.',
        };
      case 'customers':
        return {
          title: 'Customers Directory',
          subtitle: 'Manage your verified customer accounts and credit balances.',
        };
      case 'suppliers':
        return {
          title: 'Suppliers & Vendors',
          subtitle: 'Directory of wholesale suppliers and procurement accounts.',
        };
      case 'products':
        return {
          title: 'Products & Inventory',
          subtitle: 'Track product SKU barcodes, stock balances, and low-inventory alerts.',
        };
      case 'purchases':
        return {
          title: 'Purchase Management',
          subtitle: 'Record supplier procurement deliveries and increment stock.',
        };
      case 'billing':
        return {
          title: 'New Bill (Point of Sale)',
          subtitle: 'Rapid barcode billing checkout, tax calculations, and printable receipts.',
        };
      case 'notifications':
        return {
          title: 'Messages & Notifications',
          subtitle: 'Review low stock warnings and system operational notices.',
        };
      case 'transactions':
        return {
          title: 'Transactions & Receipts',
          subtitle: 'Complete financial ledger of Cash, Bank Transfer, and UPI payments.',
        };
      case 'settings':
        return {
          title: 'Settings & Security',
          subtitle: 'Manage multi-tenant business profile, tax configuration, and audit logs.',
        };
      default:
        return { title: 'Simple Billing Suite', subtitle: 'Desktop management system' };
    }
  };

  const { title, subtitle } = getModuleMeta();

  return (
    <div className="flex h-screen w-screen overflow-hidden bg-[#f4f5f9] dark:bg-[#13141f] text-slate-900 dark:text-slate-100 transition-colors">
      {/* ── Left Sidebar ── */}
      <Sidebar
        activeModule={activeModule}
        onSelectModule={(id) => setActiveModule(id)}
        user={user}
        role={role}
      />

      {/* ── Main Content Canvas ── */}
      <div className="flex-1 flex flex-col min-w-0 overflow-hidden">
        {/* Top Header */}
        <Header
          title={title}
          subtitle={subtitle}
          isDarkMode={isDarkMode}
          onToggleTheme={() => setIsDarkMode(!isDarkMode)}
          businessName={business?.name || 'Simple Billing SuperStore'}
          onQuickBillClick={() => setActiveModule('billing')}
          searchQuery={searchQuery}
          onSearchChange={(q) => setSearchQuery(q)}
        />

        {/* Scrollable Canvas */}
        <main className="flex-1 overflow-y-auto px-8 py-2 bg-[#f4f5f9] dark:bg-[#13141f] transition-colors">
          <div className="max-w-[1600px] mx-auto">
            {activeModule === 'overview' && (
              <DashboardModule
                onNavigate={(m) => setActiveModule(m)}
                currencySymbol={business?.currencySymbol || '₹'}
              />
            )}
            {activeModule === 'billing' && (
              <BillingPosModule
                currencySymbol={business?.currencySymbol || '₹'}
                business={business}
              />
            )}
            {activeModule === 'products' && (
              <ProductsModule currencySymbol={business?.currencySymbol || '₹'} />
            )}
            {activeModule === 'customers' && (
              <CustomersModule currencySymbol={business?.currencySymbol || '₹'} />
            )}
            {activeModule === 'suppliers' && (
              <SuppliersModule currencySymbol={business?.currencySymbol || '₹'} />
            )}
            {activeModule === 'purchases' && (
              <PurchasesModule currencySymbol={business?.currencySymbol || '₹'} />
            )}
            {activeModule === 'transactions' && (
              <TransactionsModule currencySymbol={business?.currencySymbol || '₹'} />
            )}
            {activeModule === 'statistics' && (
              <StatisticsModule currencySymbol={business?.currencySymbol || '₹'} />
            )}
            {activeModule === 'notifications' && <NotificationsModule />}
            {activeModule === 'settings' && (
              <SettingsModule
                business={business}
                onBusinessUpdated={loadBusinessDetails}
              />
            )}
          </div>
        </main>
      </div>
    </div>
  );
};

export default App;
