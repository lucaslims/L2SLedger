import { useQuery } from '@tanstack/react-query';
import { dashboardService } from '../services/dashboardService';
import { QUERY_KEYS } from '@/shared/lib/utils/constants';

/**
 * Hook para buscar transações recentes (preview do dashboard)
 */
export function useRecentTransactions() {
  return useQuery({
    queryKey: [QUERY_KEYS.TRANSACTIONS, 'recent'],
    queryFn: () => dashboardService.getRecentTransactions(),
  });
}
