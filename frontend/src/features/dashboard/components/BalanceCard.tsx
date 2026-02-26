import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { formatCurrency } from '@/shared/lib/utils/formatters';
import { TrendingUp, TrendingDown, Wallet } from 'lucide-react';
import { cn } from '@/shared/lib/utils/cn';

interface BalanceCardProps {
  /** Tipo do card: receita, despesa ou saldo */
  type: 'income' | 'expense' | 'balance';
  /** Valor monetário */
  value: number;
  /** Label exibido no header */
  label: string;
  /** Indica se houve erro ao carregar os dados */
  error?: boolean;
}

const cardConfig = {
  income: {
    icon: TrendingUp,
    textColor: 'text-income',
    bgColor: 'bg-income/10',
  },
  expense: {
    icon: TrendingDown,
    textColor: 'text-expense',
    bgColor: 'bg-expense/10',
  },
  balance: {
    icon: Wallet,
    textColor: 'text-primary',
    bgColor: 'bg-primary/10',
  },
} as const;

/**
 * BalanceCard
 *
 * Card para exibição de valores financeiros no dashboard.
 * Suporta 3 tipos: income (receita), expense (despesa), balance (saldo).
 *
 * Não contém lógica financeira — apenas formatação e exibição.
 */
export function BalanceCard({ type, value, label, error = false }: BalanceCardProps) {
  const { icon: Icon, textColor, bgColor } = cardConfig[type];

  return (
    <Card>
      <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
        <CardTitle className="text-sm font-medium">{label}</CardTitle>
        <div className={cn('rounded-full p-2', bgColor)}>
          <Icon className={cn('h-4 w-4', textColor)} />
        </div>
      </CardHeader>
      <CardContent>
        {error ? (
          <div className="space-y-1">
            <div className="font-mono text-2xl font-bold text-destructive">—</div>
            <p className="text-xs text-muted-foreground">Erro ao carregar</p>
          </div>
        ) : (
          <div
            className={cn('font-mono text-2xl font-bold', textColor)}
            data-testid={`balance-value-${type}`}
          >
            {formatCurrency(value)}
          </div>
        )}
      </CardContent>
    </Card>
  );
}
