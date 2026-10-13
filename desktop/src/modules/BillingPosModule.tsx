import React, { useState, useEffect } from 'react';
import {
  Search,
  Plus,
  Trash2,
  Receipt,
  User,
  Phone,
  Barcode,
  CheckCircle2,
  CreditCard,
  QrCode,
  Banknote,
  Building,
} from 'lucide-react';
import { api } from '../services/api';
import { PrintInvoiceModal } from '../components/PrintInvoiceModal';

interface BillingPosModuleProps {
  currencySymbol: string;
  business: any;
}

export const BillingPosModule: React.FC<BillingPosModuleProps> = ({ currencySymbol, business }) => {
  const [products, setProducts] = useState<any[]>([]);
  const [searchQuery, setSearchQuery] = useState('');
  const [cart, setCart] = useState<any[]>([]);
  const [customerName, setCustomerName] = useState('');
  const [customerPhone, setCustomerPhone] = useState('');
  const [discountAmount, setDiscountAmount] = useState(0);
  const [paymentMethod, setPaymentMethod] = useState<'CASH' | 'UPI' | 'BANK_TRANSFER' | 'CARD'>('CASH');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [generatedInvoice, setGeneratedInvoice] = useState<any>(null);
  const [errorMessage, setErrorMessage] = useState('');

  useEffect(() => {
    loadProducts();
  }, []);

  const loadProducts = async () => {
    try {
      const res = await api.getProducts();
      setProducts(res.products || []);
    } catch (err: any) {
      console.error('Failed to load products:', err);
    }
  };

  const addToCart = (product: any) => {
    const existing = cart.find((item) => item.productId === product.id);
    if (existing) {
      if (existing.quantity >= product.stockQuantity) {
        setErrorMessage(`Cannot exceed available stock of ${product.stockQuantity} for ${product.name}`);
        return;
      }
      setCart(
        cart.map((item) =>
          item.productId === product.id ? { ...item, quantity: item.quantity + 1 } : item
        )
      );
    } else {
      if (product.stockQuantity <= 0) {
        setErrorMessage(`"${product.name}" is out of stock!`);
        return;
      }
      setCart([
        ...cart,
        {
          productId: product.id,
          name: product.name,
          unitPrice: product.sellingPrice,
          taxRate: product.taxRatePercent || 0,
          quantity: 1,
          unit: product.unit,
        },
      ]);
    }
    setErrorMessage('');
  };

  const updateQuantity = (productId: string, delta: number) => {
    setCart(
      cart
        .map((item) => {
          if (item.productId === productId) {
            const newQty = item.quantity + delta;
            return newQty > 0 ? { ...item, quantity: newQty } : null;
          }
          return item;
        })
        .filter(Boolean) as any[]
    );
  };

  const removeFromCart = (productId: string) => {
    setCart(cart.filter((item) => item.productId !== productId));
  };

  // Calculations
  const subtotal = cart.reduce((acc, item) => acc + item.unitPrice * item.quantity, 0);
  const totalTax = cart.reduce(
    (acc, item) => acc + (item.unitPrice * item.quantity * item.taxRate) / 100,
    0
  );
  const validDiscount = Math.min(subtotal, Math.max(0, discountAmount));
  const grandTotal = Math.max(0, subtotal - validDiscount + totalTax);

  const handleCheckout = async () => {
    if (cart.length === 0) {
      setErrorMessage('Add at least one item to cart before checkout.');
      return;
    }

    try {
      setIsSubmitting(true);
      setErrorMessage('');

      const payload = {
        customerName: customerName || 'Walk-in Customer',
        customerPhone,
        items: cart.map((i) => ({
          productId: i.productId,
          quantity: i.quantity,
          unitPrice: i.unitPrice,
          taxRate: i.taxRate,
        })),
        discountAmount: validDiscount,
        paymentMethod,
        notes: `POS Checkout via ${paymentMethod}`,
      };

      const res = await api.createInvoice(payload);
      setGeneratedInvoice(res.invoice);

      // Reset cart
      setCart([]);
      setCustomerName('');
      setCustomerPhone('');
      setDiscountAmount(0);
      loadProducts();
    } catch (err: any) {
      setErrorMessage(err.message || 'Checkout failed.');
    } finally {
      setIsSubmitting(false);
    }
  };

  const filteredProducts = products.filter(
    (p) =>
      p.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
      p.sku.toLowerCase().includes(searchQuery.toLowerCase()) ||
      (p.barcode && p.barcode.includes(searchQuery))
  );

  return (
    <div className="grid grid-cols-1 lg:grid-cols-12 gap-6 h-[calc(100vh-120px)]">
      {/* ── LEFT: Product Catalogue Selector (7 Cols) ── */}
      <div className="lg:col-span-7 flex flex-col h-full bg-white dark:bg-[#161722] rounded-2xl border border-slate-200 dark:border-[#232536] p-5 shadow-sm">
        {/* Search & Barcode Header */}
        <div className="flex items-center gap-3 mb-4">
          <div className="relative flex-1">
            <Search className="w-4 h-4 absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" />
            <input
              type="text"
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              placeholder="Search by product name, SKU or scan barcode..."
              className="w-full pl-9 pr-3 py-2 text-xs rounded-xl bg-slate-100 dark:bg-[#1a1c2a] border border-slate-200 dark:border-[#2b2e46] text-slate-800 dark:text-slate-200 placeholder-slate-400 focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>
          <button className="flex items-center gap-1.5 px-3 py-2 bg-slate-100 dark:bg-slate-800 hover:bg-slate-200 dark:hover:bg-slate-700 rounded-xl text-xs font-bold text-slate-700 dark:text-slate-300">
            <Barcode className="w-4 h-4 text-blue-500" />
            <span>Barcode</span>
          </button>
        </div>

        {/* Product Cards Grid */}
        <div className="flex-1 overflow-y-auto pr-1 grid grid-cols-2 md:grid-cols-3 gap-3">
          {filteredProducts.map((p) => {
            const isOutOfStock = p.stockQuantity <= 0;
            return (
              <div
                key={p.id}
                onClick={() => !isOutOfStock && addToCart(p)}
                className={`p-3.5 rounded-xl border flex flex-col justify-between transition-all cursor-pointer ${
                  isOutOfStock
                    ? 'opacity-40 border-slate-200 bg-slate-50 dark:bg-slate-900 cursor-not-allowed'
                    : 'bg-slate-50 dark:bg-[#1a1c2a] border-slate-200/80 dark:border-[#2b2e46] hover:border-blue-500/50 hover:shadow-md active:scale-95'
                }`}
              >
                <div>
                  <span className="text-[10px] uppercase font-bold text-blue-600 dark:text-blue-400 block mb-1 truncate">
                    {p.category}
                  </span>
                  <h4 className="text-xs font-bold text-slate-900 dark:text-white line-clamp-2 leading-tight">
                    {p.name}
                  </h4>
                  <p className="text-[10px] text-slate-400 mt-0.5">SKU: {p.sku}</p>
                </div>

                <div className="flex items-center justify-between mt-3 pt-2 border-t border-slate-200/60 dark:border-[#282b3f]">
                  <span className="text-sm font-extrabold text-slate-900 dark:text-white">
                    {currencySymbol}{Number(p.sellingPrice).toFixed(2)}
                  </span>
                  <span
                    className={`text-[10px] font-bold px-1.5 py-0.5 rounded ${
                      isOutOfStock
                        ? 'bg-rose-500/10 text-rose-500'
                        : p.isLowStock
                        ? 'bg-amber-500/10 text-amber-500'
                        : 'bg-emerald-500/10 text-emerald-500'
                    }`}
                  >
                    {p.stockQuantity} {p.unit}
                  </span>
                </div>
              </div>
            );
          })}
        </div>
      </div>

      {/* ── RIGHT: Active Cart & POS Terminal (5 Cols) ── */}
      <div className="lg:col-span-5 flex flex-col h-full bg-white dark:bg-[#161722] rounded-2xl border border-slate-200 dark:border-[#232536] p-5 shadow-sm">
        <div className="flex items-center justify-between pb-3 border-b border-slate-200 dark:border-[#232536] mb-3">
          <div className="flex items-center gap-2">
            <Receipt className="w-5 h-5 text-blue-600" />
            <h3 className="text-base font-extrabold text-slate-900 dark:text-white">Billing Terminal</h3>
          </div>
          <span className="text-xs font-bold px-2 py-0.5 rounded-full bg-blue-100 dark:bg-blue-900/40 text-blue-700 dark:text-blue-300">
            {cart.length} items
          </span>
        </div>

        {/* Customer Input Strip */}
        <div className="grid grid-cols-2 gap-2 mb-3">
          <div className="relative">
            <User className="w-3.5 h-3.5 absolute left-2.5 top-1/2 -translate-y-1/2 text-slate-400" />
            <input
              type="text"
              value={customerName}
              onChange={(e) => setCustomerName(e.target.value)}
              placeholder="Customer Name"
              className="w-full pl-8 pr-2 py-1.5 text-xs rounded-xl bg-slate-100 dark:bg-[#1a1c2a] border border-slate-200 dark:border-[#2b2e46] text-slate-800 dark:text-slate-200"
            />
          </div>
          <div className="relative">
            <Phone className="w-3.5 h-3.5 absolute left-2.5 top-1/2 -translate-y-1/2 text-slate-400" />
            <input
              type="text"
              value={customerPhone}
              onChange={(e) => setCustomerPhone(e.target.value)}
              placeholder="Phone (Optional)"
              className="w-full pl-8 pr-2 py-1.5 text-xs rounded-xl bg-slate-100 dark:bg-[#1a1c2a] border border-slate-200 dark:border-[#2b2e46] text-slate-800 dark:text-slate-200"
            />
          </div>
        </div>

        {errorMessage && (
          <div className="p-2 mb-3 rounded-lg bg-rose-500/10 border border-rose-500/30 text-rose-500 text-xs font-bold">
            {errorMessage}
          </div>
        )}

        {/* Cart Items Scroll Table */}
        <div className="flex-1 overflow-y-auto space-y-2 pr-1 mb-3">
          {cart.length === 0 ? (
            <div className="h-full flex flex-col items-center justify-center text-slate-400 text-center p-6">
              <Receipt className="w-12 h-12 text-slate-300 dark:text-slate-700 mb-2 stroke-1" />
              <p className="text-xs font-bold">Cart is empty</p>
              <p className="text-[11px] text-slate-400">Click items on the left to add to bill</p>
            </div>
          ) : (
            cart.map((item) => (
              <div
                key={item.productId}
                className="flex items-center justify-between p-2.5 rounded-xl bg-slate-50 dark:bg-[#1a1c2a] border border-slate-200/80 dark:border-[#2b2e46] text-xs"
              >
                <div className="flex-1 min-w-0 pr-2">
                  <p className="font-bold text-slate-800 dark:text-slate-200 truncate">{item.name}</p>
                  <p className="text-[10px] text-slate-400">
                    {currencySymbol}{item.unitPrice} × {item.quantity} (Tax: {item.taxRate}%)
                  </p>
                </div>

                {/* Qty Controls */}
                <div className="flex items-center gap-1.5 mr-3">
                  <button
                    onClick={() => updateQuantity(item.productId, -1)}
                    className="w-6 h-6 rounded-md bg-slate-200 dark:bg-slate-700 hover:bg-slate-300 font-bold flex items-center justify-center text-slate-700 dark:text-white"
                  >
                    -
                  </button>
                  <span className="font-extrabold w-5 text-center text-slate-900 dark:text-white">
                    {item.quantity}
                  </span>
                  <button
                    onClick={() => updateQuantity(item.productId, 1)}
                    className="w-6 h-6 rounded-md bg-slate-200 dark:bg-slate-700 hover:bg-slate-300 font-bold flex items-center justify-center text-slate-700 dark:text-white"
                  >
                    +
                  </button>
                </div>

                <div className="text-right flex items-center gap-2">
                  <span className="font-extrabold text-slate-900 dark:text-white min-w-[50px]">
                    {currencySymbol}{(item.unitPrice * item.quantity).toFixed(2)}
                  </span>
                  <button
                    onClick={() => removeFromCart(item.productId)}
                    className="text-slate-400 hover:text-rose-500 p-1"
                  >
                    <Trash2 className="w-3.5 h-3.5" />
                  </button>
                </div>
              </div>
            ))
          )}
        </div>

        {/* Payment Method Selector */}
        <div className="mb-3">
          <span className="text-[10px] uppercase font-bold text-slate-400 block mb-1.5">Payment Method</span>
          <div className="grid grid-cols-4 gap-1.5 text-xs">
            {[
              { id: 'CASH', label: 'Cash', icon: Banknote },
              { id: 'UPI', label: 'UPI / QR', icon: QrCode },
              { id: 'BANK_TRANSFER', label: 'Bank', icon: Building },
              { id: 'CARD', label: 'Card', icon: CreditCard },
            ].map((m) => {
              const Icon = m.icon;
              const isSelected = paymentMethod === m.id;
              return (
                <button
                  key={m.id}
                  onClick={() => setPaymentMethod(m.id as any)}
                  className={`flex flex-col items-center justify-center p-2 rounded-xl border text-[11px] font-bold transition-all ${
                    isSelected
                      ? 'border-blue-600 bg-blue-600/10 text-blue-600 dark:text-blue-400'
                      : 'border-slate-200 dark:border-[#2b2e46] text-slate-500 hover:bg-slate-50 dark:hover:bg-slate-800'
                  }`}
                >
                  <Icon className="w-4 h-4 mb-0.5" />
                  <span>{m.label}</span>
                </button>
              );
            })}
          </div>
        </div>

        {/* Calculations Footer */}
        <div className="space-y-1.5 pt-3 border-t border-slate-200 dark:border-[#232536] text-xs">
          <div className="flex justify-between text-slate-500">
            <span>Subtotal</span>
            <span>{currencySymbol}{subtotal.toFixed(2)}</span>
          </div>
          <div className="flex justify-between items-center text-slate-500">
            <span>Discount</span>
            <input
              type="number"
              value={discountAmount || ''}
              onChange={(e) => setDiscountAmount(parseFloat(e.target.value) || 0)}
              placeholder="0.00"
              className="w-20 px-2 py-0.5 text-right rounded-lg bg-slate-100 dark:bg-[#1a1c2a] border border-slate-200 dark:border-[#2b2e46] text-xs font-bold"
            />
          </div>
          <div className="flex justify-between text-slate-500">
            <span>Tax</span>
            <span>{currencySymbol}{totalTax.toFixed(2)}</span>
          </div>
          <div className="flex justify-between items-center pt-2 border-t border-slate-200 dark:border-[#232536] text-lg font-black text-slate-900 dark:text-white">
            <span>Total Payable:</span>
            <span className="text-blue-600 dark:text-blue-400">{currencySymbol}{grandTotal.toFixed(2)}</span>
          </div>
        </div>

        {/* Checkout Button */}
        <button
          onClick={handleCheckout}
          disabled={cart.length === 0 || isSubmitting}
          className="w-full mt-4 py-3 rounded-xl bg-blue-600 hover:bg-blue-700 disabled:opacity-50 text-white text-sm font-extrabold shadow-lg shadow-blue-500/25 active:scale-95 transition-all flex items-center justify-center gap-2"
        >
          {isSubmitting ? (
            <span>Processing Invoice...</span>
          ) : (
            <>
              <CheckCircle2 className="w-4 h-4" />
              <span>Complete Sale & Print Invoice</span>
            </>
          )}
        </button>
      </div>

      {/* Printable Invoice Modal Popup */}
      {generatedInvoice && (
        <PrintInvoiceModal
          invoice={generatedInvoice}
          business={business}
          onClose={() => setGeneratedInvoice(null)}
        />
      )}
    </div>
  );
};
