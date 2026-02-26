import { defineConfig, devices } from '@playwright/test';

/**
 * Playwright E2E Test Configuration — L2SLedger Frontend
 *
 * Requer:
 * - Backend em execução em http://localhost:5000 (ou via Docker)
 * - Frontend em execução em http://localhost:3000 (ou via Docker)
 *
 * Para rodar localmente:
 *   npm run dev   (em terminal separado)
 *   npx playwright test
 *
 * Em CI: usa PLAYWRIGHT_BASE_URL e APP_URL do ambiente.
 */
export default defineConfig({
  testDir: './tests/e2e',
  fullyParallel: false,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? 1 : undefined,
  reporter: process.env.CI ? 'github' : 'html',
  timeout: 30_000,
  use: {
    baseURL: process.env.PLAYWRIGHT_BASE_URL ?? 'http://localhost:3000',
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
    video: 'retain-on-failure',
  },
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
    {
      name: 'mobile-chrome',
      use: { ...devices['Pixel 5'] },
    },
  ],
  // Inicia o servidor de desenvolvimento automaticamente (apenas fora de CI)
  webServer: process.env.CI
    ? undefined
    : {
        command: 'npm run dev',
        url: 'http://localhost:3000',
        reuseExistingServer: true,
        timeout: 60_000,
      },
});


