import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@/shared/components/ui/alert-dialog';
import { useDeleteTransaction } from '../hooks/useDeleteTransaction';
import { AmountDisplay } from '@/shared/components/data-display/AmountDisplay';
import type { TransactionDto } from '../types/transaction.types';

interface TransactionDeleteDialogProps {
  transaction: TransactionDto;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export function TransactionDeleteDialog({
  transaction,
  open,
  onOpenChange,
}: TransactionDeleteDialogProps) {
  const { mutate: deleteTransaction, isPending } = useDeleteTransaction();

  const handleDelete = () => {
    deleteTransaction(transaction.id, {
      onSuccess: () => {
        onOpenChange(false);
      },
    });
  };

  return (
    <AlertDialog open={open} onOpenChange={onOpenChange}>
      <AlertDialogContent>
        <AlertDialogHeader>
          <AlertDialogTitle>Excluir Transação</AlertDialogTitle>
          <AlertDialogDescription>
            Tem certeza que deseja excluir a transação{' '}
            <strong>&quot;{transaction.description}&quot;</strong> no valor de{' '}
            <AmountDisplay amount={transaction.amount} type={transaction.type} showSign={false} />?
            Esta ação não pode ser desfeita.
          </AlertDialogDescription>
        </AlertDialogHeader>
        <AlertDialogFooter>
          <AlertDialogCancel disabled={isPending}>Cancelar</AlertDialogCancel>
          <AlertDialogAction
            onClick={handleDelete}
            disabled={isPending}
            className="bg-destructive text-destructive-foreground hover:bg-destructive/90"
          >
            {isPending ? 'Excluindo...' : 'Excluir'}
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  );
}
