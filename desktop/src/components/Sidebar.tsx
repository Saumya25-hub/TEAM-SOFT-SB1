import React from 'react';
import {
  LayoutDashboard,
  BarChart3,
  Users,
  Truck,
  Package,
  ShoppingCart,
  Receipt,
  Bell,
  CreditCard,
  ShieldCheck,
  Sparkles,
} from 'lucide-react';

export type NavModuleId =
  | 'overview'
  | 'statistics'
  | 'customers'
  | 'suppliers'
  | 'products'
  | 'purchases'
  | 'billing'
  | 'notifications'
  | 'transactions'
  | 'settings';

interface SidebarProps {
  activeModule: NavModuleId;
  onSelectModule: (id: NavModuleId) => void;
  user: { name: string; email: string } | null;
  role: string;
}

export const Sidebar: React.FC<SidebarProps> = ({ activeModule, onSelectModule, user, role }) => {
  const menuItems = [
    { id: 'overview' as NavModuleId, label: 'Overview', icon: LayoutDashboard },
    { id: 'statistics' as NavModuleId, label: 'Statistics', icon: BarChart3 },
    { id: 'customers' as NavModuleId, label: 'Customers', icon: Users },
    { id: 'suppliers' as NavModuleId, label: 'Suppliers', icon: Truck },
    { id: 'products' as NavModuleId, label: 'Products', icon: Package },
    { id: 'purchases' as NavModuleId, label: 'Purchases', icon: ShoppingCart },
    { id: 'billing' as NavModuleId, label: 'New Bill (POS)', icon: Receipt, highlight: true },
    { id: 'notifications' as NavModuleId, label: 'Messages', icon: Bell, badge: '2' },
    { id: 'transactions' as NavModuleId, label: 'Transactions', icon: CreditCard },
  ];

  const generalItems = [
    { id: 'settings' as NavModuleId, label: 'Settings & Security', icon: ShieldCheck },
  ];

  return (
    <aside className="w-64 bg-[#0e0f16] dark:bg-[#0b0c11] text-slate-300 flex flex-col flex-shrink-0 border-r border-[#1e2235] h-full select-none transition-colors">
      {/* Brand Header */}
      <div className="h-16 flex items-center px-6 gap-3 border-b border-slate-800/60">
        <div className="w-8 h-8 rounded-lg bg-gradient-to-tr from-blue-600 to-indigo-500 flex items-center justify-center text-white shadow-md shadow-blue-500/20">
          <Sparkles className="w-4 h-4" />
        </div>
        <div>
          <span className="font-bold text-white tracking-wide text-base">Simple Billing</span>
          <span className="block text-[10px] uppercase font-semibold tracking-wider text-blue-400">Windows Suite</span>
        </div>
      </div>

      {/* Navigation Scroll */}
      <div className="flex-1 overflow-y-auto px-3 py-4 space-y-6">
        {/* Main Menu Section */}
        <div>
          <span className="px-3 text-[11px] font-bold text-slate-400 dark:text-slate-500 tracking-wider uppercase block mb-2">
            MENU
          </span>
          <nav className="space-y-1">
            {menuItems.map((item) => {
              const Icon = item.icon;
              const isActive = activeModule === item.id;
              return (
                <button
                  key={item.id}
                  onClick={() => onSelectModule(item.id)}
                  className={`w-full flex items-center justify-between px-3 py-2.5 rounded-xl text-sm font-medium transition-all group ${
                    isActive
                      ? 'bg-blue-600/15 text-white font-semibold relative shadow-sm border border-blue-500/20'
                      : 'text-slate-400 hover:text-white hover:bg-slate-800/40'
                  }`}
                >
                  <div className="flex items-center gap-3">
                    {/* Active Accent Bar */}
                    {isActive && (
                      <div className="absolute left-0 top-2 bottom-2 w-1 bg-blue-500 rounded-r" />
                    )}
                    <Icon
                      className={`w-4 h-4 transition-transform group-hover:scale-110 ${
                        isActive ? 'text-blue-400' : 'text-slate-400'
                      }`}
                    />
                    <span>{item.label}</span>
                  </div>

                  {item.badge && (
                    <span className="px-1.5 py-0.5 text-[10px] font-bold rounded-full bg-blue-600 text-white">
                      {item.badge}
                    </span>
                  )}
                  {item.highlight && !isActive && (
                    <span className="w-2 h-2 rounded-full bg-emerald-400 animate-pulse" />
                  )}
                </button>
              );
            })}
          </nav>
        </div>

        {/* General Section */}
        <div>
          <span className="px-3 text-[11px] font-bold text-slate-400 dark:text-slate-500 tracking-wider uppercase block mb-2">
            GENERAL
          </span>
          <nav className="space-y-1">
            {generalItems.map((item) => {
              const Icon = item.icon;
              const isActive = activeModule === item.id;
              return (
                <button
                  key={item.id}
                  onClick={() => onSelectModule(item.id)}
                  className={`w-full flex items-center justify-between px-3 py-2.5 rounded-xl text-sm font-medium transition-all group ${
                    isActive
                      ? 'bg-blue-600/15 text-white font-semibold relative shadow-sm border border-blue-500/20'
                      : 'text-slate-400 hover:text-white hover:bg-slate-800/40'
                  }`}
                >
                  <div className="flex items-center gap-3">
                    {isActive && (
                      <div className="absolute left-0 top-2 bottom-2 w-1 bg-blue-500 rounded-r" />
                    )}
                    <Icon
                      className={`w-4 h-4 transition-transform group-hover:scale-110 ${
                        isActive ? 'text-blue-400' : 'text-slate-400'
                      }`}
                    />
                    <span>{item.label}</span>
                  </div>
                </button>
              );
            })}
          </nav>
        </div>
      </div>

      {/* Footer User Profile (Matching Reference Screenshot) */}
      <div className="p-4 border-t border-slate-800/60 bg-[#0c0d14] dark:bg-[#090a0f] flex items-center gap-3">
        <div className="w-10 h-10 rounded-full bg-gradient-to-tr from-blue-600 to-indigo-600 flex items-center justify-center text-white font-bold text-sm shadow-md">
          {user?.name?.[0] || 'J'}
        </div>
        <div className="flex-1 min-w-0">
          <p className="text-sm font-bold text-white truncate">{user?.name || 'Jenil Rana'}</p>
          <p className="text-[11px] text-slate-400 truncate">{user?.email || 'admin@simplebilling.com'}</p>
        </div>
        <span className="text-[10px] font-semibold uppercase px-2 py-0.5 rounded bg-blue-950 text-blue-300 border border-blue-800/40">
          {role}
        </span>
      </div>
    </aside>
  );
};
