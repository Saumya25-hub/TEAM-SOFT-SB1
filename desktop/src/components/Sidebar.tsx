import React from 'react';
import {
  LayoutGrid,
  ShoppingBag,
  Mail,
  Users,
  Settings,
  LogOut,
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
  user?: { name: string; email: string } | null;
  role?: string;
}

export const Sidebar: React.FC<SidebarProps> = ({
  activeModule,
  onSelectModule,
}) => {
  const mainNavItems = [
    { id: 'overview' as NavModuleId, label: 'Analytics', icon: LayoutGrid },
    { id: 'products' as NavModuleId, label: 'Products', icon: ShoppingBag },
    { id: 'notifications' as NavModuleId, label: 'Messages', icon: Mail },
    { id: 'customers' as NavModuleId, label: 'Customers', icon: Users },
  ];

  const bottomNavItems = [
    { id: 'settings' as NavModuleId, label: 'Settings', icon: Settings },
    { id: 'logout' as any, label: 'Sign Out', icon: LogOut },
  ];

  return (
    <aside className="w-60 bg-white dark:bg-[#181926] border-r border-slate-100 dark:border-[#232537] flex flex-col flex-shrink-0 h-full select-none justify-between transition-colors p-5">
      <div>
        {/* ── Brand Logo matching reference photo: "Business" + colorful cross ── */}
        <div className="flex items-center gap-2.5 px-2 mb-8 mt-1">
          {/* 4-Petal Geometric Flower Icon */}
          <div className="w-7 h-7 relative flex items-center justify-center">
            <div className="absolute top-0 w-2.5 h-3.5 bg-gradient-to-b from-fuchsia-500 to-purple-600 rounded-full transform -translate-y-0.5" />
            <div className="absolute right-0 w-3.5 h-2.5 bg-gradient-to-r from-cyan-400 to-blue-500 rounded-full transform translate-x-0.5" />
            <div className="absolute bottom-0 w-2.5 h-3.5 bg-gradient-to-t from-orange-400 to-rose-500 rounded-full transform translate-y-0.5" />
            <div className="absolute left-0 w-3.5 h-2.5 bg-gradient-to-l from-indigo-500 to-violet-500 rounded-full transform -translate-x-0.5" />
          </div>
          <span className="text-lg font-bold text-slate-900 dark:text-white tracking-tight">
            Business
          </span>
        </div>

        {/* ── Primary Navigation Links ── */}
        <nav className="space-y-1.5">
          {mainNavItems.map((item) => {
            const Icon = item.icon;
            const isActive = activeModule === item.id;

            return (
              <button
                key={item.id}
                onClick={() => onSelectModule(item.id)}
                className={`w-full flex items-center gap-3.5 px-3.5 py-2.5 rounded-xl text-xs font-semibold transition-all ${
                  isActive
                    ? 'bg-slate-50 dark:bg-[#25273a] text-slate-900 dark:text-white shadow-sm border border-slate-200/60 dark:border-[#33374e]'
                    : 'text-slate-400 dark:text-slate-400 hover:text-slate-700 dark:hover:text-slate-200 hover:bg-slate-50/50 dark:hover:bg-[#1f2130]'
                }`}
              >
                <Icon
                  className={`w-4 h-4 transition-colors ${
                    isActive ? 'text-blue-600 dark:text-blue-400' : 'text-slate-400'
                  }`}
                />
                <span>{item.label}</span>
              </button>
            );
          })}
        </nav>

        {/* ── Lower Navigation Links ── */}
        <div className="mt-8 pt-4 space-y-1.5 border-t border-slate-100 dark:border-slate-800/60">
          {bottomNavItems.map((item) => {
            const Icon = item.icon;
            const isActive = activeModule === item.id;

            return (
              <button
                key={item.label}
                onClick={() => item.id !== 'logout' && onSelectModule(item.id)}
                className={`w-full flex items-center gap-3.5 px-3.5 py-2.5 rounded-xl text-xs font-semibold transition-all ${
                  isActive
                    ? 'bg-slate-50 dark:bg-[#25273a] text-slate-900 dark:text-white shadow-sm'
                    : 'text-slate-400 dark:text-slate-400 hover:text-slate-700 dark:hover:text-slate-200 hover:bg-slate-50/50 dark:hover:bg-[#1f2130]'
                }`}
              >
                <Icon className="w-4 h-4 text-slate-400" />
                <span>{item.label}</span>
              </button>
            );
          })}
        </div>
      </div>

      {/* ── Support Card Banner matching reference photo ── */}
      <div className="relative rounded-2xl p-4 bg-[#edf4fe] dark:bg-[#1c2234] border border-blue-100/80 dark:border-[#273048] overflow-hidden transition-colors mt-6">
        <div className="flex items-center gap-3">
          {/* Character Illustration SVG */}
          <div className="w-10 h-14 flex-shrink-0 flex items-center justify-center">
            <svg viewBox="0 0 48 64" fill="none" className="w-full h-full drop-shadow">
              {/* Head */}
              <circle cx="24" cy="14" r="8" fill="#FBBF24" />
              {/* Hair */}
              <path d="M16 13 C16 7, 32 7, 32 13 C32 10, 28 8, 24 8 C20 8, 16 10, 16 13 Z" fill="#92400E" />
              {/* Glasses */}
              <rect x="18" y="12" width="5" height="4" rx="1" fill="#374151" />
              <rect x="25" y="12" width="5" height="4" rx="1" fill="#374151" />
              <line x1="23" y1="14" x2="25" y2="14" stroke="#374151" strokeWidth="1.5" />
              {/* Torso / Shirt */}
              <path d="M14 24 C14 22, 34 22, 34 24 L32 44 L16 44 Z" fill="#E2E8F0" />
              {/* Tie */}
              <polygon points="23,24 25,24 26,34 24,36 22,34" fill="#3B82F6" />
              {/* Pants */}
              <path d="M16 44 L32 44 L31 62 L26 62 L25 50 L23 50 L22 62 L17 62 Z" fill="#B45309" />
              {/* Folder/Tablet */}
              <rect x="20" y="28" width="10" height="13" rx="1" fill="#3B82F6" transform="rotate(-10 20 28)" />
            </svg>
          </div>

          {/* Texts & Button */}
          <div className="flex-1 min-w-0">
            <h4 className="text-xs font-bold text-slate-800 dark:text-white leading-tight">
              Need help
            </h4>
            <p className="text-[10px] text-slate-500 dark:text-slate-400 mt-0.5 leading-snug">
              feel free to contact
            </p>
            <button
              onClick={() => onSelectModule('notifications')}
              className="mt-2.5 px-3 py-1 rounded-full bg-[#2563eb] hover:bg-[#1d4ed8] text-white text-[10px] font-bold shadow-sm shadow-blue-500/20 active:scale-95 transition-all"
            >
              Get support
            </button>
          </div>
        </div>
      </div>
    </aside>
  );
};
