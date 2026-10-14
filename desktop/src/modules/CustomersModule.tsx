import React, { useState, useEffect } from 'react';
import { Plus, Search, User, Phone, Mail, MapPin, X } from 'lucide-react';
import { api } from '../services/api';

interface CustomersModuleProps {
  currencySymbol: string;
}

export const CustomersModule: React.FC<CustomersModuleProps> = ({ currencySymbol }) => {
  const [customers, setCustomers] = useState<any[]>([]);
  const [search, setSearch] = useState('');
  const [showModal, setShowModal] = useState(false);

  // Form State
  const [name, setName] = useState('');
  const [phone, setPhone] = useState('');
  const [email, setEmail] = useState('');
  const [address, setAddress] = useState('');

  useEffect(() => {
    loadCustomers();
  }, []);

  const loadCustomers = async () => {
    try {
      const res = await api.getCustomers();
      setCustomers(res.customers || []);
    } catch (err) {
      console.error('Failed to load customers:', err);
    }
  };

  const handleAddCustomer = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      await api.createCustomer({ name, phone, email, address });
      setShowModal(false);
      setName('');
      setPhone('');
      setEmail('');
      setAddress('');
      loadCustomers();
    } catch (err: any) {
      alert(err.message || 'Failed to add customer.');
    }
  };

  const filtered = customers.filter(
    (c) =>
      c.name.toLowerCase().includes(search.toLowerCase()) ||
      (c.phone && c.phone.includes(search)) ||
      (c.email && c.email.toLowerCase().includes(search.toLowerCase()))
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
            placeholder="Search customers by name, phone or email..."
            className="w-full pl-9 pr-3 py-2 text-xs rounded-xl bg-white dark:bg-[#161722] border border-slate-200 dark:border-[#232536] text-slate-800 dark:text-slate-200 placeholder-slate-400 focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>

        <button
          onClick={() => setShowModal(true)}
          className="flex items-center gap-1.5 px-4 py-2 rounded-xl bg-blue-600 hover:bg-blue-700 text-white text-xs font-bold shadow-md shadow-blue-500/25 transition-all"
        >
          <Plus className="w-4 h-4" />
          <span>Add New Customer</span>
        </button>
      </div>

      <div className="bg-white dark:bg-[#161722] rounded-2xl border border-slate-200 dark:border-[#232536] overflow-hidden shadow-sm">
        <table className="w-full text-left text-xs border-collapse">
          <thead>
            <tr className="border-b border-slate-200 dark:border-[#232536] bg-slate-50/70 dark:bg-[#1a1c2a] text-slate-500 dark:text-slate-400 font-bold uppercase">
              <th className="py-3 px-4">Customer Name</th>
              <th className="py-3 px-4">Contact Info</th>
              <th className="py-3 px-4">Address</th>
              <th className="py-3 px-4 text-center">Invoices</th>
              <th className="py-3 px-4 text-right">Total Spent</th>
              <th className="py-3 px-4 text-right">Outstanding</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-100 dark:divide-[#232536]">
            {filtered.map((c) => (
              <tr key={c.id} className="hover:bg-slate-50 dark:hover:bg-slate-800/40 transition-colors">
                <td className="py-3 px-4 font-bold text-slate-900 dark:text-white flex items-center gap-2">
                  <div className="w-7 h-7 rounded-full bg-blue-500/10 text-blue-600 flex items-center justify-center font-extrabold text-xs">
                    {c.name[0]}
                  </div>
                  <span>{c.name}</span>
                </td>
                <td className="py-3 px-4 text-slate-500 dark:text-slate-400">
                  {c.phone && <div className="text-slate-700 dark:text-slate-300 font-medium">{c.phone}</div>}
                  {c.email && <div className="text-[10px] text-slate-400">{c.email}</div>}
                </td>
                <td className="py-3 px-4 text-slate-500 dark:text-slate-400 truncate max-w-[200px]">
                  {c.address || '—'}
                </td>
                <td className="py-3 px-4 text-center font-bold text-slate-700 dark:text-slate-300">
                  {c.invoicesCount}
                </td>
                <td className="py-3 px-4 text-right font-extrabold text-slate-900 dark:text-white">
                  {currencySymbol}{Number(c.totalSpent).toLocaleString()}
                </td>
                <td className="py-3 px-4 text-right">
                  <span
                    className={`font-bold ${
                      c.outstandingBalance > 0 ? 'text-rose-500' : 'text-emerald-500'
                    }`}
                  >
                    {currencySymbol}{Number(c.outstandingBalance).toLocaleString()}
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
              <h3 className="font-extrabold text-base text-slate-900 dark:text-white">Add Customer</h3>
              <button onClick={() => setShowModal(false)} className="text-slate-400 hover:text-slate-600">
                <X className="w-5 h-5" />
              </button>
            </div>

            <form onSubmit={handleAddCustomer} className="space-y-3 text-xs">
              <div>
                <label className="font-bold text-slate-600 dark:text-slate-300 block mb-1">Customer Full Name *</label>
                <input
                  type="text"
                  required
                  value={name}
                  onChange={(e) => setName(e.target.value)}
                  placeholder="e.g. Ramesh Patel"
                  className="w-full px-3 py-2 rounded-xl bg-slate-100 dark:bg-[#1a1c2a] border border-slate-200 dark:border-[#2b2e46]"
                />
              </div>

              <div>
                <label className="font-bold text-slate-600 dark:text-slate-300 block mb-1">Phone Number</label>
                <input
                  type="text"
                  value={phone}
                  onChange={(e) => setPhone(e.target.value)}
                  placeholder="e.g. 9876543210"
                  className="w-full px-3 py-2 rounded-xl bg-slate-100 dark:bg-[#1a1c2a] border border-slate-200 dark:border-[#2b2e46]"
                />
              </div>

              <div>
                <label className="font-bold text-slate-600 dark:text-slate-300 block mb-1">Email Address</label>
                <input
                  type="email"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  placeholder="e.g. ramesh@example.com"
                  className="w-full px-3 py-2 rounded-xl bg-slate-100 dark:bg-[#1a1c2a] border border-slate-200 dark:border-[#2b2e46]"
                />
              </div>

              <div>
                <label className="font-bold text-slate-600 dark:text-slate-300 block mb-1">Address</label>
                <textarea
                  value={address}
                  onChange={(e) => setAddress(e.target.value)}
                  placeholder="Street address, city..."
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
                  Save Customer
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};
