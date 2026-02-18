import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/shared/components/ui/table';
import { Button } from '@/shared/components/ui/button';
import { Badge } from '@/shared/components/ui/badge';
import { Edit, Trash } from 'lucide-react';
import { AmountDisplay } from '@/shared/components/data-display/AmountDisplay';
import { DateDisplay } from '@/shared/components/data-display/DateDisplay';
import { Skeleton } from '@/shared/components/ui/skeleton';
import { TransactionTypeLabelMap } from '../types/transaction.types';
import type { TransactionDto } from '../types/transaction.types';

interface TransactionListProps {
  transactions: TransactionDto[];
  isLoading?: boolean;
  onEdit: (id: string) => void;
  onDelete: (transaction: TransactionDto) => void;
}

export function TransactionList({ transactions, isLoading, onEdit, onDelete }: TransactionListProps) {
  if (isLoading) {
    return (
      <div className="space-y-3">
        {Array.from({ length: 5 }).map((_, i) => (
          <Skeleton key={i} className="h-12 w-full" />
        ))}
      </div>
    );
  }

  if (!transactions.length) {
    return (
      <div className="flex flex-col items-center justify-center py-12 text-center">
        <p className="text-lg font-medium text-muted-foreground">
          Nenhuma transação encontrada
        </p>
        <p className="text-sm text-muted-foreground">
          Crie sua primeira transação para começar a controlar seu fluxo de caixa.
        </p>
      </div>
    );
  }

  return (
    <>
      {/* Desktop table */}
      <div className="hidden md:block">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Data</TableHead>
              <TableHead>Descrição</TableHead>
              <TableHead>Categoria</TableHead>
              <TableHead>Tipo</TableHead>
              <TableHead className="text-right">Valor</TableHead>
              <TableHead className="text-right">Ações</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {transactions.map((transaction) => (
              <TableRow key={transaction.id}>
                <TableCell>
                  <DateDisplay date={transaction.transactionDate} />
                </TableCell>
                <TableCell className="font-medium max-w-[200px] truncate">
                  {transaction.description}
                </TableCell>
                <TableCell>{transaction.categoryName}</TableCell>
                <TableCell>
                  <Badge variant={transaction.type === 1 ? 'default' : 'destructive'}>
                    {TransactionTypeLabelMap[transaction.type]}
                  </Badge>
                </TableCell>
                <TableCell className="text-right">
                  <AmountDisplay amount={transaction.amount} type={transaction.type} />
                </TableCell>
                <TableCell className="text-right">
                  <Button
                    variant="ghost"
                    size="icon"
                    onClick={() => onEdit(transaction.id)}
                    aria-label={`Editar ${transaction.description}`}
                  >
                    <Edit className="h-4 w-4" />
                  </Button>
                  <Button
                    variant="ghost"
                    size="icon"
                    onClick={() => onDelete(transaction)}
                    aria-label={`Excluir ${transaction.description}`}
                  >
                    <Trash className="h-4 w-4" />
                  </Button>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </div>

      {/* Mobile cards */}
      <div className="space-y-3 md:hidden">
        {transactions.map((transaction) => (
          <TransactionCard
            key={transaction.id}
            transaction={transaction}
            onEdit={() => onEdit(transaction.id)}
            onDelete={() => onDelete(transaction)}
          />
        ))}
      </div>
    </>
  );
}

// Inline mobile card component
function TransactionCard({
  transaction,
  onEdit,
  onDelete,
}: {
  transaction: TransactionDto;
  onEdit: () => void;
  onDelete: () => void;
}) {
  return (
    <div className="flex items-center justify-between rounded-lg border p-4">
      <div className="flex-1 min-w-0">
        <div className="flex items-center gap-2">
          <p className="font-medium truncate">{transaction.description}</p>
          <Badge variant={transaction.type === 1 ? 'default' : 'destructive'} className="shrink-0">
            {TransactionTypeLabelMap[transaction.type]}
          </Badge>
        </div>
        <div className="flex items-center gap-2 mt-1 text-sm text-muted-foreground">
          <DateDisplay date={transaction.transactionDate} />
          <span>•</span>
          <span>{transaction.categoryName}</span>
        </div>
      </div>
      <div className="flex items-center gap-2 ml-4">
        <AmountDisplay amount={transaction.amount} type={transaction.type} showSign={true} />
        <div className="flex gap-1">
          <Button variant="ghost" size="icon" onClick={onEdit} aria-label="Editar">
            <Edit className="h-4 w-4" />
          </Button>
          <Button variant="ghost" size="icon" onClick={onDelete} aria-label="Excluir">
            <Trash className="h-4 w-4" />
          </Button>
        </div>
      </div>
    </div>
  );
}
