# Fase 1: Autenticação — Frontend L2SLedger

> **Estimativa:** 16 horas  
> **Dependência:** Backend com status de usuário implementado ✅  
> **Status:** Implementado

---

## 🎯 Objetivo

Implementar fluxo completo de autenticação com Firebase e integração com backend, incluindo:
- Login e registro
- Verificação de email
- Tratamento de status de usuário (Pending, Suspended, Rejected, Active)
- Guards de rota com lazy loading seguro
- Testes unitários e E2E

---

## 📋 Tasks Detalhadas

### 1.1 AuthProvider + Context ✅ (CONCLUÍDO NA FASE 0)

**Já implementado:**
- `app/providers/AuthProvider.tsx`
- `app/providers/useAuth.ts`
- Verificação backend-first (`/auth/me`)

### 1.2 Services de Autenticação

**Arquivo:** `features/auth/services/authService.ts`

```typescript
import { apiClient } from '@/shared/lib/api/client';
import { API_ENDPOINTS } from '@/shared/lib/api/endpoints';
import type { LoginRequest, LoginResponse } from '@/shared/types/api.types';

export const authService = {
  /**
   * Login com Firebase token
   */
  async login(firebaseToken: string): Promise<LoginResponse> {
    return apiClient.post<LoginResponse>(API_ENDPOINTS.AUTH_LOGIN, {
      firebaseToken,
    });
  },

  /**
   * Logout (limpa sessão no backend)
   */
  async logout(): Promise<void> {
    return apiClient.post(API_ENDPOINTS.AUTH_LOGOUT);
  },

  /**
   * Verificar sessão atual
   */
  async me(): Promise<CurrentUserResponse> {
    return apiClient.get<CurrentUserResponse>(API_ENDPOINTS.AUTH_ME);
  },
};
```

### 1.3 Hooks de Autenticação

#### `useLogin.ts`

```typescript
import { useMutation } from '@tanstack/react-query';
import { signInWithEmail, getIdToken } from '@/shared/lib/firebase';
import { authService } from '../services/authService';
import { queryClient } from '@/shared/lib/queryClient';
import { QUERY_KEYS } from '@/shared/lib/utils/constants';

export function useLogin() {
  return useMutation({
    mutationFn: async ({ email, password }: { email: string; password: string }) => {
      // 1. Login no Firebase
      const firebaseUser = await signInWithEmail(email, password);
      
      // 2. Verificar se email está verificado
      if (!firebaseUser.emailVerified) {
        throw new Error('AUTH_EMAIL_NOT_VERIFIED');
      }
      
      // 3. Obter ID token
      const firebaseToken = await getIdToken(firebaseUser);
      
      // 4. Login no backend
      return authService.login(firebaseToken);
    },
    onSuccess: () => {
      // Invalidar cache de usuário
      queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.AUTH] });
    },
  });
}
```

#### `useRegister.ts`

```typescript
import { useMutation } from '@tanstack/react-query';
import { signUpWithEmail, sendVerificationEmail } from '@/shared/lib/firebase';

export function useRegister() {
  return useMutation({
    mutationFn: async ({ email, password, displayName }: {
      email: string;
      password: string;
      displayName: string;
    }) => {
      // 1. Criar usuário no Firebase
      const firebaseUser = await signUpWithEmail(email, password);
      
      // 2. Atualizar displayName (se necessário)
      // await updateProfile(firebaseUser, { displayName });
      
      // 3. Enviar email de verificação
      await sendVerificationEmail(firebaseUser);
      
      return { email: firebaseUser.email };
    },
  });
}
```

#### `useLogout.ts`

```typescript
import { useMutation } from '@tanstack/react-query';
import { signOutUser } from '@/shared/lib/firebase';
import { authService } from '../services/authService';
import { queryClient } from '@/shared/lib/queryClient';
import { useNavigate } from 'react-router-dom';
import { ROUTES } from '@/shared/lib/utils/constants';

export function useLogout() {
  const navigate = useNavigate();

  return useMutation({
    mutationFn: async () => {
      // 1. Logout no backend
      await authService.logout();
      
      // 2. Logout no Firebase
      await signOutUser();
    },
    onSuccess: () => {
      // Limpar cache
      queryClient.clear();
      
      // Redirecionar para login
      navigate(ROUTES.LOGIN);
    },
  });
}
```

### 1.4 Componentes de Formulários

#### `LoginForm.tsx`

