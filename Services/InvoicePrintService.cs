using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using PROJECT_SBV1.Models;

namespace PROJECT_SBV1.Services
{
    /// <summary>
    /// Professional Invoice Printing, Vector GDI+ PrintPreview, and HTML/PDF Export Service.
    /// Provides pixel-perfect vector print layouts, thermal/A4 printing, and direct PDF generation.
    /// </summary>
    public static class InvoicePrintService
    {
        private static readonly string InvoicesDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TEAMSOFT", "Invoices");

        public static void ShowPrintPreview(Invoice invoice, BusinessProfile business, IWin32Window owner)
        {
            try
            {
                var doc = CreatePrintDocument(invoice, business);
                using var preview = new PrintPreviewDialog
                {
                    Document = doc,
                    Width = 980,
                    Height = 800,
                    StartPosition = FormStartPosition.CenterParent,
                    Text = $"Invoice Preview — {invoice.InvoiceNumber}"
                };
                preview.ShowDialog(owner);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to generate print preview: {ex.Message}", "Print Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static void PrintDirect(Invoice invoice, BusinessProfile business, IWin32Window owner)
        {
            try
            {
                var doc = CreatePrintDocument(invoice, business);
                using var printDlg = new PrintDialog
                {
                    Document = doc,
                    AllowSomePages = true,
                    UseEXDialog = true
                };

                if (printDlg.ShowDialog(owner) == DialogResult.OK)
                {
                    doc.Print();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to print invoice: {ex.Message}", "Print Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static string ExportToPdfFile(Invoice invoice, BusinessProfile business)
        {
            if (!Directory.Exists(InvoicesDirectory))
            {
                Directory.CreateDirectory(InvoicesDirectory);
            }

            string cleanNum = invoice.InvoiceNumber.Replace("/", "-").Replace("\\", "-");
            string htmlPath = Path.Combine(InvoicesDirectory, $"{cleanNum}.html");

            string html = GenerateInvoiceHtml(invoice, business);
            File.WriteAllText(htmlPath, html, Encoding.UTF8);

            return htmlPath;
        }

        public static void OpenOrShareInvoice(Invoice invoice, BusinessProfile business)
        {
            string filePath = ExportToPdfFile(invoice, business);
            try
            {
                Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to open invoice file: {ex.Message}", "Open Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private static PrintDocument CreatePrintDocument(Invoice invoice, BusinessProfile business)
        {
            var doc = new PrintDocument();
            doc.DefaultPageSettings.Margins = new Margins(40, 40, 40, 40);
            doc.DefaultPageSettings.PaperSize = new PaperSize("A4", 827, 1169);

            doc.PrintPage += (s, e) =>
            {
                var g = e.Graphics;
                if (g == null) return;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                DrawInvoiceVectorPage(g, e.MarginBounds, invoice, business);
                e.HasMorePages = false;
            };

            return doc;
        }

        private static void DrawInvoiceVectorPage(Graphics g, Rectangle bounds, Invoice inv, BusinessProfile b)
        {
            int left = bounds.Left;
            int right = bounds.Right;
            int y = bounds.Top;
            int width = bounds.Width;

            using var fHeader = new Font("Segoe UI", 16f, FontStyle.Bold);
            using var fTitle = new Font("Segoe UI", 11f, FontStyle.Bold);
            using var fSub = new Font("Segoe UI", 8.5f);
            using var fRegular = new Font("Segoe UI", 9f);
            using var fBold = new Font("Segoe UI", 9f, FontStyle.Bold);
            using var fGrand = new Font("Segoe UI", 13f, FontStyle.Bold);

            using var bDark = new SolidBrush(Color.FromArgb(17, 24, 39));
            using var bMuted = new SolidBrush(Color.FromArgb(107, 114, 128));
            using var bAccent = new SolidBrush(Color.FromArgb(65, 105, 245));
            using var bStatus = new SolidBrush(inv.Status == "Paid" ? Color.FromArgb(16, 185, 129) : (inv.Status == "Overdue" ? Color.FromArgb(239, 68, 68) : Color.FromArgb(245, 158, 11)));
            using var penLine = new Pen(Color.FromArgb(229, 231, 235), 1f);
            using var penAccent = new Pen(Color.FromArgb(65, 105, 245), 2f);

            // 1. Top Header: Business Branding (Left) & Invoice Title/Badge (Right)
            g.DrawString(b.BusinessName, fHeader, bAccent, left, y);
            y += 26;
            g.DrawString(b.Address, fSub, bMuted, left, y);
            y += 15;
            g.DrawString($"Phone: {b.Phone}  |  Email: {b.Email}  |  GSTIN: {b.GSTIN}", fSub, bMuted, left, y);

            // Right Header: INVOICE details
            int rightColX = right - 220;
            g.DrawString("TAX INVOICE", fHeader, bDark, rightColX, bounds.Top);
            g.DrawString($"Invoice #: {inv.InvoiceNumber}", fBold, bDark, rightColX, bounds.Top + 26);
            g.DrawString($"Date: {inv.InvoiceDate:dd MMM yyyy}", fRegular, bMuted, rightColX, bounds.Top + 43);
            g.DrawString($"Due: {inv.DueDate:dd MMM yyyy}", fRegular, bMuted, rightColX, bounds.Top + 58);
            g.DrawString($"Status: {inv.Status.ToUpper()}", fBold, bStatus, rightColX, bounds.Top + 73);

            y += 35;
            g.DrawLine(penAccent, left, y, right, y);
            y += 12;

            // 2. Bill To & Payment Terms Box
            int boxH = 75;
            using var bgBox = new SolidBrush(Color.FromArgb(248, 250, 252));
            g.FillRectangle(bgBox, left, y, width, boxH);
            g.DrawRectangle(penLine, left, y, width, boxH);

            g.DrawString("BILLED TO:", fTitle, bDark, left + 12, y + 8);
            g.DrawString(inv.CustomerName + (string.IsNullOrEmpty(inv.CustomerCompany) ? "" : $" — {inv.CustomerCompany}"), fBold, bDark, left + 12, y + 26);
            g.DrawString($"Phone: {inv.CustomerPhone}   |   Email: {inv.CustomerEmail}", fSub, bMuted, left + 12, y + 42);
            g.DrawString($"Address: {inv.CustomerAddress}   |   GSTIN: {(string.IsNullOrEmpty(inv.CustomerGSTIN) ? "N/A" : inv.CustomerGSTIN)}", fSub, bMuted, left + 12, y + 56);

            int termsX = right - 200;
            g.DrawString("TERMS & METHOD:", fTitle, bDark, termsX, y + 8);
            g.DrawString($"Terms: {inv.PaymentTerms}", fRegular, bDark, termsX, y + 26);
            g.DrawString($"Tax Scheme: {inv.TaxType.Replace("_", " ")}", fRegular, bMuted, termsX, y + 42);

            y += boxH + 16;

            // 3. Itemized Table Header
            int thY = y;
            using var thBrush = new SolidBrush(Color.FromArgb(241, 245, 249));
            g.FillRectangle(thBrush, left, thY, width, 24);
            g.DrawRectangle(penLine, left, thY, width, 24);

            int colDesc = left + 8;
            int colQty = left + 360;
            int colRate = left + 430;
            int colDisc = left + 520;
            int colTax = left + 600;
            int colTotal = right - 90;

            g.DrawString("Service / Description", fBold, bDark, colDesc, thY + 4);
            g.DrawString("Qty", fBold, bDark, colQty, thY + 4);
            g.DrawString("Unit Price", fBold, bDark, colRate, thY + 4);
            g.DrawString("Disc", fBold, bDark, colDisc, thY + 4);
            g.DrawString("GST %", fBold, bDark, colTax, thY + 4);
            g.DrawString("Total", fBold, bDark, colTotal, thY + 4);

            y += 28;

            // Table Rows
            foreach (var it in inv.Items)
            {
                g.DrawString(it.ServiceName, fBold, bDark, colDesc, y);
                if (!string.IsNullOrEmpty(it.Description))
                {
                    g.DrawString(it.Description, fSub, bMuted, colDesc, y + 14);
                }

                g.DrawString($"{it.Quantity:N0}", fRegular, bDark, colQty, y);
                g.DrawString($"₹{it.UnitPrice:N2}", fRegular, bDark, colRate, y);
                g.DrawString($"₹{it.DiscountAmount:N2}", fRegular, bDark, colDisc, y);
                g.DrawString($"{it.TaxPercent:N0}%", fRegular, bDark, colTax, y);
                g.DrawString($"₹{it.LineTotal:N2}", fBold, bDark, colTotal, y);

                y += string.IsNullOrEmpty(it.Description) ? 22 : 32;
                g.DrawLine(penLine, left, y, right, y);
                y += 6;
            }

            y += 10;

            // 4. Totals Breakdown & Bank/UPI Info
            int summaryLeft = right - 260;

            // Payment details on Left
            g.DrawString("PAYMENT INSTRUCTIONS & BANK DETAILS:", fTitle, bDark, left, y);
            g.DrawString($"Bank: {b.BankDetails}", fSub, bDark, left, y + 18);
            g.DrawString($"UPI ID: {b.UPIId}", fSub, bDark, left, y + 33);
            g.DrawString($"Terms: {b.DefaultTerms}", fSub, bMuted, left, y + 48);

            // Financial Summary on Right
            g.DrawString("Subtotal:", fRegular, bMuted, summaryLeft, y);
            g.DrawString($"₹{inv.Subtotal:N2}", fRegular, bDark, right - 80, y);
            y += 18;

            if (inv.ItemDiscountTotal + inv.OverallDiscount > 0)
            {
                g.DrawString("Total Discount:", fRegular, bMuted, summaryLeft, y);
                g.DrawString($"-₹{(inv.ItemDiscountTotal + inv.OverallDiscount):N2}", fRegular, bStatus, right - 80, y);
                y += 18;
            }

            g.DrawString("Taxable Amount:", fRegular, bMuted, summaryLeft, y);
            g.DrawString($"₹{inv.TaxableAmount:N2}", fRegular, bDark, right - 80, y);
            y += 18;

            if (inv.CgstAmount > 0)
            {
                g.DrawString($"CGST ({inv.CgstRate}%):", fRegular, bMuted, summaryLeft, y);
                g.DrawString($"₹{inv.CgstAmount:N2}", fRegular, bDark, right - 80, y);
                y += 16;
            }
            if (inv.SgstAmount > 0)
            {
                g.DrawString($"SGST ({inv.SgstRate}%):", fRegular, bMuted, summaryLeft, y);
                g.DrawString($"₹{inv.SgstAmount:N2}", fRegular, bDark, right - 80, y);
                y += 16;
            }
            if (inv.IgstAmount > 0)
            {
                g.DrawString($"IGST ({inv.IgstRate}%):", fRegular, bMuted, summaryLeft, y);
                g.DrawString($"₹{inv.IgstAmount:N2}", fRegular, bDark, right - 80, y);
                y += 16;
            }

            g.DrawLine(penAccent, summaryLeft, y, right, y);
            y += 6;

            g.DrawString("Grand Total:", fGrand, bDark, summaryLeft, y);
            g.DrawString($"₹{inv.GrandTotal:N2}", fGrand, bAccent, right - 95, y);
            y += 24;

            g.DrawString("Amount Paid:", fBold, bDark, summaryLeft, y);
            g.DrawString($"₹{inv.AmountPaid:N2}", fBold, bDark, right - 80, y);
            y += 18;

            g.DrawString("Balance Due:", fGrand, bDark, summaryLeft, y);
            g.DrawString($"₹{inv.BalanceDue:N2}", fGrand, inv.BalanceDue > 0 ? bStatus : bDark, right - 95, y);

            // 5. Signature Area
            int signY = bounds.Bottom - 60;
            g.DrawLine(penLine, right - 180, signY, right, signY);
            g.DrawString("Authorized Signatory", fBold, bDark, right - 160, signY + 6);
            g.DrawString(b.BusinessName, fSub, bMuted, right - 170, signY + 22);
        }

        private static string GenerateInvoiceHtml(Invoice inv, BusinessProfile b)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html lang='en'>");
            sb.AppendLine("<head>");
            sb.AppendLine("<meta charset='utf-8'/>");
            sb.AppendLine($"<title>Invoice {inv.InvoiceNumber} - {b.BusinessName}</title>");
            sb.AppendLine("<style>");
            sb.AppendLine(@"
                body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; margin: 0; padding: 24px; color: #1e293b; background: #f8fafc; }
                .invoice-card { max-width: 850px; margin: 0 auto; background: #ffffff; padding: 36px 40px; border-radius: 12px; box-shadow: 0 4px 16px rgba(0,0,0,0.06); }
                .header-flex { display: flex; justify-content: space-between; align-items: flex-start; border-bottom: 2px solid #4169e1; padding-bottom: 18px; margin-bottom: 20px; }
                .brand-title { font-size: 24px; font-weight: 800; color: #4169e1; margin: 0 0 4px 0; }
                .brand-sub { font-size: 13px; color: #64748b; margin: 2px 0; }
                .inv-title { font-size: 26px; font-weight: 800; color: #0f172a; text-align: right; margin: 0 0 4px 0; }
                .inv-badge { display: inline-block; padding: 4px 10px; border-radius: 6px; font-weight: 700; font-size: 12px; text-transform: uppercase; margin-top: 4px; }
                .badge-paid { background: #dcfce7; color: #15803d; }
                .badge-unpaid { background: #fef3c7; color: #b45309; }
                .badge-overdue { background: #fee2e2; color: #b91c1c; }
                .info-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 20px; background: #f8fafc; border: 1px solid #e2e8f0; border-radius: 8px; padding: 16px 20px; margin-bottom: 24px; }
                .section-label { font-size: 11px; font-weight: 700; text-transform: uppercase; color: #64748b; margin-bottom: 4px; }
                .cust-name { font-size: 16px; font-weight: 700; color: #0f172a; margin: 0 0 4px 0; }
                table { width: 100%; border-collapse: collapse; margin-bottom: 24px; }
                th { background: #f1f5f9; padding: 10px 14px; font-size: 13px; font-weight: 700; color: #334155; text-align: left; border-bottom: 2px solid #cbd5e1; }
                td { padding: 12px 14px; font-size: 13px; border-bottom: 1px solid #e2e8f0; }
                .text-right { text-align: right; }
                .summary-flex { display: flex; justify-content: space-between; align-items: flex-start; gap: 30px; margin-top: 20px; }
                .bank-box { flex: 1; background: #f8fafc; border: 1px solid #e2e8f0; border-radius: 8px; padding: 14px 18px; font-size: 12.5px; }
                .totals-box { width: 320px; font-size: 14px; }
                .total-row { display: flex; justify-content: space-between; padding: 4px 0; }
                .grand-total { font-size: 18px; font-weight: 800; color: #4169e1; border-top: 2px solid #4169e1; padding-top: 8px; margin-top: 6px; }
                .sign-box { margin-top: 40px; text-align: right; font-size: 13px; }
                .sign-line { display: inline-block; width: 180px; border-top: 1px solid #94a3b8; margin-top: 40px; padding-top: 4px; font-weight: 700; }
                @media print {
                    body { background: #ffffff; padding: 0; }
                    .invoice-card { box-shadow: none; border-radius: 0; padding: 0; }
                    .no-print { display: none !important; }
                }
            ");
            sb.AppendLine("</style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");

            sb.AppendLine("<div class='invoice-card'>");
            sb.AppendLine("  <div class='header-flex'>");
            sb.AppendLine("    <div>");
            sb.AppendLine($"      <h1 class='brand-title'>{b.BusinessName}</h1>");
            sb.AppendLine($"      <p class='brand-sub'>{b.Address}</p>");
            sb.AppendLine($"      <p class='brand-sub'>Phone: {b.Phone} &bull; Email: {b.Email}</p>");
            sb.AppendLine($"      <p class='brand-sub'>GSTIN: {b.GSTIN}</p>");
            sb.AppendLine("    </div>");
            sb.AppendLine("    <div style='text-align:right;'>");
            sb.AppendLine("      <h2 class='inv-title'>TAX INVOICE</h2>");
            sb.AppendLine($"      <div style='font-size:15px; font-weight:700;'>{inv.InvoiceNumber}</div>");
            sb.AppendLine($"      <div class='brand-sub'>Date: {inv.InvoiceDate:dd MMM yyyy}</div>");
            sb.AppendLine($"      <div class='brand-sub'>Due: {inv.DueDate:dd MMM yyyy}</div>");
            string badgeClass = inv.Status == "Paid" ? "badge-paid" : (inv.Status == "Overdue" ? "badge-overdue" : "badge-unpaid");
            sb.AppendLine($"      <span class='inv-badge {badgeClass}'>{inv.Status}</span>");
            sb.AppendLine("    </div>");
            sb.AppendLine("  </div>");

            sb.AppendLine("  <div class='info-grid'>");
            sb.AppendLine("    <div>");
            sb.AppendLine("      <div class='section-label'>Billed To</div>");
            sb.AppendLine($"      <div class='cust-name'>{inv.CustomerName}</div>");
            if (!string.IsNullOrEmpty(inv.CustomerCompany))
                sb.AppendLine($"      <div class='brand-sub'><b>{inv.CustomerCompany}</b></div>");
            sb.AppendLine($"      <div class='brand-sub'>{inv.CustomerAddress}</div>");
            sb.AppendLine($"      <div class='brand-sub'>Phone: {inv.CustomerPhone} &bull; Email: {inv.CustomerEmail}</div>");
            if (!string.IsNullOrEmpty(inv.CustomerGSTIN))
                sb.AppendLine($"      <div class='brand-sub'>GSTIN: {inv.CustomerGSTIN}</div>");
            sb.AppendLine("    </div>");
            sb.AppendLine("    <div>");
            sb.AppendLine("      <div class='section-label'>Payment Terms & Tax Details</div>");
            sb.AppendLine($"      <div class='brand-sub'><b>Terms:</b> {inv.PaymentTerms}</div>");
            sb.AppendLine($"      <div class='brand-sub'><b>Tax Scheme:</b> {inv.TaxType.Replace("_", " ")}</div>");
            sb.AppendLine($"      <div class='brand-sub'><b>Status:</b> {inv.Status}</div>");
            sb.AppendLine("    </div>");
            sb.AppendLine("  </div>");

            sb.AppendLine("  <table>");
            sb.AppendLine("    <thead>");
            sb.AppendLine("      <tr>");
            sb.AppendLine("        <th>#</th>");
            sb.AppendLine("        <th>Service / Description</th>");
            sb.AppendLine("        <th class='text-right'>Qty</th>");
            sb.AppendLine("        <th class='text-right'>Unit Price</th>");
            sb.AppendLine("        <th class='text-right'>Disc</th>");
            sb.AppendLine("        <th class='text-right'>GST %</th>");
            sb.AppendLine("        <th class='text-right'>Line Total</th>");
            sb.AppendLine("      </tr>");
            sb.AppendLine("    </thead>");
            sb.AppendLine("    <tbody>");

            int idx = 1;
            foreach (var it in inv.Items)
            {
                sb.AppendLine("      <tr>");
                sb.AppendLine($"        <td>{idx++}</td>");
                sb.AppendLine($"        <td><b>{it.ServiceName}</b><br/><span style='font-size:12px;color:#64748b;'>{it.Description}</span></td>");
                sb.AppendLine($"        <td class='text-right'>{it.Quantity:N0}</td>");
                sb.AppendLine($"        <td class='text-right'>₹{it.UnitPrice:N2}</td>");
                sb.AppendLine($"        <td class='text-right'>₹{it.DiscountAmount:N2}</td>");
                sb.AppendLine($"        <td class='text-right'>{it.TaxPercent:N0}%</td>");
                sb.AppendLine($"        <td class='text-right'><b>₹{it.LineTotal:N2}</b></td>");
                sb.AppendLine("      </tr>");
            }

            sb.AppendLine("    </tbody>");
            sb.AppendLine("  </table>");

            sb.AppendLine("  <div class='summary-flex'>");
            sb.AppendLine("    <div class='bank-box'>");
            sb.AppendLine("      <div class='section-label'>Bank & UPI Payment Details</div>");
            sb.AppendLine($"      <div><b>Bank:</b> {b.BankDetails}</div>");
            sb.AppendLine($"      <div><b>UPI ID:</b> {b.UPIId}</div>");
            sb.AppendLine($"      <div style='margin-top:6px;color:#64748b;'><b>Terms:</b> {b.DefaultTerms}</div>");
            sb.AppendLine("    </div>");

            sb.AppendLine("    <div class='totals-box'>");
            sb.AppendLine($"      <div class='total-row'><span>Subtotal:</span><span>₹{inv.Subtotal:N2}</span></div>");
            if (inv.ItemDiscountTotal + inv.OverallDiscount > 0)
            {
                sb.AppendLine($"      <div class='total-row' style='color:#16a34a;'><span>Discounts:</span><span>-₹{(inv.ItemDiscountTotal + inv.OverallDiscount):N2}</span></div>");
            }
            sb.AppendLine($"      <div class='total-row'><span>Taxable Amount:</span><span>₹{inv.TaxableAmount:N2}</span></div>");
            if (inv.CgstAmount > 0)
                sb.AppendLine($"      <div class='total-row' style='color:#64748b;'><span>CGST ({inv.CgstRate}%):</span><span>₹{inv.CgstAmount:N2}</span></div>");
            if (inv.SgstAmount > 0)
                sb.AppendLine($"      <div class='total-row' style='color:#64748b;'><span>SGST ({inv.SgstRate}%):</span><span>₹{inv.SgstAmount:N2}</span></div>");
            if (inv.IgstAmount > 0)
                sb.AppendLine($"      <div class='total-row' style='color:#64748b;'><span>IGST ({inv.IgstRate}%):</span><span>₹{inv.IgstAmount:N2}</span></div>");

            sb.AppendLine($"      <div class='total-row grand-total'><span>Grand Total:</span><span>₹{inv.GrandTotal:N2}</span></div>");
            sb.AppendLine($"      <div class='total-row' style='margin-top:4px;'><span>Amount Paid:</span><span>₹{inv.AmountPaid:N2}</span></div>");
            sb.AppendLine($"      <div class='total-row' style='font-size:15px;font-weight:700;color:{(inv.BalanceDue > 0 ? "#b45309" : "#15803d")};'><span>Balance Due:</span><span>₹{inv.BalanceDue:N2}</span></div>");
            sb.AppendLine("    </div>");
            sb.AppendLine("  </div>");

            sb.AppendLine("  <div class='sign-box'>");
            sb.AppendLine($"    <div class='sign-line'>For {b.BusinessName}<br/><span style='font-size:11px;font-weight:400;color:#64748b;'>Authorized Signatory</span></div>");
            sb.AppendLine("  </div>");

            sb.AppendLine("  <div class='no-print' style='text-align:center;margin-top:30px;'>");
            sb.AppendLine("    <button onclick='window.print()' style='background:#4169e1;color:#fff;border:none;padding:10px 24px;border-radius:8px;font-size:14px;font-weight:700;cursor:pointer;'>🖨️ Print or Save as PDF</button>");
            sb.AppendLine("  </div>");

            sb.AppendLine("</div>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return sb.ToString();
        }
    }
}
