import React, { useState, useEffect } from 'react';
import { CreditCard, Banknote, QrCode, Building, Plus, Receipt, X } from 'lucide-react';
import { api } from '../services/api';

interface TransactionsModuleProps {
  currencySymbol: string;
}

export const TransactionsModule: React.FC<TransactionsModuleProps> = ({ currencySymbol }) => {
  const [transactions, setTransactions] = useState<any[]>([]);
  const [filterMethod, setFilterMethod] = useState('');
  const [showModal, setShowModal] = useState(false);

  // Form State
  const [amount, setAmount] = useState(0);
  const [paymentMethod, setPaymentMethod] = useState('CASH');
  const [notes, setNotes] = useState('');

  useEffect(() => {
    loadTransactions();
  }, [filterMethod]);

  const loadTransactions = async () => {
    try {
      const res = await api.getTransactions(filterMethod || undefined);
      setTransactions(res.transactions || []);
    } catch (err) {
      console.error('Failed to load transactions:', err);
    }
  };

  const handleRecordPayment = async (e: React.FormEvent) => {
    e.preventDefault();
    if (amount <= 0) return;

    try {
      await api.recordTransaction({ amount, paymentMethod, notes });
      setShowModal(false);
      setAmount(0);
      setNotes('');
      loadTransactions();
    } catch (err: any) {
      alert(err.message || 'Failed to record payment.');
    }
  };

  return (
    <div className="space-y-5">
      {/* Top Filter Bar */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-2">
          {['', 'CASH', 'UPI', 'BANK_TRANSFER', 'CARD'].map((m) => (
            <button
              key={m}
              onClick={() => setFilterMethod(m)}
              className={`px-3 py-1.5 rounded-xl text-xs font-bold transition-all ${
                filterMethod === m
                  ? 'bg-blue-600 text-white shadow-md'
                  : 'bg-white dark:bg-[#161722] border border-slate-200 dark:border-[#232536] text-slate-600 dark:text-slate-300 hover:bg-slate-50'
              }`}
            >
              {m === '' ? 'All Methods' : m.replace('_', ' ')}
            </button>
          ))}
        </div>

        <button
          onClick={() => setShowModal(true)}
          className="flex items-center gap-1.5 px-4 py-2 rounded-xl bg-blue-600 hover:bg-blue-700 text-white text-xs font-bold shadow-md shadow-blue-500/25 transition-all"
        >
          <Plus className="w-4 h-4" />
          <span>Record Payment Receipt</span>
        </button>
      </div>

      {/* Ledger Table */}
      <div className="bg-white dark:bg-[#161722] rounded-2xl border border-slate-200 dark:border-[#232536] overflow-hidden shadow-sm">
        <table className="w-full text-left text-xs border-collapse">
          <thead>
            <tr className="border-b border-slate-200 dark:border-[#232536] bg-slate-50/70 dark:bg-[#1a1c2a] text-slate-500 dark:text-slate-400 font-bold uppercase">
              <th className="py-3 px-4">Receipt #</th>
              <th className="py-3 px-4">Type / Reference</th>
              <th className="py-3 px-4">Payment Method</th>
              <th className="py-3 px-4 text-right">Amount</th>
              <th className="py-3 px-4">Notes</th>
              <th className="py-3 px-4 text-right">Date & Time</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-100 dark:divide-[#232536]">
            {transactions.map((t) => (
              <tr key={t.id} className="hover:bg-slate-50 dark:hover:bg-slate-800/40 transition-colors">
                <td className="py-3 px-4 font-mono font-bold text-slate-900 dark:text-white">{t.receiptNumber}</td>
                <td className="py-3 px-4 text-slate-700 dark:text-slate-300">
                  {t.invoiceNumber && <span className="font-bold text-blue-600">{t.invoiceNumber} (Sale)</span>}
                  {t.purchaseNumber && <span className="font-bold text-violet-600">{t.purchaseNumber} (PO)</span>}
                  {!t.invoiceNumber && !t.purchaseNumber && <span>Direct Receipt</span>}
                </td>
                <td className="py-3 px-4">
                  <span className="inline-flex items-center gap-1.5 font-bold text-slate-700 dark:text-slate-200">
                    {t.paymentMethod === 'CASH' && <Banknote className="w-3.5 h-3.5 text-emerald-500" />}
                    {t.paymentMethod === 'UPI' && <QrCode className="w-3.5 h-3.5 text-blue-500" />}
                    {t.paymentMethod === 'BANK_TRANSFER' && <Building className="w-3.5 h-3.5 text-violet-500" />}
                    {t.paymentMethod === 'CARD' && <CreditCard className="w-3.5 h-3.5 text-amber-500" />}
                    {t.paymentMethod}
                  </span>
                </td>
                <td className="py-3 px-4 text-right font-extrabold text-slate-900 dark:text-white">
                  {currencySymbol}{Number(t.amount).toLocaleString()}
                </td>
                <td className="py-3 px-4 text-slate-400 truncate max-w-[200px]">{t.notes || '—'}</td>
                <td className="py-3 px-4 text-right text-slate-400">
                  {new Date(t.date).toLocaleString()}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {showModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm p-4">
          <div className="bg-white dark:bg-[#161722] text-slate-800 dark:text-slate-200 rounded-2xl shadow-2xl max-w-sm w-full p-6 border border-slate-200 dark:border-[#232536]">
            <div className="flex justify-between items-center pb-3 border-b border-slate-200 dark:border-[#232536] mb-4">
              <h3 className="font-extrabold text-base text-slate-900 dark:text-white">Record Receipt</h3>
              <button onClick={() => setShowModal(false)} className="text-slate-400 hover:text-slate-600">
                <X className="w-5 h-5" />
              </button>
            </div>

            <form onSubmit={handleRecordPayment} className="space-y-3 text-xs">
              <div>
                <label className="font-bold text-slate-600 dark:text-slate-300 block mb-1">Amount ({currencySymbol}) *</label>
                <input
                  type="number"
                  step="0.01"
                  required
                  value={amount || ''}
                  onChange={(e) => setAmount(parseFloat(e.target.value) || 0)}
                  placeholder="0.00"
                  className="w-full px-3 py-2 rounded-xl bg-slate-100 dark:bg-[#1a1c2a] border border-slate-200 dark:border-[#2b2e46] text-base font-extrabold"
                />
              </div>

              <div>
                <label className="font-bold text-slate-600 dark:text-slate-300 block mb-1">Payment Method</label>
                <select
                  value={paymentMethod}
                  onChange={(e) => setPaymentMethod(e.target.value)}
                  className="w-full px-3 py-2 rounded-xl bg-slate-100 dark:bg-[#1a1c2a] border border-slate-200 dark:border-[#2b2e46] font-bold"
                >
                  <option value="CASH">Cash</option>
                  <option value="UPI">UPI / GooglePay / PhonePe</option>
                  <option value="BANK_TRANSFER">Bank Transfer / NEFT</option>
                  <option value="CARD">Credit / Debit Card</option>
                </select>
              </div>

              <div>
                <label className="font-bold text-slate-600 dark:text-slate-300 block mb-1">Payment Note / Ref #</label>
                <input
                  type="text"
                  value={notes}
                  onChange={(e) => setNotes(e.target.value)}
                  placeholder="e.g. Counter deposit, UTR #48102"
                  className="w-full px-3 py-2 rounded-xl bg-slate-100 dark:bg-[#1a1c2a] border border-slate-200 dark:border-[#2b2e46]"
                />
              </div>

              <div className="pt-4 flex justify-end gap-2 border-t border-slate-200 dark:border-[#232536]">
                <button
                  type="button"
                  onClick={() => setShowModal(false)}
                  className="px-4 py-2 rounded-xl bg-slate-100 dark:bg-slate-800 font-bold"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  className="px-4 py-2 rounded-xl bg-blue-600 text-white font-bold shadow-md hover:bg-blue-700"
                >
                  Save Receipt
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};
