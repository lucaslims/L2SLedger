# SPEC.md — Frontend L2SLedger

> **Data:** 2026-01-27  
> **Versão:** 1.1  
> **Status:** Aprovado  
> **Dependência:** [user-status-plan.md](../api-planning/user-status-plan.md), [ADR-021-A](../../adr/adr-021-a.md)

---

## 📋 Índice

1. [Visão Geral](#1-visão-geral)
2. [Stack Tecnológica](#2-stack-tecnológica)
3. [Design System](#3-design-system)
4. [Estrutura de Pastas](#4-estrutura-de-pastas)
5. [Arquitetura de Segurança](#5-arquitetura-de-segurança)
6. [Fluxos de Autenticação](#6-fluxos-de-autenticação)
7. [Fases de Implementação](#7-fases-de-implementação)
8. [Convenções de Código](#8-convenções-de-código)
9. [Estratégia de Testes](#9-estratégia-de-testes)
10. [Infraestrutura e Deploy](#10-infraestrutura-e-deploy)
11. [Roadmap Pós-MVP](#11-roadmap-pós-mvp)

---

## 1. Visão Geral

### 1.1 Propósito

O frontend do L2SLedger é uma **Single Page Application (SPA)** construída com React e TypeScript, responsável por fornecer uma interface de usuário para o sistema de controle financeiro.

### 1.2 Princípios Arquiteturais

| Princípio | Descrição |
|-----------|-----------|
| **Backend é a Verdade** | Toda lógica financeira reside no backend |
| **Frontend Stateless** | Não armazena tokens; usa cookies HttpOnly |
| **Fail-Fast** | Erros são semânticos e exibidos imediatamente |
| **Contratos Imutáveis** | Consome API contracts versionados |
| **Security by Default** | Código protegido não carrega sem autenticação |

### 1.3 Diagrama de Arquitetura

```
┌─────────────────────────────────────────────────────────────────┐
│                        CLOUDFLARE (CDN)                         │
│                   Cache, Proteção DDoS, SSL                     │
└─────────────────────────────────────────────────────────────────┘
                                 │
                                 ▼
┌─────────────────────────────────────────────────────────────────┐
│                    OCI OBJECT STORAGE                           │
│                  (Arquivos Estáticos)                           │
└─────────────────────────────────────────────────────────────────┘
                                 │
┌─────────────────────────────────────────────────────────────────┐
│                      FRONTEND SPA                                │
│  ┌───────────┐  ┌───────────┐  ┌───────────┐  ┌───────────┐    │
│  │   Auth    │  │ Dashboard │  │Transactions│  │   Admin   │    │
│  └───────────┘  └───────────┘  └───────────┘  └───────────┘    │
│                         │                                        │
│              ┌──────────┴──────────┐                            │
│              │   Shared Layer      │                            │
│              │  (UI, Hooks, API)   │                            │
│              └─────────────────────┘                            │
└─────────────────────────────────────────────────────────────────┘
          │                                    │
          ▼                                    ▼
┌──────────────────┐                ┌──────────────────┐
│  FIREBASE AUTH   │                │   BACKEND API    │
│     (IdP)        │                │   (.NET 10)      │
└──────────────────┘                └──────────────────┘
```

---

## 2. Stack Tecnológica

### 2.1 Core

| Categoria | Tecnologia | Versão | Justificativa |
|-----------|------------|--------|---------------|
| **Framework** | React | 18.x | Ecossistema maduro, ADR existente |
| **Linguagem** | TypeScript | 5.x | Type safety, DX |
| **Bundler** | Vite | 5.x | Performance, HMR rápido |
| **Roteamento** | React Router | 6.x | Padrão consolidado |

### 2.2 State Management

| Categoria | Tecnologia | Versão | Uso |
|-----------|------------|--------|-----|
| **Server State** | TanStack Query | 5.x | Cache, sincronização com API |
| **UI State** | React Context API | - | Estado local de UI |

### 2.3 UI/Styling

| Categoria | Tecnologia | Versão | Justificativa |
|-----------|------------|--------|---------------|
| **Styling** | Tailwind CSS | 3.x | Utility-first, mobile-first |
| **Componentes** | Shadcn/ui | latest | Customizável, acessível |
| **Gráficos** | Tremor | 3.x | Integração Tailwind |
| **Ícones** | Lucide React | latest | Consistente com Shadcn |

### 2.4 Formulários e Validação

| Categoria | Tecnologia | Versão | Justificativa |
|-----------|------------|--------|---------------|
| **Formulários** | React Hook Form | 7.x | Performance, uncontrolled |
| **Validação** | Zod | 3.x | Type-safe, composable |

### 2.5 Autenticação

| Categoria | Tecnologia | Versão | Justificativa |
|-----------|------------|--------|---------------|
| **Firebase** | Firebase SDK | 10.x | Modular (tree-shaking) |
| **HTTP** | Fetch API | nativo | Sem dependência extra |

### 2.6 Testes

| Categoria | Tecnologia | Versão | Uso |
|-----------|------------|--------|-----|
| **Unitários** | Vitest | 1.x | Compatível com Vite |
| **Componentes** | Testing Library | 14.x | Behavior-driven |
| **Visual/Docs** | Storybook | 8.x | Design system docs |
| **E2E** | Playwright | 1.x | Cross-browser |

### 2.7 PWA

| Categoria | Tecnologia | Versão | Uso |
|-----------|------------|--------|-----|
| **PWA Plugin** | vite-plugin-pwa | 0.x | Service worker, manifest |

---

## 3. Design System

### 3.1 Paleta de Cores

```typescript
// tailwind.config.ts
const colors = {
  primary: {
    DEFAULT: '#1a73e8',
    dark: '#004a99',
    light: '#4a9eff',
    50: '#eff6ff',
    100: '#dbeafe',
    500: '#1a73e8',
    600: '#004a99',
    700: '#003d80',
  },
  income: {
    DEFAULT: '#28a745',
    light: '#48c764',
    50: '#f0fdf4',
    500: '#28a745',
    600: '#16a34a',
  },
  expense: {
    DEFAULT: '#dc3545',
    light: '#ff6b6b',
    50: '#fef2f2',
    500: '#dc3545',
    600: '#dc2626',
  },
  background: {
    DEFAULT: '#ffffff',
    secondary: '#f4f6f8',
    tertiary: '#e0e0e0',
  },
  foreground: {
    DEFAULT: '#333333',
    muted: '#666666',
    subtle: '#999999',
  },
}
```

### 3.2 Tipografia

| Uso | Fonte | Peso | Tamanho |
|-----|-------|------|---------|
| **Títulos H1** | Inter | Bold (700) | 36px |
| **Títulos H2** | Inter | Bold (700) | 24px |
| **Títulos H3** | Inter | SemiBold (600) | 20px |
| **Corpo** | Inter | Regular (400) | 16px |
| **Tabelas** | Inter | Regular (400) | 14px |
| **Dados Numéricos** | Inter / JetBrains Mono | SemiBold (600) | 14px |
| **Labels** | Inter | Medium (500) | 12px |

```typescript
// tailwind.config.ts
const fontFamily = {
  sans: ['Inter', 'system-ui', 'sans-serif'],
  mono: ['JetBrains Mono', 'Consolas', 'monospace'],
}
```

### 3.3 Bordas e Espaçamento

| Aspecto | Valor | Uso |
|---------|-------|-----|
| **Border Radius (sm)** | 0.5rem (8px) | Inputs, badges |
| **Border Radius (default)** | 0.75rem (12px) | Cards, buttons |
| **Border Radius (lg)** | 1rem (16px) | Modais, dialogs |
| **Border Radius (xl)** | 1.5rem (24px) | Cards destacados |

### 3.4 Componentes Customizados

| Componente | Customização |
|------------|--------------|
| **Button** | Bordas mais arredondadas, variantes primary/income/expense/ghost |
| **Card** | Shadow suave (`shadow-sm`), hover state |
| **Input** | Focus ring na cor primária, border-radius 8px |
| **Table** | Alternância de cores (striped), hover states |
| **Badge** | Variantes para status (pending, active, suspended, rejected) |
| **Dialog** | Animações suaves, overlay com blur |
| **Toast** | Posição bottom-right, auto-dismiss |

---

## 4. Estrutura de Pastas

```
frontend/
├── .github/
│   └── workflows/
│       ├── ci.yml                    # Build + Testes
│       ├── deploy-storybook.yml      # Deploy Storybook → GitHub Pages
│       └── deploy-prod.yml           # Deploy → OCI Object Storage
│
├── .storybook/
│   ├── main.ts                       # Configuração Storybook
│   ├── preview.ts                    # Decorators globais
│   └── manager.ts                    # UI customization
│
├── public/
│   ├── favicon.ico
│   ├── manifest.json                 # PWA Manifest
│   ├── robots.txt
│   └── icons/
│       ├── icon-192.png
│       └── icon-512.png
│
├── src/
│   ├── app/
│   │   ├── App.tsx                   # Root component
│   │   ├── main.tsx                  # Entry point
│   │   ├── providers/
│   │   │   ├── index.tsx             # Provider composition
│   │   │   ├── QueryProvider.tsx     # TanStack Query
│   │   │   ├── AuthProvider.tsx      # Firebase + Session
│   │   │   └── ToastProvider.tsx     # Notificações
│   │   └── routes/
│   │       ├── index.tsx             # Route definitions
│   │       ├── ProtectedRoute.tsx    # Auth guard (lazy loading)
│   │       ├── AdminRoute.tsx        # Admin guard (lazy loading)
│   │       └── PublicRoute.tsx       # Guest only routes
│   │
│   ├── features/
│   │   ├── auth/
│   │   │   ├── components/
│   │   │   │   ├── LoginForm.tsx
│   │   │   │   ├── RegisterForm.tsx
│   │   │   │   ├── VerifyEmailCard.tsx
│   │   │   │   └── PendingApprovalCard.tsx
│   │   │   ├── hooks/
│   │   │   │   ├── useAuth.ts
│   │   │   │   ├── useLogin.ts
│   │   │   │   ├── useRegister.ts
│   │   │   │   └── useLogout.ts
│   │   │   ├── pages/
│   │   │   │   ├── LoginPage.tsx
│   │   │   │   ├── RegisterPage.tsx
│   │   │   │   ├── VerifyEmailPage.tsx
│   │   │   │   └── PendingApprovalPage.tsx
│   │   │   ├── services/
│   │   │   │   └── authService.ts
│   │   │   ├── types/
│   │   │   │   └── auth.types.ts
│   │   │   └── index.ts
│   │   │
│   │   ├── dashboard/
│   │   │   ├── components/
│   │   │   │   ├── BalanceCard.tsx
│   │   │   │   ├── BalanceChart.tsx
│   │   │   │   ├── RecentTransactions.tsx
│   │   │   │   └── QuickActions.tsx
│   │   │   ├── hooks/
│   │   │   │   ├── useBalances.ts
│   │   │   │   └── useDailyBalances.ts
│   │   │   ├── pages/
│   │   │   │   └── DashboardPage.tsx
│   │   │   ├── services/
│   │   │   │   └── dashboardService.ts
│   │   │   └── index.ts
│   │   │
│   │   ├── categories/
│   │   │   ├── components/
│   │   │   │   ├── CategoryList.tsx
│   │   │   │   ├── CategoryForm.tsx
│   │   │   │   ├── CategoryCard.tsx
│   │   │   │   └── CategoryDeleteDialog.tsx
│   │   │   ├── hooks/
│   │   │   │   ├── useCategories.ts
│   │   │   │   ├── useCreateCategory.ts
│   │   │   │   ├── useUpdateCategory.ts
│   │   │   │   └── useDeleteCategory.ts
│   │   │   ├── pages/
│   │   │   │   ├── CategoriesPage.tsx
│   │   │   │   └── CategoryFormPage.tsx
│   │   │   ├── services/
│   │   │   │   └── categoryService.ts
│   │   │   └── index.ts
│   │   │
│   │   ├── transactions/
│   │   │   ├── components/
│   │   │   │   ├── TransactionList.tsx
│   │   │   │   ├── TransactionForm.tsx
│   │   │   │   ├── TransactionCard.tsx
│   │   │   │   ├── TransactionFilters.tsx
│   │   │   │   └── TransactionDeleteDialog.tsx
│   │   │   ├── hooks/
│   │   │   │   ├── useTransactions.ts
│   │   │   │   ├── useTransaction.ts
│   │   │   │   ├── useCreateTransaction.ts
│   │   │   │   ├── useUpdateTransaction.ts
│   │   │   │   └── useDeleteTransaction.ts
│   │   │   ├── pages/
│   │   │   │   ├── TransactionsPage.tsx
│   │   │   │   └── TransactionFormPage.tsx
│   │   │   ├── services/
│   │   │   │   └── transactionService.ts
│   │   │   └── index.ts
│   │   │
│   │   └── admin/
│   │       ├── users/
│   │       │   ├── components/
│   │       │   │   ├── UserList.tsx
│   │       │   │   ├── UserStatusBadge.tsx
│   │       │   │   ├── UserApprovalDialog.tsx
│   │       │   │   ├── UserRolesForm.tsx
│   │       │   │   └── PendingUsersAlert.tsx
│   │       │   ├── hooks/
│   │       │   │   ├── useUsers.ts
│   │       │   │   ├── usePendingUsers.ts
│   │       │   │   ├── useApproveUser.ts
│   │       │   │   ├── useRejectUser.ts
│   │       │   │   ├── useSuspendUser.ts
│   │       │   │   └── useUpdateUserRoles.ts
│   │       │   ├── pages/
│   │       │   │   ├── UsersPage.tsx
│   │       │   │   └── UserDetailPage.tsx
│   │       │   └── services/
│   │       │       └── userService.ts
│   │       └── index.ts
│   │
│   ├── shared/
│   │   ├── components/
│   │   │   ├── ui/                   # Shadcn/ui customizados
│   │   │   │   ├── button.tsx
│   │   │   │   ├── input.tsx
│   │   │   │   ├── card.tsx
│   │   │   │   ├── dialog.tsx
│   │   │   │   ├── table.tsx
│   │   │   │   ├── form.tsx
│   │   │   │   ├── select.tsx
│   │   │   │   ├── toast.tsx
│   │   │   │   ├── skeleton.tsx
│   │   │   │   ├── badge.tsx
│   │   │   │   ├── dropdown-menu.tsx
│   │   │   │   └── ...
│   │   │   ├── layout/
│   │   │   │   ├── AppLayout.tsx     # Layout autenticado
│   │   │   │   ├── AuthLayout.tsx    # Layout público
│   │   │   │   ├── Header.tsx
│   │   │   │   ├── Sidebar.tsx
│   │   │   │   ├── MobileNav.tsx
│   │   │   │   └── Footer.tsx
│   │   │   ├── feedback/
│   │   │   │   ├── LoadingScreen.tsx # Tela de loading inicial
│   │   │   │   ├── LoadingSpinner.tsx
│   │   │   │   ├── ErrorBoundary.tsx
│   │   │   │   ├── EmptyState.tsx
│   │   │   │   └── ErrorMessage.tsx
│   │   │   └── data-display/
│   │   │       ├── DataTable.tsx
│   │   │       ├── Pagination.tsx
│   │   │       ├── AmountDisplay.tsx # Formatação de valores
│   │   │       └── DateDisplay.tsx   # Formatação de datas
│   │   │
│   │   ├── hooks/
│   │   │   ├── useApiClient.ts
│   │   │   ├── useDebounce.ts
│   │   │   ├── useMediaQuery.ts
│   │   │   ├── useLocalStorage.ts
│   │   │   └── usePagination.ts
│   │   │
│   │   ├── lib/
│   │   │   ├── api/
│   │   │   │   ├── client.ts         # Fetch wrapper
│   │   │   │   ├── endpoints.ts      # API endpoints constants
│   │   │   │   └── errors.ts         # Error handling
│   │   │   ├── firebase/
│   │   │   │   ├── config.ts         # Firebase config
│   │   │   │   ├── auth.ts           # Firebase auth helpers
│   │   │   │   └── index.ts
│   │   │   ├── utils/
│   │   │   │   ├── cn.ts             # Tailwind merge (clsx + twMerge)
│   │   │   │   ├── formatters.ts     # Datas, moedas PT-BR
│   │   │   │   ├── validators.ts     # Validações comuns
│   │   │   │   └── constants.ts      # Constantes globais
│   │   │   └── queryClient.ts        # TanStack Query config
│   │   │
│   │   ├── types/
│   │   │   ├── api.types.ts          # DTOs da API
│   │   │   ├── common.types.ts       # Types genéricos
│   │   │   └── errors.types.ts       # Error types
│   │   │
│   │   └── styles/
│   │       ├── globals.css           # Tailwind imports + base styles
│   │       └── fonts.css             # Inter font import
│   │
│   └── assets/
│       ├── images/
│       │   └── logo.svg
│       └── icons/
│
├── tests/
│   ├── e2e/
│   │   ├── auth.spec.ts
│   │   ├── dashboard.spec.ts
│   │   ├── transactions.spec.ts
│   │   └── admin.spec.ts
│   ├── mocks/
│   │   ├── handlers.ts               # MSW handlers
│   │   └── server.ts                 # MSW server setup
│   └── setup.ts                      # Test setup
│
├── .env.example
├── .env.development
├── .env.production
├── .eslintrc.cjs
├── .prettierrc
├── .gitignore
├── components.json                   # Shadcn/ui config
├── Dockerfile
├── index.html
├── package.json
├── playwright.config.ts
├── postcss.config.js
├── tailwind.config.ts
├── tsconfig.json
├── tsconfig.node.json
├── vite.config.ts
└── vitest.config.ts
```

---

## 5. Arquitetura de Segurança

### 5.1 Princípio: Lazy Loading Protegido

**Regra fundamental:** Nenhum código de páginas protegidas é carregado no bundle inicial. O carregamento só ocorre **após confirmação do backend**.

### 5.2 Estrutura de Bundles

```
Bundle Structure:
├── main.js                    # Core + Router + Auth check + Loading Screen
├── public.js                  # Login, Register, VerifyEmail, PendingApproval
├── protected.js               # Dashboard, Transactions, Categories (lazy)
└── admin.js                   # Admin pages (lazy)
```

### 5.3 Fluxo de Carregamento Seguro

```
┌─────────────────────────────────────────────────────────────────┐
│                    USUÁRIO ACESSA URL                           │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│              CARREGA BUNDLE INICIAL (main.js)                   │
│         Contém: Router, AuthProvider, Loading Screen            │
│         NÃO contém: Dashboard, Transactions, Admin              │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                  VERIFICA SESSÃO NO BACKEND                     │
│                   GET /api/v1/auth/me                           │
│                   (com credentials: 'include')                  │
└─────────────────────────────────────────────────────────────────┘
                              │
            ┌─────────────────┼─────────────────┐
            │                 │                 │
            ▼                 ▼                 ▼
      ┌──────────┐     ┌───────────┐     ┌───────────┐
      │ 401/403  │     │ 200 +     │     │ 200 +     │
      │ ou erro  │     │ Pending/  │     │ Active    │
      └────┬─────┘     │ Suspended │     └─────┬─────┘
           │           └─────┬─────┘           │
           ▼                 ▼                 ▼
    ┌────────────┐    ┌────────────┐    ┌────────────────┐
    │ CARREGA    │    │ CARREGA    │    │ CARREGA        │
    │ public.js  │    │ public.js  │    │ protected.js   │
    │ LoginPage  │    │ Pending/   │    │ (lazy import)  │
    └────────────┘    │ Suspended  │    └────────────────┘
                      └────────────┘           │
                                               ▼
                                    ┌────────────────────┐
                                    │ SE role === Admin  │
                                    │ CARREGA admin.js   │
                                    │ (lazy import)      │
                                    └────────────────────┘
```

### 5.4 Comportamento por Cenário

| Cenário | Bundle Carregado | Página Exibida |
|---------|------------------|----------------|
| Sem cookie/sessão inválida | `main.js` + `public.js` | LoginPage |
| Sessão válida + status `Pending` | `main.js` + `public.js` | PendingApprovalPage |
| Sessão válida + status `Suspended` | `main.js` + `public.js` | SuspendedPage |
| Sessão válida + status `Rejected` | `main.js` + `public.js` | RejectedPage |
| Sessão válida + status `Active` | `main.js` + `protected.js` | Dashboard (ou rota solicitada) |
| Sessão válida + status `Active` + role `Admin` | `main.js` + `protected.js` + `admin.js` | Acesso admin liberado |

### 5.5 Regras de Segurança Obrigatórias

| Regra | Descrição |
|-------|-----------|
| **Nenhum import estático** | Páginas protegidas usam `React.lazy()` |
| **Verificação síncrona** | App mostra loading até `/auth/me` responder |
| **Sem cache de sessão local** | Cada reload verifica backend |
| **Fallback seguro** | Qualquer erro → redireciona para login |
| **Sem flash de conteúdo** | Usuário nunca vê página protegida antes da verificação |

### 5.6 Loading Screen Inicial

Durante a verificação de sessão, exibir:

```
┌─────────────────────────────────────────┐
│                                         │
│           [Logo L2SLedger]              │
│                                         │
│          ◐ Verificando sessão...        │
│                                         │
└─────────────────────────────────────────┘
```

- Presente no bundle inicial (`main.js`)
- Sem dados sensíveis
- Exibida até confirmação do backend

---

## 6. Fluxos de Autenticação

### 6.1 Fluxo de Registro

```
┌─────────────────┐
│  RegisterPage   │
│  (email, senha) │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ Firebase SDK    │
│ createUser()    │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ sendEmailVerify │
└────────┬────────┘
         │
         ▼
┌─────────────────────────────────────────┐
│ Exibir mensagem: "Verifique seu email"  │
│ Botão: "Reenviar email de verificação"  │
└─────────────────────────────────────────┘
         │
         │ (usuário clica no link do email)
         ▼
┌─────────────────┐
│ VerifyEmailPage │
│ (auto-redirect) │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ POST /auth/login│──────► Backend cria User com status=Pending
└────────┬────────┘
         │
         ▼
┌─────────────────────┐
│ PendingApprovalPage │
│ "Aguardando Admin"  │
└─────────────────────┘
```

### 6.2 Fluxo de Login

```
┌─────────────────┐
│   LoginPage     │
│ (email, senha)  │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ Firebase SDK    │
│ signIn()        │
└────────┬────────┘
         │
    ┌────┴────┐
    │         │
    ▼         ▼
┌───────┐ ┌───────────────┐
│Verified│ │Not Verified   │
└───┬───┘ └───────┬───────┘
    │             │
    │             ▼
    │     ┌───────────────┐
    │     │VerifyEmailPage│
    │     │ + Reenviar    │
    │     └───────────────┘
    ▼
┌─────────────────┐
│POST /auth/login │
│ (firebaseToken) │
└────────┬────────┘
         │
    ┌────┴────┬────────────┬────────────┐
    │         │            │            │
    ▼         ▼            ▼            ▼
┌───────┐ ┌────────┐ ┌──────────┐ ┌────────┐
│Active │ │Pending │ │Suspended │ │Rejected│
└───┬───┘ └────┬───┘ └────┬─────┘ └────┬───┘
    │          │          │            │
    ▼          ▼          ▼            ▼
┌────────┐ ┌────────────┐ ┌───────────┐ ┌───────────┐
│Dashboard│ │PendingPage │ │SuspendPage│ │RejectPage │
└────────┘ └────────────┘ └───────────┘ └───────────┘
```

### 6.3 Tratamento de Erros de Autenticação

| Código Backend | HTTP | Ação Frontend |
|----------------|------|---------------|
| `AUTH_INVALID_TOKEN` | 401 | Redirecionar para login |
| `AUTH_EMAIL_NOT_VERIFIED` | 400 | Exibir VerifyEmailPage |
| `AUTH_USER_PENDING` | 403 | Exibir PendingApprovalPage |
| `AUTH_USER_SUSPENDED` | 403 | Exibir SuspendedPage |
| `AUTH_USER_REJECTED` | 403 | Exibir RejectedPage |
| `AUTH_USER_INACTIVE` | 403 | Exibir mensagem de conta inativa |
| `AUTH_USER_NOT_FOUND` | 404 | Exibir erro "Usuário não encontrado" |
| `AUTH_SESSION_EXPIRED` | 401 | Redirecionar para login |
| `AUTH_UNAUTHORIZED` | 401 | Redirecionar para login |
| `AUTH_FIREBASE_ERROR` | 502 | Exibir erro de serviço indisponível |

### 6.4 Catálogo Completo de Códigos de Erro

> 📚 **Referência:** O catálogo completo de 67 códigos de erro está documentado em [ADR-021-A](../../adr/adr-021-a.md).

#### Resumo por Categoria

| Prefixo | Categoria | Qtd | HTTP Codes Típicos | Ação Padrão Frontend |
|---------|-----------|-----|---------------------|---------------------|
| `AUTH_` | Autenticação | 10 | 400, 401, 403, 404, 502 | Redirecionar login / Exibir página status |
| `VAL_` | Validação | 10 | 400 | Exibir erro no formulário |
| `FIN_` | Regras Financeiras | 18 | 400, 404, 409, 410, 422 | Exibir alerta / Bloquear ação |
| `PERM_` | Permissões | 3 | 403 | Exibir acesso negado |
| `USER_` | Gestão Usuários | 12 | 400, 404 | Exibir erro específico |
| `SYS_` | Sistema | 3 | 500, 503 | Exibir erro genérico com traceId |
| `AUDIT_` | Auditoria | 1 | 404 | Exibir "não encontrado" |
| `INT_` | Integrações | 3 | 502, 503 | Exibir serviço indisponível |
| `EXPORT_` | Exportações | 7 | 400, 403, 404 | Exibir erro específico |

#### Tratamento Global de Erros

```typescript
// shared/lib/api/errors.ts
import type { ErrorResponse } from '@/shared/types/errors.types';

export const handleApiError = (error: ErrorResponse): void => {
  const { code, message, traceId } = error.error;
  
  switch (code) {
    // ═══════════════════════════════════════════════════════════════
    // AUTH_ — Autenticação (redirecionar ou exibir página de status)
    // ═══════════════════════════════════════════════════════════════
    case 'AUTH_INVALID_TOKEN':
    case 'AUTH_SESSION_EXPIRED':
    case 'AUTH_UNAUTHORIZED':
      redirectToLogin();
      break;
    
    case 'AUTH_EMAIL_NOT_VERIFIED':
      redirectTo('/verify-email');
      break;
    
    case 'AUTH_USER_PENDING':
      redirectTo('/pending-approval');
      break;
    
    case 'AUTH_USER_SUSPENDED':
      redirectTo('/suspended');
      break;
    
    case 'AUTH_USER_REJECTED':
      redirectTo('/rejected');
      break;
    
    case 'AUTH_USER_INACTIVE':
    case 'AUTH_USER_NOT_FOUND':
      showToast({ type: 'error', message });
      redirectToLogin();
      break;
    
    case 'AUTH_FIREBASE_ERROR':
      showToast({ 
        type: 'error', 
        message: 'Serviço de autenticação indisponível. Tente novamente.' 
      });
      break;
    
    // ═══════════════════════════════════════════════════════════════
    // VAL_ — Validação (exibir no formulário)
    // ═══════════════════════════════════════════════════════════════
    case 'VAL_REQUIRED_FIELD':
    case 'VAL_VALIDATION_FAILED':
    case 'VAL_INVALID_VALUE':
    case 'VAL_INVALID_FORMAT':
    case 'VAL_AMOUNT_NEGATIVE':
    case 'VAL_INVALID_DATE':
    case 'VAL_INVALID_RANGE':
    case 'VAL_DUPLICATE_NAME':
    case 'VAL_INVALID_REFERENCE':
    case 'VAL_BUSINESS_RULE_VIOLATION':
      showValidationError(message);
      break;
    
    // ═══════════════════════════════════════════════════════════════
    // FIN_ — Regras Financeiras (exibir alerta contextual)
    // ═══════════════════════════════════════════════════════════════
    case 'FIN_PERIOD_CLOSED':
    case 'FIN_PERIOD_ALREADY_CLOSED':
      showAlert({ 
        type: 'warning', 
        title: 'Período Fechado', 
        message: 'Este período está fechado. Operação não permitida.' 
      });
      break;
    
    case 'FIN_PERIOD_ALREADY_OPENED':
      showAlert({ 
        type: 'info', 
        title: 'Período Aberto', 
        message: 'Este período já está aberto.' 
      });
      break;
    
    case 'FIN_CATEGORY_NOT_FOUND':
    case 'FIN_TRANSACTION_NOT_FOUND':
    case 'FIN_PERIOD_NOT_FOUND':
    case 'FIN_ADJUSTMENT_NOT_FOUND':
      showToast({ type: 'error', message: 'Registro não encontrado.' });
      break;
    
    case 'FIN_CATEGORY_INVALID_NAME':
    case 'FIN_CATEGORY_NAME_TOO_LONG':
      showValidationError(message);
      break;
    
    case 'FIN_DUPLICATE_ENTRY':
    case 'FIN_PERIOD_ALREADY_EXISTS':
      showToast({ type: 'warning', message: 'Registro duplicado.' });
      break;
    
    case 'FIN_INSUFFICIENT_BALANCE':
      showAlert({ 
        type: 'error', 
        title: 'Saldo Insuficiente', 
        message 
      });
      break;
    
    case 'FIN_ADJUSTMENT_PERIOD_CLOSED':
    case 'FIN_ADJUSTMENT_INVALID_ORIGINAL':
    case 'FIN_ADJUSTMENT_ALREADY_DELETED':
    case 'FIN_PERIOD_INVALID_OPERATION':
    case 'FIN_INVALID_TRANSACTION':
      showToast({ type: 'error', message });
      break;
    
    case 'FIN_ADJUSTMENT_UNAUTHORIZED':
      showAccessDenied();
      break;
    
    // ═══════════════════════════════════════════════════════════════
    // PERM_ — Permissões (exibir acesso negado)
    // ═══════════════════════════════════════════════════════════════
    case 'PERM_ACCESS_DENIED':
    case 'PERM_ROLE_REQUIRED':
    case 'PERM_INSUFFICIENT_PRIVILEGES':
      showAccessDenied();
      break;
    
    // ═══════════════════════════════════════════════════════════════
    // USER_ — Gestão de Usuários (feedback específico)
    // ═══════════════════════════════════════════════════════════════
    case 'USER_NOT_FOUND':
      showToast({ type: 'error', message: 'Usuário não encontrado.' });
      break;
    
    case 'USER_CANNOT_REMOVE_OWN_ADMIN':
      showAlert({ 
        type: 'warning', 
        title: 'Operação Bloqueada', 
        message: 'Você não pode remover sua própria permissão de Admin.' 
      });
      break;
    
    case 'USER_LAST_ADMIN':
      showAlert({ 
        type: 'error', 
        title: 'Operação Bloqueada', 
        message: 'Não é possível remover o último Admin do sistema.' 
      });
      break;
    
    case 'USER_INVALID_STATUS_TRANSITION':
    case 'USER_STATUS_REQUIRED':
    case 'USER_STATUS_REASON_REQUIRED':
    case 'USER_STATUS_REASON_TOO_LONG':
    case 'USER_INVALID_STATUS':
    case 'USER_CANNOT_MODIFY_OWN_STATUS':
    case 'USER_ROLES_REQUIRED':
    case 'USER_ROLE_EMPTY':
    case 'USER_INVALID_ROLE':
      showToast({ type: 'error', message });
      break;
    
    // ═══════════════════════════════════════════════════════════════
    // EXPORT_ — Exportações
    // ═══════════════════════════════════════════════════════════════
    case 'EXPORT_NOT_FOUND':
      showToast({ type: 'error', message: 'Exportação não encontrada.' });
      break;
    
    case 'EXPORT_DELETE_UNAUTHORIZED':
    case 'EXPORT_UNAUTHORIZED':
      showAccessDenied();
      break;
    
    case 'EXPORT_NOT_COMPLETED':
    case 'EXPORT_NOT_READY':
    case 'EXPORT_INVALID_STATE':
    case 'EXPORT_INVALID_PARAMETERS':
      showToast({ type: 'warning', message });
      break;
    
    // ═══════════════════════════════════════════════════════════════
    // AUDIT_ — Auditoria
    // ═══════════════════════════════════════════════════════════════
    case 'AUDIT_EVENT_NOT_FOUND':
      showToast({ type: 'error', message: 'Evento de auditoria não encontrado.' });
      break;
    
    // ═══════════════════════════════════════════════════════════════
    // INT_ — Integrações Externas
    // ═══════════════════════════════════════════════════════════════
    case 'INT_FIREBASE_UNAVAILABLE':
    case 'INT_DB_CONNECTION':
    case 'INT_EXTERNAL_SERVICE_ERROR':
      showAlert({ 
        type: 'error', 
        title: 'Serviço Indisponível', 
        message: 'Um serviço externo está temporariamente indisponível. Tente novamente.',
        traceId 
      });
      break;
    
    // ═══════════════════════════════════════════════════════════════
    // SYS_ — Erros de Sistema (exibir erro genérico com traceId)
    // ═══════════════════════════════════════════════════════════════
    case 'SYS_INTERNAL_ERROR':
    case 'SYS_UNAVAILABLE':
    case 'SYS_CONFIGURATION_ERROR':
      showSystemError({ 
        message: 'Ocorreu um erro inesperado.', 
        traceId 
      });
      break;
    
    // ═══════════════════════════════════════════════════════════════
    // Default — Erro não catalogado
    // ═══════════════════════════════════════════════════════════════
    default:
      console.error(`Unhandled error code: ${code}`, error);
      showToast({ type: 'error', message: message || 'Erro desconhecido.' });
  }
};
```

#### Tipos TypeScript para Erros

```typescript
// shared/types/errors.types.ts

export interface ErrorResponse {
  error: {
    code: string;
    message: string;
    details?: string | null;
    timestamp: string;
    traceId: string;
  };
}

// Constantes de códigos de erro (espelhando backend)
export const ErrorCodes = {
  // AUTH_
  AUTH_INVALID_TOKEN: 'AUTH_INVALID_TOKEN',
  AUTH_EMAIL_NOT_VERIFIED: 'AUTH_EMAIL_NOT_VERIFIED',
  AUTH_SESSION_EXPIRED: 'AUTH_SESSION_EXPIRED',
  AUTH_UNAUTHORIZED: 'AUTH_UNAUTHORIZED',
  AUTH_USER_PENDING: 'AUTH_USER_PENDING',
  AUTH_USER_SUSPENDED: 'AUTH_USER_SUSPENDED',
  AUTH_USER_REJECTED: 'AUTH_USER_REJECTED',
  AUTH_USER_INACTIVE: 'AUTH_USER_INACTIVE',
  AUTH_USER_NOT_FOUND: 'AUTH_USER_NOT_FOUND',
  AUTH_FIREBASE_ERROR: 'AUTH_FIREBASE_ERROR',
  
  // VAL_
  VAL_REQUIRED_FIELD: 'VAL_REQUIRED_FIELD',
  VAL_VALIDATION_FAILED: 'VAL_VALIDATION_FAILED',
  VAL_INVALID_VALUE: 'VAL_INVALID_VALUE',
  VAL_INVALID_FORMAT: 'VAL_INVALID_FORMAT',
  VAL_AMOUNT_NEGATIVE: 'VAL_AMOUNT_NEGATIVE',
  VAL_INVALID_DATE: 'VAL_INVALID_DATE',
  VAL_INVALID_RANGE: 'VAL_INVALID_RANGE',
  VAL_DUPLICATE_NAME: 'VAL_DUPLICATE_NAME',
  VAL_INVALID_REFERENCE: 'VAL_INVALID_REFERENCE',
  VAL_BUSINESS_RULE_VIOLATION: 'VAL_BUSINESS_RULE_VIOLATION',
  
  // FIN_
  FIN_PERIOD_CLOSED: 'FIN_PERIOD_CLOSED',
  FIN_INSUFFICIENT_BALANCE: 'FIN_INSUFFICIENT_BALANCE',
  FIN_DUPLICATE_ENTRY: 'FIN_DUPLICATE_ENTRY',
  FIN_INVALID_TRANSACTION: 'FIN_INVALID_TRANSACTION',
  FIN_PERIOD_NOT_FOUND: 'FIN_PERIOD_NOT_FOUND',
  FIN_PERIOD_ALREADY_EXISTS: 'FIN_PERIOD_ALREADY_EXISTS',
  FIN_PERIOD_ALREADY_CLOSED: 'FIN_PERIOD_ALREADY_CLOSED',
  FIN_PERIOD_ALREADY_OPENED: 'FIN_PERIOD_ALREADY_OPENED',
  FIN_PERIOD_INVALID_OPERATION: 'FIN_PERIOD_INVALID_OPERATION',
  FIN_CATEGORY_NOT_FOUND: 'FIN_CATEGORY_NOT_FOUND',
  FIN_CATEGORY_INVALID_NAME: 'FIN_CATEGORY_INVALID_NAME',
  FIN_CATEGORY_NAME_TOO_LONG: 'FIN_CATEGORY_NAME_TOO_LONG',
  FIN_TRANSACTION_NOT_FOUND: 'FIN_TRANSACTION_NOT_FOUND',
  FIN_ADJUSTMENT_NOT_FOUND: 'FIN_ADJUSTMENT_NOT_FOUND',
  FIN_ADJUSTMENT_PERIOD_CLOSED: 'FIN_ADJUSTMENT_PERIOD_CLOSED',
  FIN_ADJUSTMENT_INVALID_ORIGINAL: 'FIN_ADJUSTMENT_INVALID_ORIGINAL',
  FIN_ADJUSTMENT_UNAUTHORIZED: 'FIN_ADJUSTMENT_UNAUTHORIZED',
  FIN_ADJUSTMENT_ALREADY_DELETED: 'FIN_ADJUSTMENT_ALREADY_DELETED',
  
  // PERM_
  PERM_ACCESS_DENIED: 'PERM_ACCESS_DENIED',
  PERM_ROLE_REQUIRED: 'PERM_ROLE_REQUIRED',
  PERM_INSUFFICIENT_PRIVILEGES: 'PERM_INSUFFICIENT_PRIVILEGES',
  
  // USER_
  USER_NOT_FOUND: 'USER_NOT_FOUND',
  USER_INVALID_STATUS_TRANSITION: 'USER_INVALID_STATUS_TRANSITION',
  USER_STATUS_REQUIRED: 'USER_STATUS_REQUIRED',
  USER_STATUS_REASON_REQUIRED: 'USER_STATUS_REASON_REQUIRED',
  USER_STATUS_REASON_TOO_LONG: 'USER_STATUS_REASON_TOO_LONG',
  USER_INVALID_STATUS: 'USER_INVALID_STATUS',
  USER_CANNOT_MODIFY_OWN_STATUS: 'USER_CANNOT_MODIFY_OWN_STATUS',
  USER_CANNOT_REMOVE_OWN_ADMIN: 'USER_CANNOT_REMOVE_OWN_ADMIN',
  USER_LAST_ADMIN: 'USER_LAST_ADMIN',
  USER_ROLES_REQUIRED: 'USER_ROLES_REQUIRED',
  USER_ROLE_EMPTY: 'USER_ROLE_EMPTY',
  USER_INVALID_ROLE: 'USER_INVALID_ROLE',
  
  // SYS_
  SYS_INTERNAL_ERROR: 'SYS_INTERNAL_ERROR',
  SYS_UNAVAILABLE: 'SYS_UNAVAILABLE',
  SYS_CONFIGURATION_ERROR: 'SYS_CONFIGURATION_ERROR',
  
  // AUDIT_
  AUDIT_EVENT_NOT_FOUND: 'AUDIT_EVENT_NOT_FOUND',
  
  // INT_
  INT_FIREBASE_UNAVAILABLE: 'INT_FIREBASE_UNAVAILABLE',
  INT_DB_CONNECTION: 'INT_DB_CONNECTION',
  INT_EXTERNAL_SERVICE_ERROR: 'INT_EXTERNAL_SERVICE_ERROR',
  
  // EXPORT_
  EXPORT_NOT_FOUND: 'EXPORT_NOT_FOUND',
  EXPORT_DELETE_UNAUTHORIZED: 'EXPORT_DELETE_UNAUTHORIZED',
  EXPORT_UNAUTHORIZED: 'EXPORT_UNAUTHORIZED',
  EXPORT_NOT_COMPLETED: 'EXPORT_NOT_COMPLETED',
  EXPORT_NOT_READY: 'EXPORT_NOT_READY',
  EXPORT_INVALID_STATE: 'EXPORT_INVALID_STATE',
  EXPORT_INVALID_PARAMETERS: 'EXPORT_INVALID_PARAMETERS',
} as const;

export type ErrorCode = typeof ErrorCodes[keyof typeof ErrorCodes];
```

---

## 7. Fases de Implementação

### 7.0 Pré-requisito: Backend

> ⚠️ **IMPORTANTE:** A alteração de backend para incluir status do usuário deve ser concluída antes da Fase 1.
> 
> Ver: [user-status-plan.md](../api-planning/user-status-plan.md)

---

### Fase 0: Setup Inicial

**Estimativa:** 8 horas

| # | Task | Descrição | Dependência |
|---|------|-----------|-------------|
| 0.1 | Criar estrutura de pastas | Conforme seção 4 | - |
| 0.2 | Configurar Vite + TypeScript | vite.config.ts, tsconfig.json | 0.1 |
| 0.3 | Configurar Tailwind CSS | tailwind.config.ts, postcss.config.js | 0.2 |
| 0.4 | Configurar Shadcn/ui | components.json, instalar dependências | 0.3 |
| 0.5 | Configurar React Query | QueryProvider, queryClient | 0.2 |
| 0.6 | Configurar React Router | routes/index.tsx | 0.2 |
| 0.7 | Configurar Firebase SDK | lib/firebase/config.ts | 0.2 |
| 0.8 | Criar API Client base | lib/api/client.ts | 0.5 |
| 0.9 | Configurar Vitest + Testing Library | vitest.config.ts, setup.ts | 0.2 |
| 0.10 | Configurar Storybook | .storybook/\*.ts | 0.3 |
| 0.11 | Configurar PWA (vite-plugin-pwa) | manifest.json, sw config | 0.2 |
| 0.12 | Configurar ESLint + Prettier | .eslintrc.cjs, .prettierrc | 0.2 |
| 0.13 | Criar layouts base (Auth, App) | shared/components/layout/\* | 0.4 |
| 0.14 | Criar LoadingScreen | shared/components/feedback/LoadingScreen.tsx | 0.4 |
| 0.15 | Configurar Code Splitting | React.lazy para rotas protegidas | 0.6 |
| 0.16 | Criar Dockerfile + .dockerignore | Dockerfile | 0.2 |
| 0.17 | Criar CI básico | .github/workflows/ci.yml | 0.16 |

**Critérios de Aceite:**
- [ ] `npm run dev` funciona
- [ ] `npm run build` gera bundles separados (verificável)
- [ ] `npm run test` passa
- [ ] `npm run storybook` funciona
- [ ] Lint passa sem erros
- [ ] Tailwind + Shadcn funcionam
- [ ] PWA instalável (localhost)

---

### Fase 1: Autenticação

**Estimativa:** 16 horas

**Dependência:** Backend com status de usuário implementado

| # | Task | Descrição | Dependência |
|---|------|-----------|-------------|
| 1.1 | Criar AuthProvider + Context | Gerenciamento de sessão | Fase 0 |
| 1.2 | Implementar useAuth hook | Estado do usuário | 1.1 |
| 1.3 | Implementar LoginPage | Formulário + validação | 1.1 |
| 1.4 | Implementar RegisterPage | Formulário + Firebase createUser | 1.1 |
| 1.5 | Implementar VerifyEmailPage | Verificação + reenvio | 1.1 |
| 1.6 | Implementar PendingApprovalPage | Tela de aguardando | 1.1 |
| 1.7 | Implementar SuspendedPage | Tela de suspenso | 1.1 |
| 1.8 | Implementar RejectedPage | Tela de rejeitado | 1.1 |
| 1.9 | Criar ProtectedRoute | Guard com lazy loading | 1.2 |
| 1.10 | Criar PublicRoute | Redirect se autenticado | 1.2 |
| 1.11 | Implementar AuthGuard | Verificação backend-first | 1.9 |
| 1.12 | Implementar Loading State global | Durante verificação | 1.11 |
| 1.13 | Garantir lazy loading de rotas | React.lazy + Suspense | 1.11 |
| 1.14 | Implementar Logout | Limpar sessão | 1.2 |
| 1.15 | Tratar erros de autenticação | Por código de erro | 1.3 |
| 1.16 | Testes unitários auth hooks | useAuth, useLogin, etc. | 1.14 |
| 1.17 | Testes E2E fluxo login/registro | Playwright | 1.16 |
| 1.18 | Stories Storybook auth | Componentes de auth | 1.8 |

**Critérios de Aceite:**
- [ ] Login funcional com Firebase + Backend
- [ ] Registro funcional com verificação de email
- [ ] Usuário pendente vê tela de aguardando
- [ ] Usuário suspenso/rejeitado vê tela apropriada
- [ ] Código protegido não carrega sem autenticação (verificar DevTools)
- [ ] Guards funcionando
- [ ] Logout limpa sessão
- [ ] Cobertura ≥ 85%
- [ ] E2E passando

---

### Fase 2: Dashboard

**Estimativa:** 12 horas

| # | Task | Descrição | Dependência |
|---|------|-----------|-------------|
| 2.1 | Criar AppLayout completo | Header, Sidebar, Mobile Nav | Fase 1 |
| 2.2 | Implementar Header | Logo, user menu, logout | 2.1 |
| 2.3 | Implementar Sidebar | Navegação, active states | 2.1 |
| 2.4 | Implementar MobileNav | Drawer/sheet mobile | 2.1 |
| 2.5 | Criar useBalances hook | GET /balances | 2.1 |
| 2.6 | Criar useDailyBalances hook | GET /balances/daily | 2.1 |
| 2.7 | Implementar BalanceCard | Receitas, despesas, saldo | 2.5 |
| 2.8 | Implementar BalanceChart | Tremor line chart | 2.6 |
| 2.9 | Implementar QuickActions | Atalhos para ações | 2.1 |
| 2.10 | Implementar RecentTransactions | Preview de transações | 2.1 |
| 2.11 | Implementar DashboardPage | Composição de componentes | 2.10 |
| 2.12 | Responsividade mobile | Ajustes breakpoints | 2.11 |
| 2.13 | Testes unitários dashboard hooks | useBalances, etc. | 2.6 |
| 2.14 | Testes E2E dashboard | Playwright | 2.13 |
| 2.15 | Stories Storybook dashboard | BalanceCard, Chart | 2.11 |

**Critérios de Aceite:**
- [ ] Dashboard exibe dados reais da API
- [ ] Gráfico de evolução funcional
- [ ] Layout responsivo (mobile-first)
- [ ] Navegação funcional
- [ ] Loading states implementados
- [ ] Cobertura ≥ 85%

---

### Fase 3: Categorias

**Estimativa:** 10 horas

| # | Task | Descrição | Dependência |
|---|------|-----------|-------------|
| 3.1 | Criar categoryService | Chamadas API | Fase 2 |
| 3.2 | Criar useCategories hook | Lista + cache | 3.1 |
| 3.3 | Criar useCreateCategory hook | Mutation | 3.1 |
| 3.4 | Criar useUpdateCategory hook | Mutation | 3.1 |
| 3.5 | Criar useDeleteCategory hook | Mutation | 3.1 |
| 3.6 | Implementar CategoryList | Tabela/cards | 3.2 |
| 3.7 | Implementar CategoryForm | Formulário + validação | 3.3 |
| 3.8 | Implementar CategoryCard | Exibição individual | 3.6 |
| 3.9 | Implementar CategoryDeleteDialog | Confirmação | 3.5 |
| 3.10 | Implementar CategoriesPage | Lista + ações | 3.9 |
| 3.11 | Implementar CategoryFormPage | Criar/Editar | 3.7 |
| 3.12 | Testes unitários category hooks | CRUD hooks | 3.5 |
| 3.13 | Testes E2E CRUD categorias | Playwright | 3.12 |
| 3.14 | Stories Storybook categorias | Componentes | 3.11 |

**Critérios de Aceite:**
- [ ] CRUD completo de categorias
- [ ] Validações funcionando
- [ ] Confirmação de exclusão
- [ ] Feedback de sucesso/erro
- [ ] Cobertura ≥ 85%

---

### Fase 4: Transações

**Estimativa:** 16 horas

| # | Task | Descrição | Dependência |
|---|------|-----------|-------------|
| 4.1 | Criar transactionService | Chamadas API | Fase 3 |
| 4.2 | Criar useTransactions hook | Lista + paginação | 4.1 |
| 4.3 | Criar useTransaction hook | Detalhe | 4.1 |
| 4.4 | Criar useCreateTransaction hook | Mutation | 4.1 |
| 4.5 | Criar useUpdateTransaction hook | Mutation | 4.1 |
| 4.6 | Criar useDeleteTransaction hook | Mutation | 4.1 |
| 4.7 | Implementar TransactionList | Tabela responsiva | 4.2 |
| 4.8 | Implementar TransactionFilters | Filtros (data, tipo, categoria) | 4.7 |
| 4.9 | Implementar TransactionForm | Formulário complexo | 4.4, Fase 3 |
| 4.10 | Implementar TransactionCard | Card mobile | 4.7 |
| 4.11 | Implementar TransactionDeleteDialog | Confirmação | 4.6 |
| 4.12 | Implementar Pagination | Componente reutilizável | 4.7 |
| 4.13 | Implementar TransactionsPage | Lista + filtros + paginação | 4.12 |
| 4.14 | Implementar TransactionFormPage | Criar/Editar | 4.9 |
| 4.15 | Implementar AmountDisplay | Formatação valores | 4.7 |
| 4.16 | Atualizar Dashboard | RecentTransactions real | 4.2 |
| 4.17 | Testes unitários transaction hooks | CRUD hooks | 4.6 |
| 4.18 | Testes E2E CRUD transações | Playwright | 4.17 |
| 4.19 | Stories Storybook transações | Componentes | 4.14 |

**Critérios de Aceite:**
- [ ] CRUD completo de transações
- [ ] Filtros funcionando
- [ ] Paginação funcionando
- [ ] Integração com categorias
- [ ] Valores formatados corretamente (BRL)
- [ ] Dashboard atualizado com dados reais
- [ ] Cobertura ≥ 85%

---

### Fase 5: Admin - Usuários

**Estimativa:** 14 horas

| # | Task | Descrição | Dependência |
|---|------|-----------|-------------|
| 5.1 | Criar AdminRoute | Guard para admin | Fase 1 |
| 5.2 | Criar userService | Chamadas API admin | 5.1 |
| 5.3 | Criar useUsers hook | Lista + filtros | 5.2 |
| 5.4 | Criar usePendingUsers hook | Contagem pendentes | 5.2 |
| 5.5 | Criar useApproveUser hook | Mutation status | 5.2 |
| 5.6 | Criar useRejectUser hook | Mutation status | 5.2 |
| 5.7 | Criar useSuspendUser hook | Mutation status | 5.2 |
| 5.8 | Criar useUpdateUserRoles hook | Mutation roles | 5.2 |
| 5.9 | Implementar UserList | Tabela com filtros | 5.3 |
| 5.10 | Implementar UserStatusBadge | Badge por status | 5.9 |
| 5.11 | Implementar PendingUsersAlert | Alerta no header | 5.4 |
| 5.12 | Implementar UserApprovalDialog | Aprovar/Rejeitar | 5.5 |
| 5.13 | Implementar UserRolesForm | Gerenciar roles | 5.8 |
| 5.14 | Implementar UsersPage | Lista completa | 5.13 |
| 5.15 | Implementar UserDetailPage | Detalhes + ações | 5.13 |
| 5.16 | Adicionar menu Admin no Sidebar | Navegação | 5.1 |
| 5.17 | Testes unitários admin hooks | Todos hooks | 5.8 |
| 5.18 | Testes E2E gestão usuários | Playwright | 5.17 |
| 5.19 | Stories Storybook admin | Componentes | 5.15 |

**Critérios de Aceite:**
- [ ] Lista de usuários com filtros
- [ ] Aprovação/rejeição funcional
- [ ] Suspensão funcional
- [ ] Gestão de roles funcional
- [ ] Alerta de pendentes visível
- [ ] Apenas admin acessa área admin
- [ ] Cobertura ≥ 85%

---

## 8. Convenções de Código

### 8.1 Nomenclatura

| Tipo | Convenção | Exemplo |
|------|-----------|---------|
| **Componentes** | PascalCase | `TransactionForm.tsx` |
| **Hooks** | camelCase, prefixo `use` | `useTransactions.ts` |
| **Services** | camelCase, sufixo `Service` | `transactionService.ts` |
| **Types/Interfaces** | PascalCase | `TransactionDto`, `CreateTransactionRequest` |
| **Pastas** | kebab-case | `data-display/` |
| **Constantes** | UPPER_SNAKE_CASE | `API_BASE_URL` |
| **Variáveis CSS** | kebab-case | `--color-primary` |

### 8.2 Estrutura de Componentes

```typescript
// Ordem recomendada dentro de um componente
// 1. Imports (externos, internos, types, styles)
// 2. Types/Interfaces locais
// 3. Constantes
// 4. Componente
// 5. Sub-componentes (se inline)
// 6. Export

import { useState } from 'react';
import { useTransactions } from '../hooks/useTransactions';
import { Button } from '@/shared/components/ui/button';
import type { TransactionDto } from '../types/transaction.types';

interface TransactionListProps {
  categoryId?: string;
}

export function TransactionList({ categoryId }: TransactionListProps) {
  const { data, isLoading } = useTransactions({ categoryId });
  
  if (isLoading) return <Skeleton />;
  
  return (
    <div>
      {/* ... */}
    </div>
  );
}
```

### 8.3 Imports

```typescript
// Usar path alias @/ para imports absolutos
import { Button } from '@/shared/components/ui/button';
import { useAuth } from '@/features/auth/hooks/useAuth';
import { formatCurrency } from '@/shared/lib/utils/formatters';

// Configurar em tsconfig.json:
{
  "compilerOptions": {
    "paths": {
      "@/*": ["./src/*"]
    }
  }
}
```

### 8.4 Exports

```typescript
// Preferir named exports
export function TransactionList() {}
export function TransactionCard() {}

// Re-export via index.ts
// features/transactions/index.ts
export * from './components/TransactionList';
export * from './hooks/useTransactions';
export * from './pages/TransactionsPage';
```

### 8.5 Props e Types

```typescript
// Usar interface para props de componentes
interface ButtonProps {
  variant?: 'primary' | 'secondary' | 'ghost';
  size?: 'sm' | 'md' | 'lg';
  children: React.ReactNode;
  onClick?: () => void;
}

// Usar type para unions e utilitários
type TransactionType = 'income' | 'expense';
type Nullable<T> = T | null;
```

---

## 9. Estratégia de Testes

### 9.1 Pirâmide de Testes

```
         ┌───────────┐
         │    E2E    │  ← Poucos, fluxos críticos
         │ Playwright│
         └─────┬─────┘
               │
       ┌───────┴───────┐
       │  Integration  │  ← Moderado, interação entre componentes
       │Testing Library│
       └───────┬───────┘
               │
    ┌──────────┴──────────┐
    │       Unit          │  ← Muitos, hooks, utils, lógica
    │      Vitest         │
    └─────────────────────┘
```

### 9.2 O Que Testar

| Camada | O Que Testar | Ferramentas |
|--------|--------------|-------------|
| **Unit** | Hooks, utils, formatters, validators | Vitest |
| **Component** | Renderização, interações, estados | Testing Library |
| **Integration** | Fluxos de formulários, navegação | Testing Library |
| **E2E** | Fluxos críticos completos | Playwright |
| **Visual** | Componentes UI isolados | Storybook |

### 9.3 O Que NÃO Testar

- Lógica de negócio financeira (testada no backend)
- Detalhes de implementação interna
- Bibliotecas de terceiros
- CSS/estilos (use Storybook para visual)

### 9.4 Cobertura Mínima

| Métrica | Mínimo |
|---------|--------|
| Statements | 85% |
| Branches | 80% |
| Functions | 85% |
| Lines | 85% |

### 9.5 Testes E2E Críticos

| Fluxo | Cenários |
|-------|----------|
| **Autenticação** | Login sucesso, login falha, registro, verificação email, logout |
| **Dashboard** | Exibição de dados, navegação |
| **Transações** | CRUD completo, filtros, paginação |
| **Categorias** | CRUD completo |
| **Admin** | Aprovação usuário, gestão roles |

### 9.6 Teste de Segurança (E2E)

```gherkin
Scenario: Código protegido não carrega sem autenticação
  Given usuário não está autenticado
  When usuário acessa diretamente "/dashboard"
  Then apenas bundles "main.js" e "public.js" são carregados
  And usuário é redirecionado para "/login"
  And bundle "protected.js" NÃO foi requisitado

Scenario: Código protegido carrega após autenticação
  Given usuário faz login com sucesso
  And backend retorna status "Active"
  When rota "/dashboard" é acessada
  Then bundle "protected.js" é carregado via lazy loading
  And Dashboard é renderizado
```

---

## 10. Infraestrutura e Deploy

### 10.1 Arquitetura de Deploy

```
┌─────────────────────────────────────────────────────────────────┐
│                      GITHUB ACTIONS                             │
│                                                                 │
│  ┌─────────────┐    ┌─────────────┐    ┌─────────────────────┐ │
│  │    Build    │───▶│    Test     │───▶│  Deploy to OCI      │ │
│  │  (npm run   │    │  (vitest,   │    │  (Object Storage)   │ │
│  │   build)    │    │  playwright)│    │                     │ │
│  └─────────────┘    └─────────────┘    └─────────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
                                                    │
                                                    ▼
┌─────────────────────────────────────────────────────────────────┐
│                    OCI OBJECT STORAGE                           │
│                                                                 │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │  Bucket: l2sledger-frontend-{env}                       │   │
│  │  ├── index.html                                         │   │
│  │  ├── assets/                                            │   │
│  │  │   ├── main-[hash].js                                 │   │
│  │  │   ├── public-[hash].js                               │   │
│  │  │   ├── protected-[hash].js                            │   │
│  │  │   ├── admin-[hash].js                                │   │
│  │  │   └── *.css                                          │   │
│  │  └── manifest.json                                      │   │
│  └─────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────────┐
│                        CLOUDFLARE                               │
│                                                                 │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │  - DNS                                                   │   │
│  │  - SSL/TLS                                               │   │
│  │  - CDN Cache                                             │   │
│  │  - DDoS Protection                                       │   │
│  │  - Page Rules (SPA routing)                              │   │
│  └─────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
                                │
                                ▼
                         ┌─────────────┐
                         │   Usuário   │
                         └─────────────┘
```

### 10.2 Ambientes

| Ambiente | URL | Branch | Deploy |
|----------|-----|--------|--------|
| **DEV** | dev.l2sledger.com | `develop` | Automático |
| **DEMO** | demo.l2sledger.com | `release/*` | Manual |
| **PROD** | app.l2sledger.com | `main` | Manual com aprovação |

### 10.3 Dockerfile

```dockerfile
# Build stage
FROM node:20-alpine AS builder
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
RUN npm run build

# Production stage (para Caddy local/dev)
FROM caddy:2-alpine
COPY --from=builder /app/dist /srv
COPY Caddyfile /etc/caddy/Caddyfile
EXPOSE 80
CMD ["caddy", "run", "--config", "/etc/caddy/Caddyfile"]
```

### 10.4 Caddyfile

```caddyfile
:80 {
    root * /srv
    
    # SPA routing - todas as rotas vão para index.html
    try_files {path} /index.html
    
    file_server
    
    # Cache para assets com hash
    @assets {
        path /assets/*
    }
    header @assets Cache-Control "public, max-age=31536000, immutable"
    
    # Sem cache para index.html
    @html {
        path /index.html
    }
    header @html Cache-Control "no-cache, no-store, must-revalidate"
}
```

### 10.5 Variáveis de Ambiente

```bash
# .env.example
VITE_API_URL=http://localhost:5000
VITE_FIREBASE_API_KEY=your-api-key
VITE_FIREBASE_AUTH_DOMAIN=your-project.firebaseapp.com
VITE_FIREBASE_PROJECT_ID=your-project-id
VITE_FIREBASE_APP_ID=your-app-id
```

---

## 11. Roadmap Pós-MVP

### 11.1 Funcionalidades Futuras

| Prioridade | Feature | Dependência |
|------------|---------|-------------|
| Alta | Admin - Auditoria | Backend pronto |
| Alta | Contexto Comercial | Backend /api/v1/me/commercial-context |
| Alta | Períodos Financeiros | Fase 4 + Backend |
| Média | Exportações | Fase 4 + Backend |
| Média | Relatórios | Dashboard + Backend |
| Média | Multi-idioma (i18n) | react-i18next |
| Baixa | Google AdSense | Contexto Comercial |
| Baixa | Tema Dark/Light | Design System |
| Baixa | Notificações Push | PWA + Backend |

### 11.2 Melhorias Técnicas

| Prioridade | Melhoria |
|------------|----------|
| Alta | Otimização de bundle size |
| Alta | Testes de acessibilidade (a11y) |
| Média | Monitoring (Sentry, LogRocket) |
| Média | Analytics (Plausible, Posthog) |
| Baixa | Micro-frontends (se necessário) |

---

## 📌 Referências

- [ADR Index](../../adr/adr-index.md)
- [Frontend API Integration Guide](../frontend-api-integration-guide.md)
- [User Status Plan](../api-planning/user-status-plan.md)
- [Governance Flow](../../governance/flow-planejar-provar-executar.md)

---

> **Este documento foi aprovado em 2026-01-25.**
> 
> **Próximo passo:** Execução da Fase 0 após conclusão do plano de backend.
