import { expect, test } from '@playwright/test'

test('admin entry point is reachable and starts authentication', async ({ page }) => {
  const response = await page.goto('/')
  expect(response?.ok()).toBe(true)
  await expect(page).toHaveURL(/localhost:5176|localhost:8080/)
  await expect(page.locator('body')).not.toBeEmpty()
})

test('API live health endpoint is reachable through the frontend proxy', async ({ request }) => {
  const response = await request.get('/api/health/live')
  expect(response.ok()).toBe(true)
})
