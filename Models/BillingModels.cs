using System;
using System.Collections.Generic;

namespace PROJECT_SBV1.Models
{
    public class BusinessProfile
    {
        public string BusinessName { get; set; } = "TEAM SOFT Technologies Pvt Ltd";
        public string Address { get; set; } = "Tower B, Silicon Tech Park, SG Highway, Ahmedabad, Gujarat 380054";
        public string Phone { get; set; } = "+91 98250 99881";
        public string Email { get; set; } = "billing@teamsoft.in";
        public string GSTIN { get; set; } = "24AAACT8821R1Z5";
        public string DefaultTerms { get; set; } = "Payment due within 15 days of invoice date. Late payments attract 1.5% interest per month.";
        public string UPIId { get; set; } = "teamsoft@icici";
        public string BankDetails { get; set; } = "HDFC Bank | A/C: 50200088912344 | IFSC: HDFC0001025";
        public string LogoPath { get; set; } = "";
        public string Website { get; set; } = "https://teamsoft.in";
        public string AuthorizedSignatory { get; set; } = "Saumya Chaudhari";
        public string SignatoryTitle { get; set; } = "Lead Architect & Director";
        public string InvoiceFooter { get; set; } = "Thank you for choosing TEAM SOFT. Computer generated invoice, signature not required.";
        public string BankName { get; set; } = "HDFC Bank";
        public string AccountNumber { get; set; } = "50200088912344";
        public string IFSCCode { get; set; } = "HDFC0001025";
        public string BranchName { get; set; } = "SG Highway, Ahmedabad";
    }

    public class ProductItem
    {
        public long Id { get; set; }
        public string Barcode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = "General";
        public string HSN { get; set; } = "8471";
        public string Unit { get; set; } = "PCS";
        public decimal CostPrice { get; set; } = 0m;
        public decimal SalePrice { get; set; } = 0m;
        public decimal StockQuantity { get; set; } = 0m;
        public decimal TaxRate { get; set; } = 18m;
        public decimal MinStockAlert { get; set; } = 10m;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string DisplayText => $"{Name} [Barcode: {Barcode}] (Rate: ₹{SalePrice:N2} | Stock: {StockQuantity} {Unit})";
    }

