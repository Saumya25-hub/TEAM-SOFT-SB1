using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using Microsoft.Data.Sqlite;
using PROJECT_SBV1.Models;

namespace PROJECT_SBV1.Services
{
    /// <summary>
    /// Thread-safe SQLite Data Access Layer for TEAM SOFT Sales & Billing Module.
    /// Manages persistent schema, ACID transactions, parameterized queries, and sales metrics.
    /// Database File: %LOCALAPPDATA%\TEAMSOFT\teamsoft_sales.db
    /// </summary>
    public static class DatabaseService
    {
        private static readonly string DbDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TEAMSOFT");

        public static readonly string DbPath = Path.Combine(DbDirectory, "teamsoft_sales.db");
        private static readonly string ConnectionString = $"Data Source={DbPath}";
        private static readonly object s_lock = new();

        public static void InitializeDatabase()
        {
            lock (s_lock)
            {
                if (!Directory.Exists(DbDirectory))
                {
                    Directory.CreateDirectory(DbDirectory);
                }

                using var conn = new SqliteConnection(ConnectionString);
                conn.Open();

                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    PRAGMA journal_mode = WAL;
                    PRAGMA foreign_keys = ON;

                    CREATE TABLE IF NOT EXISTS BusinessProfile (
                        Id INTEGER PRIMARY KEY CHECK (Id = 1),
                        BusinessName TEXT NOT NULL,
                        Address TEXT NOT NULL,
                        Phone TEXT NOT NULL,
                        Email TEXT NOT NULL,
                        GSTIN TEXT NOT NULL,
                        DefaultTerms TEXT NOT NULL,
                        UPIId TEXT NOT NULL,
                        BankDetails TEXT NOT NULL
                    );

                    CREATE TABLE IF NOT EXISTS Customers (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL,
                        CompanyName TEXT,
                        Phone TEXT,
                        Email TEXT,
                        BillingAddress TEXT,
                        GSTIN TEXT,
                        OutstandingBalance REAL DEFAULT 0,
                        CreatedAt TEXT NOT NULL
                    );

                    CREATE TABLE IF NOT EXISTS Invoices (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        InvoiceNumber TEXT NOT NULL UNIQUE,
                        CustomerId INTEGER NOT NULL,
                        CustomerName TEXT NOT NULL,
                        CustomerCompany TEXT,
                        CustomerPhone TEXT,
                        CustomerEmail TEXT,
                        CustomerAddress TEXT,
                        CustomerGSTIN TEXT,
                        InvoiceDate TEXT NOT NULL,
                        DueDate TEXT NOT NULL,
                        PaymentTerms TEXT NOT NULL,
                        TaxType TEXT NOT NULL,
                        Subtotal REAL NOT NULL,
                        ItemDiscountTotal REAL NOT NULL,
                        OverallDiscount REAL NOT NULL,
                        TaxableAmount REAL NOT NULL,
                        CgstRate REAL NOT NULL,
                        CgstAmount REAL NOT NULL,
                        SgstRate REAL NOT NULL,
                        SgstAmount REAL NOT NULL,
                        IgstRate REAL NOT NULL,
                        IgstAmount REAL NOT NULL,
                        GrandTotal REAL NOT NULL,
                        AmountPaid REAL NOT NULL,
                        BalanceDue REAL NOT NULL,
                        Status TEXT NOT NULL,
                        Notes TEXT,
                        CreatedAt TEXT NOT NULL,
                        FOREIGN KEY (CustomerId) REFERENCES Customers(Id) ON DELETE RESTRICT
                    );

                    CREATE TABLE IF NOT EXISTS InvoiceItems (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        InvoiceId INTEGER NOT NULL,
                        ServiceName TEXT NOT NULL,
                        Description TEXT,
                        Quantity REAL NOT NULL,
                        UnitPrice REAL NOT NULL,
                        DiscountPercent REAL NOT NULL,
                        DiscountAmount REAL NOT NULL,
                        TaxPercent REAL NOT NULL,
                        LineTotal REAL NOT NULL,
                        FOREIGN KEY (InvoiceId) REFERENCES Invoices(Id) ON DELETE CASCADE
                    );

                    CREATE TABLE IF NOT EXISTS Payments (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        InvoiceId INTEGER NOT NULL,
                        PaymentDate TEXT NOT NULL,
                        Amount REAL NOT NULL,
                        PaymentMethod TEXT NOT NULL,
                        ReferenceNumber TEXT,
                        Notes TEXT,
                        CreatedAt TEXT NOT NULL,
                        FOREIGN KEY (InvoiceId) REFERENCES Invoices(Id) ON DELETE CASCADE
                    );

                    CREATE INDEX IF NOT EXISTS idx_invoices_cust ON Invoices(CustomerId);
                    CREATE INDEX IF NOT EXISTS idx_invoices_status ON Invoices(Status);
                    CREATE INDEX IF NOT EXISTS idx_invoices_date ON Invoices(InvoiceDate);
                    CREATE INDEX IF NOT EXISTS idx_items_invoice ON InvoiceItems(InvoiceId);
                    CREATE INDEX IF NOT EXISTS idx_payments_invoice ON Payments(InvoiceId);
                ";
                cmd.ExecuteNonQuery();

                // Seed Default Business Profile if missing
                using (var checkCmd = conn.CreateCommand())
                {
                    checkCmd.CommandText = "SELECT COUNT(*) FROM BusinessProfile;";
                    long count = (long)(checkCmd.ExecuteScalar() ?? 0);
                    if (count == 0)
                    {
                        var profile = new BusinessProfile();
                        using var insertProfile = conn.CreateCommand();
                        insertProfile.CommandText = @"
                            INSERT INTO BusinessProfile (Id, BusinessName, Address, Phone, Email, GSTIN, DefaultTerms, UPIId, BankDetails)
                            VALUES (1, @name, @addr, @phone, @email, @gstin, @terms, @upi, @bank);
                        ";
                        insertProfile.Parameters.AddWithValue("@name", profile.BusinessName);
                        insertProfile.Parameters.AddWithValue("@addr", profile.Address);
                        insertProfile.Parameters.AddWithValue("@phone", profile.Phone);
                        insertProfile.Parameters.AddWithValue("@email", profile.Email);
                        insertProfile.Parameters.AddWithValue("@gstin", profile.GSTIN);
                        insertProfile.Parameters.AddWithValue("@terms", profile.DefaultTerms);
                        insertProfile.Parameters.AddWithValue("@upi", profile.UPIId);
                        insertProfile.Parameters.AddWithValue("@bank", profile.BankDetails);
                        insertProfile.ExecuteNonQuery();
                    }
                }

                // Seed initial service-based customers and sample invoices if database is fresh
                SeedInitialDataIfEmpty(conn);
            }
        }

