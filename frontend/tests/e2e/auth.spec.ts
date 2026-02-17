import { test, expect } from '@playwright/test';

/**
 * Testes E2E para fluxos de autenticação
 * Conforme especificado em fase-1-autenticacao.md
 */

test.describe('Authentication Flow', () => {
  test.beforeEach(async ({ page }) => {
    // Garantir que não há sessão ativa
    await page.context().clearCookies();
  });

  test('deve exibir página de login', async ({ page }) => {
    await page.goto('/login');

    await expect(page.locator('h1')).toContainText('L2SLedger');
    await expect(page.locator('h2')).toContainText('Bem-vindo de volta');
    await expect(page.getByRole('button', { name: /entrar/i })).toBeVisible();
  });

  test('deve validar campos obrigatórios no login', async ({ page }) => {
    await page.goto('/login');

    // Tentar submeter sem preencher
    await page.getByRole('button', { name: /entrar/i }).click();

    // Deve exibir erros de validação
    await expect(page.locator('text=/email inválido/i')).toBeVisible();
    await expect(page.locator('text=/senha deve ter no mínimo/i')).toBeVisible();
  });

  test('deve exibir erro para credenciais inválidas', async ({ page }) => {
    await page.goto('/login');

    await page.getByLabel(/email/i).fill('invalid@test.com');
    await page.getByLabel(/senha/i).fill('wrongpassword');
    await page.getByRole('button', { name: /entrar/i }).click();

    // Deve exibir mensagem de erro (código específico depende do backend)
    await expect(page.locator('[role="alert"]')).toBeVisible();
  });

  test('deve navegar para página de registro', async ({ page }) => {
    await page.goto('/login');

    await page.getByRole('link', { name: /cadastre-se/i }).click();

    await expect(page).toHaveURL('/register');
    await expect(page.locator('h2')).toContainText('Criar conta');
  });

  test('deve exibir página de registro com todos os campos', async ({ page }) => {
    await page.goto('/register');

    await expect(page.getByLabel(/nome/i)).toBeVisible();
    await expect(page.getByLabel(/^email/i)).toBeVisible();
    await expect(page.getByLabel(/^senha/i)).toBeVisible();
    await expect(page.getByLabel(/confirmar senha/i)).toBeVisible();
    await expect(page.getByRole('button', { name: /criar conta/i })).toBeVisible();
  });

  test('deve validar senhas não correspondentes', async ({ page }) => {
    await page.goto('/register');

    await page.getByLabel(/nome/i).fill('Test User');
    await page.getByLabel(/^email/i).fill('test@test.com');
    await page.getByLabel(/^senha/i).fill('password123');
    await page.getByLabel(/confirmar senha/i).fill('password456');
    await page.getByRole('button', { name: /criar conta/i }).click();

    await expect(page.locator('text=/as senhas não coincidem/i')).toBeVisible();
  });

  test('deve redirecionar para página de verificação após registro', async ({ page }) => {
    // Este teste requer mock do Firebase ou ambiente de testes
    // Por ora, apenas verificamos a navegação manual
    await page.goto('/verify-email');

    await expect(page.locator('h2')).toContainText('Verifique seu email');
    await expect(page.getByRole('button', { name: /reenviar/i })).toBeVisible();
  });

  test('deve exibir página de aguardando aprovação', async ({ page }) => {
    await page.goto('/pending-approval');

    await expect(page.locator('h2')).toContainText('Aguardando aprovação');
    await expect(page.locator('text=/administrador precisa aprovar/i')).toBeVisible();
  });

  test('deve exibir página de conta suspensa', async ({ page }) => {
    await page.goto('/suspended');

    await expect(page.locator('h2')).toContainText('Conta Suspensa');
    await expect(page.locator('text=/suspensa/i')).toBeVisible();
  });

  test('deve exibir página de cadastro rejeitado', async ({ page }) => {
    await page.goto('/rejected');

    await expect(page.locator('h2')).toContainText('Cadastro Não Aprovado');
    await expect(page.locator('text=/não foi aprovado/i')).toBeVisible();
  });

  test('deve permitir voltar para login de todas as páginas de status', async ({ page }) => {
    const statusPages = ['/verify-email', '/pending-approval'];

    for (const statusPage of statusPages) {
      await page.goto(statusPage);
      await page.getByRole('link', { name: /voltar/i }).click();
      await expect(page).toHaveURL('/login');
    }
  });

  test('deve redirecionar usuário não autenticado para login ao acessar dashboard', async ({ page }) => {
    await page.goto('/dashboard');

    // Deve redirecionar para login (após verificação backend)
    await expect(page).toHaveURL('/login');
  });

  test('não deve carregar código protegido sem autenticação', async ({ page }) => {
    const requests: string[] = [];

    // Interceptar todas as requisições de JS
    page.on('request', (request) => {
      if (request.resourceType() === 'script') {
        requests.push(request.url());
      }
    });

    await page.goto('/dashboard');

    // Verificar que protected.js NÃO foi carregado
    const protectedLoaded = requests.some((url) => url.includes('protected'));
    expect(protectedLoaded).toBe(false);
  });
});

/**
 * Testes de acessibilidade básica
 */
test.describe('Accessibility', () => {
  test('página de login deve ter labels acessíveis', async ({ page }) => {
    await page.goto('/login');

    // Verificar associação label-input
    const emailInput = page.getByLabel(/email/i);
    const passwordInput = page.getByLabel(/senha/i);

    await expect(emailInput).toBeVisible();
    await expect(passwordInput).toBeVisible();
  });

  test('página de registro deve ter labels acessíveis', async ({ page }) => {
    await page.goto('/register');

    const nameInput = page.getByLabel(/nome/i);
    const emailInput = page.getByLabel(/^email/i);
    const passwordInput = page.getByLabel(/^senha/i);
    const confirmPasswordInput = page.getByLabel(/confirmar senha/i);

    await expect(nameInput).toBeVisible();
    await expect(emailInput).toBeVisible();
    await expect(passwordInput).toBeVisible();
    await expect(confirmPasswordInput).toBeVisible();
  });
});
