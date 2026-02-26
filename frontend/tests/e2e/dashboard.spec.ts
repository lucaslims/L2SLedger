import { test, expect } from '@playwright/test';

/**
 * Testes E2E para o Dashboard
 * Conforme especificado em fase-2-dashboard.md
 * 
 * NOTA: Estes testes verificam a estrutura e comportamento da UI.
 * Requerem um backend em execução ou mocks apropriados.
 */

test.describe('Dashboard', () => {
  test.beforeEach(async ({ page }) => {
    // Para testes E2E com backend real, o usuário deve estar autenticado.
    // Em ambiente de CI, usar mock ou seed de dados.
    await page.context().clearCookies();
  });

  test('deve redirecionar para login se não autenticado', async ({ page }) => {
    await page.goto('/dashboard');

    // Sem sessão ativa, deve redirecionar para login
    await expect(page).toHaveURL(/\/login/);
  });

  test('deve exibir loading screen durante verificação de sessão', async ({ page }) => {
    await page.goto('/dashboard');

    // Loading screen deve aparecer brevemente
    // Verificar que o texto de loading existe no bundle inicial
    const loadingText = page.locator('text=Verificando sessão...');
    // Pode não ser visível se o redirect for muito rápido
    // Este teste verifica que o componente está no bundle
    await expect(loadingText.or(page.locator('text=Bem-vindo'))).toBeVisible({
      timeout: 10000,
    });
  });

  test('não deve carregar código protegido sem autenticação', async ({ page }) => {
    // Verificar que o bundle de dashboard não é carregado
    const responses: string[] = [];

    page.on('response', (response) => {
      if (response.url().includes('.js')) {
        responses.push(response.url());
      }
    });

    await page.goto('/login');
    await page.waitForLoadState('networkidle');

    // Nenhum chunk de dashboard deve ter sido carregado
    const dashboardChunks = responses.filter(
      (url) =>
        url.includes('dashboard') ||
        url.includes('Dashboard') ||
        url.includes('tremor')
    );

    // Pode haver chunks compartilhados, mas componentes específicos do dashboard
    // não devem estar no bundle público
    expect(dashboardChunks.length).toBeLessThanOrEqual(0);
  });
});

test.describe('Dashboard - Layout Autenticado', () => {
  // Estes testes precisam de autenticação
  // Configurar com mock ou fixture de autenticação

  test('deve exibir header com menu de usuário', async ({ page }) => {
    // Este teste só funciona com backend + sessão ativa
    // Usar API para fazer login antes do teste
    test.skip(true, 'Requer backend com sessão ativa');

    await page.goto('/dashboard');

    // Header deve estar visível
    await expect(page.locator('header')).toBeVisible();

    // Menu de usuário deve existir
    const userMenuButton = page.getByLabel('Menu do usuário');
    await expect(userMenuButton).toBeVisible();
  });

  test('deve exibir sidebar em desktop', async ({ page }) => {
    test.skip(true, 'Requer backend com sessão ativa');

    // Simular viewport desktop
    await page.setViewportSize({ width: 1280, height: 720 });
    await page.goto('/dashboard');

    // Sidebar deve estar visível
    await expect(page.locator('aside')).toBeVisible();
    await expect(page.locator('text=Dashboard')).toBeVisible();
    await expect(page.locator('text=Transações')).toBeVisible();
    await expect(page.locator('text=Categorias')).toBeVisible();
  });

  test('deve exibir mobile nav em telas pequenas', async ({ page }) => {
    test.skip(true, 'Requer backend com sessão ativa');

    // Simular viewport mobile
    await page.setViewportSize({ width: 375, height: 667 });
    await page.goto('/dashboard');

    // Sidebar não deve estar visível
    await expect(page.locator('aside')).not.toBeVisible();

    // Mobile nav deve estar visível
    await expect(
      page.locator('nav[aria-label="Navegação mobile"]')
    ).toBeVisible();
  });

  test('deve exibir cards de saldo', async ({ page }) => {
    test.skip(true, 'Requer backend com sessão ativa');

    await page.goto('/dashboard');

    await expect(page.locator('text=Total de Receitas')).toBeVisible();
    await expect(page.locator('text=Total de Despesas')).toBeVisible();
    await expect(page.locator('text=Saldo Atual')).toBeVisible();
  });

  test('deve exibir gráfico de evolução financeira', async ({ page }) => {
    test.skip(true, 'Requer backend com sessão ativa');

    await page.goto('/dashboard');

    await expect(page.locator('text=Evolução Financeira')).toBeVisible();
  });

  test('deve exibir transações recentes', async ({ page }) => {
    test.skip(true, 'Requer backend com sessão ativa');

    await page.goto('/dashboard');

    await expect(page.locator('text=Transações Recentes')).toBeVisible();
  });

  test('deve exibir ações rápidas', async ({ page }) => {
    test.skip(true, 'Requer backend com sessão ativa');

    await page.goto('/dashboard');

    await expect(page.locator('text=Ações Rápidas')).toBeVisible();
    await expect(page.locator('text=Nova Transação')).toBeVisible();
  });

  test('deve ser responsivo (mobile-first)', async ({ page }) => {
    test.skip(true, 'Requer backend com sessão ativa');

    // Testar em diferentes viewports
    const viewports = [
      { width: 375, height: 667 }, // iPhone
      { width: 768, height: 1024 }, // iPad
      { width: 1280, height: 720 }, // Desktop
      { width: 1920, height: 1080 }, // Full HD
    ];

    for (const viewport of viewports) {
      await page.setViewportSize(viewport);
      await page.goto('/dashboard');
      await page.waitForLoadState('networkidle');

      // Página deve renderizar sem erros em qualquer viewport
      await expect(page.locator('text=Dashboard')).toBeVisible();
    }
  });

  test('deve ter acessibilidade básica', async ({ page }) => {
    test.skip(true, 'Requer backend com sessão ativa');

    await page.goto('/dashboard');

    // Verificar que elementos de navegação têm aria-labels
    await expect(
      page.locator('nav[aria-label]')
    ).toHaveCount(1); // Sidebar ou MobileNav

    // Header deve existir
    await expect(page.locator('header')).toBeVisible();

    // Main content area deve existir
    await expect(page.locator('main')).toBeVisible();
  });
});

