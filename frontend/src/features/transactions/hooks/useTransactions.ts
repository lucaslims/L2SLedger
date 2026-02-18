import { useQuery, keepPreviousData } from '@tanstack/react-query';
import { transactionService } from '../services/transactionService';
import { QUERY_KEYS } from '@/shared/lib/utils/constants';
import type { TransactionFilters } from '../types/transaction.types';

/**
 * Hook para listar transações com filtros e paginação
 *
 * Usa React Query para:
 * - Cache automático por filtros
 * - keepPreviousData para paginação suave
 * - Refetch em reconexão
 */
export function useTransactions(filters: TransactionFilters = {}) {
  return useQuery({
    queryKey: [QUERY_KEYS.TRANSACTIONS, filters],
    queryFn: () => transactionService.getAll(filters),
    placeholderData: keepPreviousData,
  });
}