```typescript
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Button } from '@/shared/components/ui/button';
import { Input } from '@/shared/components/ui/input';
import { Form, FormField, FormItem, FormLabel, FormControl, FormMessage } from '@/shared/components/ui/form';
import { useLogin } from '../hooks/useLogin';
import { useNavigate } from 'react-router-dom';
import { ROUTES } from '@/shared/lib/utils/constants';

const loginSchema = z.object({
  email: z.string().email('Email inválido'),
  password: z.string().min(6, 'Senha deve ter no mínimo 6 caracteres'),
});

type LoginFormData = z.infer<typeof loginSchema>;

export function LoginForm() {
  const navigate = useNavigate();
  const { mutate: login, isPending, error } = useLogin();

  const form = useForm<LoginFormData>({
    resolver: zodResolver(loginSchema),
    defaultValues: {
      email: '',
      password: '',
    },
  });

  const onSubmit = (data: LoginFormData) => {
    login(data, {
      onSuccess: () => {
        navigate(ROUTES.DASHBOARD);
      },
    });
  };

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
        <FormField
          control={form.control}
          name="email"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Email</FormLabel>
              <FormControl>
                <Input type="email" placeholder="seu@email.com" {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <FormField
          control={form.control}
          name="password"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Senha</FormLabel>
              <FormControl>
                <Input type="password" placeholder="••••••" {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        {error && (
          <div className="rounded-md bg-destructive/10 p-3 text-sm text-destructive">
            {error.message}
          </div>
        )}

        <Button type="submit" className="w-full" disabled={isPending}>
          {isPending ? 'Entrando...' : 'Entrar'}
        </Button>
      </form>
    </Form>
  );
}
```

#### `RegisterForm.tsx`

Similar ao LoginForm, mas com campos adicionais (displayName, confirmPassword).

### 1.5 Páginas Completas

#### `LoginPage.tsx` (SUBSTITUIR PLACEHOLDER)

```typescript
import { LoginForm } from '../components/LoginForm';
import { Link } from 'react-router-dom';
import { ROUTES } from '@/shared/lib/utils/constants';

export default function LoginPage() {
  return (
    <div className="flex min-h-screen items-center justify-center bg-background px-4">
      <div className="w-full max-w-md space-y-6 rounded-lg border bg-card p-8 shadow-sm">
        <div className="text-center">
          <h1 className="text-3xl font-bold text-primary">L2SLedger</h1>
          <h2 className="mt-2 text-xl font-semibold">Bem-vindo de volta</h2>
          <p className="mt-2 text-sm text-muted-foreground">
            Entre com suas credenciais
          </p>
        </div>

        <LoginForm />

        <div className="text-center text-sm">
          <span className="text-muted-foreground">Não tem uma conta? </span>
          <Link to={ROUTES.REGISTER} className="text-primary hover:underline">
            Cadastre-se
          </Link>
        </div>
      </div>
    </div>
  );
}
```

#### `SuspendedPage.tsx` e `RejectedPage.tsx`

Páginas informativas para usuários com status específico.

### 1.6 Tratamento de Erros

**Arquivo:** `shared/lib/api/errors.ts`

```typescript
import { ApiError } from '@/shared/types/errors.types';
import { ROUTES } from '@/shared/lib/utils/constants';

export function handleAuthError(error: ApiError): void {
  switch (error.code) {
    case 'AUTH_EMAIL_NOT_VERIFIED':
      window.location.href = ROUTES.VERIFY_EMAIL;
      break;
    
    case 'AUTH_USER_PENDING':
      window.location.href = ROUTES.PENDING_APPROVAL;
      break;
    
    case 'AUTH_USER_SUSPENDED':
      window.location.href = ROUTES.SUSPENDED;
      break;
    
    case 'AUTH_USER_REJECTED':
      window.location.href = ROUTES.REJECTED;
      break;
    
    default:
      // Mostrar toast com erro
      console.error('Auth error:', error);
  }
}
```

---

## 🧪 Testes

### Testes Unitários (Vitest)

**Arquivo:** `features/auth/hooks/useLogin.test.ts`

```typescript
import { describe, it, expect, vi } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import { useLogin } from './useLogin';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';

const createWrapper = () => {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  });
  return ({ children }: { children: React.ReactNode }) => (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  );
};

describe('useLogin', () => {
  it('should login successfully', async () => {
    const { result } = renderHook(() => useLogin(), { wrapper: createWrapper() });
    
    result.current.mutate({ email: 'test@test.com', password: '123456' });
    
    await waitFor(() => expect(result.current.isSuccess).toBe(true));
  });

  it('should handle login error', async () => {
    // Mock error scenario
    const { result } = renderHook(() => useLogin(), { wrapper: createWrapper() });
    
    result.current.mutate({ email: 'invalid@test.com', password: 'wrong' });
    
    await waitFor(() => expect(result.current.isError).toBe(true));
  });
});
```

### Testes E2E (Playwright)

**Arquivo:** `tests/e2e/auth.spec.ts`

