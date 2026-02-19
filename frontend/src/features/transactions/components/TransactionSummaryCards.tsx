import { Card, CardContent } from '@/shared/components/ui/card';
import { formatCurrency } from '@/shared/lib/utils/formatters';
import { TrendingUp, TrendingDown, Wallet } from 'lucide-react';
import { cn } from '@/shared/lib/utils/cn';

interface TransactionSummaryCardsProps {
  totalIncome: number;
  totalExpense: number;
  balance: number;
}

export function TransactionSummaryCards({
  totalIncome,
  totalExpense,
  balance,
}: TransactionSummaryCardsProps) {
  return (
    <div className="grid gap-4 md:grid-cols-3">
      <Card>
        <CardContent className="flex items-center gap-4 p-4">
          <div className="rounded-full bg-green-100 p-3 dark:bg-green-900/30">
            <TrendingUp className="h-5 w-5 text-green-600 dark:text-green-400" />
          </div>
          <div>
            <p className="text-sm text-muted-foreground">Receitas</p>
            <p className="text-xl font-bold text-green-600 dark:text-green-400 font-mono tabular-nums">
              {formatCurrency(totalIncome)}
            </p>
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardContent className="flex items-center gap-4 p-4">
          <div className="rounded-full bg-red-100 p-3 dark:bg-red-900/30">
            <TrendingDown className="h-5 w-5 text-red-600 dark:text-red-400" />
          </div>
          <div>
            <p className="text-sm text-muted-foreground">Despesas</p>
            <p className="text-xl font-bold text-red-600 dark:text-red-400 font-mono tabular-nums">
              {formatCurrency(totalExpense)}
            </p>
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardContent className="flex items-center gap-4 p-4">
          <div className={cn(
            'rounded-full p-3',
            balance >= 0 ? 'bg-blue-100 dark:bg-blue-900/30' : 'bg-orange-100 dark:bg-orange-900/30'
          )}>
            <Wallet className={cn(
              'h-5 w-5',
              balance >= 0 ? 'text-blue-600 dark:text-blue-400' : 'text-orange-600 dark:text-orange-400'
            )} />
          </div>
          <div>
            <p className="text-sm text-muted-foreground">Saldo</p>
            <p className={cn(
              'text-xl font-bold font-mono tabular-nums',
              balance >= 0 ? 'text-blue-600 dark:text-blue-400' : 'text-orange-600 dark:text-orange-400'
            )}>
              {formatCurrency(balance)}
            </p>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
