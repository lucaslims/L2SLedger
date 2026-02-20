import { AppLayout } from '@/shared/components/layout/AppLayout';
import { BalanceCard } from '../components/BalanceCard';
import { BalanceChart } from '../components/BalanceChart';
import { QuickActions } from '../components/QuickActions';
import { RecentTransactions } from '../components/RecentTransactions';
import { useBalances } from '../hooks/useBalances';
import { Skeleton } from '@/shared/components/ui/skeleton';

/**
 * DashboardPage
 *
 * Página principal do sistema autenticado.
 * Exibe:
 * - Cards de saldo (receitas, despesas, saldo atual)
 * - Gráfico de evolução financeira
 * - Transações recentes
 * - Ações rápidas
 *
 * Não contém lógica financeira — consome API via hooks.
 */
export default function DashboardPage() {
  const { data: balances, isLoading } = useBalances();

  return (
    <AppLayout>
      <div className="space-y-6">
        {/* Header */}
        <div>
          <h1 className="text-3xl font-bold">Dashboard</h1>
          <p className="text-muted-foreground">Visão geral das suas finanças</p>
        </div>

        {/* Balance Cards */}
        {isLoading ? (
          <div className="grid gap-4 md:grid-cols-3">
            <Skeleton className="h-32 w-full" />
            <Skeleton className="h-32 w-full" />
            <Skeleton className="h-32 w-full" />
          </div>
        ) : (
          <div className="grid gap-4 md:grid-cols-3">
            <BalanceCard
              type="income"
              value={balances?.totalIncome ?? 0}
              label="Total de Receitas"
            />
            <BalanceCard
              type="expense"
              value={balances?.totalExpense ?? 0}
              label="Total de Despesas"
            />
            <BalanceCard type="balance" value={balances?.currentBalance ?? 0} label="Saldo Atual" />
          </div>
        )}

        {/* Charts and Actions */}
        <div className="grid gap-6 lg:grid-cols-3">
          <div className="lg:col-span-2">
            <BalanceChart />
          </div>
          <div className="space-y-6">
            <QuickActions />
          </div>
        </div>

        {/* Recent Transactions */}
        <RecentTransactions />
      </div>
    </AppLayout>
  );
}