        private static void SeedInitialDataIfEmpty(SqliteConnection conn)
        {
            using var countCmd = conn.CreateCommand();
            countCmd.CommandText = "SELECT COUNT(*) FROM Customers;";
            long custCount = (long)(countCmd.ExecuteScalar() ?? 0);
            if (custCount > 0) return;

            // Seed Customers
            var customers = new[]
            {
                new Customer { Name = "Rajesh Sharma", CompanyName = "Apex Cloud Solutions", Phone = "+91 98251 00112", Email = "rajesh@apexcloud.in", BillingAddress = "402, Navratna Corporate Park, Ambli, Ahmedabad", GSTIN = "24AABCA1234F1ZX" },
                new Customer { Name = "Anita Patel", CompanyName = "Zenith Digital Agency", Phone = "+91 94260 88231", Email = "anita@zenithagency.com", BillingAddress = "701, Iscon Elegance, Prahlad Nagar, Ahmedabad", GSTIN = "24BBAPA5678K1ZY" },
                new Customer { Name = "Vikram Mehta", CompanyName = "Mehta Global Logistics", Phone = "+91 98240 12345", Email = "vikram@mehtaglobal.com", BillingAddress = "12, Transport Nagar, Narol, Ahmedabad", GSTIN = "24CCAPM9012M1Z3" },
                new Customer { Name = "Kiran Joshi", CompanyName = "Starlight Brand Consultants", Phone = "+91 97245 44109", Email = "kiran@starlightbrand.com", BillingAddress = "G-14, Safal Pegasus, Anand Nagar, Ahmedabad", GSTIN = "24DDAPJ3456P1Z7" }
            };

            foreach (var c in customers)
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    INSERT INTO Customers (Name, CompanyName, Phone, Email, BillingAddress, GSTIN, OutstandingBalance, CreatedAt)
                    VALUES (@name, @comp, @phone, @email, @addr, @gstin, 0, @created);
                    SELECT last_insert_rowid();
                ";
                cmd.Parameters.AddWithValue("@name", c.Name);
                cmd.Parameters.AddWithValue("@comp", c.CompanyName);
                cmd.Parameters.AddWithValue("@phone", c.Phone);
                cmd.Parameters.AddWithValue("@email", c.Email);
                cmd.Parameters.AddWithValue("@addr", c.BillingAddress);
                cmd.Parameters.AddWithValue("@gstin", c.GSTIN);
                cmd.Parameters.AddWithValue("@created", DateTime.Now.ToString("o"));
                c.Id = (long)(cmd.ExecuteScalar() ?? 0);
            }

            // Seed Sample Invoices
            var sampleInvoices = new[]
            {
                new { Cust = customers[0], Num = "INV-2026-0001", Date = DateTime.Today.AddDays(-6), Due = DateTime.Today.AddDays(9), Status = "Paid",
                      Items = new[] { ("Enterprise Software License (Annual)", "TEAM SOFT ERP Cloud - 10 Users", 1m, 35000m, 0m, 18m),
                                      ("Cloud Migration & Setup Support", "Database migration & user onboarding", 1m, 10000m, 0m, 18m) },
                      Paid = 53100m, Method = "Bank Transfer", Ref = "NEFT-HDFC-992144" },

                new { Cust = customers[1], Num = "INV-2026-0002", Date = DateTime.Today.AddDays(-3), Due = DateTime.Today.AddDays(12), Status = "Partially Paid",
                      Items = new[] { ("Custom Web Application Architecture", "Microservices backend & API integration", 1m, 45000m, 10m, 18m),
                                      ("SSL & Domain Security Certification", "Wildcard SSL certificate 2-year provisioning", 1m, 6000m, 0m, 18m) },
                      Paid = 25000m, Method = "UPI", Ref = "UPI/261014/88912" },

                new { Cust = customers[2], Num = "INV-2026-0003", Date = DateTime.Today.AddDays(-1), Due = DateTime.Today.AddDays(14), Status = "Unpaid",
                      Items = new[] { ("Corporate Identity & UI/UX Design", "Figma design system & brand guidelines", 1m, 28000m, 0m, 18m),
                                      ("Digital Signatures (Class 3 DSC - 2 Yrs)", "Encrypted USB token with org verification", 2m, 2500m, 0m, 18m) },
                      Paid = 0m, Method = "", Ref = "" }
            };

