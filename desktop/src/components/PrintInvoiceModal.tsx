import React from 'react';
import { Printer, X, CheckCircle2 } from 'lucide-react';

interface PrintInvoiceModalProps {
  invoice: any;
  business: any;
  onClose: () => void;
}

export const PrintInvoiceModal: React.FC<PrintInvoiceModalProps> = ({ invoice, business, onClose }) => {
  if (!invoice) return null;

  const handlePrint = () => {
    window.print();
  };

  const currencySymbol = business?.currencySymbol || '₹';

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm p-4 overflow-y-auto">
      <div className="bg-white text-slate-800 rounded-2xl shadow-2xl max-w-2xl w-full p-6 relative border border-slate-200">
        {/* Modal Controls */}
        <div className="flex items-center justify-between pb-4 border-b border-slate-200 mb-6 print:hidden">
          <div className="flex items-center gap-2 text-emerald-600">
            <CheckCircle2 className="w-5 h-5" />
            <span className="font-bold text-sm">Invoice Generated Successfully</span>
          </div>
          <div className="flex items-center gap-2">
            <button
              onClick={handlePrint}
              className="flex items-center gap-1.5 px-3.5 py-1.5 bg-blue-600 hover:bg-blue-700 text-white rounded-xl text-xs font-bold shadow-md transition-all"
            >
              <Printer className="w-3.5 h-3.5" />
              <span>Print Invoice</span>
            </button>
            <button
              onClick={onClose}
              className="p-1.5 rounded-lg hover:bg-slate-100 text-slate-400 hover:text-slate-600 transition-colors"
            >
              <X className="w-5 h-5" />
            </button>
          </div>
        </div>

        {/* Printable Invoice Sheet */}
        <div className="p-4 bg-white border border-slate-200 rounded-xl space-y-6 text-sm">
          {/* Header */}
          <div className="flex justify-between items-start">
            <div>
              <h2 className="text-xl font-extrabold text-slate-900 tracking-tight">{business?.name || 'Simple Billing SuperStore'}</h2>
              <p className="text-xs text-slate-500">{business?.address || 'Surat, Gujarat'}</p>
              <p className="text-xs text-slate-500">Phone: {business?.phone || '+91 9876543210'} | Tax/GST: {business?.taxNumber || '24AAACT1234F1Z5'}</p>
            </div>
            <div className="text-right">
              <span className="inline-block px-2.5 py-1 text-xs font-bold rounded-lg bg-blue-50 text-blue-700 border border-blue-200 uppercase tracking-wider">
                TAX INVOICE
              </span>
              <p className="text-xs font-bold text-slate-900 mt-1">{invoice.invoiceNumber}</p>
              <p className="text-[11px] text-slate-500">Date: {new Date(invoice.createdAt || Date.now()).toLocaleDateString()}</p>
            </div>
          </div>

          {/* Customer & Payment Meta */}
          <div className="grid grid-cols-2 gap-4 p-3 bg-slate-50 rounded-xl border border-slate-200/80 text-xs">
            <div>
              <span className="font-bold text-slate-500 uppercase block mb-0.5">Billed To:</span>
              <p className="font-bold text-slate-800 text-sm">{invoice.customerName || invoice.customer?.name || 'Walk-in Customer'}</p>
              {invoice.customerPhone && <p className="text-slate-600">Ph: {invoice.customerPhone}</p>}
            </div>
            <div className="text-right">
              <span className="font-bold text-slate-500 uppercase block mb-0.5">Payment Method:</span>
              <p className="font-bold text-emerald-700 uppercase">{invoice.paymentMethod || 'PAID (Cash/UPI)'}</p>
              <p className="text-slate-500">Status: <span className="font-bold text-emerald-600">{invoice.paymentStatus || 'PAID'}</span></p>
            </div>
          </div>

          {/* Line Items Table */}
          <table className="w-full text-left text-xs border-collapse">
            <thead>
              <tr className="border-b-2 border-slate-300 text-slate-500 font-bold uppercase">
                <th className="py-2">Item</th>
                <th className="py-2 text-center">Qty</th>
                <th className="py-2 text-right">Unit Price</th>
                <th className="py-2 text-right">Tax</th>
                <th className="py-2 text-right">Amount</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100">
              {invoice.items?.map((item: any, idx: number) => (
                <tr key={idx} className="hover:bg-slate-50">
                  <td className="py-2.5 font-medium text-slate-800">{item.product?.name || item.name || 'Product Item'}</td>
                  <td className="py-2.5 text-center text-slate-600">{item.quantity}</td>
                  <td className="py-2.5 text-right text-slate-600">{currencySymbol}{Number(item.unitPrice).toFixed(2)}</td>
                  <td className="py-2.5 text-right text-slate-500">{item.taxRate}%</td>
                  <td className="py-2.5 text-right font-bold text-slate-900">{currencySymbol}{Number(item.totalPrice).toFixed(2)}</td>
                </tr>
              ))}
            </tbody>
          </table>

          {/* Calculations Summary */}
          <div className="flex justify-end pt-4 border-t border-slate-200">
            <div className="w-64 space-y-1.5 text-xs">
              <div className="flex justify-between text-slate-600">
                <span>Subtotal:</span>
                <span>{currencySymbol}{Number(invoice.subtotal).toFixed(2)}</span>
              </div>
              {invoice.discountAmount > 0 && (
                <div className="flex justify-between text-rose-600">
                  <span>Discount:</span>
                  <span>-{currencySymbol}{Number(invoice.discountAmount).toFixed(2)}</span>
                </div>
              )}
              <div className="flex justify-between text-slate-600">
                <span>Tax:</span>
                <span>{currencySymbol}{Number(invoice.taxAmount).toFixed(2)}</span>
              </div>
              <div className="flex justify-between text-base font-extrabold text-blue-700 pt-2 border-t border-slate-300">
                <span>Total Amount:</span>
                <span>{currencySymbol}{Number(invoice.totalAmount).toFixed(2)}</span>
              </div>
            </div>
          </div>

          {/* Footer Note */}
          <div className="text-center text-[10px] text-slate-400 pt-4 border-t border-slate-100">
            Thank you for your business! Please visit us again.
          </div>
        </div>
      </div>
    </div>
  );
};
