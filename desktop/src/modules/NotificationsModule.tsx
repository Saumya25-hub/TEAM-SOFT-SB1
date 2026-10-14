import React from 'react';
import { Bell, AlertTriangle, CheckCircle2, ShieldAlert } from 'lucide-react';

export const NotificationsModule: React.FC = () => {
  const notifications = [
    {
      id: '1',
      title: 'Low Stock Threshold Reached',
      message: 'Product "Desk Mouse Pad" stock has dropped below minimum threshold (8 remaining).',
      type: 'warning',
      time: '10 minutes ago',
    },
    {
      id: '2',
      title: 'Daily Data Integrity Check Passed',
      message: 'All multi-tenant database transactions and stock movements reconciled with 0 discrepancies.',
      type: 'success',
      time: '1 hour ago',
    },
    {
      id: '3',
      title: 'Security Session Active',
      message: 'Logged in as Jenil Rana with Owner permissions.',
      type: 'info',
      time: 'Today, 09:30 AM',
    },
  ];

  return (
    <div className="space-y-4 max-w-3xl">
      <div>
        <h2 className="text-base font-extrabold text-slate-900 dark:text-white">Messages & Alerts</h2>
        <p className="text-xs text-slate-500">System warnings, low-stock triggers, and operational notices</p>
      </div>

      <div className="space-y-3">
        {notifications.map((n) => (
          <div
            key={n.id}
            className="p-4 rounded-2xl bg-white dark:bg-[#161722] border border-slate-200 dark:border-[#232536] flex items-start gap-4 shadow-sm"
          >
            <div
              className={`p-2.5 rounded-xl ${
                n.type === 'warning'
                  ? 'bg-amber-500/10 text-amber-500'
                  : n.type === 'success'
                  ? 'bg-emerald-500/10 text-emerald-500'
                  : 'bg-blue-500/10 text-blue-500'
              }`}
            >
              {n.type === 'warning' && <AlertTriangle className="w-5 h-5" />}
              {n.type === 'success' && <CheckCircle2 className="w-5 h-5" />}
              {n.type === 'info' && <Bell className="w-5 h-5" />}
            </div>

            <div className="flex-1">
              <div className="flex items-center justify-between">
                <h4 className="text-xs font-bold text-slate-900 dark:text-white">{n.title}</h4>
                <span className="text-[10px] text-slate-400">{n.time}</span>
              </div>
              <p className="text-xs text-slate-600 dark:text-slate-300 mt-1">{n.message}</p>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
};
