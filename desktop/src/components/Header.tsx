import React from 'react';
import { Search, Sun, Moon, Building2, Plus, Bell } from 'lucide-react';

interface HeaderProps {
  title: string;
  subtitle: string;
  isDarkMode: boolean;
  onToggleTheme: () => void;
  businessName: string;
  onQuickBillClick: () => void;
  searchQuery: string;
  onSearchChange: (q: string) => void;
}

export const Header: React.FC<HeaderProps> = ({
  title,
  subtitle,
  isDarkMode,
  onToggleTheme,
  businessName,
  onQuickBillClick,
  searchQuery,
  onSearchChange,
}) => {
  return (
    <header className="h-16 px-6 bg-white dark:bg-[#12131c] border-b border-slate-200 dark:border-[#232536] flex items-center justify-between flex-shrink-0 transition-colors">
      {/* Title & Subtitle */}
      <div>
        <h1 className="text-lg font-bold text-slate-900 dark:text-white leading-tight">{title}</h1>
        <p className="text-xs text-slate-500 dark:text-slate-400">{subtitle}</p>
      </div>

      {/* Right Controls */}
      <div className="flex items-center gap-3">
        {/* Global Search Bar */}
        <div className="relative w-64">
          <Search className="w-4 h-4 absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" />
          <input
            type="text"
            value={searchQuery}
            onChange={(e) => onSearchChange(e.target.value)}
            placeholder="Search items, bills, contacts..."
            className="w-full pl-9 pr-3 py-1.5 text-xs rounded-xl bg-slate-100 dark:bg-[#1a1c2a] border border-slate-200 dark:border-[#2b2e46] text-slate-800 dark:text-slate-200 placeholder-slate-400 focus:outline-none focus:ring-2 focus:ring-blue-500 transition-colors"
          />
        </div>

        {/* Animated Light/Dark Mode Capsule Toggle */}
        <button
          onClick={onToggleTheme}
          title={isDarkMode ? 'Switch to Light Mode' : 'Switch to Dark Mode'}
          className={`relative w-16 h-8 rounded-full p-1 transition-colors duration-150 flex items-center border ${
            isDarkMode
              ? 'bg-[#181a28] border-indigo-500/40'
              : 'bg-slate-200 border-slate-300'
          }`}
        >
          {/* Background Sun and Moon icons */}
          <div className="absolute inset-0 px-2 flex items-center justify-between pointer-events-none text-slate-400">
            <Sun className="w-3.5 h-3.5 text-amber-500" />
            <Moon className="w-3.5 h-3.5 text-indigo-400" />
          </div>

          {/* Sliding Knob (Fast, snappy 100ms transition) */}
          <div
            className={`w-6 h-6 rounded-full bg-white dark:bg-gradient-to-tr dark:from-blue-600 dark:to-indigo-500 shadow-md flex items-center justify-center transform transition-transform duration-150 ease-out z-10 ${
              isDarkMode ? 'translate-x-8' : 'translate-x-0'
            }`}
          >
            {isDarkMode ? (
              <Moon className="w-3 h-3 text-white" />
            ) : (
              <Sun className="w-3.5 h-3.5 text-amber-500" />
            )}
          </div>
        </button>

        {/* Business Selector Pill */}
        <div className="flex items-center gap-2 px-3 py-1.5 rounded-xl bg-slate-100 dark:bg-[#1a1c2a] border border-slate-200 dark:border-[#2b2e46] text-xs font-semibold text-slate-700 dark:text-slate-300">
          <Building2 className="w-3.5 h-3.5 text-blue-500" />
          <span className="truncate max-w-[140px]">{businessName}</span>
        </div>

        {/* Notification Bell */}
        <button className="relative p-2 rounded-xl text-slate-500 dark:text-slate-400 hover:bg-slate-100 dark:hover:bg-slate-800 transition-colors">
          <Bell className="w-4 h-4" />
          <span className="absolute top-1.5 right-1.5 w-2 h-2 rounded-full bg-rose-500 animate-ping" />
          <span className="absolute top-1.5 right-1.5 w-2 h-2 rounded-full bg-rose-500" />
        </button>

        {/* POS Quick Bill CTA */}
        <button
          onClick={onQuickBillClick}
          className="flex items-center gap-1.5 px-3.5 py-1.5 rounded-xl bg-blue-600 hover:bg-blue-700 text-white text-xs font-bold shadow-md shadow-blue-500/25 active:scale-95 transition-all"
        >
          <Plus className="w-3.5 h-3.5" />
          <span>New Bill</span>
        </button>
      </div>
    </header>
  );
};
