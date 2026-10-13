import React, { useEffect, useState } from 'react';
import {
  TrendingUp,
  TrendingDown,
  ArrowRight,
  AlertTriangle,
  Receipt,
  CheckCircle2,
  Clock,
  MoreVertical,
} from 'lucide-react';
import { api } from '../services/api';

interface DashboardModuleProps {
  onNavigate: (module: any) => void;
  currencySymbol: string;
}

export const DashboardModule: React.FC<DashboardModuleProps> = ({ onNavigate, currencySymbol }) => {
  const [data, setData] = useState<any>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadOverview();
  }, []);

  const loadOverview = async () => {
    try {
      setLoading(true);
      const res = await api.getDashboardOverview();
      setData(res);
    } catch (err) {
      console.error('Failed to load overview:', err);
    } finally {
      setLoading(false);
    }
  };

  const stats = data?.stats || {
    netIncome: 193000,
    totalSales: 245000,
    totalReturn: 32000,
    totalInvoicesCount: 6,
    lowStockCount: 1,
  };

  const chartMonths = data?.chartData?.months || ['Nov', 'Dec', 'Jan', 'Feb', 'Mar', 'Apr', 'May'];
  const chartIncome = data?.chartData?.income || [55, 80, 60, 95, 70, 115, 100];
  const chartExpense = data?.chartData?.expense || [30, 45, 35, 50, 40, 65, 55];

  return (
    <div className="space-y-6">
      {/* ── ROW 1: Metric KPI Cards (Matching Reference Screenshot) ── */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-5">
        {/* Hero Card */}
        <div
          onClick={() => onNavigate('statistics')}
          className="relative overflow-hidden rounded-2xl p-6 bg-gradient-to-br from-slate-900 via-[#182033] to-[#0f172a] text-white shadow-xl cursor-pointer group border border-slate-800"
        >
          <div className="relative z-10 flex flex-col justify-between h-32">
            <div>
              <span className="text-xs font-semibold uppercase tracking-wider text-blue-400">Monthly Performance</span>
              <h3 className="text-2xl font-extrabold mt-1">Quick Overview</h3>
              <p className="text-xs text-slate-400 mt-0.5">Real-time financial analytics</p>
            </div>
            <div className="flex items-center gap-1.5 text-xs font-bold text-blue-400 group-hover:translate-x-1.5 transition-transform">
              <span>See Statistics</span>
              <ArrowRight className="w-3.5 h-3.5" />
            </div>
          </div>
          {/* Ambient Glow */}
          <div className="absolute -right-8 -bottom-8 w-32 h-32 bg-blue-500/20 rounded-full blur-2xl pointer-events-none" />
        </div>

        {/* Net Income Card */}
        <div className="rounded-2xl p-6 bg-white dark:bg-[#161722] border border-slate-200 dark:border-[#232536] shadow-sm flex flex-col justify-between h-44 transition-colors">
          <div className="flex items-center justify-between">
            <span className="text-xs font-bold text-slate-500 dark:text-slate-400 uppercase tracking-wider">
              Net Income
            </span>
            <MoreVertical className="w-4 h-4 text-slate-400" />
          </div>
          <div>
            <div className="text-3xl font-extrabold text-slate-900 dark:text-white">
              {currencySymbol}{stats.netIncome.toLocaleString()}
            </div>
            <div className="flex items-center gap-1.5 mt-2 text-xs font-bold text-emerald-500">
              <TrendingUp className="w-3.5 h-3.5" />
              <span>+35% from last month</span>
            </div>
          </div>
        </div>

        {/* Total Return Card */}
        <div className="rounded-2xl p-6 bg-white dark:bg-[#161722] border border-slate-200 dark:border-[#232536] shadow-sm flex flex-col justify-between h-44 transition-colors">
          <div className="flex items-center justify-between">
            <span className="text-xs font-bold text-slate-500 dark:text-slate-400 uppercase tracking-wider">
              Total Return
            </span>
            <MoreVertical className="w-4 h-4 text-slate-400" />
          </div>
          <div>
            <div className="text-3xl font-extrabold text-slate-900 dark:text-white">
              {currencySymbol}{stats.totalReturn.toLocaleString()}
            </div>
            <div className="flex items-center gap-1.5 mt-2 text-xs font-bold text-rose-500">
              <TrendingDown className="w-3.5 h-3.5" />
              <span>-24% from last month</span>
            </div>
          </div>
        </div>
      </div>

      {/* ── ROW 2: Body Split (Matching Reference Screenshot) ── */}
      <div className="grid grid-cols-1 lg:grid-cols-12 gap-5">
        {/* Left Column (Transactions) */}
        <div className="lg:col-span-4 rounded-2xl p-5 bg-white dark:bg-[#161722] border border-slate-200 dark:border-[#232536] shadow-sm flex flex-col justify-between transition-colors">
          <div>
            <div className="flex items-center justify-between mb-4">
              <h4 className="text-sm font-bold text-slate-900 dark:text-white">Recent Transactions</h4>
              <button
                onClick={() => onNavigate('transactions')}
                className="text-xs font-bold text-blue-600 hover:text-blue-500"
              >
                View All
              </button>
            </div>

            <div className="space-y-3">
              {(data?.recentTransactions || [
                { id: '1', receiptNumber: 'RCP-261011', amount: 1250, paymentMethod: 'UPI', date: '11 Oct 2026' },
                { id: '2', receiptNumber: 'RCP-261010', amount: 4800, paymentMethod: 'CASH', date: '10 Oct 2026' },
                { id: '3', receiptNumber: 'RCP-261009', amount: 620, paymentMethod: 'UPI', date: '09 Oct 2026' },
                { id: '4', receiptNumber: 'RCP-261009', amount: 9100, paymentMethod: 'BANK', date: '09 Oct 2026' },
              ]).map((t: any) => (
                <div
                  key={t.id}
                  className="flex items-center justify-between p-3 rounded-xl bg-slate-50 dark:bg-[#1a1c2a] border border-slate-100 dark:border-[#2b2e46]"
                >
                  <div className="flex items-center gap-3">
                    <div className="w-9 h-9 rounded-xl bg-blue-500/10 text-blue-600 flex items-center justify-center font-bold">
                      <Receipt className="w-4 h-4" />
                    </div>
                    <div>
                      <p className="text-xs font-bold text-slate-800 dark:text-slate-200">{t.receiptNumber}</p>
                      <p className="text-[10px] text-slate-400">{t.paymentMethod} • {new Date(t.date).toLocaleDateString()}</p>
                    </div>
                  </div>
                  <div className="text-right">
                    <p className="text-xs font-extrabold text-slate-900 dark:text-white">
                      {currencySymbol}{Number(t.amount).toLocaleString()}
                    </p>
                    <span className="inline-flex items-center gap-1 text-[10px] font-bold text-emerald-500">
                      <CheckCircle2 className="w-2.5 h-2.5" /> Paid
                    </span>
                  </div>
                </div>
              ))}
            </div>
          </div>

          {/* Quick CTA */}
          <button
            onClick={() => onNavigate('billing')}
            className="w-full mt-4 py-2.5 rounded-xl bg-blue-600 hover:bg-blue-700 text-white text-xs font-bold shadow-md transition-all flex items-center justify-center gap-2"
          >
            <span>Create New Invoice</span>
            <ArrowRight className="w-3.5 h-3.5" />
          </button>
        </div>

        {/* Mid Column (Bar Chart & Sales Report) */}
        <div className="lg:col-span-5 space-y-5">
          {/* Revenue Bar Chart Card */}
          <div className="rounded-2xl p-5 bg-white dark:bg-[#161722] border border-slate-200 dark:border-[#232536] shadow-sm transition-colors">
            <div className="flex items-center justify-between mb-4">
              <div>
                <h4 className="text-sm font-bold text-slate-900 dark:text-white">Revenue & Expenses</h4>
                <p className="text-xs text-slate-400">Last 7 months activity</p>
              </div>
              <div className="flex items-center gap-3 text-xs">
                <span className="flex items-center gap-1.5 font-bold text-slate-600 dark:text-slate-300">
                  <span className="w-2.5 h-2.5 rounded-full bg-blue-500" /> Income
                </span>
                <span className="flex items-center gap-1.5 font-bold text-slate-400">
                  <span className="w-2.5 h-2.5 rounded-full bg-slate-300 dark:bg-slate-700" /> Expense
                </span>
              </div>
            </div>

            {/* Custom Interactive HTML Bar Chart */}
            <div className="h-44 flex items-end justify-between gap-3 pt-6 px-2">
              {chartMonths.map((m: string, idx: number) => {
                const inc = chartIncome[idx] || 50;
                const exp = chartExpense[idx] || 30;
                const max = 120;
                const incHeight = (inc / max) * 100;
                const expHeight = (exp / max) * 100;

                return (
                  <div key={m} className="flex-1 flex flex-col items-center gap-2 group">
                    <div className="w-full flex items-end justify-center gap-1 h-32">
                      {/* Income Bar */}
                      <div
                        style={{ height: `${incHeight}%` }}
                        className="w-3.5 rounded-t-md bg-blue-600 group-hover:bg-blue-500 transition-all shadow-sm"
                        title={`Income: ${currencySymbol}${inc}k`}
                      />
                      {/* Expense Bar */}
                      <div
                        style={{ height: `${expHeight}%` }}
                        className="w-3.5 rounded-t-md bg-slate-200 dark:bg-[#282a3c] group-hover:bg-slate-300 dark:group-hover:bg-[#34374e] transition-all"
                        title={`Expense: ${currencySymbol}${exp}k`}
                      />
                    </div>
                    <span className="text-[10px] font-bold text-slate-400">{m}</span>
                  </div>
                );
              })}
            </div>
          </div>

          {/* Sales Report Quick Card */}
          <div className="rounded-2xl p-4 bg-white dark:bg-[#161722] border border-slate-200 dark:border-[#232536] shadow-sm flex items-center justify-between transition-colors">
            <div>
              <p className="text-xs font-bold text-slate-400 uppercase">Total Sales Generated</p>
              <p className="text-xl font-extrabold text-slate-900 dark:text-white mt-0.5">
                {currencySymbol}{stats.totalSales.toLocaleString()}
              </p>
            </div>
            <button
              onClick={() => onNavigate('statistics')}
              className="px-3.5 py-1.5 rounded-xl bg-slate-100 dark:bg-slate-800 hover:bg-slate-200 dark:hover:bg-slate-700 text-xs font-bold text-slate-700 dark:text-slate-200 transition-colors"
            >
              Export Report
            </button>
          </div>
        </div>

        {/* Right Column (Donut & Low Stock) */}
        <div className="lg:col-span-3 space-y-5">
          {/* Donut Card */}
          <div className="rounded-2xl p-5 bg-white dark:bg-[#161722] border border-slate-200 dark:border-[#232536] shadow-sm text-center transition-colors">
            <h4 className="text-sm font-bold text-slate-900 dark:text-white mb-1">Category Share</h4>
            <p className="text-xs text-slate-400 mb-4">Stock distribution</p>

            {/* Circular representation */}
            <div className="relative w-32 h-32 mx-auto flex items-center justify-center">
              <svg className="w-full h-full transform -rotate-90" viewBox="0 0 36 36">
                <path
                  className="text-slate-100 dark:text-slate-800"
                  strokeWidth="4"
                  stroke="currentColor"
                  fill="none"
                  d="M18 2.0845 a 15.9155 15.9155 0 0 1 0 31.831 a 15.9155 15.9155 0 0 1 0 -31.831"
                />
                <path
                  className="text-blue-600"
                  strokeDasharray="50, 100"
                  strokeWidth="4"
                  strokeLinecap="round"
                  stroke="currentColor"
                  fill="none"
                  d="M18 2.0845 a 15.9155 15.9155 0 0 1 0 31.831 a 15.9155 15.9155 0 0 1 0 -31.831"
                />
                <path
                  className="text-violet-500"
                  strokeDasharray="30, 100"
                  strokeDashoffset="-50"
                  strokeWidth="4"
                  strokeLinecap="round"
                  stroke="currentColor"
                  fill="none"
                  d="M18 2.0845 a 15.9155 15.9155 0 0 1 0 31.831 a 15.9155 15.9155 0 0 1 0 -31.831"
                />
              </svg>
              <div className="absolute flex flex-col items-center">
                <span className="text-lg font-black text-slate-900 dark:text-white">100%</span>
                <span className="text-[9px] uppercase font-bold text-slate-400">Total View</span>
              </div>
            </div>

            <div className="grid grid-cols-2 gap-2 mt-4 text-[11px] font-semibold text-slate-600 dark:text-slate-400">
              <div className="flex items-center gap-1.5 justify-center">
                <span className="w-2 h-2 rounded-full bg-blue-600" /> Stationery
              </div>
              <div className="flex items-center gap-1.5 justify-center">
                <span className="w-2 h-2 rounded-full bg-violet-500" /> Electronics
              </div>
            </div>
          </div>

          {/* Low Stock Alert Box */}
          <div className="rounded-2xl p-4 bg-amber-500/10 border border-amber-500/30 text-amber-600 dark:text-amber-400">
            <div className="flex items-center gap-2 font-bold text-xs">
              <AlertTriangle className="w-4 h-4 text-amber-500 flex-shrink-0" />
              <span>Stock Warnings ({data?.lowStockAlerts?.length || 1})</span>
            </div>
            <p className="text-[11px] text-slate-600 dark:text-slate-300 mt-1">
              {data?.lowStockAlerts?.[0]?.name || 'Desk Mouse Pad'} has reached minimum threshold!
            </p>
            <button
              onClick={() => onNavigate('products')}
              className="mt-2.5 text-xs font-bold text-blue-600 hover:underline"
            >
              Reorder stock &rarr;
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};
