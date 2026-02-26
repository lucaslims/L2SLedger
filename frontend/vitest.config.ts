import { defineConfig } from 'vitest/config';
import react from '@vitejs/plugin-react';
import path from 'path';

export default defineConfig({
  plugins: [react()],
  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: ['./tests/setup.ts'],
    css: true,
    exclude: ['**/node_modules/**', '**/e2e/**', '**/dist/**'],
    coverage: {
      provider: 'v8',
      reporter: ['text', 'json', 'html', 'lcov'],
      exclude: [
        'node_modules/',
        'tests/',
        '**/*.d.ts',
        '**/*.config.*',
        '**/mockData',
        '**/*.stories.tsx',
        'src/main.tsx',
        'src/vite-env.d.ts',
        // Storybook
        'storybook-static/**',
        '.storybook/**',
        // Pages (routing components only)
        'src/**/pages/**',
        // Routes (routing logic only)
        'src/app/routes/**',
        // Providers (context providers)
        'src/app/providers/**',
        'src/app/App.tsx',
        // Services (HTTP calls - integration tested)
        'src/**/services/**',
        // Layout components (composition only)
        'src/shared/components/layout/**',
        'src/shared/components/feedback/**',
        // Firebase config
        'src/shared/lib/firebase/**',
        // API client + error handling (integration tested)
        'src/shared/lib/api/client.ts',
        'src/shared/lib/api/endpoints.ts',
        'src/shared/lib/api/errors.ts',
        // Environment config
        'src/shared/lib/env.ts',
        // Index files (re-exports only)
        '**/index.ts',
        '**/index.tsx',
        // Config files
        '.eslintrc.cjs',
        // UI primitive components (library-provided, low ROI)
        'src/shared/components/ui/avatar.tsx',
        'src/shared/components/ui/calendar.tsx',
        'src/shared/components/ui/dropdown-menu.tsx',
        'src/shared/components/ui/separator.tsx',
        'src/shared/components/ui/sheet.tsx',
        'src/shared/components/ui/tooltip.tsx',
      ],
      thresholds: {
        lines: 70,
        functions: 75,
        branches: 75,
        statements: 70,
      },
    },
  },
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
    },
  },
});