```typescript
import { test, expect } from '@playwright/test';

test.describe('Authentication', () => {
  test('should login successfully', async ({ page }) => {
    await page.goto('/login');
    
    await page.fill('input[name="email"]', 'test@test.com');
    await page.fill('input[name="password"]', 'password123');
    await page.click('button[type="submit"]');
    
    await expect(page).toHaveURL('/dashboard');
  });

  test('should show error for invalid credentials', async ({ page }) => {
    await page.goto('/login');
    
    await page.fill('input[name="email"]', 'wrong@test.com');
    await page.fill('input[name="password"]', 'wrongpass');
    await page.click('button[type="submit"]');
    
    await expect(page.locator('.text-destructive')).toBeVisible();
  });

  test('should redirect pending user to pending page', async ({ page }) => {
    // Login with pending user
    await page.goto('/login');
    await page.fill('input[name="email"]', 'pending@test.com');
    await page.fill('input[name="password"]', 'password123');
    await page.click('button[type="submit"]');
    
    await expect(page).toHaveURL('/pending-approval');
  });

  test('should not load protected code without auth', async ({ page }) => {
    // Interceptar requests
    const requests: string[] = [];
    page.on('request', (request) => {
      requests.push(request.url());
    });
    
    await page.goto('/dashboard');
    
    // Verificar que protected.js NÃO foi carregado
    const protectedLoaded = requests.some(url => url.includes('protected'));
    expect(protectedLoaded).toBe(false);
    
    // Deve redirecionar para login
    await expect(page).toHaveURL('/login');
  });
});
```

---

## 📦 Arquivos a Criar

```
features/auth/
├── components/
│   ├── LoginForm.tsx               ✅ CRIAR
│   ├── RegisterForm.tsx            ✅ CRIAR
│   ├── VerifyEmailCard.tsx         ✅ CRIAR
│   └── PendingApprovalCard.tsx     ✅ CRIAR
├── hooks/
│   ├── useLogin.ts                 ✅ CRIAR
│   ├── useRegister.ts              ✅ CRIAR
│   ├── useLogout.ts                ✅ CRIAR
│   └── useResendVerification.ts    ✅ CRIAR
├── pages/
│   ├── LoginPage.tsx               🔄 ATUALIZAR (substituir placeholder)
│   ├── RegisterPage.tsx            🔄 ATUALIZAR
│   ├── VerifyEmailPage.tsx         🔄 ATUALIZAR
│   ├── SuspendedPage.tsx           ✅ CRIAR
│   └── RejectedPage.tsx            ✅ CRIAR
├── services/
│   └── authService.ts              ✅ CRIAR
└── __tests__/
    ├── useLogin.test.ts            ✅ CRIAR
    ├── useRegister.test.ts         ✅ CRIAR
    └── LoginForm.test.tsx          ✅ CRIAR

shared/lib/api/
└── errors.ts                       ✅ CRIAR

shared/components/ui/
├── form.tsx                        ✅ INSTALAR (shadcn)
├── toast.tsx                       ✅ INSTALAR (shadcn)
└── sonner.tsx                      ✅ INSTALAR (shadcn)

tests/e2e/
└── auth.spec.ts                    ✅ CRIAR
```

---

## ✅ Critérios de Aceite (conforme approval-checklist.md)

### Funcionalidade
- [ ] Login funcional com Firebase + Backend
- [ ] Registro funcional com verificação de email
- [ ] Usuário pendente vê tela de aguardando
- [ ] Usuário suspenso/rejeitado vê tela apropriada
- [ ] Código protegido não carrega sem autenticação (verificar DevTools)
- [ ] Guards funcionando corretamente
- [ ] Logout limpa sessão no backend e Firebase
- [ ] Tratamento de erros por código semântico

### Testes (OBRIGATÓRIO)
- [ ] Testes unitários criados (≥85% cobertura)
- [ ] Testes de componentes criados
- [ ] Testes E2E passando (mínimo 5 cenários)
- [ ] Pipeline CI passou sem falhas

### Documentação
- [ ] Storybook com componentes de auth
- [ ] README atualizado se necessário

---

## 🔧 Comandos de Validação

```bash
# Testes unitários
npm test

# Testes E2E
npm run test:e2e

# Cobertura
npm run test:coverage

# Lint
npm run lint

# Build
npm run build

# Verificar bundles
npm run build && ls -lh dist/assets/
```

---

## 📚 Referências

- [ADR-001](../../adr/adr-001.md) — Firebase como IdP
- [ADR-002](../../adr/adr-002.md) — Fluxo de autenticação
- [ADR-021-A](../../adr/adr-021-a.md) — Códigos de erro
- [user-status-plan.md](../api-planning/user-status-plan.md)

---

---

## ⚠️ Considerações ADRs

### ADR-002 — Fluxo de Autenticação
- ✅ Firebase como IdP
- ✅ Backend valida token e cria sessão
- ✅ Cookie HttpOnly (credentials: 'include')
- ✅ Frontend não persiste tokens

### ADR-021-A — Códigos de Erro
- ✅ Tratamento por código semântico (não apenas HTTP status)
- ✅ Todos os 10 códigos AUTH_ cobertos

### ADR-040 — Testes de Frontend
- ✅ Testes unitários para hooks
- ✅ Testes de componentes com Testing Library
- ✅ Testes E2E para fluxos críticos

---

**Próximo passo:** Fase 2 — Dashboard
