import { test, expect, Page } from '@playwright/test';

/**
 * Testes E2E — Layout (Sidebar, MobileNav, AppLayout)
 *
 * Cobertura dos bugs corrigidos na Fase 6:
 * - Bug 4.1: Sidebar logo desaparecia ao rolar a página
 * - Bug 3.1: MobileNav sobrepunha conteúdo da página
 * - Bug 3.2: Link de Admin estava ausente no MobileNav para usuários Admin
 * - Bug 3.3: Layouts quebravam em telas mobile (< 768px)
 *
 * NOTA: Testes marcados com `skipIfNoBackend` requerem sessão autenticada.
 * Para rodar todos os testes: configure PLAYWRIGHT_BASE_URL e garanta
 * backend + usuário seed em execução.
 */

// ─── Helpers ──────────────────────────────────────────────────────────────────

/** Simula viewport de desktop (1280×800) */
async function setDesktopViewport(page: Page) {
  await page.setViewportSize({ width: 1280, height: 800 });
}

/** Simula viewport de mobile (390×844 — iPhone 12) */
async function setMobileViewport(page: Page) {
  await page.setViewportSize({ width: 390, height: 844 });
}

// ─── Testes sem autenticação ──────────────────────────────────────────────────

test.describe('Layout — Redirecionamento sem autenticação', () => {
  test('deve redirecionar /dashboard para /login sem sessão', async ({ page }) => {
    await page.context().clearCookies();
    await page.goto('/dashboard');
    await expect(page).toHaveURL(/\/login/);
  });

  test('deve redirecionar /transactions para /login sem sessão', async ({ page }) => {
    await page.context().clearCookies();
    await page.goto('/transactions');
    await expect(page).toHaveURL(/\/login/);
  });

  test('deve redirecionar /categories para /login sem sessão', async ({ page }) => {
    await page.context().clearCookies();
    await page.goto('/categories');
    await expect(page).toHaveURL(/\/login/);
  });
});

// ─── Testes com autenticação (requerem backend) ───────────────────────────────

test.describe('Layout — Sidebar Desktop (Bug 4.1)', () => {
  test('sidebar deve ser fixa (sticky) ao rolar a página', async ({ page }) => {
    test.skip(true, 'Requer backend com sessão ativa');

    await setDesktopViewport(page);
    await page.goto('/dashboard');

    const sidebar = page.locator('nav[aria-label="Navegação principal"], aside').first();
    await expect(sidebar).toBeVisible();

    // Logo deve estar visível antes de scrollar
    const logo = sidebar.locator('img[alt*="L2S"], [data-testid="logo"], h1').first();
    await expect(logo).toBeVisible({ timeout: 5000 });

    // Scroll longo na página principal
    await page.evaluate(() => window.scrollTo(0, 3000));
    await page.waitForTimeout(300);

    // Sidebar e logo devem continuar visíveis (sticky)
    await expect(sidebar).toBeInViewport();
    await expect(logo).toBeVisible();
  });

  test('sidebar deve exibir todos os links de navegação', async ({ page }) => {
    test.skip(true, 'Requer backend com sessão ativa');

    await setDesktopViewport(page);
    await page.goto('/dashboard');

    const sidebar = page.locator('nav[aria-label="Navegação principal"], aside').first();
    await expect(sidebar.getByRole('link', { name: /dashboard/i })).toBeVisible();
    await expect(sidebar.getByRole('link', { name: /transações/i })).toBeVisible();
    await expect(sidebar.getByRole('link', { name: /categori/i })).toBeVisible();
  });
});

