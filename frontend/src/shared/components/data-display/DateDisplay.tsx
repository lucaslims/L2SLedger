import { formatDate, formatDateTime, formatRelativeDate } from '@/shared/lib/utils/formatters';
import { cn } from '@/shared/lib/utils/cn';

interface DateDisplayProps {
  date: string | Date;
  format?: 'date' | 'datetime' | 'relative';
  className?: string;
}

/**
 * Exibe data formatada no padrão brasileiro
 * Suporta: data, data+hora, relativa
 */
export function DateDisplay({ date, format: displayFormat = 'date', className }: DateDisplayProps) {
  const formatted =
    displayFormat === 'datetime'
      ? formatDateTime(date)
      : displayFormat === 'relative'
        ? formatRelativeDate(date)
        : formatDate(date);

  return (
    <time
      dateTime={typeof date === 'string' ? date : date.toISOString()}
      className={cn('text-sm', className)}
    >
      {formatted}
    </time>
  );
}
