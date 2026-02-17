import { useQuery } from '@tanstack/react-query';
import { dashboardService } from '../services/dashboardService';
import { QUERY_KEYS } from '@/shared/lib/utils/constants';

/**
 * Hook para buscar saldos consolidados
 * 
 * Usa React Query para:
 * - Cache automático (5min stale)
 * - Refetch em reconexão
 * - Loading e error states
 */
export function useBalances() {
  return useQuery({
    queryKey: [QUERY_KEYS.BALANCES],
    queryFn: () => dashboardService.getBalances(),
  });
}
