import { test, expect } from '@playwright/test';

test.describe('Smoke Tests', () => {
  test.skip(!process.env.SHOPENGINE_RUN_SMOKE, 'Smoke tests are not enabled');

  test('should load the home page', async ({ page }) => {
    await page.goto('/');
    await expect(page).toHaveTitle(/.*SHOPENGINE.*/i);
  });
});
