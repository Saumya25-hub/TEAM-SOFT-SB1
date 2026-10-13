import React, { useState } from 'react';
import {
  TrendingUp,
  TrendingDown,
  Package,
  CheckSquare,
  DollarSign,
  CreditCard,
  Wallet,
  RotateCcw,
  ChevronDown,
} from 'lucide-react';

interface DashboardModuleProps {
  onNavigate?: (module: any) => void;
  currencySymbol?: string;
}

export const DashboardModule: React.FC<DashboardModuleProps> = () => {
  const [selectedYear, setSelectedYear] = useState('2021');

  // Customer orders matching reference photo
  const customerOrders = [
    {
      id: '1',
      name: 'Press',
      address: 'London',
      date: '22.08.2022',
      status: 'Delivered',
      price: '$920',
      avatar: 'https://images.unsplash.com/photo-1534528741775-53994a69daeb?w=80&auto=format&fit=crop&q=80',
      statusColor: 'bg-amber-100 text-amber-700 dark:bg-amber-950/60 dark:text-amber-400 border border-amber-300 dark:border-amber-800/50',
    },
    {
      id: '2',
      name: 'Marina',
      address: 'Man city',
      date: '24.08.2022',
      status: 'Processed',
      price: '$452',
      avatar: 'https://images.unsplash.com/photo-1517841905240-472988babdf9?w=80&auto=format&fit=crop&q=80',
      statusColor: 'bg-emerald-100 text-emerald-700 dark:bg-emerald-950/60 dark:text-emerald-400 border border-emerald-300 dark:border-emerald-800/50',
    },
    {
      id: '3',
      name: 'Alex',
      address: 'Unknown',
      date: '19.08.2022',
      status: 'Cancelled',
      price: '$1200',
      avatar: 'https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=80&auto=format&fit=crop&q=80',
      statusColor: 'bg-rose-100 text-rose-700 dark:bg-rose-950/60 dark:text-rose-400 border border-rose-300 dark:border-rose-800/50',
    },
    {
      id: '4',
      name: 'Robert',
      address: 'New York',
      date: '03.08.2022',
      status: 'Delivered',
      price: '$1235',
      avatar: 'https://images.unsplash.com/photo-1500648767791-00dcc994a43e?w=80&auto=format&fit=crop&q=80',
      statusColor: 'bg-amber-100 text-amber-700 dark:bg-amber-950/60 dark:text-amber-400 border border-amber-300 dark:border-amber-800/50',
    },
  ];

  // Sales dynamics 12-month data (% fill of bar)
  const salesMonths = [
    { label: 'JAN', fill: 20 },
    { label: 'FEB', fill: 55 },
    { label: 'MAR', fill: 35 },
    { label: 'APR', fill: 68 },
    { label: 'MAY', fill: 40 },
    { label: 'JUN', fill: 78 },
    { label: 'JUL', fill: 62 },
    { label: 'AUG', fill: 28 },
    { label: 'SEP', fill: 50 },
    { label: 'OCT', fill: 75 },
    { label: 'NOV', fill: 88 },
    { label: 'DEC', fill: 95 },
  ];

  return (
    <div className="space-y-5 pb-8 font-sans select-none">
      {/* ══════════════════════════════════════════════════════════════════
          ROW 1: TOP METRICS (4 Small Cards + 2 Donut Cards)
         ══════════════════════════════════════════════════════════════════ */}
      <div className="grid grid-cols-1 lg:grid-cols-12 gap-5">
        {/* Left 4 Mini Cards in a 2x2 Grid (5 Columns) */}
        <div className="lg:col-span-5 grid grid-cols-2 gap-4">
          {/* Orders */}
          <div className="bg-white dark:bg-[#1e202e] rounded-2xl p-4 border border-slate-100 dark:border-[#27293c] shadow-[0_2px_12px_rgba(0,0,0,0.03)] flex flex-col justify-between transition-colors">
            <div className="flex items-center justify-between">
              <span className="text-xs font-semibold text-slate-500 dark:text-slate-400">Orders</span>
              <Package className="w-4 h-4 text-slate-400" />
            </div>
            <div className="mt-2">
              <div className="text-2xl font-bold text-slate-900 dark:text-white">201</div>
              <div className="flex items-center gap-1 text-[11px] font-semibold text-emerald-500 mt-1">
                <TrendingUp className="w-3 h-3" />
                <span>0.2% since last month</span>
              </div>
            </div>
          </div>

          {/* Approved */}
          <div className="bg-white dark:bg-[#1e202e] rounded-2xl p-4 border border-slate-100 dark:border-[#27293c] shadow-[0_2px_12px_rgba(0,0,0,0.03)] flex flex-col justify-between transition-colors">
            <div className="flex items-center justify-between">
              <span className="text-xs font-semibold text-slate-500 dark:text-slate-400">Approved</span>
              <CheckSquare className="w-4 h-4 text-slate-400" />
            </div>
            <div className="mt-2">
              <div className="text-2xl font-bold text-slate-900 dark:text-white">36</div>
              <div className="flex items-center gap-1 text-[11px] font-semibold text-emerald-500 mt-1">
                <TrendingUp className="w-3 h-3" />
                <span>2.4% since last month</span>
              </div>
            </div>
          </div>

          {/* Month total */}
          <div className="bg-white dark:bg-[#1e202e] rounded-2xl p-4 border border-slate-100 dark:border-[#27293c] shadow-[0_2px_12px_rgba(0,0,0,0.03)] flex flex-col justify-between transition-colors">
            <div className="flex items-center justify-between">
              <span className="text-xs font-semibold text-slate-500 dark:text-slate-400">Month total</span>
              <DollarSign className="w-4 h-4 text-slate-400" />
            </div>
            <div className="mt-2">
              <div className="text-2xl font-bold text-slate-900 dark:text-white">25410</div>
              <div className="flex items-center gap-1 text-[11px] font-semibold text-rose-500 mt-1">
                <TrendingDown className="w-3 h-3" />
                <span>0.2% since last month</span>
              </div>
            </div>
          </div>

          {/* Revenue */}
          <div className="bg-white dark:bg-[#1e202e] rounded-2xl p-4 border border-slate-100 dark:border-[#27293c] shadow-[0_2px_12px_rgba(0,0,0,0.03)] flex flex-col justify-between transition-colors">
            <div className="flex items-center justify-between">
              <span className="text-xs font-semibold text-slate-500 dark:text-slate-400">Revenue</span>
              <CreditCard className="w-4 h-4 text-slate-400" />
            </div>
            <div className="mt-2">
              <div className="text-2xl font-bold text-slate-900 dark:text-white">1352</div>
              <div className="flex items-center gap-1 text-[11px] font-semibold text-rose-500 mt-1">
                <TrendingDown className="w-3 h-3" />
                <span>1.2% since last month</span>
              </div>
            </div>
          </div>
        </div>

        {/* Center Donut Card: Users (3.5 Columns) */}
        <div className="lg:col-span-3.5 bg-white dark:bg-[#1e202e] rounded-2xl p-5 border border-slate-100 dark:border-[#27293c] shadow-[0_2px_12px_rgba(0,0,0,0.03)] flex flex-col justify-between transition-colors">
          <div>
            <span className="text-xs font-semibold text-slate-500 dark:text-slate-400">Users</span>
            <div className="text-2xl font-bold text-slate-900 dark:text-white mt-1">4.890</div>
            <p className="text-[10px] text-slate-400">since last month</p>
          </div>

          <div className="flex items-center justify-between mt-3">
            {/* Legend */}
            <div className="space-y-1.5 text-[11px] font-medium text-slate-600 dark:text-slate-300">
              <div className="flex items-center gap-2">
                <span className="w-2 h-2 rounded-full bg-[#f59e0b]" />
                <span>65% New</span>
              </div>
              <div className="flex items-center gap-2">
                <span className="w-2 h-2 rounded-full bg-[#181926] dark:bg-[#2d3149]" />
                <span>23% Returning</span>
              </div>
              <div className="flex items-center gap-2">
                <span className="w-2 h-2 rounded-full bg-[#e2e8f0] dark:bg-[#4b5563]" />
                <span>12% Inactive</span>
              </div>
            </div>

            {/* Donut Chart SVG */}
            <div className="relative w-24 h-24 flex items-center justify-center flex-shrink-0">
              <svg className="w-full h-full transform -rotate-90" viewBox="0 0 36 36">
                {/* Background Ring */}
                <circle
                  cx="18"
                  cy="18"
                  r="14"
                  fill="none"
                  stroke="currentColor"
                  strokeWidth="4"
                  className="text-slate-100 dark:text-[#282a3c]"
                />
                {/* 65% Yellow Segment */}
                <circle
                  cx="18"
                  cy="18"
                  r="14"
                  fill="none"
                  stroke="#f59e0b"
                  strokeWidth="4"
                  strokeDasharray="57.18 87.96"
                  strokeDashoffset="0"
                  strokeLinecap="round"
                />
                {/* 23% Dark Segment */}
                <circle
                  cx="18"
                  cy="18"
                  r="14"
                  fill="none"
                  stroke="currentColor"
                  className="text-[#181926] dark:text-[#2d3149]"
                  strokeWidth="4"
                  strokeDasharray="20.23 87.96"
                  strokeDashoffset="-59"
                  strokeLinecap="round"
                />
                {/* 12% Light Segment */}
                <circle
                  cx="18"
                  cy="18"
                  r="14"
                  fill="none"
                  stroke="currentColor"
                  className="text-[#e2e8f0] dark:text-[#4b5563]"
                  strokeWidth="4"
                  strokeDasharray="10.55 87.96"
                  strokeDashoffset="-80"
                  strokeLinecap="round"
                />
              </svg>
            </div>
          </div>
        </div>

        {/* Right Donut Card: Subscriptions (3.5 Columns) */}
        <div className="lg:col-span-3.5 bg-white dark:bg-[#1e202e] rounded-2xl p-5 border border-slate-100 dark:border-[#27293c] shadow-[0_2px_12px_rgba(0,0,0,0.03)] flex flex-col justify-between transition-colors">
          <div>
            <span className="text-xs font-semibold text-slate-500 dark:text-slate-400">Subscriptions</span>
            <div className="text-2xl font-bold text-slate-900 dark:text-white mt-1">1.201</div>
            <p className="text-[10px] text-slate-400">since last month</p>
          </div>

          <div className="flex items-center justify-between mt-3">
            {/* Legend */}
            <div className="space-y-1.5 text-[11px] font-medium text-slate-600 dark:text-slate-300">
              <div className="flex items-center gap-2">
                <span className="w-2 h-2 rounded-full bg-[#2563eb]" />
                <span>70% Paid</span>
              </div>
              <div className="flex items-center gap-2">
                <span className="w-2 h-2 rounded-full bg-[#60a5fa]" />
                <span>30% Trial</span>
              </div>
            </div>

            {/* Donut Chart SVG */}
            <div className="relative w-24 h-24 flex items-center justify-center flex-shrink-0">
              <svg className="w-full h-full transform -rotate-90" viewBox="0 0 36 36">
                {/* Background Ring */}
                <circle
                  cx="18"
                  cy="18"
                  r="14"
                  fill="none"
                  stroke="currentColor"
                  strokeWidth="4"
                  className="text-slate-100 dark:text-[#282a3c]"
                />
                {/* 70% Blue Segment */}
                <circle
                  cx="18"
                  cy="18"
                  r="14"
                  fill="none"
                  stroke="#2563eb"
                  strokeWidth="4"
                  strokeDasharray="61.57 87.96"
                  strokeDashoffset="0"
                  strokeLinecap="round"
                />
                {/* 30% Light Blue Segment */}
                <circle
                  cx="18"
                  cy="18"
                  r="14"
                  fill="none"
                  stroke="#60a5fa"
                  strokeWidth="4"
                  strokeDasharray="26.39 87.96"
                  strokeDashoffset="-63"
                  strokeLinecap="round"
                />
              </svg>
            </div>
          </div>
        </div>
      </div>

      {/* ══════════════════════════════════════════════════════════════════
          ROW 2: SALES DYNAMICS BAR CHART + PAID INVOICES & FUNDS RECEIVED
         ══════════════════════════════════════════════════════════════════ */}
      <div className="grid grid-cols-1 lg:grid-cols-12 gap-5">
        {/* Left: Sales Dynamics Chart (7 Columns) */}
        <div className="lg:col-span-7 bg-white dark:bg-[#1e202e] rounded-2xl p-5 border border-slate-100 dark:border-[#27293c] shadow-[0_2px_12px_rgba(0,0,0,0.03)] transition-colors">
          <div className="flex items-center justify-between mb-6">
            <h3 className="text-sm font-bold text-slate-900 dark:text-white">Sales dynamics</h3>
            <button
              onClick={() => setSelectedYear(selectedYear === '2021' ? '2022' : '2021')}
              className="flex items-center gap-1.5 px-2.5 py-1 text-xs font-semibold text-slate-600 dark:text-slate-300 bg-slate-100 dark:bg-[#282a3c] rounded-lg hover:bg-slate-200 dark:hover:bg-[#34374e] transition-colors"
            >
              <span>{selectedYear}</span>
              <ChevronDown className="w-3.5 h-3.5" />
            </button>
          </div>

          {/* Bar Chart Container */}
          <div className="relative h-44 flex">
            {/* Y-Axis Labels */}
            <div className="flex flex-col justify-between text-[10px] text-slate-400 font-medium pr-3 select-none pb-5">
              <span>300k</span>
              <span>200k</span>
              <span>100k</span>
              <span>0</span>
            </div>

            {/* Horizontal Grid lines */}
            <div className="flex-1 relative flex flex-col justify-between pb-5">
              <div className="absolute inset-x-0 top-0 border-b border-dashed border-slate-100 dark:border-slate-800/60 pointer-events-none" />
              <div className="absolute inset-x-0 top-1/3 border-b border-dashed border-slate-100 dark:border-slate-800/60 pointer-events-none" />
              <div className="absolute inset-x-0 top-2/3 border-b border-dashed border-slate-100 dark:border-slate-800/60 pointer-events-none" />
              <div className="absolute inset-x-0 bottom-5 border-b border-slate-100 dark:border-slate-800 pointer-events-none" />

              {/* 12 Vertical Pill Bars */}
              <div className="flex items-end justify-between h-full pt-2 px-1 z-10">
                {salesMonths.map((m) => (
                  <div key={m.label} className="flex flex-col items-center gap-2 group flex-1">
                    {/* Pill Bar Column */}
                    <div className="relative w-2.5 h-28 bg-[#e0e7ff] dark:bg-[#25283c] rounded-full overflow-hidden flex items-end">
                      <div
                        style={{ height: `${m.fill}%` }}
                        className="w-full bg-[#2563eb] rounded-full transition-all duration-300 group-hover:bg-[#1d4ed8]"
                      />
                    </div>
                    {/* Month Label */}
                    <span className="text-[9px] font-semibold text-slate-400 group-hover:text-blue-500 transition-colors">
                      {m.label}
                    </span>
                  </div>
                ))}
              </div>
            </div>
          </div>
        </div>

        {/* Right: Paid Invoices & Funds Received (5 Columns) */}
        <div className="lg:col-span-5 grid grid-cols-1 sm:grid-cols-2 gap-4">
          {/* Paid Invoices Card */}
          <div className="bg-white dark:bg-[#1e202e] rounded-2xl p-5 border border-slate-100 dark:border-[#27293c] shadow-[0_2px_12px_rgba(0,0,0,0.03)] flex flex-col justify-between transition-colors">
            <div className="flex items-center justify-between">
              <div className="w-8 h-8 rounded-xl bg-slate-100 dark:bg-[#282a3c] flex items-center justify-center text-slate-700 dark:text-slate-300">
                <Wallet className="w-4 h-4" />
              </div>
              {/* +15% Circular Gauge */}
              <div className="relative w-8 h-8 flex items-center justify-center">
                <svg className="w-full h-full transform -rotate-90" viewBox="0 0 32 32">
                  <circle cx="16" cy="16" r="12" fill="none" stroke="currentColor" strokeWidth="2.5" className="text-slate-100 dark:text-[#2a2c3e]" />
                  <circle cx="16" cy="16" r="12" fill="none" stroke="#c084fc" strokeWidth="2.5" strokeDasharray="15 75.4" strokeLinecap="round" />
                </svg>
                <span className="absolute text-[8px] font-bold text-purple-500">+15%</span>
              </div>
            </div>

            <div className="mt-4">
              <span className="text-xs font-semibold text-slate-400">Paid Invoices</span>
              <div className="text-xl font-extrabold text-slate-900 dark:text-white mt-1">$30256.23</div>
              <p className="text-[10px] text-slate-400 mt-0.5">Current Financial Year</p>
            </div>
          </div>

          {/* Funds received Card */}
          <div className="bg-white dark:bg-[#1e202e] rounded-2xl p-5 border border-slate-100 dark:border-[#27293c] shadow-[0_2px_12px_rgba(0,0,0,0.03)] flex flex-col justify-between transition-colors">
            <div className="flex items-center justify-between">
              <div className="w-8 h-8 rounded-xl bg-slate-100 dark:bg-[#282a3c] flex items-center justify-center text-slate-700 dark:text-slate-300">
                <DollarSign className="w-4 h-4" />
              </div>
              {/* +59% Circular Gauge */}
              <div className="relative w-8 h-8 flex items-center justify-center">
                <svg className="w-full h-full transform -rotate-90" viewBox="0 0 32 32">
                  <circle cx="16" cy="16" r="12" fill="none" stroke="currentColor" strokeWidth="2.5" className="text-slate-100 dark:text-[#2a2c3e]" />
                  <circle cx="16" cy="16" r="12" fill="none" stroke="#34d399" strokeWidth="2.5" strokeDasharray="45 75.4" strokeLinecap="round" />
                </svg>
                <span className="absolute text-[8px] font-bold text-emerald-500">+59%</span>
              </div>
            </div>

            <div className="mt-4">
              <span className="text-xs font-semibold text-slate-400">Funds received</span>
              <div className="text-xl font-extrabold text-slate-900 dark:text-white mt-1">$150256.23</div>
              <p className="text-[10px] text-slate-400 mt-0.5">Current Financial Year</p>
            </div>
          </div>
        </div>
      </div>

      {/* ══════════════════════════════════════════════════════════════════
          ROW 3: OVERALL USER ACTIVITY SPLINE CHART + CUSTOMER ORDER TABLE
         ══════════════════════════════════════════════════════════════════ */}
      <div className="grid grid-cols-1 lg:grid-cols-12 gap-5">
        {/* Left: Overall User Activity (7 Columns) */}
        <div className="lg:col-span-7 bg-white dark:bg-[#1e202e] rounded-2xl p-5 border border-slate-100 dark:border-[#27293c] shadow-[0_2px_12px_rgba(0,0,0,0.03)] transition-colors">
          <div className="flex items-center justify-between mb-4">
            <h3 className="text-sm font-bold text-slate-900 dark:text-white">Overall User Activity</h3>
            <button
              onClick={() => setSelectedYear(selectedYear === '2021' ? '2022' : '2021')}
              className="flex items-center gap-1.5 px-2.5 py-1 text-xs font-semibold text-slate-600 dark:text-slate-300 bg-slate-100 dark:bg-[#282a3c] rounded-lg hover:bg-slate-200 dark:hover:bg-[#34374e] transition-colors"
            >
              <span>{selectedYear}</span>
              <ChevronDown className="w-3.5 h-3.5" />
            </button>
          </div>

          {/* Spline Wave Line Chart */}
          <div className="relative h-44 flex">
            {/* Y-Axis Labels */}
            <div className="flex flex-col justify-between text-[10px] text-slate-400 font-medium pr-3 select-none pb-2">
              <span>400k</span>
              <span>300k</span>
              <span>200k</span>
              <span>100k</span>
            </div>

            {/* Canvas / SVG Curve */}
            <div className="flex-1 relative pb-2">
              <div className="absolute inset-x-0 top-0 border-b border-dashed border-slate-100 dark:border-slate-800/60 pointer-events-none" />
              <div className="absolute inset-x-0 top-1/3 border-b border-dashed border-slate-100 dark:border-slate-800/60 pointer-events-none" />
              <div className="absolute inset-x-0 top-2/3 border-b border-dashed border-slate-100 dark:border-slate-800/60 pointer-events-none" />
              <div className="absolute inset-x-0 bottom-2 border-b border-slate-100 dark:border-slate-800 pointer-events-none" />

              <svg className="w-full h-full overflow-visible" viewBox="0 0 500 130" preserveAspectRatio="none">
                <defs>
                  <linearGradient id="purpleGradient" x1="0" y1="0" x2="0" y2="1">
                    <stop offset="0%" stopColor="#c084fc" stopOpacity="0.35" />
                    <stop offset="100%" stopColor="#c084fc" stopOpacity="0.0" />
                  </linearGradient>
                </defs>
                {/* Area under curve */}
                <path
                  d="M 0 115 Q 120 120 220 100 T 360 70 T 450 35 L 500 10 L 500 130 L 0 130 Z"
                  fill="url(#purpleGradient)"
                />
                {/* Spline stroke curve */}
                <path
                  d="M 0 115 Q 120 120 220 100 T 360 70 T 450 35 L 500 10"
                  fill="none"
                  stroke="#c084fc"
                  strokeWidth="3.5"
                  strokeLinecap="round"
                />
              </svg>
            </div>
          </div>
        </div>

        {/* Right: Customer Order Table (5 Columns) */}
        <div className="lg:col-span-5 bg-white dark:bg-[#1e202e] rounded-2xl p-5 border border-slate-100 dark:border-[#27293c] shadow-[0_2px_12px_rgba(0,0,0,0.03)] transition-colors flex flex-col justify-between">
          <div>
            <div className="flex items-center justify-between mb-3">
              <h3 className="text-sm font-bold text-slate-900 dark:text-white">Customer order</h3>
              <button
                title="Refresh orders"
                className="p-1 rounded-lg text-slate-400 hover:text-slate-600 dark:hover:text-slate-200 transition-colors"
              >
                <RotateCcw className="w-3.5 h-3.5" />
              </button>
            </div>

            {/* Table */}
            <div className="w-full overflow-x-auto">
              <table className="w-full text-left text-xs">
                <thead>
                  <tr className="text-[10px] text-slate-400 font-semibold border-b border-slate-100 dark:border-slate-800">
                    <th className="pb-2 font-normal">Profile</th>
                    <th className="pb-2 font-normal">Address</th>
                    <th className="pb-2 font-normal">Date</th>
                    <th className="pb-2 font-normal">Status</th>
                    <th className="pb-2 font-normal text-right">Price</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-100/60 dark:divide-slate-800/40">
                  {customerOrders.map((order) => (
                    <tr key={order.id} className="hover:bg-slate-50/60 dark:hover:bg-[#25283c]/50 transition-colors">
                      {/* Profile */}
                      <td className="py-2.5 flex items-center gap-2">
                        <img
                          src={order.avatar}
                          alt={order.name}
                          className="w-5 h-5 rounded-full object-cover"
                        />
                        <span className="font-semibold text-slate-800 dark:text-slate-200">{order.name}</span>
                      </td>
                      {/* Address */}
                      <td className="py-2.5 text-slate-500 dark:text-slate-400 text-[11px]">{order.address}</td>
                      {/* Date */}
                      <td className="py-2.5 text-slate-400 text-[10px]">{order.date}</td>
                      {/* Status */}
                      <td className="py-2.5">
                        <span className={`px-2 py-0.5 rounded-full text-[10px] font-bold ${order.statusColor}`}>
                          {order.status}
                        </span>
                      </td>
                      {/* Price */}
                      <td className="py-2.5 text-right font-bold text-slate-900 dark:text-white text-[11px]">
                        {order.price}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};
