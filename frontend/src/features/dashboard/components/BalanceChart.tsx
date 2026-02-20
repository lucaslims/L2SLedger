import { AreaChart } from '@tremor/react';
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { useDailyBalances } from '../hooks/useDailyBalances';
import { formatCurrency } from '@/shared/lib/utils/formatters';
import { formatDate } from '@/shared/lib/utils/formatters';
import { Skeleton } from '@/shared/components/ui/skeleton';

/**
 * BalanceChart
 *
 * Gráfico de evolução financeira diária.
 * Exibe receitas, despesas e saldo ao longo do tempo.
 *
 * Usa Tremor AreaChart para renderização.
 * Não contém lógica financeira — apenas visualização.
 */
export function BalanceChart() {
  const { data, isLoading, isError } = useDailyBalances();

  if (isLoading) {
    return (
      <Card>
        <CardHeader>
          <CardTitle>Evolução Financeira</CardTitle>
        </CardHeader>
        <CardContent>
          <Skeleton className="h-72 w-full" />
        </CardContent>
      </Card>
    );
  }

  if (isError) {
    return (
      <Card>
        <CardHeader>
          <CardTitle>Evolução Financeira</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="flex h-72 items-center justify-center text-muted-foreground">
            Erro ao carregar dados do gráfico
          </div>
        </CardContent>
      </Card>
    );
  }

  const chartData =
    data?.map((item) => ({
      data: formatDate(item.date),
      Receitas: item.income,
      Despesas: item.expense,
      Saldo: item.balance,
    })) || [];

  // Se não há dados, exibir estado vazio
  if (chartData.length === 0) {
    return (
      <Card>
        <CardHeader>
          <CardTitle>Evolução Financeira</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="flex h-72 items-center justify-center text-muted-foreground">
            Nenhum dado disponível para o período
          </div>
        </CardContent>
      </Card>
    );
  }

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
          colors={['emerald', 'red', 'blue']}
          valueFormatter={formatCurrency}
          showLegend
          showGridLines
          className="h-72"
        />
      </CardContent>
    </Card>
  );
}
