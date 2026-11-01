import { expect, type Locator, type Page } from '@playwright/test';

export class InvoiceAppPage {
  constructor(private readonly page: Page) {}

  async open() {
    await this.page.goto('/');
  }

  async signIn(username: string, password: string) {
    await this.page.getByLabel('Username').fill(username);
    await this.page.getByLabel('Password').fill(password);
    await this.page.getByRole('button', { name: 'Sign in' }).click();
    await expect(this.page.getByRole('heading', { name: 'Invoices', exact: true })).toBeVisible();
  }

  async createAndSubmitInvoice(customerName: string, amount: number) {
    await this.page.getByLabel('Customer name').fill(customerName);
    await this.page.getByLabel('Amount').fill(String(amount));
    await this.page.getByRole('button', { name: 'Create invoice' }).click();

    const row = this.invoiceRow(customerName);
    await row.getByRole('button', { name: /Submit invoice .* for approval/ }).click();
    await expect(row.getByRole('cell', { name: 'Pending Approval' })).toBeVisible();
  }

  async openApprovals() {
    await this.page.getByRole('tab', { name: 'Approvals' }).click();
  }

  async approve(customerName: string) {
    await this.pendingItem(customerName).getByRole('button', { name: /Approve invoice/ }).click();
  }

  pendingItem(customerName: string): Locator {
    return this.page.getByRole('listitem').filter({ hasText: customerName });
  }

  invoiceRow(customerName: string): Locator {
    return this.page.getByRole('row').filter({ hasText: customerName });
  }

  refusalMessage(): Locator {
    return this.page.getByRole('alert').filter({ hasText: 'limit' });
  }
}
