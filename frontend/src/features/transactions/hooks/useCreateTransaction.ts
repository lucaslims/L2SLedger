import { useMutation, useQueryClient } from '@tanstack/react-query';
import { transactionService } from '../services/transactionService';
import { QUERY_KEYS } from '@/shared/lib/utils/constants';
import { getErrorMessage } from '@/shared/lib/api/errors';
import { toast } from 'sonner';
import type { ApiError } from '@/shared/types/errors.types';

/**
 * Hook para criar transação
 *
 * Invalida cache de transações e balanços após sucesso.
 * Exibe toast de sucesso/erro.
 */
export function useCreateTransaction() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: transactionService.create,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.TRANSACTIONS] });
      queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.BALANCES] });
      queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.DAILY_BALANCES] });
      toast.success('Transação criada com sucesso!');
    },
    onError: (error: ApiError) => {
      toast.error(getErrorMessage(error.code));
    },
  });
}
