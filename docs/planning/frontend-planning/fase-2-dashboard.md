# Fase 2: Dashboard — Frontend L2SLedger

> **Estimativa:** 12 horas  
> **Dependência:** Fase 1 (Autenticação) completa  
> **Status:** Implementado

---

## 🎯 Objetivo

Implementar o dashboard principal do sistema com:
- Layout autenticado (Header, Sidebar, Navigation)
- Exibição de saldos (receitas, despesas, saldo atual)
- Gráfico de evolução financeira
- Preview de transações recentes
- Ações rápidas
- Responsividade mobile-first

---

## 📋 Tasks Detalhadas

### 2.1 Layout Autenticado

#### `AppLayout.tsx`

```typescript
import { Header } from './Header';
import { Sidebar } from './Sidebar';
import { MobileNav } from './MobileNav';
import { useMediaQuery } from '@/shared/hooks/useMediaQuery';

interface AppLayoutProps {
  children: React.ReactNode;
}

export function AppLayout({ children }: AppLayoutProps) {
  const isMobile = useMediaQuery('(max-width: 768px)');

  return (
    <div className="flex min-h-screen bg-background">
      {/* Sidebar Desktop */}
      {!isMobile && <Sidebar />}

      {/* Main Content */}
      <div className="flex flex-1 flex-col">
        <Header />
        
        <main className="flex-1 overflow-y-auto p-4 md:p-6 lg:p-8">
          {children}
        </main>
      </div>

      {/* Mobile Navigation */}
      {isMobile && <MobileNav />}
    </div>
  );
}
```

#### `Header.tsx`

```typescript
import { useAuth } from '@/app/providers/useAuth';
import { useLogout } from '@/features/auth/hooks/useLogout';
import { Button } from '@/shared/components/ui/button';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/shared/components/ui/dropdown-menu';
import { User, LogOut, Settings } from 'lucide-react';

export function Header() {
  const { currentUser } = useAuth();
  const { mutate: logout } = useLogout();

  return (
    <header className="sticky top-0 z-40 border-b bg-background/95 backdrop-blur supports-[backdrop-filter]:bg-background/60">
      <div className="flex h-16 items-center justify-between px-4 md:px-6">
        {/* Logo (mobile only) */}
        <div className="md:hidden">
          <h1 className="text-xl font-bold text-primary">L2SLedger</h1>
        </div>

        {/* Spacer */}
        <div className="flex-1" />

        {/* User Menu */}
        <DropdownMenu>
          <DropdownMenuTrigger asChild>
            <Button variant="ghost" size="icon">
              <User className="h-5 w-5" />
            </Button>
          </DropdownMenuTrigger>
          <DropdownMenuContent align="end">
            <DropdownMenuLabel>
              <div className="flex flex-col space-y-1">
                <p className="text-sm font-medium">{currentUser?.displayName}</p>
                <p className="text-xs text-muted-foreground">{currentUser?.email}</p>
              </div>
            </DropdownMenuLabel>
            <DropdownMenuSeparator />
            <DropdownMenuItem>
              <Settings className="mr-2 h-4 w-4" />
              Configurações
            </DropdownMenuItem>
            <DropdownMenuSeparator />
            <DropdownMenuItem onClick={() => logout()} className="text-destructive">
              <LogOut className="mr-2 h-4 w-4" />
              Sair
            </DropdownMenuItem>
          </DropdownMenuContent>
        </DropdownMenu>
      </div>
    </header>
  );
}
```

#### `Sidebar.tsx`

```typescript
import { NavLink } from 'react-router-dom';
import { ROUTES } from '@/shared/lib/utils/constants';
import { Home, CreditCard, FolderOpen, Users } from 'lucide-react';
import { cn } from '@/shared/lib/utils/cn';
import { useAuth } from '@/app/providers/useAuth';
import { ROLES } from '@/shared/lib/utils/constants';

const navItems = [
  { to: ROUTES.DASHBOARD, label: 'Dashboard', icon: Home },
  { to: ROUTES.TRANSACTIONS, label: 'Transações', icon: CreditCard },
  { to: ROUTES.CATEGORIES, label: 'Categorias', icon: FolderOpen },
];

export function Sidebar() {
  const { currentUser } = useAuth();
  const isAdmin = currentUser?.roles.includes(ROLES.ADMIN);

  return (
    <aside className="flex w-64 flex-col border-r bg-card">
      {/* Logo */}
      <div className="flex h-16 items-center border-b px-6">
        <h1 className="text-2xl font-bold text-primary">L2SLedger</h1>
      </div>

      {/* Navigation */}
      <nav className="flex-1 space-y-1 p-4">
        {navItems.map((item) => (
          <NavLink
            key={item.to}
            to={item.to}
            className={({ isActive }) =>
              cn(
                'flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium transition-colors',
                isActive
                  ? 'bg-primary text-primary-foreground'
                  : 'text-muted-foreground hover:bg-accent hover:text-accent-foreground'
              )
            }
          >
            <item.icon className="h-5 w-5" />
            {item.label}
          </NavLink>
        ))}

        {/* Admin Section */}
        {isAdmin && (
          <>
            <div className="my-4 border-t pt-4">
              <p className="px-3 text-xs font-semibold text-muted-foreground">
                ADMINISTRAÇÃO
              </p>
            </div>
            <NavLink
              to={ROUTES.ADMIN_USERS}
              className={({ isActive }) =>
                cn(
                  'flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium transition-colors',
                  isActive
                    ? 'bg-primary text-primary-foreground'
                    : 'text-muted-foreground hover:bg-accent hover:text-accent-foreground'
                )
              }
            >
              <Users className="h-5 w-5" />
              Usuários
            </NavLink>
          </>
        )}
      </nav>
    </aside>
  );
}
```

