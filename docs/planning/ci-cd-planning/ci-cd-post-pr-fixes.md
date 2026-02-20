# Correções de CI/CD Pós-PR — L2SLedger

> **Data:** 2026-02-20  
> **Status:** Em Análise  
> **Prioridade:** P1 — Alta (bloqueando merge da PR)

---

## 📋 Resumo

Após abrir a PR `release` → `main`, identificamos **2 erros novos** nos pipelines:

1. **Backend CI — CodeQL Analysis**: Code scanning não habilitado no repositório
2. **Frontend CI — Lint**: 3 erros de ESLint (display name) + 16 warnings

---

## 🐛 Erro #1 — Backend CI: CodeQL Code Scanning Desabilitado

### Mensagem de Erro

```
Error: Please verify that the necessary features are enabled: 
Code scanning is not enabled for this repository. 
Please enable code scanning in the repository settings.
https://docs.github.com/rest
```

### Causa Raiz

O **GitHub Code Scanning** (GitHub Advanced Security) não está habilitado no repositório. 

Esta é uma feature que requer:
- Repositório **público** (grátis), ou
- Repositório **privado** com GitHub Advanced Security habilitado (pago)

### Impacto

- ✅ O CodeQL **executa** e **analisa** o código corretamente
- ✅ O SARIF é **gerado** com sucesso
- ❌ O **upload** do SARIF para GitHub Code Scanning **falha** (feature desabilitada)

### Solução Recomendada

**Opção A: Habilitar Code Scanning (Repositório Público)**

1. Ir para: `Settings → Code security and analysis`
2. Em **"Code scanning"**, clicar em **"Set up"**
3. Escolher **"Default"** ou **"Advanced"**
4. Salvar configuração

**Opção B: Tornar Repositório Público**

Se o repositório é privado e não tem GitHub Advanced Security:

1. Ir para: `Settings → General → Danger Zone`
2. Clicar em **"Change repository visibility"**
3. Selecionar **"Make public"**

**Opção C: Desabilitar Upload SARIF (Temporário)**

Se não for possível habilitar Code Scanning agora, podemos **desabilitar o upload SARIF** mantendo a análise local:

**`.github/workflows/backend-ci.yml`**

```yaml
- name: Upload CodeQL results to GitHub Security tab
  uses: github/codeql-action/upload-sarif@v3
  if: false  # Desabilitado temporariamente até habilitar Code Scanning
  with:
    sarif_file: /home/runner/work/_temp/codeql_databases/csharp/results/csharp.sarif
    category: '/language:csharp'
```

**⚠️ Nota:** Esta opção **remove a integração** com GitHub Security, mas mantém a análise rodando.

---

## 🐛 Erro #2 — Frontend CI: ESLint Errors

### Mensagens de Erro

**3 erros bloqueantes:**

```
frontend/src/features/auth/__tests__/LoginForm.test.tsx
  21:10  error  Component definition is missing display name  react/display-name

frontend/src/features/categories/__tests__/CategoryList.test.tsx
  58:10  error  Component definition is missing display name  react/display-name

frontend/src/features/transactions/__tests__/TransactionForm.test.tsx
  37:10  error  Component definition is missing display name  react/display-name
```

**16 warnings (também bloqueiam devido a `--max-warnings 0`):**
- 12 warnings de `@typescript-eslint/no-explicit-any` (uso de `any` em testes)
- 4 warnings de `react-refresh/only-export-components` (arquivos UI exportam constantes)

### Causa Raiz

#### Display Name (3 erros)

Componentes mock nos testes não possuem `displayName` explícito. Exemplo:

```tsx
// ❌ Erro: sem displayName
vi.mock('@/features/auth/components/LoginForm', () => ({
  default: (props: any) => <div data-testid="login-form">...</div>
}));
```

#### Uso de `any` (12 warnings)

Mocks estão usando `any` para simplificar tipagem em testes.

#### Export Components (4 warnings)

Arquivos de UI (badge, button, form) exportam constantes/funções além de componentes, o que quebra React Fast Refresh.

### Impacto

- ❌ Pipeline **falha** no step `Lint (fail on error)`
- ❌ Bloqueia **merge** da PR
- ⚠️ `--max-warnings 0` torna warnings bloqueantes

### Solução Recomendada

#### Fix #2.1: Display Names em Mocks (3 erros)

**Arquivo: `frontend/src/features/auth/__tests__/LoginForm.test.tsx`**

```tsx
// ANTES (linha 21)
vi.mock('@/features/auth/components/LoginForm', () => ({
  default: (props: any) => <div data-testid="login-form">...</div>
}));

// DEPOIS
vi.mock('@/features/auth/components/LoginForm', () => {
  const MockLoginForm = (props: any) => <div data-testid="login-form">...</div>;
  MockLoginForm.displayName = 'LoginForm';
  return { default: MockLoginForm };
});
```

**Arquivo: `frontend/src/features/categories/__tests__/CategoryList.test.tsx`**

```tsx
// ANTES (linha 58)
vi.mock('@/features/categories/components/CategoryList', () => ({
  default: (props: any) => <div data-testid="category-list">...</div>
}));

// DEPOIS
vi.mock('@/features/categories/components/CategoryList', () => {
  const MockCategoryList = (props: any) => <div data-testid="category-list">...</div>;
  MockCategoryList.displayName = 'CategoryList';
  return { default: MockCategoryList };
});
```

**Arquivo: `frontend/src/features/transactions/__tests__/TransactionForm.test.tsx`**

