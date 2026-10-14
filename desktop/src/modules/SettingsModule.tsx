import React, { useState, useEffect } from 'react';
import { ShieldCheck, Building2, Save, History, CheckCircle2 } from 'lucide-react';
import { api } from '../services/api';

interface SettingsModuleProps {
  business: any;
  onBusinessUpdated: () => void;
}

export const SettingsModule: React.FC<SettingsModuleProps> = ({ business, onBusinessUpdated }) => {
  const [name, setName] = useState('');
  const [legalName, setLegalName] = useState('');
  const [taxNumber, setTaxNumber] = useState('');
  const [phone, setPhone] = useState('');
  const [email, setEmail] = useState('');
  const [address, setAddress] = useState('');
  const [currencySymbol, setCurrencySymbol] = useState('₹');

  const [auditLogs, setAuditLogs] = useState<any[]>([]);
  const [isSaving, setIsSaving] = useState(false);
  const [saveSuccess, setSaveSuccess] = useState(false);

  useEffect(() => {
    if (business) {
      setName(business.name || '');
      setLegalName(business.legalName || '');
      setTaxNumber(business.taxNumber || '');
      phone && setPhone(business.phone || '');
      email && setEmail(business.email || '');
      setAddress(business.address || '');
      setCurrencySymbol(business.currencySymbol || '₹');
    }
    loadAuditLogs();
  }, [business]);

  const loadAuditLogs = async () => {
    try {
      const res = await api.getAuditLogs();
      setAuditLogs(res.auditLogs || []);
    } catch (err) {
      console.error('Failed to load audit logs:', err);
    }
  };

  const handleSaveSettings = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      setIsSaving(true);
      await api.updateBusiness({
        name,
        legalName,
        taxNumber,
        phone,
        email,
        address,
        currencySymbol,
      });
      setSaveSuccess(true);
      onBusinessUpdated();
      setTimeout(() => setSaveSuccess(false), 3000);
    } catch (err: any) {
      alert(err.message || 'Failed to save settings.');
    } finally {
      setIsSaving(false);
    }
  };

  return (
    <div className="space-y-6 max-w-4xl">
      {/* Business Details Form */}
      <div className="p-6 rounded-2xl bg-white dark:bg-[#161722] border border-slate-200 dark:border-[#232536] shadow-sm">
        <div className="flex items-center justify-between pb-4 border-b border-slate-200 dark:border-[#232536] mb-5">
          <div className="flex items-center gap-2.5">
            <Building2 className="w-5 h-5 text-blue-600" />
            <div>
              <h3 className="text-base font-extrabold text-slate-900 dark:text-white">Business Profile & Tax Rules</h3>
              <p className="text-xs text-slate-500">Configure multi-tenant store details and receipt headers</p>
            </div>
          </div>
          {saveSuccess && (
            <span className="flex items-center gap-1.5 text-xs font-bold text-emerald-500 bg-emerald-500/10 px-3 py-1 rounded-xl">
              <CheckCircle2 className="w-3.5 h-3.5" /> Saved
            </span>
          )}
        </div>

        <form onSubmit={handleSaveSettings} className="space-y-4 text-xs">
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="font-bold text-slate-600 dark:text-slate-300 block mb-1">Trading Store Name</label>
              <input
                type="text"
                required
                value={name}
                onChange={(e) => setName(e.target.value)}
                className="w-full px-3 py-2 rounded-xl bg-slate-100 dark:bg-[#1a1c2a] border border-slate-200 dark:border-[#2b2e46] text-xs font-bold"
              />
            </div>
            <div>
              <label className="font-bold text-slate-600 dark:text-slate-300 block mb-1">Legal Registered Entity</label>
              <input
                type="text"
                value={legalName}
                onChange={(e) => setLegalName(e.target.value)}
                className="w-full px-3 py-2 rounded-xl bg-slate-100 dark:bg-[#1a1c2a] border border-slate-200 dark:border-[#2b2e46] text-xs"
              />
            </div>
          </div>

          <div className="grid grid-cols-3 gap-4">
            <div>
              <label className="font-bold text-slate-600 dark:text-slate-300 block mb-1">Tax / GST Number</label>
              <input
                type="text"
                value={taxNumber}
                onChange={(e) => setTaxNumber(e.target.value)}
                className="w-full px-3 py-2 rounded-xl bg-slate-100 dark:bg-[#1a1c2a] border border-slate-200 dark:border-[#2b2e46] text-xs font-mono"
              />
            </div>
            <div>
              <label className="font-bold text-slate-600 dark:text-slate-300 block mb-1">Phone Number</label>
              <input
                type="text"
                value={phone}
                onChange={(e) => setPhone(e.target.value)}
                className="w-full px-3 py-2 rounded-xl bg-slate-100 dark:bg-[#1a1c2a] border border-slate-200 dark:border-[#2b2e46] text-xs"
              />
            </div>
            <div>
              <label className="font-bold text-slate-600 dark:text-slate-300 block mb-1">Currency Symbol</label>
              <input
                type="text"
                value={currencySymbol}
                onChange={(e) => setCurrencySymbol(e.target.value)}
                className="w-full px-3 py-2 rounded-xl bg-slate-100 dark:bg-[#1a1c2a] border border-slate-200 dark:border-[#2b2e46] text-xs font-bold"
              />
            </div>
          </div>

          <div>
            <label className="font-bold text-slate-600 dark:text-slate-300 block mb-1">Store Address</label>
            <input
              type="text"
              value={address}
              onChange={(e) => setAddress(e.target.value)}
              className="w-full px-3 py-2 rounded-xl bg-slate-100 dark:bg-[#1a1c2a] border border-slate-200 dark:border-[#2b2e46] text-xs"
            />
          </div>

          <div className="pt-2 flex justify-end">
            <button
              type="submit"
              disabled={isSaving}
              className="flex items-center gap-1.5 px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-xl text-xs font-bold shadow-md transition-all disabled:opacity-50"
            >
              <Save className="w-3.5 h-3.5" />
              <span>{isSaving ? 'Saving Changes...' : 'Save Business Settings'}</span>
            </button>
          </div>
        </form>
      </div>

      {/* Audit Trail Section */}
      <div className="p-6 rounded-2xl bg-white dark:bg-[#161722] border border-slate-200 dark:border-[#232536] shadow-sm">
        <div className="flex items-center gap-2 mb-4">
          <History className="w-4 h-4 text-slate-400" />
          <h3 className="text-sm font-extrabold text-slate-900 dark:text-white">Security & Audit Logs</h3>
        </div>

        <div className="space-y-2 max-h-60 overflow-y-auto pr-1">
          {auditLogs.map((log) => (
            <div
              key={log.id}
              className="p-3 rounded-xl bg-slate-50 dark:bg-[#1a1c2a] border border-slate-100 dark:border-[#2b2e46] flex items-center justify-between text-xs"
            >
              <div>
                <span className="font-bold text-blue-600 dark:text-blue-400 mr-2">{log.action}</span>
                <span className="text-slate-600 dark:text-slate-300">{log.entity}</span>
                <span className="text-[10px] text-slate-400 block mt-0.5">By {log.userName}</span>
              </div>
              <span className="text-[10px] text-slate-400 font-mono">
                {new Date(log.timestamp).toLocaleString()}
              </span>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
};
