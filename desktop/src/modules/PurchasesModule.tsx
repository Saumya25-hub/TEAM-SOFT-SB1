import React, { useState, useEffect } from 'react';
import { Plus, ShoppingCart, Calendar, CheckCircle2, X } from 'lucide-react';
import { api } from '../services/api';

interface PurchasesModuleProps {
  currencySymbol: string;
}

export const PurchasesModule: React.FC<PurchasesModuleProps> = ({ currencySymbol }) => {
  const [purchases, setPurchases] = useState<any[]>([]);
  const [suppliers, setSuppliers] = useState<any[]>([]);
  const [products, setProducts] = useState<any[]>([]);
  const [showModal, setShowModal] = useState(false);

  // Form State
  const [selectedSupplier, setSelectedSupplier] = useState('');
  const [selectedProduct, setSelectedProduct] = useState('');
  const [quantity, setQuantity] = useState(1);
  const [unitCost, setUnitCost] = useState(0);
  const [notes, setNotes] = useState('');

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    try {
      const [purRes, supRes, prodRes] = await Promise.all([
        api.getPurchases(),
        api.getSuppliers(),
        api.getProducts(),
      ]);
      setPurchases(purRes.purchases || []);
      setSuppliers(supRes.suppliers || []);
      setProducts(prodRes.products || []);
      if (prodRes.products?.[0]) {
        setSelectedProduct(prodRes.products[0].id);
        setUnitCost(prodRes.products[0].purchasePrice);
      }
    } catch (err) {
      console.error('Failed to load purchases data:', err);
    }
  };

  const handleProductChange = (prodId: string) => {
    setSelectedProduct(prodId);
    const prod = products.find((p) => p.id === prodId);
    if (prod) setUnitCost(prod.purchasePrice);
  };

  const handleCreatePurchase = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedProduct || quantity <= 0) return;

    try {
      await api.createPurchase({
        supplierId: selectedSupplier || null,
        items: [{ productId: selectedProduct, quantity, unitPrice: unitCost }],
        notes,
      });
      setShowModal(false);
      setQuantity(1);
      setNotes('');
      loadData();
    } catch (err: any) {
      alert(err.message || 'Failed to create purchase.');
    }
  };

  return (
    <div className="space-y-5">
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-sm font-extrabold text-slate-900 dark:text-white">Procurement & Purchase Invoices</h2>
          <p className="text-xs text-slate-500">Record incoming stock deliveries and supplier invoices</p>
        </div>

        <button
          onClick={() => setShowModal(true)}
          className="flex items-center gap-1.5 px-4 py-2 rounded-xl bg-blue-600 hover:bg-blue-700 text-white text-xs font-bold shadow-md shadow-blue-500/25 transition-all"
        >
          <Plus className="w-4 h-4" />
          <span>New Purchase Entry</span>
        </button>
      </div>

      <div className="bg-white dark:bg-[#161722] rounded-2xl border border-slate-200 dark:border-[#232536] overflow-hidden shadow-sm">
        <table className="w-full text-left text-xs border-collapse">
          <thead>
            <tr className="border-b border-slate-200 dark:border-[#232536] bg-slate-50/70 dark:bg-[#1a1c2a] text-slate-500 dark:text-slate-400 font-bold uppercase">
              <th className="py-3 px-4">PO Number</th>
              <th className="py-3 px-4">Supplier</th>
              <th className="py-3 px-4">Items Received</th>
              <th className="py-3 px-4 text-right">Total Cost</th>
              <th className="py-3 px-4 text-center">Status</th>
              <th className="py-3 px-4 text-right">Date</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-100 dark:divide-[#232536]">
            {purchases.map((p) => (
              <tr key={p.id} className="hover:bg-slate-50 dark:hover:bg-slate-800/40 transition-colors">
                <td className="py-3 px-4 font-mono font-bold text-blue-600 dark:text-blue-400">{p.purchaseNumber}</td>
                <td className="py-3 px-4 font-bold text-slate-800 dark:text-slate-200">
                  {p.supplier?.name || 'Local Direct Sourcing'}
                </td>
                <td className="py-3 px-4 text-slate-600 dark:text-slate-400">
                  {p.items?.map((item: any, idx: number) => (
                    <span key={idx} className="block text-[11px]">
                      {item.product?.name || 'Item'} ({item.quantity} pcs)
                    </span>
                  ))}
                </td>
                <td className="py-3 px-4 text-right font-extrabold text-slate-900 dark:text-white">
                  {currencySymbol}{Number(p.totalAmount).toLocaleString()}
                </td>
                <td className="py-3 px-4 text-center">
                  <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-[10px] font-bold bg-emerald-500/10 text-emerald-500">
                    <CheckCircle2 className="w-3 h-3" /> Received
                  </span>
                </td>
                <td className="py-3 px-4 text-right text-slate-400">
                  {new Date(p.createdAt).toLocaleDateString()}
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
              <h3 className="font-extrabold text-base text-slate-900 dark:text-white">New Purchase Entry</h3>
              <button onClick={() => setShowModal(false)} className="text-slate-400 hover:text-slate-600">
                <X className="w-5 h-5" />
              </button>
            </div>

            <form onSubmit={handleCreatePurchase} className="space-y-3 text-xs">
              <div>
                <label className="font-bold text-slate-600 dark:text-slate-300 block mb-1">Select Supplier</label>
                <select
                  value={selectedSupplier}
                  onChange={(e) => setSelectedSupplier(e.target.value)}
                  className="w-full px-3 py-2 rounded-xl bg-slate-100 dark:bg-[#1a1c2a] border border-slate-200 dark:border-[#2b2e46]"
                >
                  <option value="">-- Direct Local Vendor --</option>
                  {suppliers.map((s) => (
                    <option key={s.id} value={s.id}>
                      {s.name}
                    </option>
                  ))}
                </select>
              </div>

              <div>
                <label className="font-bold text-slate-600 dark:text-slate-300 block mb-1">Select Product *</label>
                <select
                  value={selectedProduct}
                  onChange={(e) => handleProductChange(e.target.value)}
                  className="w-full px-3 py-2 rounded-xl bg-slate-100 dark:bg-[#1a1c2a] border border-slate-200 dark:border-[#2b2e46]"
                >
                  {products.map((p) => (
                    <option key={p.id} value={p.id}>
                      {p.name} (Current Stock: {p.stockQuantity})
                    </option>
                  ))}
                </select>
              </div>

              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="font-bold text-slate-600 dark:text-slate-300 block mb-1">Quantity Received</label>
                  <input
                    type="number"
                    min="1"
                    required
                    value={quantity}
                    onChange={(e) => setQuantity(parseInt(e.target.value, 10) || 1)}
                    className="w-full px-3 py-2 rounded-xl bg-slate-100 dark:bg-[#1a1c2a] border border-slate-200 dark:border-[#2b2e46] font-bold"
                  />
                </div>
                <div>
                  <label className="font-bold text-slate-600 dark:text-slate-300 block mb-1">Cost Per Unit ({currencySymbol})</label>
                  <input
                    type="number"
                    step="0.01"
                    required
                    value={unitCost}
                    onChange={(e) => setUnitCost(parseFloat(e.target.value) || 0)}
                    className="w-full px-3 py-2 rounded-xl bg-slate-100 dark:bg-[#1a1c2a] border border-slate-200 dark:border-[#2b2e46] font-bold"
                  />
                </div>
              </div>

              <div>
                <label className="font-bold text-slate-600 dark:text-slate-300 block mb-1">Delivery / Notes</label>
                <input
                  type="text"
                  value={notes}
                  onChange={(e) => setNotes(e.target.value)}
                  placeholder="e.g. Sourced via batch #5012"
                  className="w-full px-3 py-2 rounded-xl bg-slate-100 dark:bg-[#1a1c2a] border border-slate-200 dark:border-[#2b2e46]"
                />
              </div>

              <div className="pt-2 text-right">
                <span className="text-xs text-slate-500">Total Purchase Value: </span>
                <span className="text-base font-extrabold text-blue-600">
                  {currencySymbol}{(quantity * unitCost).toFixed(2)}
                </span>
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
                  Record & Increment Stock
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};
