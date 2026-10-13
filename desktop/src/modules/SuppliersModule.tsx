import React, { useState, useEffect } from 'react';
import { Plus, Search, Truck, Phone, Mail, Building, X } from 'lucide-react';
import { api } from '../services/api';

interface SuppliersModuleProps {
  currencySymbol: string;
}

export const SuppliersModule: React.FC<SuppliersModuleProps> = ({ currencySymbol }) => {
  const [suppliers, setSuppliers] = useState<any[]>([]);
  const [search, setSearch] = useState('');
  const [showModal, setShowModal] = useState(false);

  // Form State
  const [name, setName] = useState('');
  const [phone, setPhone] = useState('');
  const [email, setEmail] = useState('');
  const [address, setAddress] = useState('');
  const [gstNumber, setGstNumber] = useState('');

  useEffect(() => {
    loadSuppliers();
  }, []);

  const loadSuppliers = async () => {
    try {
      const res = await api.getSuppliers();
      setSuppliers(res.suppliers || []);
    } catch (err) {
      console.error('Failed to load suppliers:', err);
    }
  };

  const handleAddSupplier = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      await api.createSupplier({ name, phone, email, address, gstNumber });
      setShowModal(false);
      setName('');
      setPhone('');
      setEmail('');
      setAddress('');
      setGstNumber('');
      loadSuppliers();
    } catch (err: any) {
      alert(err.message || 'Failed to add supplier.');
    }
  };

  const filtered = suppliers.filter(
    (s) =>
      s.name.toLowerCase().includes(search.toLowerCase()) ||
      (s.phone && s.phone.includes(search)) ||
      (s.gstNumber && s.gstNumber.toLowerCase().includes(search.toLowerCase()))
  );

  return (
    <div className="space-y-5">
      <div className="flex items-center justify-between">
        <div className="relative w-80">
          <Search className="w-4 h-4 absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" />
          <input
            type="text"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            placeholder="Search suppliers by name, GST or contact..."
            className="w-full pl-9 pr-3 py-2 text-xs rounded-xl bg-white dark:bg-[#161722] border border-slate-200 dark:border-[#232536] text-slate-800 dark:text-slate-200 placeholder-slate-400 focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>

        <button
          onClick={() => setShowModal(true)}
          className="flex items-center gap-1.5 px-4 py-2 rounded-xl bg-blue-600 hover:bg-blue-700 text-white text-xs font-bold shadow-md shadow-blue-500/25 transition-all"
        >
          <Plus className="w-4 h-4" />
          <span>Add New Supplier</span>
        </button>
      </div>

      <div className="bg-white dark:bg-[#161722] rounded-2xl border border-slate-200 dark:border-[#232536] overflow-hidden shadow-sm">
        <table className="w-full text-left text-xs border-collapse">
          <thead>
            <tr className="border-b border-slate-200 dark:border-[#232536] bg-slate-50/70 dark:bg-[#1a1c2a] text-slate-500 dark:text-slate-400 font-bold uppercase">
              <th className="py-3 px-4">Supplier Name</th>
              <th className="py-3 px-4">Contact Info</th>
              <th className="py-3 px-4">GST / Tax ID</th>
              <th className="py-3 px-4 text-center">Purchases</th>
              <th className="py-3 px-4 text-right">Total Procured</th>
              <th className="py-3 px-4 text-right">Payable Balance</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-100 dark:divide-[#232536]">
            {filtered.map((s) => (
              <tr key={s.id} className="hover:bg-slate-50 dark:hover:bg-slate-800/40 transition-colors">
                <td className="py-3 px-4 font-bold text-slate-900 dark:text-white flex items-center gap-2">
                  <div className="w-7 h-7 rounded-full bg-violet-500/10 text-violet-600 flex items-center justify-center font-extrabold text-xs">
                    <Truck className="w-3.5 h-3.5" />
                  </div>
                  <span>{s.name}</span>
                </td>
                <td className="py-3 px-4 text-slate-500 dark:text-slate-400">
                  {s.phone && <div className="text-slate-700 dark:text-slate-300 font-medium">{s.phone}</div>}
                  {s.email && <div className="text-[10px] text-slate-400">{s.email}</div>}
                </td>
                <td className="py-3 px-4 text-slate-600 dark:text-slate-300 font-semibold font-mono text-[11px]">
                  {s.gstNumber || '—'}
                </td>
                <td className="py-3 px-4 text-center font-bold text-slate-700 dark:text-slate-300">
                  {s.purchasesCount}
                </td>
                <td className="py-3 px-4 text-right font-extrabold text-slate-900 dark:text-white">
                  {currencySymbol}{Number(s.totalPurchased).toLocaleString()}
                </td>
                <td className="py-3 px-4 text-right">
                  <span
                    className={`font-bold ${
                      s.outstandingBalance > 0 ? 'text-rose-500' : 'text-slate-400'
                    }`}
                  >
                    {currencySymbol}{Number(s.outstandingBalance).toLocaleString()}
                  </span>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {showModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm p-4">
          <div className="bg-white dark:bg-[#161722] text-slate-800 dark:text-slate-200 rounded-2xl shadow-2xl max-w-md w-full p-6 border border-slate-200 dark:border-[#232536]">
            <div className="flex justify-between items-center pb-3 border-b border-slate-200 dark:border-[#232536] mb-4">
              <h3 className="font-extrabold text-base text-slate-900 dark:text-white">Add Supplier</h3>
              <button onClick={() => setShowModal(false)} className="text-slate-400 hover:text-slate-600">
                <X className="w-5 h-5" />
              </button>
            </div>

            <form onSubmit={handleAddSupplier} className="space-y-3 text-xs">
              <div>
                <label className="font-bold text-slate-600 dark:text-slate-300 block mb-1">Company / Supplier Name *</label>
                <input
                  type="text"
                  required
                  value={name}
                  onChange={(e) => setName(e.target.value)}
                  placeholder="e.g. Navneet Wholesale Distributors"
                  className="w-full px-3 py-2 rounded-xl bg-slate-100 dark:bg-[#1a1c2a] border border-slate-200 dark:border-[#2b2e46]"
                />
              </div>

              <div>
                <label className="font-bold text-slate-600 dark:text-slate-300 block mb-1">GST / Tax Number</label>
                <input
                  type="text"
                  value={gstNumber}
                  onChange={(e) => setGstNumber(e.target.value)}
                  placeholder="e.g. 24AAACN1234E1Z8"
                  className="w-full px-3 py-2 rounded-xl bg-slate-100 dark:bg-[#1a1c2a] border border-slate-200 dark:border-[#2b2e46]"
                />
              </div>

              <div className="grid grid-cols-2 gap-2">
                <div>
                  <label className="font-bold text-slate-600 dark:text-slate-300 block mb-1">Phone</label>
                  <input
                    type="text"
                    value={phone}
                    onChange={(e) => setPhone(e.target.value)}
                    placeholder="e.g. 022-28491000"
                    className="w-full px-3 py-2 rounded-xl bg-slate-100 dark:bg-[#1a1c2a] border border-slate-200 dark:border-[#2b2e46]"
                  />
                </div>
                <div>
                  <label className="font-bold text-slate-600 dark:text-slate-300 block mb-1">Email</label>
                  <input
                    type="email"
                    value={email}
                    onChange={(e) => setEmail(e.target.value)}
                    placeholder="orders@supplier.com"
                    className="w-full px-3 py-2 rounded-xl bg-slate-100 dark:bg-[#1a1c2a] border border-slate-200 dark:border-[#2b2e46]"
                  />
                </div>
              </div>

              <div>
                <label className="font-bold text-slate-600 dark:text-slate-300 block mb-1">Address</label>
                <textarea
                  value={address}
                  onChange={(e) => setAddress(e.target.value)}
                  placeholder="Warehouse / city address..."
                  className="w-full px-3 py-2 rounded-xl bg-slate-100 dark:bg-[#1a1c2a] border border-slate-200 dark:border-[#2b2e46] h-16"
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
                  Save Supplier
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};