            foreach (var s in sampleInvoices)
            {
                var inv = new Invoice
                {
                    InvoiceNumber = s.Num,
                    CustomerId = s.Cust.Id,
                    CustomerName = s.Cust.Name,
                    CustomerCompany = s.Cust.CompanyName,
                    CustomerPhone = s.Cust.Phone,
                    CustomerEmail = s.Cust.Email,
                    CustomerAddress = s.Cust.BillingAddress,
                    CustomerGSTIN = s.Cust.GSTIN,
                    InvoiceDate = s.Date,
                    DueDate = s.Due,
                    PaymentTerms = "Net 15",
                    TaxType = "GST_INTRA",
                    Status = s.Status
                };

                foreach (var it in s.Items)
                {
                    var item = new InvoiceItem
                    {
                        ServiceName = it.Item1,
                        Description = it.Item2,
                        Quantity = it.Item3,
                        UnitPrice = it.Item4,
                        DiscountPercent = it.Item5,
                        TaxPercent = it.Item6
                    };
                    item.CalculateLineTotal();
                    inv.Items.Add(item);
                }

                inv.AmountPaid = s.Paid;
                inv.RecalculateTotals();

                // Save to database
                using var insertInvCmd = conn.CreateCommand();
                insertInvCmd.CommandText = @"
                    INSERT INTO Invoices (InvoiceNumber, CustomerId, CustomerName, CustomerCompany, CustomerPhone, CustomerEmail, 
                        CustomerAddress, CustomerGSTIN, InvoiceDate, DueDate, PaymentTerms, TaxType, Subtotal, ItemDiscountTotal, 
                        OverallDiscount, TaxableAmount, CgstRate, CgstAmount, SgstRate, SgstAmount, IgstRate, IgstAmount, 
                        GrandTotal, AmountPaid, BalanceDue, Status, Notes, CreatedAt)
                    VALUES (@num, @cid, @cname, @ccomp, @cphone, @cemail, @caddr, @cgst, @idate, @ddate, @terms, @taxtype, 
                        @sub, @itemdisc, @ovrdisc, @taxable, @cgstr, @cgsta, @sgstr, @sgsta, @igstr, @igsta, 
                        @grand, @paid, @bal, @status, @notes, @created);
                    SELECT last_insert_rowid();
                ";
                insertInvCmd.Parameters.AddWithValue("@num", inv.InvoiceNumber);
                insertInvCmd.Parameters.AddWithValue("@cid", inv.CustomerId);
                insertInvCmd.Parameters.AddWithValue("@cname", inv.CustomerName);
                insertInvCmd.Parameters.AddWithValue("@ccomp", inv.CustomerCompany);
                insertInvCmd.Parameters.AddWithValue("@cphone", inv.CustomerPhone);
                insertInvCmd.Parameters.AddWithValue("@cemail", inv.CustomerEmail);
                insertInvCmd.Parameters.AddWithValue("@caddr", inv.CustomerAddress);
                insertInvCmd.Parameters.AddWithValue("@cgst", inv.CustomerGSTIN);
                insertInvCmd.Parameters.AddWithValue("@idate", inv.InvoiceDate.ToString("yyyy-MM-dd"));
                insertInvCmd.Parameters.AddWithValue("@ddate", inv.DueDate.ToString("yyyy-MM-dd"));
                insertInvCmd.Parameters.AddWithValue("@terms", inv.PaymentTerms);
                insertInvCmd.Parameters.AddWithValue("@taxtype", inv.TaxType);
                insertInvCmd.Parameters.AddWithValue("@sub", inv.Subtotal);
                insertInvCmd.Parameters.AddWithValue("@itemdisc", inv.ItemDiscountTotal);
                insertInvCmd.Parameters.AddWithValue("@ovrdisc", inv.OverallDiscount);
                insertInvCmd.Parameters.AddWithValue("@taxable", inv.TaxableAmount);
                insertInvCmd.Parameters.AddWithValue("@cgstr", inv.CgstRate);
                insertInvCmd.Parameters.AddWithValue("@cgsta", inv.CgstAmount);
                insertInvCmd.Parameters.AddWithValue("@sgstr", inv.SgstRate);
                insertInvCmd.Parameters.AddWithValue("@sgsta", inv.SgstAmount);
                insertInvCmd.Parameters.AddWithValue("@igstr", inv.IgstRate);
                insertInvCmd.Parameters.AddWithValue("@igsta", inv.IgstAmount);
                insertInvCmd.Parameters.AddWithValue("@grand", inv.GrandTotal);
                insertInvCmd.Parameters.AddWithValue("@paid", inv.AmountPaid);
                insertInvCmd.Parameters.AddWithValue("@bal", inv.BalanceDue);
                insertInvCmd.Parameters.AddWithValue("@status", inv.Status);
                insertInvCmd.Parameters.AddWithValue("@notes", inv.Notes);
                insertInvCmd.Parameters.AddWithValue("@created", DateTime.Now.ToString("o"));

                inv.Id = (long)(insertInvCmd.ExecuteScalar() ?? 0);

                foreach (var item in inv.Items)
                {
                    using var itemCmd = conn.CreateCommand();
                    itemCmd.CommandText = @"
                        INSERT INTO InvoiceItems (InvoiceId, ServiceName, Description, Quantity, UnitPrice, DiscountPercent, DiscountAmount, TaxPercent, LineTotal)
                        VALUES (@invid, @name, @desc, @qty, @price, @disc_pct, @disc_amt, @tax_pct, @total);
                    ";
                    itemCmd.Parameters.AddWithValue("@invid", inv.Id);
                    itemCmd.Parameters.AddWithValue("@name", item.ServiceName);
                    itemCmd.Parameters.AddWithValue("@desc", item.Description);
                    itemCmd.Parameters.AddWithValue("@qty", item.Quantity);
                    itemCmd.Parameters.AddWithValue("@price", item.UnitPrice);
                    itemCmd.Parameters.AddWithValue("@disc_pct", item.DiscountPercent);
                    itemCmd.Parameters.AddWithValue("@disc_amt", item.DiscountAmount);
                    itemCmd.Parameters.AddWithValue("@tax_pct", item.TaxPercent);
                    itemCmd.Parameters.AddWithValue("@total", item.LineTotal);
                    itemCmd.ExecuteNonQuery();
                }

                if (s.Paid > 0)
                {
                    using var payCmd = conn.CreateCommand();
                    payCmd.CommandText = @"
                        INSERT INTO Payments (InvoiceId, PaymentDate, Amount, PaymentMethod, ReferenceNumber, Notes, CreatedAt)
                        VALUES (@invid, @date, @amount, @method, @ref, @notes, @created);
                    ";
                    payCmd.Parameters.AddWithValue("@invid", inv.Id);
                    payCmd.Parameters.AddWithValue("@date", s.Date.ToString("yyyy-MM-dd HH:mm:ss"));
                    payCmd.Parameters.AddWithValue("@amount", s.Paid);
                    payCmd.Parameters.AddWithValue("@method", s.Method);
                    payCmd.Parameters.AddWithValue("@ref", s.Ref);
                    payCmd.Parameters.AddWithValue("@notes", "Initial payment on invoice creation");
                    payCmd.Parameters.AddWithValue("@created", DateTime.Now.ToString("o"));
                    payCmd.ExecuteNonQuery();
                }

                // Update customer outstanding balance
                using var custBalCmd = conn.CreateCommand();
                custBalCmd.CommandText = @"
                    UPDATE Customers 
                    SET OutstandingBalance = (
                        SELECT COALESCE(SUM(BalanceDue), 0) FROM Invoices WHERE CustomerId = @cid AND Status != 'Cancelled'
                    )
                    WHERE Id = @cid;
                ";
                custBalCmd.Parameters.AddWithValue("@cid", inv.CustomerId);
                custBalCmd.ExecuteNonQuery();
            }
        }

