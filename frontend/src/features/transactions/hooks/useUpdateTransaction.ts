import { useMutation, useQueryClient } from '@tanstack/react-query';
import { transactionService } from '../services/transactionService';
import { QUERY_KEYS } from '@/shared/lib/utils/constants';
import { getErrorMessage } from '@/shared/lib/api/errors';
import { toast } from 'sonner';
import type { ApiError } from '@/shared/types/errors.types';
import type { UpdateTransactionRequest } from '../types/transaction.types';

/**
 * Hook para atualizar transação
 *
 * Invalida cache de transações, transação individual e balanços após sucesso.
 * Exibe toast de sucesso/erro.
 */
export function useUpdateTransaction() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateTransactionRequest }) =>
      transactionService.update(id, data),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.TRANSACTIONS] });
      queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.TRANSACTION, variables.id] });
      queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.BALANCES] });
      queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.DAILY_BALANCES] });
      toast.success('Transação atualizada com sucesso!');
    },
    onError: (error: ApiError) => {
      toast.error(getErrorMessage(error.code));
    },
  });
}
