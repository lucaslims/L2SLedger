import { test, expect } from '@playwright/test';

/**
 * Testes E2E para gestão de usuários (Admin)
 * Conforme especificado em fase-5-admin-usuarios.md
 * 
 * NOTA: Muitos testes exigem backend ativo + sessão admin autenticada,
 * por isso são condicionados com test.skip.
 */

test.describe('Admin - User Management', () => {
  test.beforeEach(async ({ page }) => {
    await page.context().clearCookies();
  });

  test('deve redirecionar para login quando não autenticado', async ({ page }) => {
    await page.goto('/admin/users');

    // Sem autenticação, deve redirecionar para login
    await expect(page).toHaveURL(/\/login/);
  });

  test.describe('Authenticated Admin', () => {
    test.skip(true, 'Requer backend com sessão admin ativa');

    test('deve exibir página de gestão de usuários', async ({ page }) => {
      await page.goto('/admin/users');

      await expect(page.locator('h1')).toContainText('Gestão de Usuários');
      await expect(
        page.locator('text=Aprovar, suspender e gerenciar usuários do sistema')
      ).toBeVisible();
    });

    test('deve exibir filtro de status', async ({ page }) => {
      await page.goto('/admin/users');

      await expect(page.locator('[role="combobox"]')).toBeVisible();
    });

    test('deve filtrar usuários por status Pendente', async ({ page }) => {
      await page.goto('/admin/users');

      // Selecionar filtro Pendentes
      await page.locator('[role="combobox"]').click();
      await page.getByRole('option', { name: /pendentes/i }).click();

      // URL deve refletir o filtro
      await expect(page).toHaveURL(/status=Pending/);
    });

    test('deve exibir tabela com colunas corretas', async ({ page }) => {
      await page.goto('/admin/users');

      await expect(page.getByRole('columnheader', { name: 'Nome' })).toBeVisible();
      await expect(page.getByRole('columnheader', { name: 'Email' })).toBeVisible();
      await expect(page.getByRole('columnheader', { name: 'Status' })).toBeVisible();
      await expect(page.getByRole('columnheader', { name: 'Roles' })).toBeVisible();
    });

    test('deve navegar para detalhes do usuário', async ({ page }) => {
      await page.goto('/admin/users');

      // Clicar no primeiro botão de visualizar
      const viewButtons = page.getByRole('button').filter({ has: page.locator('svg') });
      await viewButtons.first().click();

      // Deve navegar para página de detalhes
      await expect(page).toHaveURL(/\/admin\/users\/.+/);
    });

    test('deve exibir detalhes do usuário', async ({ page }) => {
      await page.goto('/admin/users');

      const viewButtons = page.getByRole('button').filter({ has: page.locator('svg') });
      await viewButtons.first().click();

      await expect(page.locator('text=Informações')).toBeVisible();
      await expect(page.locator('text=Ações')).toBeVisible();
    });

    test('deve exibir alerta de usuários pendentes', async ({ page }) => {
      await page.goto('/admin/users');

      // Se houver pendentes, alerta deve ser visível
      const alert = page.locator('[role="alert"]');
      if (await alert.isVisible()) {
        await expect(alert).toContainText('aguardando aprovação');
        await expect(page.getByRole('button', { name: /ver pendentes/i })).toBeVisible();
      }
    });

    test('deve abrir dialog de aprovação para usuário pendente', async ({ page }) => {
      await page.goto('/admin/users?status=Pending');

      const viewButtons = page.getByRole('button').filter({ has: page.locator('svg') });
      if (await viewButtons.first().isVisible()) {
        await viewButtons.first().click();

        // Na página de detalhes, clicar em Aprovar/Rejeitar
        await page.getByRole('button', { name: /aprovar.*rejeitar/i }).click();

        // Dialog deve abrir
        await expect(page.getByRole('dialog')).toBeVisible();
        await expect(page.getByRole('button', { name: /^aprovar$/i })).toBeVisible();
        await expect(page.getByRole('button', { name: /^rejeitar$/i })).toBeVisible();
      }
    });

    test('deve exibir formulário de roles para usuário ativo', async ({ page }) => {
      await page.goto('/admin/users?status=Active');

      const viewButtons = page.getByRole('button').filter({ has: page.locator('svg') });
      if (await viewButtons.first().isVisible()) {
        await viewButtons.first().click();

        // Deve exibir checkboxes de roles
        await expect(page.getByRole('checkbox', { name: /admin/i })).toBeVisible();
        await expect(page.getByRole('checkbox', { name: /leitura/i })).toBeVisible();
        await expect(page.getByRole('checkbox', { name: /financeiro/i })).toBeVisible();
      }
    });

    test('deve proteger acesso para não-admins', async ({ page }) => {
      // Esse teste precisaria de um usuário com role Leitura/Financeiro
      // O AdminRoute deve redirecionar para /dashboard
      await page.goto('/admin/users');

      // Se não for admin, deve redirecionar
      await expect(page).not.toHaveURL('/admin/users');
    });
  });
});
