import { formatCurrency } from '@/shared/lib/utils/formatters';
import { cn } from '@/shared/lib/utils/cn';

interface AmountDisplayProps {
  amount: number;
  type: 'Income' | 'Expense' | 1 | 2;
  className?: string;
  showSign?: boolean;
}

/**
 * Exibe valor monetário formatado em BRL com cor semântica
 * Income = verde (+), Expense = vermelho (-)
 * 
 * Aceita tipo como string ('Income'/'Expense') ou int (1/2) do backend
 */
export function AmountDisplay({ amount, type, className, showSign = true }: AmountDisplayProps) {
  const isIncome = type === 'Income' || type === 1;

  return (
    <span
      className={cn(
        'font-semibold font-mono tabular-nums',
        isIncome ? 'text-green-600 dark:text-green-400' : 'text-red-600 dark:text-red-400',
        className
      )}
    >
      {showSign && (isIncome ? '+ ' : '- ')}
      {formatCurrency(Math.abs(amount))}
    </span>
  );
}