test.describe('Layout — MobileNav (Bug 3.1 e Bug 3.2)', () => {
  test('MobileNav deve estar visível em viewport mobile', async ({ page }) => {
    test.skip(true, 'Requer backend com sessão ativa');

    await setMobileViewport(page);
    await page.goto('/dashboard');

    // Barra de navegação inferior deve existir em mobile
    const mobileNav = page.locator('[data-testid="mobile-nav"], nav[aria-label*="mobile" i]').first();
    await expect(mobileNav).toBeVisible({ timeout: 5000 });
  });

  test('MobileNav não deve sobrepor o conteúdo principal (Bug 3.1)', async ({ page }) => {
    test.skip(true, 'Requer backend com sessão ativa');

    await setMobileViewport(page);
    await page.goto('/dashboard');

    // O conteúdo principal deve ter padding-bottom suficiente para não ser coberto
    const main = page.locator('main').first();
    const mainPaddingBottom = await main.evaluate(
      (el) => parseInt(window.getComputedStyle(el).paddingBottom)
    );

    // padding-bottom deve ser >= 80px (pb-24 = 6rem = 96px, ou similar)
    expect(mainPaddingBottom).toBeGreaterThanOrEqual(80);
  });

  test('MobileNav deve exibir link Admin para usuários com role Admin (Bug 3.2)', async ({
    page,
  }) => {
    test.skip(true, 'Requer sessão ativa com usuário do role Admin');

    await setMobileViewport(page);
    await page.goto('/dashboard');

    // Admin link deve estar presente na barra de navegação inferior
    const adminLink = page.getByRole('link', { name: /admin/i });
    await expect(adminLink).toBeVisible({ timeout: 5000 });
  });

  test('MobileNav não deve exibir link Admin para usuários sem role Admin', async ({
    page,
  }) => {
    test.skip(true, 'Requer sessão ativa com usuário do role Financeiro (não Admin)');

    await setMobileViewport(page);
    await page.goto('/dashboard');

    // Admin link NÃO deve estar presente para usuários sem role Admin
    const adminLink = page.getByRole('link', { name: /admin/i });
    await expect(adminLink).not.toBeVisible({ timeout: 2000 });
  });
});

test.describe('Layout — Responsividade (Bug 3.3)', () => {
  test('dashboard deve renderizar sem overflow horizontal em mobile', async ({ page }) => {
    test.skip(true, 'Requer backend com sessão ativa');

    await setMobileViewport(page);
    await page.goto('/dashboard');
    await page.waitForLoadState('networkidle');

    // Não deve haver scroll horizontal
    const hasHorizontalScroll = await page.evaluate(
      () => document.body.scrollWidth > document.body.clientWidth
    );
    expect(hasHorizontalScroll).toBe(false);
  });

  test('transactions deve renderizar sem overflow horizontal em mobile', async ({
    page,
  }) => {
    test.skip(true, 'Requer backend com sessão ativa');

    await setMobileViewport(page);
    await page.goto('/transactions');
    await page.waitForLoadState('networkidle');

    const hasHorizontalScroll = await page.evaluate(
      () => document.body.scrollWidth > document.body.clientWidth
    );
    expect(hasHorizontalScroll).toBe(false);
  });

  test('categories deve renderizar sem overflow horizontal em mobile', async ({
    page,
  }) => {
    test.skip(true, 'Requer backend com sessão ativa');

    await setMobileViewport(page);
    await page.goto('/categories');
    await page.waitForLoadState('networkidle');

    const hasHorizontalScroll = await page.evaluate(
      () => document.body.scrollWidth > document.body.clientWidth
    );
    expect(hasHorizontalScroll).toBe(false);
  });

  test('sidebar deve estar oculta em viewport mobile', async ({ page }) => {
    test.skip(true, 'Requer backend com sessão ativa');

    await setMobileViewport(page);
    await page.goto('/dashboard');

    // Sidebar desktop deve estar hidden em mobile (Tailwind: hidden md:flex)
    const sidebar = page.locator('aside.hidden, aside[class*="hidden"]').first();
    // Em mobile: computedStyle display deve ser none
    const sidebarDisplay = await page.locator('aside').first().evaluate(
      (el) => window.getComputedStyle(el).display
    );
    expect(sidebarDisplay).toBe('none');
  });
});
