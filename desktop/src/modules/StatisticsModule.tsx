import React, { useState, useEffect } from 'react';
import { Download, BarChart2, TrendingUp, Layers, PieChart, FileSpreadsheet } from 'lucide-react';
import { api } from '../services/api';

interface StatisticsModuleProps {
  currencySymbol: string;
}

export const StatisticsModule: React.FC<StatisticsModuleProps> = ({ currencySymbol }) => {
  const [salesReport, setSalesReport] = useState<any>(null);
  const [inventoryReport, setInventoryReport] = useState<any>(null);

  useEffect(() => {
    loadReports();
  }, []);

  const loadReports = async () => {
    try {
      const [sRes, iRes] = await Promise.all([api.getSalesReport(), api.getInventoryReport()]);
      setSalesReport(sRes.report);
      setInventoryReport(iRes.report);
    } catch (err) {
      console.error('Failed to load reports:', err);
    }
  };

  const handleExportCSV = () => {
    const csvContent =
      'data:text/csv;charset=utf-8,' +
      'Metric,Value\n' +
      `Total Revenue,${salesReport?.totalRevenue || 0}\n` +
      `Total Invoices,${salesReport?.totalInvoices || 0}\n` +
      `Tax Collected,${salesReport?.totalTaxCollected || 0}\n` +
      `Total Stock Valuation,${inventoryReport?.totalRetailSellingValuation || 0}\n` +
      `Projected Gross Margin,${inventoryReport?.projectedGrossMargin || 0}\n`;

    const encodedUri = encodeURI(csvContent);
    const link = document.createElement('a');
    link.setAttribute('href', encodedUri);
    link.setAttribute('download', `sales_report_${new Date().toISOString().slice(0, 10)}.csv`);
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-base font-extrabold text-slate-900 dark:text-white">Business Intelligence & Analytics</h2>
          <p className="text-xs text-slate-500">Live financial metrics and inventory valuation</p>
        </div>

        <button
          onClick={handleExportCSV}
          className="flex items-center gap-1.5 px-4 py-2 rounded-xl bg-slate-900 dark:bg-slate-100 dark:text-slate-900 text-white text-xs font-bold shadow-md transition-all hover:bg-slate-800"
        >
          <FileSpreadsheet className="w-4 h-4" />
          <span>Export CSV Report</span>
        </button>
      </div>

      {/* KPI Cards */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <div className="p-5 rounded-2xl bg-white dark:bg-[#161722] border border-slate-200 dark:border-[#232536] shadow-sm">
          <span className="text-xs font-bold text-slate-400 uppercase">Gross Revenue</span>
          <p className="text-2xl font-black text-slate-900 dark:text-white mt-1">
            {currencySymbol}{(salesReport?.totalRevenue || 0).toLocaleString()}
          </p>
          <span className="text-[10px] text-emerald-500 font-bold block mt-1">+28% growth</span>
        </div>

        <div className="p-5 rounded-2xl bg-white dark:bg-[#161722] border border-slate-200 dark:border-[#232536] shadow-sm">
          <span className="text-xs font-bold text-slate-400 uppercase">Tax Collected</span>
          <p className="text-2xl font-black text-blue-600 mt-1">
            {currencySymbol}{(salesReport?.totalTaxCollected || 0).toLocaleString()}
          </p>
          <span className="text-[10px] text-slate-400 block mt-1">Automated tax breakdown</span>
        </div>

        <div className="p-5 rounded-2xl bg-white dark:bg-[#161722] border border-slate-200 dark:border-[#232536] shadow-sm">
          <span className="text-xs font-bold text-slate-400 uppercase">Inventory Value</span>
          <p className="text-2xl font-black text-violet-600 mt-1">
            {currencySymbol}{(inventoryReport?.totalRetailSellingValuation || 0).toLocaleString()}
          </p>
          <span className="text-[10px] text-slate-400 block mt-1">
            {inventoryReport?.totalUnitsInStock || 0} units in warehouse
          </span>
        </div>

        <div className="p-5 rounded-2xl bg-white dark:bg-[#161722] border border-slate-200 dark:border-[#232536] shadow-sm">
          <span className="text-xs font-bold text-slate-400 uppercase">Projected Margin</span>
          <p className="text-2xl font-black text-emerald-600 mt-1">
            {currencySymbol}{(inventoryReport?.projectedGrossMargin || 0).toLocaleString()}
          </p>
          <span className="text-[10px] text-emerald-500 font-bold block mt-1">Retail margin</span>
        </div>
      </div>
    </div>
  );
};