### 2.2 Hooks de Dashboard

#### `useBalances.ts`

```typescript
import { useQuery } from '@tanstack/react-query';
import { apiClient } from '@/shared/lib/api/client';
import { API_ENDPOINTS } from '@/shared/lib/api/endpoints';
import { QUERY_KEYS } from '@/shared/lib/utils/constants';

interface BalancesResponse {
  totalIncome: number;
  totalExpense: number;
  currentBalance: number;
  period: {
    start: string;
    end: string;
  };
}

export function useBalances() {
  return useQuery({
    queryKey: [QUERY_KEYS.BALANCES],
    queryFn: () => apiClient.get<BalancesResponse>(API_ENDPOINTS.BALANCES),
  });
}
```

#### `useDailyBalances.ts`

```typescript
import { useQuery } from '@tanstack/react-query';
import { apiClient } from '@/shared/lib/api/client';
import { API_ENDPOINTS } from '@/shared/lib/api/endpoints';
import { QUERY_KEYS } from '@/shared/lib/utils/constants';

interface DailyBalance {
  date: string;
  income: number;
  expense: number;
  balance: number;
}

export function useDailyBalances(startDate?: string, endDate?: string) {
  return useQuery({
    queryKey: [QUERY_KEYS.DAILY_BALANCES, startDate, endDate],
    queryFn: () =>
      apiClient.get<DailyBalance[]>(API_ENDPOINTS.DAILY_BALANCES, {
        params: { startDate, endDate },
      }),
  });
}
```

### 2.3 Componentes de Dashboard

#### `BalanceCard.tsx`

```typescript
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { formatCurrency } from '@/shared/lib/utils/formatters';
import { TrendingUp, TrendingDown, Wallet } from 'lucide-react';
import { cn } from '@/shared/lib/utils/cn';

interface BalanceCardProps {
  type: 'income' | 'expense' | 'balance';
  value: number;
  label: string;
}

export function BalanceCard({ type, value, label }: BalanceCardProps) {
  const icons = {
    income: TrendingUp,
    expense: TrendingDown,
    balance: Wallet,
  };

  const colors = {
    income: 'text-income',
    expense: 'text-expense',
    balance: 'text-primary',
  };

  const bgColors = {
    income: 'bg-income/10',
    expense: 'bg-expense/10',
    balance: 'bg-primary/10',
  };

  const Icon = icons[type];

  return (
    <Card>
      <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
        <CardTitle className="text-sm font-medium">{label}</CardTitle>
        <div className={cn('rounded-full p-2', bgColors[type])}>
          <Icon className={cn('h-4 w-4', colors[type])} />
        </div>
      </CardHeader>
      <CardContent>
        <div className={cn('text-2xl font-bold', colors[type])}>
          {formatCurrency(value)}
        </div>
      </CardContent>
    </Card>
  );
}
```

#### `BalanceChart.tsx`

```typescript
import { AreaChart } from '@tremor/react';
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { useDailyBalances } from '../hooks/useDailyBalances';
import { formatCurrency } from '@/shared/lib/utils/formatters';

export function BalanceChart() {
  const { data, isLoading } = useDailyBalances();

  if (isLoading) return <div>Carregando...</div>;

  const chartData = data?.map((item) => ({
    data: item.date,
    Receitas: item.income,
    Despesas: item.expense,
    Saldo: item.balance,
  })) || [];

  return (
    <Card>
      <CardHeader>
        <CardTitle>Evolução Financeira</CardTitle>
      </CardHeader>
      <CardContent>
        <AreaChart
          data={chartData}
          index="data"
          categories={['Receitas', 'Despesas', 'Saldo']}
          colors={['green', 'red', 'blue']}
          valueFormatter={formatCurrency}
          showLegend
          showGridLines
          className="h-72"
        />
      </CardContent>
    </Card>
  );
}
```

#### `QuickActions.tsx`

