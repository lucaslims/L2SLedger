import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from '@/shared/components/ui/card';
import { useRecentTransactions } from '../hooks/useRecentTransactions';
import { formatCurrency, formatDate } from '@/shared/lib/utils/formatters';
import { cn } from '@/shared/lib/utils/cn';
import { Skeleton } from '@/shared/components/ui/skeleton';
import { Button } from '@/shared/components/ui/button';
import { useNavigate } from 'react-router-dom';
import { ROUTES } from '@/shared/lib/utils/constants';
import { ArrowRight } from 'lucide-react';

/**
 * RecentTransactions
 * 
 * Preview das últimas transações no dashboard.
 * Exibe até 5 transações recentes com tipo, valor e data.
 * 
 * Não contém lógica financeira — apenas exibição.
 */
export function RecentTransactions() {
  const { data: transactions, isLoading, isError } = useRecentTransactions();
  const navigate = useNavigate();

  if (isLoading) {
    return (
      <Card>
        <CardHeader>
          <CardTitle>Transações Recentes</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          {Array.from({ length: 5 }).map((_, i) => (
            <div key={i} className="flex items-center justify-between">
              <div className="space-y-1">
                <Skeleton className="h-4 w-32" />
                <Skeleton className="h-3 w-20" />
              </div>
              <Skeleton className="h-4 w-24" />
            </div>
          ))}
        </CardContent>
      </Card>
    );
  }

  if (isError) {
    return (
      <Card>
        <CardHeader>
          <CardTitle>Transações Recentes</CardTitle>
        </CardHeader>
        <CardContent>
          <p className="text-sm text-muted-foreground">
            Erro ao carregar transações recentes
          </p>
        </CardContent>
      </Card>
    );
  }

  return (
    <Card>
      <CardHeader className="flex flex-row items-center justify-between">
        <CardTitle>Transações Recentes</CardTitle>
        <Button
          variant="ghost"
          size="sm"
          onClick={() => navigate(ROUTES.TRANSACTIONS)}
          className="text-xs"
        >
          Ver todas
          <ArrowRight className="ml-1 h-3 w-3" />
        </Button>
      </CardHeader>
      <CardContent>
        {!transactions || transactions.length === 0 ? (
          <p className="text-sm text-muted-foreground">
            Nenhuma transação encontrada
          </p>
        ) : (
          <div className="space-y-4">
            {transactions.map((transaction) => (
              <div
                key={transaction.id}
                className="flex items-center justify-between"
              >
                <div className="space-y-0.5">
                  <p className="text-sm font-medium">
                    {transaction.description}
                  </p>
                  <p className="text-xs text-muted-foreground">
                    {transaction.categoryName} •{' '}
                    {formatDate(transaction.date)}
                  </p>
                </div>
                <span
                  className={cn(
                    'text-sm font-mono font-semibold',
                    transaction.type === 'Income'
                      ? 'text-income'
                      : 'text-expense'
                  )}
                >
                  {transaction.type === 'Income' ? '+' : '-'}
                  {formatCurrency(transaction.amount)}
                </span>
              </div>
            ))}
          </div>
        )}
      </CardContent>
    </Card>
  );
}