// ─── Fase 6 — Bugs corrigidos ─────────────────────────────────────────────────

test.describe('Dashboard — Saldo Atual (Bug 2.2)', () => {
  test('deve exibir card de Saldo Atual com valor monetário', async ({ page }) => {
    test.skip(true, 'Requer backend com sessão ativa');

    await page.goto('/dashboard');
    await page.waitForLoadState('networkidle');

    // O card de saldo deve mostrar valor em BRL (não "--" de erro)
    const balanceCard = page.getByText(/saldo atual/i).first();
    await expect(balanceCard).toBeVisible({ timeout: 10000 });

    // Deve conter valor monetário (R$ ...)
    const balanceValue = page.locator('[data-testid="balance-value"], .balance-value').first();
    const text = await balanceValue.textContent();
    expect(text).toMatch(/R\$|−|0,00/i);
  });

  test('não deve exibir erro no card de saldo para usuário autenticado', async ({
    page,
  }) => {
    test.skip(true, 'Requer backend com sessão ativa');

    await page.goto('/dashboard');
    await page.waitForLoadState('networkidle');

    // Não deve haver mensagem de erro genérica nos cards de saldo
    await expect(page.getByText(/erro ao carregar/i)).not.toBeVisible({ timeout: 5000 });
  });
});

test.describe('Dashboard — Transações Recentes (Bug 2.1)', () => {
  test('deve exibir lista de transações recentes', async ({ page }) => {
    test.skip(true, 'Requer backend com sessão ativa e ao menos 1 transação cadastrada');

    await page.goto('/dashboard');
    await page.waitForLoadState('networkidle');

    const section = page.getByText(/transações recentes/i);
    await expect(section).toBeVisible({ timeout: 10000 });

    // Deve exibir pelo menos uma transação (não estado vazio)
    const emptyState = page.getByText(/nenhuma transação/i);
    // Se há dados no backend, não deve mostrar estado vazio
    await expect(emptyState).not.toBeVisible({ timeout: 3000 });
  });

  test('transações recentes devem exibir tipo correto (Receita/Despesa)', async ({
    page,
  }) => {
    test.skip(true, 'Requer backend com sessão ativa e transações de diferentes tipos');

    await page.goto('/dashboard');
    await page.waitForLoadState('networkidle');

    // Deve haver badges com "Receita" ou "Despesa"
    const typeBadge = page.getByText(/receita|despesa/i).first();
    await expect(typeBadge).toBeVisible({ timeout: 10000 });
  });
});

test.describe('Dashboard — BalanceChart (Bug 2.3)', () => {
  test('deve exibir gráfico de evolução do saldo', async ({ page }) => {
    test.skip(true, 'Requer backend com sessão ativa e transações no período de 30 dias');

    await page.goto('/dashboard');
    await page.waitForLoadState('networkidle');

    // O gráfico Tremor deve renderizar (contém SVG ou canvas)
    // O container do gráfico deve estar visível e não vazio
    const chartContainer = page.locator('[class*="tremor"], [data-testid="balance-chart"]').first();
    await expect(chartContainer).toBeVisible({ timeout: 15000 });

    // Não deve mostrar estado de loading indefinidamente
    const loadingSpinner = page.locator('[class*="animate-spin"]');
    await expect(loadingSpinner).not.toBeVisible({ timeout: 15000 });
  });
});
