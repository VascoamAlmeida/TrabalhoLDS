const { defineConfig } = require('@playwright/test');

module.exports = defineConfig({
  testDir: './tests',
  workers: 1,
  timeout: 60 * 1000,

  use: {
    baseURL: process.env.E2E_BASE_URL || 'http://localhost:5079',
    headless: process.env.HEADED !== '1',
    ignoreHTTPSErrors: true,
    trace: 'retain-on-failure',
  },

  projects: [
    {
      name: 'chromium-desktop',
      use: { browserName: 'chromium' },
    },
  ],

  reporter: [
    ['list'],
    ['html', { outputFolder: 'playwright-report', open: 'never' }],
    ['json', { outputFile: 'playwright-report/results.json' }],
  ],

  outputDir: 'test-results',
});