    public class Broker
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public decimal CommissionPercent { get; set; } = 0m;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public override string ToString() => Name;
    }

    public class Customer
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string BillingAddress { get; set; } = string.Empty;
        public string GSTIN { get; set; } = string.Empty;
        public decimal OutstandingBalance { get; set; } = 0m;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string DisplayName => string.IsNullOrWhiteSpace(CompanyName) 
            ? Name 
            : $"{Name} ({CompanyName})";

        public override string ToString() => DisplayName;
    }

    public class InvoiceItem
    {
        public long Id { get; set; }
        public long InvoiceId { get; set; }
        public long ProductId { get; set; } = 0;
        public string ServiceName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string HSN { get; set; } = "8471";
        public string Unit { get; set; } = "PCS";
        public decimal Quantity { get; set; } = 1m;
        public decimal UnitPrice { get; set; } = 0m;
        public decimal DiscountPercent { get; set; } = 0m;
        public decimal DiscountAmount { get; set; } = 0m;
        public decimal TaxPercent { get; set; } = 18m;
        public decimal TaxableAmount { get; set; } = 0m;
        public decimal LineTotal { get; set; } = 0m;

        public void CalculateLineTotal()
        {
            decimal rawTotal = Quantity * UnitPrice;
            if (DiscountPercent > 0)
            {
                DiscountAmount = Math.Round(rawTotal * (DiscountPercent / 100m), 2);
            }
            TaxableAmount = Math.Max(0m, rawTotal - DiscountAmount);
            decimal tax = Math.Round(TaxableAmount * (TaxPercent / 100m), 2);
            LineTotal = TaxableAmount + tax;
        }
    }

    public class PaymentRecord
    {
        public long Id { get; set; }
        public long InvoiceId { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.Now;
        public decimal Amount { get; set; } = 0m;
        public string PaymentMethod { get; set; } = "Bank Transfer"; // Cash, UPI, Bank Transfer, Card, Other
        public string ReferenceNumber { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    public class Invoice
    {
        public long Id { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public long CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerCompany { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerAddress { get; set; } = string.Empty;
        public string CustomerGSTIN { get; set; } = string.Empty;
        public string BrokerName { get; set; } = string.Empty;

        public DateTime InvoiceDate { get; set; } = DateTime.Today;
        public DateTime DueDate { get; set; } = DateTime.Today.AddDays(15);
        public string PaymentTerms { get; set; } = "Net 15";
        public string TaxType { get; set; } = "GST_INTRA"; // GST_INTRA (CGST+SGST), GST_INTER (IGST), NONE

        public decimal Subtotal { get; set; } = 0m;
        public decimal ItemDiscountTotal { get; set; } = 0m;
        public decimal OverallDiscount { get; set; } = 0m;
        public decimal TaxableAmount { get; set; } = 0m;

        public decimal CgstRate { get; set; } = 9m;
        public decimal CgstAmount { get; set; } = 0m;
        public decimal SgstRate { get; set; } = 9m;
        public decimal SgstAmount { get; set; } = 0m;
        public decimal IgstRate { get; set; } = 18m;
        public decimal IgstAmount { get; set; } = 0m;
        public decimal TotalTax => CgstAmount + SgstAmount + IgstAmount;

        public decimal RoundOff { get; set; } = 0m;
        public decimal TotalQuantity { get; set; } = 0m;
        public decimal GrandTotal { get; set; } = 0m;
        public decimal AmountPaid { get; set; } = 0m;
        public decimal BalanceDue { get; set; } = 0m;

        public string Status { get; set; } = "Unpaid"; // Draft, Unpaid, Partially Paid, Paid, Overdue, Cancelled
        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public List<InvoiceItem> Items { get; set; } = new();
        public List<PaymentRecord> Payments { get; set; } = new();

        public string PaymentMode => Payments.Count > 0 ? Payments[0].PaymentMethod : (Status == "Paid" ? "Bank" : "Pending");
        public decimal TotalAmount => GrandTotal;

        public void RecalculateTotals()
        {
            Subtotal = 0m;
            ItemDiscountTotal = 0m;
            TotalQuantity = 0m;
            decimal totalTaxable = 0m;
            decimal totalTax = 0m;

            foreach (var item in Items)
            {
                TotalQuantity += item.Quantity;
                decimal itemRaw = item.Quantity * item.UnitPrice;
                decimal itemDisc = item.DiscountPercent > 0 
                    ? Math.Round(itemRaw * (item.DiscountPercent / 100m), 2)
                    : item.DiscountAmount;
                decimal itemNet = Math.Max(0m, itemRaw - itemDisc);
                decimal itemTax = Math.Round(itemNet * (item.TaxPercent / 100m), 2);

                item.DiscountAmount = itemDisc;
                item.TaxableAmount = itemNet;
                item.LineTotal = itemNet + itemTax;

                Subtotal += itemRaw;
                ItemDiscountTotal += itemDisc;
                totalTaxable += itemNet;
                totalTax += itemTax;
            }

            decimal netBeforeOverall = Math.Max(0m, Subtotal - ItemDiscountTotal);
            TaxableAmount = Math.Max(0m, netBeforeOverall - OverallDiscount);

            if (netBeforeOverall > 0 && OverallDiscount > 0)
            {
                decimal ratio = TaxableAmount / netBeforeOverall;
                totalTax = Math.Round(totalTax * ratio, 2);
            }

            if (TaxType == "GST_INTRA")
            {
                decimal halfRate = 9m;
                CgstRate = halfRate;
                SgstRate = halfRate;
                IgstRate = 0m;
                IgstAmount = 0m;

                decimal halfTax = Math.Round(totalTax / 2m, 2);
                CgstAmount = halfTax;
                SgstAmount = totalTax - halfTax;
            }
            else if (TaxType == "GST_INTER")
            {
                CgstRate = 0m;
                CgstAmount = 0m;
                SgstRate = 0m;
                SgstAmount = 0m;
                IgstRate = 18m;
                IgstAmount = totalTax;
            }
            else
            {
                CgstRate = 0m;
                CgstAmount = 0m;
                SgstRate = 0m;
                SgstAmount = 0m;
                IgstRate = 0m;
                IgstAmount = 0m;
            }

            decimal exactGrand = TaxableAmount + CgstAmount + SgstAmount + IgstAmount;
            decimal roundedGrand = Math.Round(exactGrand, 0, MidpointRounding.AwayFromZero);
            RoundOff = roundedGrand - exactGrand;
            GrandTotal = roundedGrand;
            BalanceDue = Math.Max(0m, GrandTotal - AmountPaid);


            // Determine status
            if (Status == "Cancelled") return;
            if (Status == "Draft") return;

            if (AmountPaid >= GrandTotal && GrandTotal > 0)
            {
                Status = "Paid";
                BalanceDue = 0m;
            }
            else if (AmountPaid > 0 && AmountPaid < GrandTotal)
            {
                Status = "Partially Paid";
            }
            else
            {
                if (DueDate < DateTime.Today && BalanceDue > 0)
                    Status = "Overdue";
                else
                    Status = "Unpaid";
            }
        }
    }

    public class SalesDashboardMetrics
    {
        public decimal TotalSales { get; set; } = 0m;
        public int TotalInvoicesCount { get; set; } = 0;
        public int PaidInvoicesCount { get; set; } = 0;
        public int UnpaidInvoicesCount { get; set; } = 0;
        public int OverdueInvoicesCount { get; set; } = 0;
        public decimal OutstandingBalance { get; set; } = 0m;
        public decimal TodaySales { get; set; } = 0m;
        public decimal ThisMonthSales { get; set; } = 0m;
        public List<Invoice> RecentInvoices { get; set; } = new();

        public string TotalSalesFormatted => $"₹{TotalSales:N2}";
        public string OutstandingDueFormatted => $"₹{OutstandingBalance:N2}";
        public int TotalInvoices => TotalInvoicesCount;
        public int PaidInvoices => PaidInvoicesCount;
        public int UnpaidInvoices => UnpaidInvoicesCount;
        public int TotalCustomers { get; set; } = 0;
    }

    public class MonthlySalesBarItem
    {
        public string MonthLabel { get; set; } = string.Empty;
        public int MonthNumber { get; set; }
        public decimal SalesAmount { get; set; } = 0m;
        public decimal PurchasesAmount { get; set; } = 0m;
    }

    public class CategorySalesDistribution
    {
        public string CategoryName { get; set; } = string.Empty;
        public decimal TotalSales { get; set; } = 0m;
        public double Percentage { get; set; } = 0;
        public float Angle { get; set; } = 0f;
    }

    public class PaymentTransactionItem
    {
        public long Id { get; set; }
        public long InvoiceId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; } = DateTime.Now;
        public decimal Amount { get; set; } = 0m;
        public string PaymentMethod { get; set; } = "Bank Transfer";
        public string ReferenceNumber { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }

    public class Supplier
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string GSTIN { get; set; } = string.Empty;
        public decimal OutstandingBalance { get; set; } = 0m;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string DisplayName => string.IsNullOrWhiteSpace(CompanyName) 
            ? Name 
            : $"{Name} ({CompanyName})";
    }

    public class PurchaseBill
    {
        public long Id { get; set; }
        public string BillNumber { get; set; } = string.Empty;
        public long SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public string SupplierCompany { get; set; } = string.Empty;
        public string SupplierPhone { get; set; } = string.Empty;
        public string SupplierGSTIN { get; set; } = string.Empty;
        public DateTime BillDate { get; set; } = DateTime.Today;
        public DateTime DueDate { get; set; } = DateTime.Today.AddDays(15);
        public string TaxType { get; set; } = "GST_INTRA";
        public decimal Subtotal { get; set; } = 0m;
        public decimal TaxableAmount { get; set; } = 0m;
        public decimal CgstAmount { get; set; } = 0m;
        public decimal SgstAmount { get; set; } = 0m;
        public decimal IgstAmount { get; set; } = 0m;
        public decimal GrandTotal { get; set; } = 0m;
        public decimal AmountPaid { get; set; } = 0m;
        public decimal BalanceDue { get; set; } = 0m;
        public string Status { get; set; } = "Unpaid";
        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public List<PurchaseBillItem> Items { get; set; } = new();

        public void RecalculateTotals()
        {
            Subtotal = 0m;
            foreach (var item in Items)
            {
                item.TotalAmount = item.Quantity * item.UnitCost;
                Subtotal += item.TotalAmount;
            }
            TaxableAmount = Subtotal;

            if (TaxType == "GST_INTRA")
            {
                CgstAmount = Math.Round(TaxableAmount * 0.09m, 2);
                SgstAmount = Math.Round(TaxableAmount * 0.09m, 2);
                IgstAmount = 0m;
            }
            else if (TaxType == "GST_INTER")
            {
                CgstAmount = 0m;
                SgstAmount = 0m;
                IgstAmount = Math.Round(TaxableAmount * 0.18m, 2);
            }
            else
            {
                CgstAmount = 0m;
                SgstAmount = 0m;
                IgstAmount = 0m;
            }

            GrandTotal = TaxableAmount + CgstAmount + SgstAmount + IgstAmount;
            BalanceDue = Math.Max(0m, GrandTotal - AmountPaid);

            if (AmountPaid >= GrandTotal && GrandTotal > 0)
                Status = "Paid";
            else if (AmountPaid > 0)
                Status = "Partially Paid";
            else
                Status = DueDate < DateTime.Today ? "Overdue" : "Unpaid";
        }
    }

    public class PurchaseBillItem
    {
        public long Id { get; set; }
        public long PurchaseBillId { get; set; }
        public long ProductId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public decimal Quantity { get; set; } = 1m;
        public decimal UnitCost { get; set; } = 0m;
        public decimal TaxPercent { get; set; } = 18m;
        public decimal TotalAmount { get; set; } = 0m;
    }

    public class PurchaseDashboardMetrics
    {
        public decimal TotalPurchases { get; set; } = 0m;
        public int TotalBillsCount { get; set; } = 0;
        public int PaidBillsCount { get; set; } = 0;
        public int UnpaidBillsCount { get; set; } = 0;
        public decimal TotalPayablesDue { get; set; } = 0m;
        public int TotalSuppliers { get; set; } = 0;
    }
}
