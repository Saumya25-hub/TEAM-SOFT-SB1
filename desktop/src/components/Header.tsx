import React from 'react';
import { Sun, Moon, CalendarDays, Search } from 'lucide-react';

interface HeaderProps {
  title: string;
  subtitle?: string;
  isDarkMode: boolean;
  onToggleTheme: () => void;
  businessName?: string;
  onQuickBillClick?: () => void;
  searchQuery?: string;
  onSearchChange?: (q: string) => void;
  userName?: string;
}

export const Header: React.FC<HeaderProps> = ({
  title,
  isDarkMode,
  onToggleTheme,
  userName = 'Kristi Kamiykova',
}) => {
  return (
    <header className="h-20 px-8 bg-transparent flex items-center justify-between flex-shrink-0 select-none">
      {/* ── Left: Title + Date Range Pill ── */}
      <div className="flex items-center gap-6">
        <h1 className="text-2xl font-extrabold text-slate-900 dark:text-white tracking-tight">
          {title}
        </h1>

        {/* Date Filter Pill matching reference photo: "01.08.2022 - 31.08.2022 [calendar]" */}
        <div className="flex items-center gap-2 px-3.5 py-1.5 rounded-xl bg-white dark:bg-[#1e202e] border border-slate-200/80 dark:border-[#2b2e46] text-xs font-medium text-slate-700 dark:text-slate-300 shadow-sm cursor-pointer hover:border-slate-300 dark:hover:border-slate-600 transition-colors">
          <span>01.08.2022 - 31.08.2022</span>
          <CalendarDays className="w-3.5 h-3.5 text-slate-400" />
        </div>
      </div>

      {/* ── Right Controls: Theme Toggle + User Profile ── */}
      <div className="flex items-center gap-5">
        {/* Animated Light/Dark Mode Capsule Toggle */}
        <div className="flex items-center gap-2">
          <button
            onClick={onToggleTheme}
            type="button"
            title={isDarkMode ? 'Switch to Light Mode' : 'Switch to Dark Mode'}
            className={`relative w-14 h-7 rounded-full p-0.5 transition-colors duration-200 flex items-center border ${
              isDarkMode
                ? 'bg-[#1e202e] border-[#383c56]'
                : 'bg-slate-200/80 border-slate-300/80'
            }`}
          >
            {/* Background Sun and Moon icons */}
            <div className="absolute inset-0 px-1.5 flex items-center justify-between pointer-events-none text-slate-400">
              <Sun className={`w-3.5 h-3.5 transition-colors ${!isDarkMode ? 'text-amber-500' : 'text-slate-500'}`} />
              <Moon className={`w-3.5 h-3.5 transition-colors ${isDarkMode ? 'text-indigo-400' : 'text-slate-400'}`} />
            </div>

            {/* Sliding Knob */}
            <div
              className={`w-6 h-6 rounded-full shadow-md flex items-center justify-center transform transition-transform duration-200 ease-out z-10 ${
                isDarkMode
                  ? 'translate-x-7 bg-gradient-to-tr from-blue-600 to-indigo-600 text-white'
                  : 'translate-x-0 bg-white text-amber-500'
              }`}
            >
              {isDarkMode ? (
                <Moon className="w-3 h-3 text-white" />
              ) : (
                <Sun className="w-3 h-3 text-amber-500" />
              )}
            </div>
          </button>
        </div>

        {/* User Profile matching reference photo: Kristi Kamiykova */}
        <div className="flex items-center gap-2.5 cursor-pointer">
          <img
            src="https://images.unsplash.com/photo-1534528741775-53994a69daeb?w=80&auto=format&fit=crop&q=80"
            alt={userName}
            className="w-9 h-9 rounded-full object-cover ring-2 ring-white dark:ring-slate-800 shadow-sm"
          />
          <span className="text-xs font-bold text-slate-800 dark:text-slate-200">
            {userName}
          </span>
        </div>
      </div>
    </header>
  );
};
