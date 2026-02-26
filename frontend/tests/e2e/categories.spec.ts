import { test, expect, Page } from '@playwright/test';

/**
 * Testes E2E — Categorias (CategoryList)
 *
 * Cobertura dos bugs corrigidos na Fase 6:
 * - Bug 3.3: CategoryList não tinha versão mobile — apenas table (quebrava layout)
 *
 * A CategoryList agora renderiza:
 * - Desktop (≥ 768px): <Table> com colunas Nome, Tipo, Status, Ações
 * - Mobile (< 768px): Cards com informações condensadas e botões de ação
 *
 * NOTA: Testes que requerem dados reais usam `test.skip`.
 */

// ─── Helpers ──────────────────────────────────────────────────────────────────

async function setDesktopViewport(page: Page) {
  await page.setViewportSize({ width: 1280, height: 800 });
}

async function setMobileViewport(page: Page) {
  await page.setViewportSize({ width: 390, height: 844 });
}

// ─── Protegido sem autenticação ───────────────────────────────────────────────

test.describe('CategoryList — Autenticação', () => {
  test('deve redirecionar /categories para /login sem sessão', async ({ page }) => {
    await page.context().clearCookies();
    await page.goto('/categories');
    await expect(page).toHaveURL(/\/login/);
  });
});

// ─── Testes com sessão ativa ──────────────────────────────────────────────────

test.describe('CategoryList — Renderização Desktop (Bug 3.3)', () => {
  test('deve exibir tabela com categorias em viewport desktop', async ({ page }) => {
    test.skip(true, 'Requer backend com sessão ativa e categorias cadastradas');

    await setDesktopViewport(page);
    await page.goto('/categories');
    await page.waitForLoadState('networkidle');

    // Desktop: tabela deve estar visível
    const table = page.getByRole('table');
    await expect(table).toBeVisible({ timeout: 10000 });

    // Colunas esperadas
    await expect(page.getByRole('columnheader', { name: /nome/i })).toBeVisible();
    await expect(page.getByRole('columnheader', { name: /tipo/i })).toBeVisible();
  });

  test('tabela não deve ter overflow horizontal em desktop', async ({ page }) => {
    test.skip(true, 'Requer backend com sessão ativa');

    await setDesktopViewport(page);
    await page.goto('/categories');
    await page.waitForLoadState('networkidle');

    const hasHorizontalScroll = await page.evaluate(
      () => document.body.scrollWidth > document.body.clientWidth
    );
    expect(hasHorizontalScroll).toBe(false);
  });
});

test.describe('CategoryList — Renderização Mobile (Bug 3.3)', () => {
  test('deve exibir cards de categoria em viewport mobile', async ({ page }) => {
    test.skip(true, 'Requer backend com sessão ativa e categorias cadastradas');

    await setMobileViewport(page);
    await page.goto('/categories');
    await page.waitForLoadState('networkidle');

    // Mobile: cards devem estar visíveis (tabela hidden)
    // Os cards têm botões de ação com aria-labels
    const editButtons = page.getByRole('button', { name: /editar/i });
    await expect(editButtons.first()).toBeVisible({ timeout: 10000 });
  });

  test('tabela deve estar oculta em viewport mobile', async ({ page }) => {
    test.skip(true, 'Requer backend com sessão ativa');

    await setMobileViewport(page);
    await page.goto('/categories');
    await page.waitForLoadState('networkidle');

    // A tabela tem classe `hidden md:block` — deve estar display:none em mobile
    const tableWrapper = page.locator('[class*="hidden"][class*="md"]').first();
    const tableDisplay = await tableWrapper.evaluate(
      (el) => window.getComputedStyle(el).display
    );
    expect(tableDisplay).toBe('none');
  });

  test('cards mobile devem ter botões Editar e Excluir para cada categoria', async ({
    page,
  }) => {
    test.skip(true, 'Requer backend com sessão ativa e categorias cadastradas');

    await setMobileViewport(page);
    await page.goto('/categories');
    await page.waitForLoadState('networkidle');

    // Cada card deve ter aria-label específico: "Editar {nome}" e "Excluir {nome}"
    const editButtons = page.getByRole('button', { name: /^Editar /i });
    const deleteButtons = page.getByRole('button', { name: /^Excluir /i });

    const editCount = await editButtons.count();
    const deleteCount = await deleteButtons.count();

    expect(editCount).toBeGreaterThan(0);
    expect(editCount).toBe(deleteCount); // Par de botões por categoria
  });

  test('mobile não deve ter overflow horizontal em viewport mobile', async ({ page }) => {
    test.skip(true, 'Requer backend com sessão ativa');

    await setMobileViewport(page);
    await page.goto('/categories');
    await page.waitForLoadState('networkidle');

    const hasHorizontalScroll = await page.evaluate(
      () => document.body.scrollWidth > document.body.clientWidth
    );
    expect(hasHorizontalScroll).toBe(false);
  });
});

test.describe('CategoryList — Ações (criar, editar, excluir)', () => {
  test('botão "Nova Categoria" deve abrir formulário', async ({ page }) => {
    test.skip(true, 'Requer backend com sessão ativa');

    await setDesktopViewport(page);
    await page.goto('/categories');
    await page.waitForLoadState('networkidle');

    const newButton = page.getByRole('button', { name: /nova categoria/i });
    await expect(newButton).toBeVisible({ timeout: 5000 });

    await newButton.click();

    // Formulário ou sheet de edição deve abrir
    const formHeading = page.getByRole('heading', { name: /nova categoria/i });
    await expect(formHeading).toBeVisible({ timeout: 3000 });
  });

  test('clicar em Excluir deve abrir diálogo de confirmação', async ({ page }) => {
    test.skip(true, 'Requer backend com sessão ativa e ao menos 1 categoria cadastrada');

    await setDesktopViewport(page);
    await page.goto('/categories');
    await page.waitForLoadState('networkidle');

    // Clicar no primeiro botão de excluir da lista
    const firstDeleteButton = page.getByRole('button', { name: /^Excluir /i }).first();
    await expect(firstDeleteButton).toBeVisible({ timeout: 5000 });
    await firstDeleteButton.click();

    // Diálogo de confirmação deve aparecer
    await expect(page.getByRole('dialog')).toBeVisible({ timeout: 2000 });
    await expect(page.getByText(/confirmar exclus/i)).toBeVisible();
    await expect(page.getByRole('button', { name: /cancelar/i })).toBeVisible();
    await expect(page.getByRole('button', { name: /excluir/i })).toBeVisible();
  });
});
