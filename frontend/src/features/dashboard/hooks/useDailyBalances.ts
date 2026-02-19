import { useQuery } from '@tanstack/react-query';
import { dashboardService } from '../services/dashboardService';
import { QUERY_KEYS } from '@/shared/lib/utils/constants';

/**
 * Hook para buscar saldos diários (gráficos)
 * 
 * @param startDate - Data inicial (ISO string, opcional)
 * @param endDate - Data final (ISO string, opcional)
 */
export function useDailyBalances(startDate?: string, endDate?: string) {
  return useQuery({
    queryKey: [QUERY_KEYS.DAILY_BALANCES, startDate, endDate],
    queryFn: () => dashboardService.getDailyBalances(startDate, endDate),
  });
}
