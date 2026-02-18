import { useQuery } from '@tanstack/react-query';
import { transactionService } from '../services/transactionService';
import { QUERY_KEYS } from '@/shared/lib/utils/constants';

/**
 * Hook para buscar uma transação por ID
 *
 * @param id - ID da transação (enabled apenas quando presente)
 */
export function useTransaction(id?: string) {
  return useQuery({
    queryKey: [QUERY_KEYS.TRANSACTION, id],
    queryFn: () => transactionService.getById(id!),
    enabled: !!id,
  });
}
