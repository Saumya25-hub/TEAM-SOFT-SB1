import React, { useState, useEffect } from 'react';
import { Plus, Search, Package, AlertTriangle, ArrowUpDown, X, Check } from 'lucide-react';
import { api } from '../services/api';

interface ProductsModuleProps {
  currencySymbol: string;
}

export const ProductsModule: React.FC<ProductsModuleProps> = ({ currencySymbol }) => {
  const [products, setProducts] = useState<any[]>([]);
  const [search, setSearch] = useState('');
  const [showAddModal, setShowAddModal] = useState(false);
  const [showAdjustModal, setShowAdjustModal] = useState<any>(null);
  const [adjustDelta, setAdjustDelta] = useState(0);
  const [adjustReason, setAdjustReason] = useState('');

  // Add Product Form State
  const [newSku, setNewSku] = useState('');
  const [newBarcode, setNewBarcode] = useState('');
  const [newName, setNewName] = useState('');
  const [newUnit, setNewUnit] = useState('Piece');
  const [newPurchasePrice, setNewPurchasePrice] = useState(0);
  const [newSellingPrice, setNewSellingPrice] = useState(0);
  const [newStock, setNewStock] = useState(0);
  const [newMinAlert, setNewMinAlert] = useState(5);
  const [newTaxRate, setNewTaxRate] = useState(18);

  useEffect(() => {
    loadProducts();
  }, []);

  const loadProducts = async () => {
    try {
      const res = await api.getProducts();
      setProducts(res.products || []);
    } catch (err) {
      console.error('Failed to load products:', err);
    }
  };

  const handleCreateProduct = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      await api.createProduct({
        sku: newSku,
        barcode: newBarcode,
        name: newName,
        unit: newUnit,
        purchasePrice: newPurchasePrice,
        sellingPrice: newSellingPrice,
        stockQuantity: newStock,
        minStockAlert: newMinAlert,
        taxRatePercent: newTaxRate,
      });
      setShowAddModal(false);
      resetForm();
      loadProducts();
    } catch (err: any) {
      alert(err.message || 'Failed to create product.');
    }
  };

  const handleAdjustStock = async () => {
    if (!showAdjustModal || adjustDelta === 0) return;
    try {
      await api.adjustStock(showAdjustModal.id, adjustDelta, adjustReason || 'Manual Stock Adjustment');
      setShowAdjustModal(null);
      setAdjustDelta(0);
      setAdjustReason('');
      loadProducts();
    } catch (err: any) {
      alert(err.message || 'Failed to adjust stock.');
    }
  };

  const resetForm = () => {
    setNewSku('');
    setNewBarcode('');
    setNewName('');
    setNewUnit('Piece');
    setNewPurchasePrice(0);
    setNewSellingPrice(0);
    setNewStock(0);
    setNewMinAlert(5);
    setNewTaxRate(18);
  };

  const filtered = products.filter(
    (p) =>
      p.name.toLowerCase().includes(search.toLowerCase()) ||
      p.sku.toLowerCase().includes(search.toLowerCase()) ||
      (p.barcode && p.barcode.includes(search))
  );

  return (
    <div className="space-y-5">
      {/* Top Header & Actions */}
      <div className="flex items-center justify-between">
        <div className="relative w-80">
          <Search className="w-4 h-4 absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" />
          <input
            type="text"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            placeholder="Search products by SKU, name or barcode..."
            className="w-full pl-9 pr-3 py-2 text-xs rounded-xl bg-white dark:bg-[#161722] border border-slate-200 dark:border-[#232536] text-slate-800 dark:text-slate-200 placeholder-slate-400 focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>

        <button
          onClick={() => setShowAddModal(true)}
          className="flex items-center gap-1.5 px-4 py-2 rounded-xl bg-blue-600 hover:bg-blue-700 text-white text-xs font-bold shadow-md shadow-blue-500/25 transition-all"
        >
          <Plus className="w-4 h-4" />
          <span>Add New Product</span>
        </button>
      </div>

      {/* Products Table */}
      <div className="bg-white dark:bg-[#161722] rounded-2xl border border-slate-200 dark:border-[#232536] overflow-hidden shadow-sm">
        <table className="w-full text-left text-xs border-collapse">
          <thead>
            <tr className="border-b border-slate-200 dark:border-[#232536] bg-slate-50/70 dark:bg-[#1a1c2a] text-slate-500 dark:text-slate-400 font-bold uppercase">
              <th className="py-3 px-4">Product Details</th>
              <th className="py-3 px-4">Category</th>
              <th className="py-3 px-4 text-right">Cost Price</th>
              <th className="py-3 px-4 text-right">Selling Price</th>
              <th className="py-3 px-4 text-center">Stock Level</th>
              <th className="py-3 px-4 text-right">Tax Rate</th>
              <th className="py-3 px-4 text-right">Actions</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-100 dark:divide-[#232536]">
            {filtered.map((p) => (
              <tr key={p.id} className="hover:bg-slate-50 dark:hover:bg-slate-800/40 transition-colors">
                <td className="py-3 px-4">
                  <div className="font-bold text-slate-900 dark:text-white">{p.name}</div>
                  <div className="text-[10px] text-slate-400 flex items-center gap-2 mt-0.5">
                    <span>SKU: {p.sku}</span>
                    {p.barcode && <span>• Barcode: {p.barcode}</span>}
                  </div>
                </td>
                <td className="py-3 px-4 text-slate-600 dark:text-slate-300 font-semibold">{p.category}</td>
                <td className="py-3 px-4 text-right text-slate-500">
                  {currencySymbol}{Number(p.purchasePrice).toFixed(2)}
                </td>
                <td className="py-3 px-4 text-right font-extrabold text-slate-900 dark:text-white">
                  {currencySymbol}{Number(p.sellingPrice).toFixed(2)}
                </td>
                <td className="py-3 px-4 text-center">
                  <span
                    className={`inline-flex items-center gap-1 px-2.5 py-1 rounded-full text-[11px] font-bold ${
                      p.stockQuantity <= 0
                        ? 'bg-rose-500/10 text-rose-500'
                        : p.isLowStock
                        ? 'bg-amber-500/10 text-amber-500'
                        : 'bg-emerald-500/10 text-emerald-500'
                    }`}
                  >
                    {p.isLowStock && <AlertTriangle className="w-3 h-3" />}
                    {p.stockQuantity} {p.unit}
                  </span>
                </td>
                <td className="py-3 px-4 text-right text-slate-500">{p.taxRatePercent}%</td>
                <td className="py-3 px-4 text-right">
                  <button
                    onClick={() => setShowAdjustModal(p)}
                    className="px-2.5 py-1 rounded-lg bg-slate-100 dark:bg-slate-800 hover:bg-slate-200 dark:hover:bg-slate-700 text-slate-700 dark:text-slate-300 text-xs font-bold transition-colors"
                  >
                    Adjust Stock
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {/* ── MODAL: Add Product ── */}
      {showAddModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm p-4">
          <div className="bg-white dark:bg-[#161722] text-slate-800 dark:text-slate-200 rounded-2xl shadow-2xl max-w-lg w-full p-6 border border-slate-200 dark:border-[#232536]">
            <div className="flex justify-between items-center pb-3 border-b border-slate-200 dark:border-[#232536] mb-4">
              <h3 className="font-extrabold text-base text-slate-900 dark:text-white">Add New Product</h3>
              <button onClick={() => setShowAddModal(false)} className="text-slate-400 hover:text-slate-600">
                <X className="w-5 h-5" />
              </button>
            </div>

            <form onSubmit={handleCreateProduct} className="space-y-3 text-xs">
              <div>
                <label className="font-bold text-slate-600 dark:text-slate-300 block mb-1">Product Name *</label>
                <input
                  type="text"
                  required
                  value={newName}
                  onChange={(e) => setNewName(e.target.value)}
                  placeholder="e.g. Wireless Ergonomic Mouse"
                  className="w-full px-3 py-2 rounded-xl bg-slate-100 dark:bg-[#1a1c2a] border border-slate-200 dark:border-[#2b2e46]"
                />
              </div>

              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="font-bold text-slate-600 dark:text-slate-300 block mb-1">SKU Code *</label>
                  <input
                    type="text"
                    required
                    value={newSku}
                    onChange={(e) => setNewSku(e.target.value)}
                    placeholder="e.g. EL-MOU-001"
                    className="w-full px-3 py-2 rounded-xl bg-slate-100 dark:bg-[#1a1c2a] border border-slate-200 dark:border-[#2b2e46]"
                  />
                </div>
                <div>
                  <label className="font-bold text-slate-600 dark:text-slate-300 block mb-1">Barcode</label>
                  <input
                    type="text"
                    value={newBarcode}
                    onChange={(e) => setNewBarcode(e.target.value)}
                    placeholder="e.g. 890123456789"
                    className="w-full px-3 py-2 rounded-xl bg-slate-100 dark:bg-[#1a1c2a] border border-slate-200 dark:border-[#2b2e46]"
                  />
                </div>
              </div>

              <div className="grid grid-cols-3 gap-3">
                <div>
                  <label className="font-bold text-slate-600 dark:text-slate-300 block mb-1">Unit</label>
                  <input
                    type="text"
                    value={newUnit}
                    onChange={(e) => setNewUnit(e.target.value)}
                    placeholder="Piece / Kg / Box"
                    className="w-full px-3 py-2 rounded-xl bg-slate-100 dark:bg-[#1a1c2a] border border-slate-200 dark:border-[#2b2e46]"
                  />
                </div>
                <div>
                  <label className="font-bold text-slate-600 dark:text-slate-300 block mb-1">Cost Price ({currencySymbol})</label>
                  <input
                    type="number"
                    step="0.01"
                    value={newPurchasePrice}
                    onChange={(e) => setNewPurchasePrice(parseFloat(e.target.value) || 0)}
                    className="w-full px-3 py-2 rounded-xl bg-slate-100 dark:bg-[#1a1c2a] border border-slate-200 dark:border-[#2b2e46]"
                  />
                </div>
                <div>
                  <label className="font-bold text-slate-600 dark:text-slate-300 block mb-1">Selling Price ({currencySymbol})</label>
                  <input
                    type="number"
                    step="0.01"
                    value={newSellingPrice}
                    onChange={(e) => setNewSellingPrice(parseFloat(e.target.value) || 0)}
                    className="w-full px-3 py-2 rounded-xl bg-slate-100 dark:bg-[#1a1c2a] border border-slate-200 dark:border-[#2b2e46]"
                  />
                </div>
              </div>

              <div className="grid grid-cols-3 gap-3">
                <div>
                  <label className="font-bold text-slate-600 dark:text-slate-300 block mb-1">Initial Stock</label>
                  <input
                    type="number"
                    value={newStock}
                    onChange={(e) => setNewStock(parseInt(e.target.value, 10) || 0)}
                    className="w-full px-3 py-2 rounded-xl bg-slate-100 dark:bg-[#1a1c2a] border border-slate-200 dark:border-[#2b2e46]"
                  />
                </div>
                <div>
                  <label className="font-bold text-slate-600 dark:text-slate-300 block mb-1">Min Alert Level</label>
                  <input
                    type="number"
                    value={newMinAlert}
                    onChange={(e) => setNewMinAlert(parseInt(e.target.value, 10) || 5)}
                    className="w-full px-3 py-2 rounded-xl bg-slate-100 dark:bg-[#1a1c2a] border border-slate-200 dark:border-[#2b2e46]"
                  />
                </div>
                <div>
                  <label className="font-bold text-slate-600 dark:text-slate-300 block mb-1">Tax Rate (%)</label>
                  <input
                    type="number"
                    value={newTaxRate}
                    onChange={(e) => setNewTaxRate(parseFloat(e.target.value) || 0)}
                    className="w-full px-3 py-2 rounded-xl bg-slate-100 dark:bg-[#1a1c2a] border border-slate-200 dark:border-[#2b2e46]"
                  />
                </div>
              </div>

              <div className="pt-4 flex justify-end gap-2 border-t border-slate-200 dark:border-[#232536]">
                <button
                  type="button"
                  onClick={() => setShowAddModal(false)}
                  className="px-4 py-2 rounded-xl bg-slate-100 dark:bg-slate-800 font-bold"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  className="px-4 py-2 rounded-xl bg-blue-600 text-white font-bold shadow-md hover:bg-blue-700"
                >
                  Save Product
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* ── MODAL: Adjust Stock ── */}
      {showAdjustModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm p-4">
          <div className="bg-white dark:bg-[#161722] text-slate-800 dark:text-slate-200 rounded-2xl shadow-2xl max-w-sm w-full p-6 border border-slate-200 dark:border-[#232536]">
            <h3 className="font-extrabold text-base text-slate-900 dark:text-white mb-2">Adjust Stock</h3>
            <p className="text-xs text-slate-500 mb-4">{showAdjustModal.name} (Current: {showAdjustModal.stockQuantity})</p>

            <div className="space-y-3 text-xs">
              <div>
                <label className="font-bold block mb-1">Change Delta (+ to add, - to reduce)</label>
                <input
                  type="number"
                  value={adjustDelta}
                  onChange={(e) => setAdjustDelta(parseInt(e.target.value, 10) || 0)}
                  placeholder="e.g. +10 or -5"
                  className="w-full px-3 py-2 rounded-xl bg-slate-100 dark:bg-[#1a1c2a] border border-slate-200 dark:border-[#2b2e46] font-bold"
                />
              </div>
              <div>
                <label className="font-bold block mb-1">Reason</label>
                <input
                  type="text"
                  value={adjustReason}
                  onChange={(e) => setAdjustReason(e.target.value)}
                  placeholder="e.g. Physical inventory recount / Damaged"
                  className="w-full px-3 py-2 rounded-xl bg-slate-100 dark:bg-[#1a1c2a] border border-slate-200 dark:border-[#2b2e46]"
                />
              </div>
            </div>

            <div className="pt-4 mt-4 flex justify-end gap-2 border-t border-slate-200 dark:border-[#232536]">
              <button onClick={() => setShowAdjustModal(null)} className="px-3.5 py-1.5 rounded-xl bg-slate-100 dark:bg-slate-800 text-xs font-bold">
                Cancel
              </button>
              <button onClick={handleAdjustStock} className="px-3.5 py-1.5 rounded-xl bg-blue-600 text-white text-xs font-bold">
                Confirm Update
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
