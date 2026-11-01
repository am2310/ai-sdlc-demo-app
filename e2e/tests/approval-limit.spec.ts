import { expect, test } from '@playwright/test';
import { InvoiceAppPage } from '../pages/invoiceApp.page';

test('a Manager is refused an over-limit approval and the invoice stays pending', async ({ page }) => {
  const app = new InvoiceAppPage(page);
  const customer = `Over Limit Co ${Date.now()}`;

  await app.open();
  await app.signIn('demo.manager', 'ChangeMe123!');
  await app.createAndSubmitInvoice(customer, 60000);

  await app.openApprovals();
  await app.approve(customer);

  await expect(app.refusalMessage()).toContainText('Manager limit of 50,000');
  await expect(app.pendingItem(customer)).toBeVisible();
});