        public static BusinessProfile GetBusinessProfile()
        {
            lock (s_lock)
            {
                using var conn = new SqliteConnection(ConnectionString);
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT BusinessName, Address, Phone, Email, GSTIN, DefaultTerms, UPIId, BankDetails FROM BusinessProfile WHERE Id = 1;";
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return new BusinessProfile
                    {
                        BusinessName = reader.GetString(0),
                        Address = reader.GetString(1),
                        Phone = reader.GetString(2),
                        Email = reader.GetString(3),
                        GSTIN = reader.GetString(4),
                        DefaultTerms = reader.GetString(5),
                        UPIId = reader.GetString(6),
                        BankDetails = reader.GetString(7)
                    };
                }
                return new BusinessProfile();
            }
        }

        public static void SaveBusinessProfile(BusinessProfile profile)
        {
            lock (s_lock)
            {
                using var conn = new SqliteConnection(ConnectionString);
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    INSERT INTO BusinessProfile (Id, BusinessName, Address, Phone, Email, GSTIN, DefaultTerms, UPIId, BankDetails)
                    VALUES (1, @name, @addr, @phone, @email, @gstin, @terms, @upi, @bank)
                    ON CONFLICT(Id) DO UPDATE SET
                        BusinessName = excluded.BusinessName,
                        Address = excluded.Address,
                        Phone = excluded.Phone,
                        Email = excluded.Email,
                        GSTIN = excluded.GSTIN,
                        DefaultTerms = excluded.DefaultTerms,
                        UPIId = excluded.UPIId,
                        BankDetails = excluded.BankDetails;
                ";
                cmd.Parameters.AddWithValue("@name", profile.BusinessName);
                cmd.Parameters.AddWithValue("@addr", profile.Address);
                cmd.Parameters.AddWithValue("@phone", profile.Phone);
                cmd.Parameters.AddWithValue("@email", profile.Email);
                cmd.Parameters.AddWithValue("@gstin", profile.GSTIN);
                cmd.Parameters.AddWithValue("@terms", profile.DefaultTerms);
                cmd.Parameters.AddWithValue("@upi", profile.UPIId);
                cmd.Parameters.AddWithValue("@bank", profile.BankDetails);
                cmd.ExecuteNonQuery();
            }
        }

        public static List<Customer> GetCustomers(string search = "")
        {
            var list = new List<Customer>();
            lock (s_lock)
            {
                using var conn = new SqliteConnection(ConnectionString);
                conn.Open();
                using var cmd = conn.CreateCommand();
                if (string.IsNullOrWhiteSpace(search))
                {
                    cmd.CommandText = "SELECT Id, Name, CompanyName, Phone, Email, BillingAddress, GSTIN, OutstandingBalance, CreatedAt FROM Customers ORDER BY Name ASC;";
                }
                else
                {
                    cmd.CommandText = @"
                        SELECT Id, Name, CompanyName, Phone, Email, BillingAddress, GSTIN, OutstandingBalance, CreatedAt 
                        FROM Customers 
                        WHERE Name LIKE @q OR CompanyName LIKE @q OR Phone LIKE @q OR Email LIKE @q
                        ORDER BY Name ASC;
                    ";
                    cmd.Parameters.AddWithValue("@q", $"%{search}%");
                }

                using var r = cmd.ExecuteReader();
                while (r.Read())
                {
                    list.Add(new Customer
                    {
                        Id = r.GetInt64(0),
                        Name = r.GetString(1),
                        CompanyName = r.IsDBNull(2) ? "" : r.GetString(2),
                        Phone = r.IsDBNull(3) ? "" : r.GetString(3),
                        Email = r.IsDBNull(4) ? "" : r.GetString(4),
                        BillingAddress = r.IsDBNull(5) ? "" : r.GetString(5),
                        GSTIN = r.IsDBNull(6) ? "" : r.GetString(6),
                        OutstandingBalance = Convert.ToDecimal(r.GetDouble(7)),
                        CreatedAt = DateTime.TryParse(r.GetString(8), out var dt) ? dt : DateTime.Now
                    });
                }
            }
            return list;
        }

        public static Customer? GetCustomerById(long id)
        {
            lock (s_lock)
            {
                using var conn = new SqliteConnection(ConnectionString);
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT Id, Name, CompanyName, Phone, Email, BillingAddress, GSTIN, OutstandingBalance, CreatedAt FROM Customers WHERE Id = @id;";
                cmd.Parameters.AddWithValue("@id", id);
                using var r = cmd.ExecuteReader();
                if (r.Read())
                {
                    return new Customer
                    {
                        Id = r.GetInt64(0),
                        Name = r.GetString(1),
                        CompanyName = r.IsDBNull(2) ? "" : r.GetString(2),
                        Phone = r.IsDBNull(3) ? "" : r.GetString(3),
                        Email = r.IsDBNull(4) ? "" : r.GetString(4),
                        BillingAddress = r.IsDBNull(5) ? "" : r.GetString(5),
                        GSTIN = r.IsDBNull(6) ? "" : r.GetString(6),
                        OutstandingBalance = Convert.ToDecimal(r.GetDouble(7)),
                        CreatedAt = DateTime.TryParse(r.GetString(8), out var dt) ? dt : DateTime.Now
                    };
                }
            }
            return null;
        }

        public static Customer SaveCustomer(Customer c)
        {
            lock (s_lock)
            {
                using var conn = new SqliteConnection(ConnectionString);
                conn.Open();
                using var cmd = conn.CreateCommand();

                if (c.Id == 0)
                {
                    cmd.CommandText = @"
                        INSERT INTO Customers (Name, CompanyName, Phone, Email, BillingAddress, GSTIN, OutstandingBalance, CreatedAt)
                        VALUES (@name, @comp, @phone, @email, @addr, @gstin, @bal, @created);
                        SELECT last_insert_rowid();
                    ";
                    cmd.Parameters.AddWithValue("@name", c.Name.Trim());
                    cmd.Parameters.AddWithValue("@comp", c.CompanyName.Trim());
                    cmd.Parameters.AddWithValue("@phone", c.Phone.Trim());
                    cmd.Parameters.AddWithValue("@email", c.Email.Trim());
                    cmd.Parameters.AddWithValue("@addr", c.BillingAddress.Trim());
                    cmd.Parameters.AddWithValue("@gstin", c.GSTIN.Trim());
                    cmd.Parameters.AddWithValue("@bal", c.OutstandingBalance);
                    cmd.Parameters.AddWithValue("@created", DateTime.Now.ToString("o"));
                    c.Id = (long)(cmd.ExecuteScalar() ?? 0);
                }
                else
                {
                    cmd.CommandText = @"
                        UPDATE Customers 
                        SET Name = @name, CompanyName = @comp, Phone = @phone, Email = @email,
                            BillingAddress = @addr, GSTIN = @gstin
                        WHERE Id = @id;
                    ";
                    cmd.Parameters.AddWithValue("@id", c.Id);
                    cmd.Parameters.AddWithValue("@name", c.Name.Trim());
                    cmd.Parameters.AddWithValue("@comp", c.CompanyName.Trim());
                    cmd.Parameters.AddWithValue("@phone", c.Phone.Trim());
                    cmd.Parameters.AddWithValue("@email", c.Email.Trim());
                    cmd.Parameters.AddWithValue("@addr", c.BillingAddress.Trim());
                    cmd.Parameters.AddWithValue("@gstin", c.GSTIN.Trim());
                    cmd.ExecuteNonQuery();
                }
            }
            return c;
        }

        public static string GetNextInvoiceNumber()
        {
            lock (s_lock)
            {
                using var conn = new SqliteConnection(ConnectionString);
                conn.Open();
                using var cmd = conn.CreateCommand();
                int currentYear = DateTime.Today.Year;
                cmd.CommandText = "SELECT COUNT(*) FROM Invoices;";
                long count = (long)(cmd.ExecuteScalar() ?? 0);

                string prefix = $"INV-{currentYear}-";
                string candidate = $"{prefix}{(count + 1):D4}";

                // Ensure unique even if some invoices were deleted or created out of sequence
                while (true)
                {
                    using var checkCmd = conn.CreateCommand();
                    checkCmd.CommandText = "SELECT COUNT(*) FROM Invoices WHERE InvoiceNumber = @num;";
                    checkCmd.Parameters.AddWithValue("@num", candidate);
                    long exists = (long)(checkCmd.ExecuteScalar() ?? 0);
                    if (exists == 0) return candidate;

                    count++;
                    candidate = $"{prefix}{(count + 1):D4}";
                }
            }
        }

        public static List<Invoice> GetInvoices(string search = "", string statusFilter = "All", DateTime? fromDate = null, DateTime? toDate = null)
        {
            var list = new List<Invoice>();
            lock (s_lock)
            {
                using var conn = new SqliteConnection(ConnectionString);
                conn.Open();
                using var cmd = conn.CreateCommand();

                var whereClauses = new List<string>();
                if (!string.IsNullOrWhiteSpace(search))
                {
                    whereClauses.Add("(InvoiceNumber LIKE @q OR CustomerName LIKE @q OR CustomerCompany LIKE @q)");
                    cmd.Parameters.AddWithValue("@q", $"%{search.Trim()}%");
                }

                if (!string.IsNullOrWhiteSpace(statusFilter) && !statusFilter.Equals("All", StringComparison.OrdinalIgnoreCase))
                {
                    whereClauses.Add("Status = @status");
                    cmd.Parameters.AddWithValue("@status", statusFilter);
                }

                if (fromDate.HasValue)
                {
                    whereClauses.Add("InvoiceDate >= @from");
                    cmd.Parameters.AddWithValue("@from", fromDate.Value.ToString("yyyy-MM-dd"));
                }

                if (toDate.HasValue)
                {
                    whereClauses.Add("InvoiceDate <= @to");
                    cmd.Parameters.AddWithValue("@to", toDate.Value.ToString("yyyy-MM-dd"));
                }

                string where = whereClauses.Count > 0 ? "WHERE " + string.Join(" AND ", whereClauses) : "";
                cmd.CommandText = $@"
                    SELECT Id, InvoiceNumber, CustomerId, CustomerName, CustomerCompany, CustomerPhone, CustomerEmail, 
                           CustomerAddress, CustomerGSTIN, InvoiceDate, DueDate, PaymentTerms, TaxType, Subtotal, 
                           ItemDiscountTotal, OverallDiscount, TaxableAmount, CgstRate, CgstAmount, SgstRate, SgstAmount, 
                           IgstRate, IgstAmount, GrandTotal, AmountPaid, BalanceDue, Status, Notes, CreatedAt
                    FROM Invoices 
                    {where}
                    ORDER BY InvoiceDate DESC, Id DESC;
                ";

                using var r = cmd.ExecuteReader();
                while (r.Read())
                {
                    list.Add(ReadInvoice(r));
                }
            }
            return list;
        }

        public static Invoice? GetInvoiceById(long id)
        {
            lock (s_lock)
            {
                using var conn = new SqliteConnection(ConnectionString);
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    SELECT Id, InvoiceNumber, CustomerId, CustomerName, CustomerCompany, CustomerPhone, CustomerEmail, 
                           CustomerAddress, CustomerGSTIN, InvoiceDate, DueDate, PaymentTerms, TaxType, Subtotal, 
                           ItemDiscountTotal, OverallDiscount, TaxableAmount, CgstRate, CgstAmount, SgstRate, SgstAmount, 
                           IgstRate, IgstAmount, GrandTotal, AmountPaid, BalanceDue, Status, Notes, CreatedAt
                    FROM Invoices WHERE Id = @id;
                ";
                cmd.Parameters.AddWithValue("@id", id);
                using var r = cmd.ExecuteReader();
                if (r.Read())
                {
                    var inv = ReadInvoice(r);
                    r.Close();

                    // Load Items
                    using var itemCmd = conn.CreateCommand();
                    itemCmd.CommandText = @"
                        SELECT Id, InvoiceId, ServiceName, Description, Quantity, UnitPrice, DiscountPercent, DiscountAmount, TaxPercent, LineTotal
                        FROM InvoiceItems WHERE InvoiceId = @invid;
                    ";
                    itemCmd.Parameters.AddWithValue("@invid", inv.Id);
                    using var ir = itemCmd.ExecuteReader();
                    while (ir.Read())
                    {
                        inv.Items.Add(new InvoiceItem
                        {
                            Id = ir.GetInt64(0),
                            InvoiceId = ir.GetInt64(1),
                            ServiceName = ir.GetString(2),
                            Description = ir.IsDBNull(3) ? "" : ir.GetString(3),
                            Quantity = Convert.ToDecimal(ir.GetDouble(4)),
                            UnitPrice = Convert.ToDecimal(ir.GetDouble(5)),
                            DiscountPercent = Convert.ToDecimal(ir.GetDouble(6)),
                            DiscountAmount = Convert.ToDecimal(ir.GetDouble(7)),
                            TaxPercent = Convert.ToDecimal(ir.GetDouble(8)),
                            LineTotal = Convert.ToDecimal(ir.GetDouble(9))
                        });
                    }
                    ir.Close();

                    // Load Payments
                    using var payCmd = conn.CreateCommand();
                    payCmd.CommandText = @"
                        SELECT Id, InvoiceId, PaymentDate, Amount, PaymentMethod, ReferenceNumber, Notes, CreatedAt
                        FROM Payments WHERE InvoiceId = @invid ORDER BY PaymentDate ASC;
                    ";
                    payCmd.Parameters.AddWithValue("@invid", inv.Id);
                    using var pr = payCmd.ExecuteReader();
                    while (pr.Read())
                    {
                        inv.Payments.Add(new PaymentRecord
                        {
                            Id = pr.GetInt64(0),
                            InvoiceId = pr.GetInt64(1),
                            PaymentDate = DateTime.TryParse(pr.GetString(2), out var pDate) ? pDate : DateTime.Now,
                            Amount = Convert.ToDecimal(pr.GetDouble(3)),
                            PaymentMethod = pr.GetString(4),
                            ReferenceNumber = pr.IsDBNull(5) ? "" : pr.GetString(5),
                            Notes = pr.IsDBNull(6) ? "" : pr.GetString(6),
                            CreatedAt = DateTime.TryParse(pr.GetString(7), out var cDate) ? cDate : DateTime.Now
                        });
                    }

                    return inv;
                }
            }
            return null;
        }

        public static Invoice? GetInvoiceByNumber(string invoiceNumber)
        {
            lock (s_lock)
            {
                using var conn = new SqliteConnection(ConnectionString);
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT Id FROM Invoices WHERE InvoiceNumber = @num;";
                cmd.Parameters.AddWithValue("@num", invoiceNumber);
                var result = cmd.ExecuteScalar();
                if (result != null && long.TryParse(result.ToString(), out long id))
                {
                    return GetInvoiceById(id);
                }
                return null;
            }
        }

        public static Invoice SaveInvoice(Invoice inv, PaymentRecord? initialPayment = null)
        {
            if (string.IsNullOrWhiteSpace(inv.InvoiceNumber))
            {
                inv.InvoiceNumber = GetNextInvoiceNumber();
            }

            inv.RecalculateTotals();

            lock (s_lock)
            {
                using var conn = new SqliteConnection(ConnectionString);
                conn.Open();
                using var tx = conn.BeginTransaction();

                try
                {
                    if (inv.Id == 0)
                    {
                        using var cmd = conn.CreateCommand();
                        cmd.Transaction = tx;
                        cmd.CommandText = @"
                            INSERT INTO Invoices (InvoiceNumber, CustomerId, CustomerName, CustomerCompany, CustomerPhone, CustomerEmail, 
                                CustomerAddress, CustomerGSTIN, InvoiceDate, DueDate, PaymentTerms, TaxType, Subtotal, ItemDiscountTotal, 
                                OverallDiscount, TaxableAmount, CgstRate, CgstAmount, SgstRate, SgstAmount, IgstRate, IgstAmount, 
                                GrandTotal, AmountPaid, BalanceDue, Status, Notes, CreatedAt)
                            VALUES (@num, @cid, @cname, @ccomp, @cphone, @cemail, @caddr, @cgst, @idate, @ddate, @terms, @taxtype, 
                                @sub, @itemdisc, @ovrdisc, @taxable, @cgstr, @cgsta, @sgstr, @sgsta, @igstr, @igsta, 
                                @grand, @paid, @bal, @status, @notes, @created);
                            SELECT last_insert_rowid();
                        ";
                        SetInvoiceParameters(cmd, inv);
                        inv.Id = (long)(cmd.ExecuteScalar() ?? 0);
                    }
                    else
                    {
                        using var cmd = conn.CreateCommand();
                        cmd.Transaction = tx;
                        cmd.CommandText = @"
                            UPDATE Invoices SET 
                                CustomerId = @cid, CustomerName = @cname, CustomerCompany = @ccomp, CustomerPhone = @cphone, 
                                CustomerEmail = @cemail, CustomerAddress = @caddr, CustomerGSTIN = @cgst, InvoiceDate = @idate, 
                                DueDate = @ddate, PaymentTerms = @terms, TaxType = @taxtype, Subtotal = @sub, 
                                ItemDiscountTotal = @itemdisc, OverallDiscount = @ovrdisc, TaxableAmount = @taxable, 
                                CgstRate = @cgstr, CgstAmount = @cgsta, SgstRate = @sgstr, SgstAmount = @sgsta, 
                                IgstRate = @igstr, IgstAmount = @igsta, GrandTotal = @grand, AmountPaid = @paid, 
                                BalanceDue = @bal, Status = @status, Notes = @notes
                            WHERE Id = @id;
                        ";
                        cmd.Parameters.AddWithValue("@id", inv.Id);
                        SetInvoiceParameters(cmd, inv);
                        cmd.ExecuteNonQuery();

                        // Clear existing items for replacement
                        using var delItems = conn.CreateCommand();
                        delItems.Transaction = tx;
                        delItems.CommandText = "DELETE FROM InvoiceItems WHERE InvoiceId = @invid;";
                        delItems.Parameters.AddWithValue("@invid", inv.Id);
                        delItems.ExecuteNonQuery();
                    }

                    // Insert Invoice Items
                    foreach (var item in inv.Items)
                    {
                        item.InvoiceId = inv.Id;
                        using var itemCmd = conn.CreateCommand();
                        itemCmd.Transaction = tx;
                        itemCmd.CommandText = @"
                            INSERT INTO InvoiceItems (InvoiceId, ServiceName, Description, Quantity, UnitPrice, DiscountPercent, DiscountAmount, TaxPercent, LineTotal)
                            VALUES (@invid, @name, @desc, @qty, @price, @disc_pct, @disc_amt, @tax_pct, @total);
                        ";
                        itemCmd.Parameters.AddWithValue("@invid", inv.Id);
                        itemCmd.Parameters.AddWithValue("@name", item.ServiceName);
                        itemCmd.Parameters.AddWithValue("@desc", item.Description);
                        itemCmd.Parameters.AddWithValue("@qty", item.Quantity);
                        itemCmd.Parameters.AddWithValue("@price", item.UnitPrice);
                        itemCmd.Parameters.AddWithValue("@disc_pct", item.DiscountPercent);
                        itemCmd.Parameters.AddWithValue("@disc_amt", item.DiscountAmount);
                        itemCmd.Parameters.AddWithValue("@tax_pct", item.TaxPercent);
                        itemCmd.Parameters.AddWithValue("@total", item.LineTotal);
                        itemCmd.ExecuteNonQuery();
                    }

                    // Record initial payment if provided
                    if (initialPayment != null && initialPayment.Amount > 0)
                    {
                        initialPayment.InvoiceId = inv.Id;
                        using var payCmd = conn.CreateCommand();
                        payCmd.Transaction = tx;
                        payCmd.CommandText = @"
                            INSERT INTO Payments (InvoiceId, PaymentDate, Amount, PaymentMethod, ReferenceNumber, Notes, CreatedAt)
                            VALUES (@invid, @date, @amount, @method, @ref, @notes, @created);
                        ";
                        payCmd.Parameters.AddWithValue("@invid", inv.Id);
                        payCmd.Parameters.AddWithValue("@date", initialPayment.PaymentDate.ToString("yyyy-MM-dd HH:mm:ss"));
                        payCmd.Parameters.AddWithValue("@amount", initialPayment.Amount);
                        payCmd.Parameters.AddWithValue("@method", initialPayment.PaymentMethod);
                        payCmd.Parameters.AddWithValue("@ref", initialPayment.ReferenceNumber ?? "");
                        payCmd.Parameters.AddWithValue("@notes", initialPayment.Notes ?? "");
                        payCmd.Parameters.AddWithValue("@created", DateTime.Now.ToString("o"));
                        payCmd.ExecuteNonQuery();

                        // Refresh payment list & totals
                        inv.Payments.Add(initialPayment);
                        inv.AmountPaid += initialPayment.Amount;
                        inv.RecalculateTotals();

                        using var updatePaidCmd = conn.CreateCommand();
                        updatePaidCmd.Transaction = tx;
                        updatePaidCmd.CommandText = @"
                            UPDATE Invoices SET AmountPaid = @paid, BalanceDue = @bal, Status = @status WHERE Id = @id;
                        ";
                        updatePaidCmd.Parameters.AddWithValue("@id", inv.Id);
                        updatePaidCmd.Parameters.AddWithValue("@paid", inv.AmountPaid);
                        updatePaidCmd.Parameters.AddWithValue("@bal", inv.BalanceDue);
                        updatePaidCmd.Parameters.AddWithValue("@status", inv.Status);
                        updatePaidCmd.ExecuteNonQuery();
                    }

                    // Update customer balance
                    using var custBalCmd = conn.CreateCommand();
                    custBalCmd.Transaction = tx;
                    custBalCmd.CommandText = @"
                        UPDATE Customers 
                        SET OutstandingBalance = (
                            SELECT COALESCE(SUM(BalanceDue), 0) FROM Invoices WHERE CustomerId = @cid AND Status != 'Cancelled'
                        )
                        WHERE Id = @cid;
                    ";
                    custBalCmd.Parameters.AddWithValue("@cid", inv.CustomerId);
                    custBalCmd.ExecuteNonQuery();

                    tx.Commit();
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
            return inv;
        }

        public static void RecordPayment(PaymentRecord payment)
        {
            if (payment.Amount <= 0)
                throw new ArgumentException("Payment amount must be greater than zero.");

            lock (s_lock)
            {
                using var conn = new SqliteConnection(ConnectionString);
                conn.Open();
                using var tx = conn.BeginTransaction();

                try
                {
                    // Check invoice validity
                    using var checkCmd = conn.CreateCommand();
                    checkCmd.Transaction = tx;
                    checkCmd.CommandText = "SELECT CustomerId, GrandTotal, AmountPaid, Status FROM Invoices WHERE Id = @id;";
                    checkCmd.Parameters.AddWithValue("@id", payment.InvoiceId);
                    using var r = checkCmd.ExecuteReader();
                    if (!r.Read())
                    {
                        throw new InvalidOperationException("Invoice not found.");
                    }

                    long customerId = r.GetInt64(0);
                    decimal grandTotal = Convert.ToDecimal(r.GetDouble(1));
                    decimal currentPaid = Convert.ToDecimal(r.GetDouble(2));
                    string status = r.GetString(3);
                    r.Close();

                    if (status == "Cancelled")
                    {
                        throw new InvalidOperationException("Cannot record payment against a cancelled invoice.");
                    }

                    decimal newPaid = currentPaid + payment.Amount;
                    decimal newBalance = Math.Max(0m, grandTotal - newPaid);
                    string newStatus = newPaid >= grandTotal ? "Paid" : "Partially Paid";

                    // Insert payment record
                    using var insertPay = conn.CreateCommand();
                    insertPay.Transaction = tx;
                    insertPay.CommandText = @"
                        INSERT INTO Payments (InvoiceId, PaymentDate, Amount, PaymentMethod, ReferenceNumber, Notes, CreatedAt)
                        VALUES (@invid, @date, @amount, @method, @ref, @notes, @created);
                    ";
                    insertPay.Parameters.AddWithValue("@invid", payment.InvoiceId);
                    insertPay.Parameters.AddWithValue("@date", payment.PaymentDate.ToString("yyyy-MM-dd HH:mm:ss"));
                    insertPay.Parameters.AddWithValue("@amount", payment.Amount);
                    insertPay.Parameters.AddWithValue("@method", payment.PaymentMethod);
                    insertPay.Parameters.AddWithValue("@ref", payment.ReferenceNumber ?? "");
                    insertPay.Parameters.AddWithValue("@notes", payment.Notes ?? "");
                    insertPay.Parameters.AddWithValue("@created", DateTime.Now.ToString("o"));
                    insertPay.ExecuteNonQuery();

                    // Update invoice
                    using var updateInv = conn.CreateCommand();
                    updateInv.Transaction = tx;
                    updateInv.CommandText = @"
                        UPDATE Invoices SET AmountPaid = @paid, BalanceDue = @bal, Status = @status WHERE Id = @id;
                    ";
                    updateInv.Parameters.AddWithValue("@id", payment.InvoiceId);
                    updateInv.Parameters.AddWithValue("@paid", newPaid);
                    updateInv.Parameters.AddWithValue("@bal", newBalance);
                    updateInv.Parameters.AddWithValue("@status", newStatus);
                    updateInv.ExecuteNonQuery();

                    // Update customer balance
                    using var custBalCmd = conn.CreateCommand();
                    custBalCmd.Transaction = tx;
                    custBalCmd.CommandText = @"
                        UPDATE Customers 
                        SET OutstandingBalance = (
                            SELECT COALESCE(SUM(BalanceDue), 0) FROM Invoices WHERE CustomerId = @cid AND Status != 'Cancelled'
                        )
                        WHERE Id = @cid;
                    ";
                    custBalCmd.Parameters.AddWithValue("@cid", customerId);
                    custBalCmd.ExecuteNonQuery();

                    tx.Commit();
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }

        public static void CancelInvoice(long invoiceId)
        {
            lock (s_lock)
            {
                using var conn = new SqliteConnection(ConnectionString);
                conn.Open();
                using var tx = conn.BeginTransaction();
                try
                {
                    using var checkCmd = conn.CreateCommand();
                    checkCmd.Transaction = tx;
                    checkCmd.CommandText = "SELECT CustomerId, Status FROM Invoices WHERE Id = @id;";
                    checkCmd.Parameters.AddWithValue("@id", invoiceId);
                    using var r = checkCmd.ExecuteReader();
                    if (!r.Read()) throw new InvalidOperationException("Invoice not found.");
                    long customerId = r.GetInt64(0);
                    string status = r.GetString(1);
                    r.Close();

                    if (status == "Cancelled") return;

                    using var updateCmd = conn.CreateCommand();
                    updateCmd.Transaction = tx;
                    updateCmd.CommandText = "UPDATE Invoices SET Status = 'Cancelled', BalanceDue = 0 WHERE Id = @id;";
                    updateCmd.Parameters.AddWithValue("@id", invoiceId);
                    updateCmd.ExecuteNonQuery();

                    // Recalculate customer balance
                    using var custBalCmd = conn.CreateCommand();
                    custBalCmd.Transaction = tx;
                    custBalCmd.CommandText = @"
                        UPDATE Customers 
                        SET OutstandingBalance = (
                            SELECT COALESCE(SUM(BalanceDue), 0) FROM Invoices WHERE CustomerId = @cid AND Status != 'Cancelled'
                        )
                        WHERE Id = @cid;
                    ";
                    custBalCmd.Parameters.AddWithValue("@cid", customerId);
                    custBalCmd.ExecuteNonQuery();

                    tx.Commit();
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }

        public static SalesDashboardMetrics GetSalesDashboardMetrics()
        {
            var m = new SalesDashboardMetrics();
            lock (s_lock)
            {
                using var conn = new SqliteConnection(ConnectionString);
                conn.Open();

                // 1. Total Sales & Total Invoices (excluding cancelled)
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT 
                            COALESCE(SUM(GrandTotal), 0),
                            COUNT(*),
                            COALESCE(SUM(CASE WHEN Status = 'Paid' THEN 1 ELSE 0 END), 0),
                            COALESCE(SUM(CASE WHEN Status IN ('Unpaid', 'Draft') THEN 1 ELSE 0 END), 0),
                            COALESCE(SUM(CASE WHEN Status = 'Overdue' THEN 1 ELSE 0 END), 0),
                            COALESCE(SUM(CASE WHEN Status != 'Cancelled' THEN BalanceDue ELSE 0 END), 0)
                        FROM Invoices;
                    ";
                    using var r = cmd.ExecuteReader();
                    if (r.Read())
                    {
                        m.TotalSales = Convert.ToDecimal(r.GetDouble(0));
                        m.TotalInvoicesCount = r.GetInt32(1);
                        m.PaidInvoicesCount = r.GetInt32(2);
                        m.UnpaidInvoicesCount = r.GetInt32(3);
                        m.OverdueInvoicesCount = r.GetInt32(4);
                        m.OutstandingBalance = Convert.ToDecimal(r.GetDouble(5));
                    }
                }

                // 2. Today's Sales
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT COALESCE(SUM(GrandTotal), 0) 
                        FROM Invoices 
                        WHERE InvoiceDate = @today AND Status != 'Cancelled';
                    ";
                    cmd.Parameters.AddWithValue("@today", DateTime.Today.ToString("yyyy-MM-dd"));
                    m.TodaySales = Convert.ToDecimal(cmd.ExecuteScalar() ?? 0);
                }

                // 3. This Month's Sales
                using (var cmd = conn.CreateCommand())
                {
                    string firstDayOfMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).ToString("yyyy-MM-dd");
                    cmd.CommandText = @"
                        SELECT COALESCE(SUM(GrandTotal), 0) 
                        FROM Invoices 
                        WHERE InvoiceDate >= @firstDay AND Status != 'Cancelled';
                    ";
                    cmd.Parameters.AddWithValue("@firstDay", firstDayOfMonth);
                    m.ThisMonthSales = Convert.ToDecimal(cmd.ExecuteScalar() ?? 0);
                }

                // 4. Recent 10 Invoices
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, InvoiceNumber, CustomerId, CustomerName, CustomerCompany, CustomerPhone, CustomerEmail, 
                               CustomerAddress, CustomerGSTIN, InvoiceDate, DueDate, PaymentTerms, TaxType, Subtotal, 
                               ItemDiscountTotal, OverallDiscount, TaxableAmount, CgstRate, CgstAmount, SgstRate, SgstAmount, 
                               IgstRate, IgstAmount, GrandTotal, AmountPaid, BalanceDue, Status, Notes, CreatedAt
                        FROM Invoices 
                        ORDER BY InvoiceDate DESC, Id DESC 
                        LIMIT 10;
                    ";
                    using var r = cmd.ExecuteReader();
                    while (r.Read())
                    {
                        m.RecentInvoices.Add(ReadInvoice(r));
                    }
                }

                // 5. Total Customers Count
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT COUNT(*) FROM Customers;";
                    m.TotalCustomers = Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
                }
            }
            return m;
        }

        private static Invoice ReadInvoice(SqliteDataReader r)
        {
            return new Invoice
            {
                Id = r.GetInt64(0),
                InvoiceNumber = r.GetString(1),
                CustomerId = r.GetInt64(2),
                CustomerName = r.GetString(3),
                CustomerCompany = r.IsDBNull(4) ? "" : r.GetString(4),
                CustomerPhone = r.IsDBNull(5) ? "" : r.GetString(5),
                CustomerEmail = r.IsDBNull(6) ? "" : r.GetString(6),
                CustomerAddress = r.IsDBNull(7) ? "" : r.GetString(7),
                CustomerGSTIN = r.IsDBNull(8) ? "" : r.GetString(8),
                InvoiceDate = DateTime.TryParse(r.GetString(9), out var idate) ? idate : DateTime.Today,
                DueDate = DateTime.TryParse(r.GetString(10), out var ddate) ? ddate : DateTime.Today,
                PaymentTerms = r.GetString(11),
                TaxType = r.GetString(12),
                Subtotal = Convert.ToDecimal(r.GetDouble(13)),
                ItemDiscountTotal = Convert.ToDecimal(r.GetDouble(14)),
                OverallDiscount = Convert.ToDecimal(r.GetDouble(15)),
                TaxableAmount = Convert.ToDecimal(r.GetDouble(16)),
                CgstRate = Convert.ToDecimal(r.GetDouble(17)),
                CgstAmount = Convert.ToDecimal(r.GetDouble(18)),
                SgstRate = Convert.ToDecimal(r.GetDouble(19)),
                SgstAmount = Convert.ToDecimal(r.GetDouble(20)),
                IgstRate = Convert.ToDecimal(r.GetDouble(21)),
                IgstAmount = Convert.ToDecimal(r.GetDouble(22)),
                GrandTotal = Convert.ToDecimal(r.GetDouble(23)),
                AmountPaid = Convert.ToDecimal(r.GetDouble(24)),
                BalanceDue = Convert.ToDecimal(r.GetDouble(25)),
                Status = r.GetString(26),
                Notes = r.IsDBNull(27) ? "" : r.GetString(27),
                CreatedAt = DateTime.TryParse(r.GetString(28), out var cdate) ? cdate : DateTime.Now
            };
        }

        private static void SetInvoiceParameters(SqliteCommand cmd, Invoice inv)
        {
            cmd.Parameters.AddWithValue("@num", inv.InvoiceNumber);
            cmd.Parameters.AddWithValue("@cid", inv.CustomerId);
            cmd.Parameters.AddWithValue("@cname", inv.CustomerName);
            cmd.Parameters.AddWithValue("@ccomp", inv.CustomerCompany ?? "");
            cmd.Parameters.AddWithValue("@cphone", inv.CustomerPhone ?? "");
            cmd.Parameters.AddWithValue("@cemail", inv.CustomerEmail ?? "");
            cmd.Parameters.AddWithValue("@caddr", inv.CustomerAddress ?? "");
            cmd.Parameters.AddWithValue("@cgst", inv.CustomerGSTIN ?? "");
            cmd.Parameters.AddWithValue("@idate", inv.InvoiceDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@ddate", inv.DueDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@terms", inv.PaymentTerms);
            cmd.Parameters.AddWithValue("@taxtype", inv.TaxType);
            cmd.Parameters.AddWithValue("@sub", inv.Subtotal);
            cmd.Parameters.AddWithValue("@itemdisc", inv.ItemDiscountTotal);
            cmd.Parameters.AddWithValue("@ovrdisc", inv.OverallDiscount);
            cmd.Parameters.AddWithValue("@taxable", inv.TaxableAmount);
            cmd.Parameters.AddWithValue("@cgstr", inv.CgstRate);
            cmd.Parameters.AddWithValue("@cgsta", inv.CgstAmount);
            cmd.Parameters.AddWithValue("@sgstr", inv.SgstRate);
            cmd.Parameters.AddWithValue("@sgsta", inv.SgstAmount);
            cmd.Parameters.AddWithValue("@igstr", inv.IgstRate);
            cmd.Parameters.AddWithValue("@igsta", inv.IgstAmount);
            cmd.Parameters.AddWithValue("@grand", inv.GrandTotal);
            cmd.Parameters.AddWithValue("@paid", inv.AmountPaid);
            cmd.Parameters.AddWithValue("@bal", inv.BalanceDue);
            cmd.Parameters.AddWithValue("@status", inv.Status);
            cmd.Parameters.AddWithValue("@notes", inv.Notes ?? "");
            cmd.Parameters.AddWithValue("@created", DateTime.Now.ToString("o"));
        }
    }
}