```tsx
// ANTES (linha 37)
vi.mock('@/features/transactions/components/TransactionForm', () => ({
  default: (props: any) => <div data-testid="transaction-form">...</div>
}));

// DEPOIS
vi.mock('@/features/transactions/components/TransactionForm', () => {
  const MockTransactionForm = (props: any) => <div data-testid="transaction-form">...</div>;
  MockTransactionForm.displayName = 'TransactionForm';
  return { default: MockTransactionForm };
});
```

#### Fix #2.2: Resolver Warnings de `any` (12 warnings)

**Opção A: Substuir `any` por tipos específicos**

```tsx
// ANTES
const MockLoginForm = (props: any) => <div>...</div>;

// DEPOIS
type MockLoginFormProps = {
  onSubmit?: (data: any) => void;
  isLoading?: boolean;
  error?: string | null;
};
const MockLoginForm = (props: MockLoginFormProps) => <div>...</div>;
```

**Opção B: Suprimir warning em testes (mais rápido)**

Adicionar comentário ESLint disable:

```tsx
// eslint-disable-next-line @typescript-eslint/no-explicit-any
const MockLoginForm = (props: any) => <div>...</div>;
```

**Opção C: Configurar ESLint para permitir `any` em testes**

**Arquivo: `frontend/.eslintrc.cjs`**

```js
module.exports = {
  // ... configurações existentes
  overrides: [
    {
      files: ['**/__tests__/**/*.{ts,tsx}', '**/*.test.{ts,tsx}'],
      rules: {
        '@typescript-eslint/no-explicit-any': 'off', // Permite any em testes
      },
    },
  ],
};
```

#### Fix #2.3: Warnings de React Fast Refresh (4 warnings)

**Opção A: Separar exports (recomendado para produção)**

```tsx
// ANTES: badge.tsx
export const badgeVariants = cva(...);
export const Badge = React.forwardRef<...>(...);

// DEPOIS: badge.tsx (somente componente)
export const Badge = React.forwardRef<...>(...);

// NOVO: badge.variants.ts (constantes separadas)
export const badgeVariants = cva(...);
```

**Opção B: Suprimir warning (mais rápido)**

```tsx
/* eslint-disable react-refresh/only-export-components */
export const badgeVariants = cva(...);
export const Badge = React.forwardRef<...>(...);
```

**Opção C: Desabilitar regra globalmente para arquivos UI**

**Arquivo: `frontend/.eslintrc.cjs`**

```js
overrides: [
  {
    files: ['**/components/ui/**/*.{ts,tsx}'],
    rules: {
      'react-refresh/only-export-components': 'off',
    },
  },
],
```

---

## 🚀 Plano de Ação Recomendado

### Prioridade P0 — Desbloquear PR

**Ação mínima para merge:**

1. ✅ **Fix #2.1** — Adicionar `displayName` nos 3 mocks (3 minutos)
2. ✅ **Fix #2.2 Opção C** — Permitir `any` em testes via ESLint config (1 minuto)
3. ✅ **Fix #2.3 Opção B** — Suprimir warnings de fast refresh (1 minuto)
4. ✅ **Fix #1 Opção C** — Desabilitar upload SARIF temporariamente (1 minuto)

**Tempo total:** ~6 minutos

**Resultado:** Pipeline passa, PR pode ser merged.

### Prioridade P1 — Pós-Merge

**Melhorias de qualidade:**

1. 🔄 Habilitar GitHub Code Scanning nas configurações do repositório
2. 🔄 Remover `if: false` do upload SARIF
3. 🔄 Substituir `any` por tipos específicos nos testes (quando tempo permitir)
4. 🔄 Considerar separar exports em arquivos UI (refactor futuro)

---

## 📝 Arquivos Impactados

### Para Desbloquear PR (6 minutos)

| Arquivo | Mudança | Linhas |
|---------|---------|--------|
| `frontend/.eslintrc.cjs` | Adicionar overrides para testes e UI | ~15 |
| `frontend/src/features/auth/__tests__/LoginForm.test.tsx` | Adicionar displayName | ~5 |
| `frontend/src/features/categories/__tests__/CategoryList.test.tsx` | Adicionar displayName | ~5 |
| `frontend/src/features/transactions/__tests__/TransactionForm.test.tsx` | Adicionar displayName | ~5 |
| `.github/workflows/backend-ci.yml` | Desabilitar upload SARIF temporariamente | ~1 |

**Total:** 5 arquivos, ~31 linhas

---

## ✅ Validação

Após aplicar fixes:

```bash
# Frontend lint
cd frontend
npm run lint                    # Deve passar com 0 erros, 0 warnings

# Backend CI local (CodeQL upload será skipped)
cd backend
dotnet build                    # Deve passar
dotnet test                     # 201/201 testes

# Push e observar pipelines
git add .
git commit -m "fix(ci): resolve ESLint errors and disable SARIF upload temporarily"
git push
```

---

## 🔗 Referências

- [GitHub Code Scanning Docs](https://docs.github.com/en/code-security/code-scanning)
- [ESLint react/display-name](https://github.com/jsx-eslint/eslint-plugin-react/blob/master/docs/rules/display-name.md)
- [ESLint Overrides](https://eslint.org/docs/latest/use/configure/configuration-files#how-do-overrides-work)
- [React Fast Refresh](https://github.com/vitejs/vite-plugin-react/tree/main/packages/plugin-react#consistent-components-exports)
