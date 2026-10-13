const API_BASE_URL = 'http://localhost:5000/api';

class ApiService {
  private token: string | null = null;
  private businessId: string | null = null;

  constructor() {
    this.token = localStorage.getItem('sb_auth_token');
    this.businessId = localStorage.getItem('sb_active_business_id');
  }

  setToken(token: string) {
    this.token = token;
    localStorage.setItem('sb_auth_token', token);
  }

  clearToken() {
    this.token = null;
    localStorage.removeItem('sb_auth_token');
  }

  setBusinessId(id: string) {
    this.businessId = id;
    localStorage.setItem('sb_active_business_id', id);
  }

  getBusinessId(): string | null {
    return this.businessId;
  }

  private async request<T = any>(endpoint: string, options: RequestInit = {}): Promise<T> {
    const headers: Record<string, string> = {
      'Content-Type': 'application/json',
      ...(options.headers as Record<string, string>),
    };

    if (this.token) {
      headers['Authorization'] = `Bearer ${this.token}`;
    }

    if (this.businessId) {
      headers['x-business-id'] = this.businessId;
    }

    const response = await fetch(`${API_BASE_URL}${endpoint}`, {
      ...options,
      headers,
    });

    const data = await response.json();
    if (!response.ok) {
      throw new Error(data.message || `Request failed with status ${response.status}`);
    }

    return data;
  }

  // Auth
  async login(email: string, password: string) {
    const res = await this.request('/auth/login', {
      method: 'POST',
      body: JSON.stringify({ email, password }),
    });
    if (res.token) {
      this.setToken(res.token);
      if (res.activeBusiness?.businessId) {
        this.setBusinessId(res.activeBusiness.businessId);
      }
    }
    return res;
  }

  async getMe() {
    return this.request('/auth/me');
  }

  // Business
  async getCurrentBusiness() {
    return this.request('/businesses/current');
  }

  async updateBusiness(data: any) {
    return this.request('/businesses/current', {
      method: 'PUT',
      body: JSON.stringify(data),
    });
  }

  // Dashboard
  async getDashboardOverview() {
    return this.request('/dashboard/overview');
  }

  // Products
  async getProducts(query?: string, barcode?: string) {
    let url = '/products';
    const params = new URLSearchParams();
    if (query) params.append('q', query);
    if (barcode) params.append('barcode', barcode);
    if (params.toString()) url += `?${params.toString()}`;
    return this.request(url);
  }

  async createProduct(product: any) {
    return this.request('/products', {
      method: 'POST',
      body: JSON.stringify(product),
    });
  }

  async adjustStock(productId: string, deltaQuantity: number, reason: string) {
    return this.request(`/products/${productId}/adjust-stock`, {
      method: 'POST',
      body: JSON.stringify({ deltaQuantity, reason }),
    });
  }

  // Customers
  async getCustomers(query?: string) {
    const url = query ? `/customers?q=${encodeURIComponent(query)}` : '/customers';
    return this.request(url);
  }

  async createCustomer(customer: any) {
    return this.request('/customers', {
      method: 'POST',
      body: JSON.stringify(customer),
    });
  }

  // Suppliers
  async getSuppliers(query?: string) {
    const url = query ? `/suppliers?q=${encodeURIComponent(query)}` : '/suppliers';
    return this.request(url);
  }

  async createSupplier(supplier: any) {
    return this.request('/suppliers', {
      method: 'POST',
      body: JSON.stringify(supplier),
    });
  }

  // Invoices & Billing
  async getInvoices(query?: string, status?: string) {
    let url = '/invoices';
    const params = new URLSearchParams();
    if (query) params.append('q', query);
    if (status) params.append('status', status);
    if (params.toString()) url += `?${params.toString()}`;
    return this.request(url);
  }

  async createInvoice(payload: any) {
    return this.request('/invoices', {
      method: 'POST',
      body: JSON.stringify(payload),
    });
  }

  // Purchases
  async getPurchases() {
    return this.request('/purchases');
  }

  async createPurchase(payload: any) {
    return this.request('/purchases', {
      method: 'POST',
      body: JSON.stringify(payload),
    });
  }

  // Transactions
  async getTransactions(method?: string) {
    const url = method ? `/transactions?method=${encodeURIComponent(method)}` : '/transactions';
    return this.request(url);
  }

  async recordTransaction(payload: any) {
    return this.request('/transactions', {
      method: 'POST',
      body: JSON.stringify(payload),
    });
  }

  // Reports
  async getSalesReport() {
    return this.request('/reports/sales');
  }

  async getInventoryReport() {
    return this.request('/reports/inventory');
  }

  // Audit Logs
  async getAuditLogs() {
    return this.request('/audit-logs');
  }
}

export const api = new ApiService();