```typescript
import { Button } from '@/shared/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { Plus, Download, Settings } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import { ROUTES } from '@/shared/lib/utils/constants';

export function QuickActions() {
  const navigate = useNavigate();

  return (
    <Card>
      <CardHeader>
        <CardTitle>Ações Rápidas</CardTitle>
      </CardHeader>
      <CardContent className="grid gap-2">
        <Button
          onClick={() => navigate(`${ROUTES.TRANSACTIONS}/new`)}
          className="justify-start"
        >
          <Plus className="mr-2 h-4 w-4" />
          Nova Transação
        </Button>
        <Button variant="outline" className="justify-start">
          <Download className="mr-2 h-4 w-4" />
          Exportar Dados
        </Button>
        <Button variant="outline" className="justify-start">
          <Settings className="mr-2 h-4 w-4" />
          Configurações
        </Button>
      </CardContent>
    </Card>
  );
}
```

### 2.4 DashboardPage Completa

```typescript
import { AppLayout } from '@/shared/components/layout/AppLayout';
import { BalanceCard } from '../components/BalanceCard';
import { BalanceChart } from '../components/BalanceChart';
import { QuickActions } from '../components/QuickActions';
import { useBalances } from '../hooks/useBalances';
import { Skeleton } from '@/shared/components/ui/skeleton';

export default function DashboardPage() {
  const { data: balances, isLoading } = useBalances();

  if (isLoading) {
    return (
      <AppLayout>
        <div className="space-y-6">
          <Skeleton className="h-32 w-full" />
          <Skeleton className="h-96 w-full" />
        </div>
      </AppLayout>
    );
  }

  return (
    <AppLayout>
      <div className="space-y-6">
        {/* Header */}
        <div>
          <h1 className="text-3xl font-bold">Dashboard</h1>
          <p className="text-muted-foreground">
            Visão geral das suas finanças
          </p>
        </div>

        {/* Balance Cards */}
        <div className="grid gap-4 md:grid-cols-3">
          <BalanceCard
            type="income"
            value={balances?.totalIncome || 0}
            label="Total de Receitas"
          />
          <BalanceCard
            type="expense"
            value={balances?.totalExpense || 0}
            label="Total de Despesas"
          />
          <BalanceCard
            type="balance"
            value={balances?.currentBalance || 0}
            label="Saldo Atual"
          />
        </div>

        {/* Charts and Actions */}
        <div className="grid gap-6 lg:grid-cols-3">
          <div className="lg:col-span-2">
            <BalanceChart />
          </div>
          <div>
            <QuickActions />
          </div>
        </div>
      </div>
    </AppLayout>
  );
}
```

---

## 📦 Arquivos a Criar

```
shared/components/layout/
├── AppLayout.tsx               ✅ CRIAR
├── Header.tsx                  ✅ CRIAR
├── Sidebar.tsx                 ✅ CRIAR
└── MobileNav.tsx               ✅ CRIAR

shared/components/ui/
├── dropdown-menu.tsx           ✅ INSTALAR (shadcn)
├── skeleton.tsx                ✅ INSTALAR (shadcn)
└── card.tsx                    ✅ INSTALAR (shadcn)

shared/hooks/
└── useMediaQuery.ts            ✅ CRIAR

features/dashboard/
├── components/
│   ├── BalanceCard.tsx         ✅ CRIAR
│   ├── BalanceChart.tsx        ✅ CRIAR
│   ├── QuickActions.tsx        ✅ CRIAR
│   └── RecentTransactions.tsx  ✅ CRIAR
├── hooks/
│   ├── useBalances.ts          ✅ CRIAR
│   └── useDailyBalances.ts     ✅ CRIAR
├── pages/
│   └── DashboardPage.tsx       🔄 ATUALIZAR
└── __tests__/
    ├── useBalances.test.ts     ✅ CRIAR
    └── BalanceCard.test.tsx    ✅ CRIAR

tests/e2e/
└── dashboard.spec.ts           ✅ CRIAR
```

---

## ✅ Critérios de Aceite

- [ ] Dashboard exibe dados reais da API
- [ ] Gráfico de evolução funcional
- [ ] Layout responsivo (mobile-first)
- [ ] Navegação funcional (Sidebar + MobileNav)
- [ ] Loading states implementados
- [ ] Header com menu de usuário funcional
- [ ] Logout funcional
- [ ] Cobertura de testes ≥ 85%
- [ ] Testes E2E passando
- [ ] Storybook com componentes de dashboard

---

---

## ⚠️ Considerações ADRs e Comerciais

### ADR-042-A — Contexto Comercial
> **IMPORTANTE:** Após MVP, o Dashboard deverá consumir `GET /api/v1/me/commercial-context` para:
> - Verificar limite de lançamentos
> - Exibir anúncios (plano FREE)
> - Mostrar features disponíveis

### ADR-040 — Testes de Frontend
- ✅ Testes de comportamento (não implementação)
- ✅ Mocks baseados em contratos

### Regra Comercial (plans-and-features.md)
- FREE: Limite de 100 lançamentos/mês
- PRO: Limite de 1.000 lançamentos/mês
- BUSINESS: Ilimitado

> O Dashboard deve ser preparado para exibir alertas de limite futuramente.

---

**Próximo passo:** Fase 3 — Categorias
