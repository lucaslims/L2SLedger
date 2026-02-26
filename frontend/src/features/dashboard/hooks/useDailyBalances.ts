import { useQuery } from '@tanstack/react-query';
import { dashboardService } from '../services/dashboardService';
import { QUERY_KEYS } from '@/shared/lib/utils/constants';

/**
 * Hook para buscar saldos diários (gráficos)
 *
 * @param startDate - Data inicial (ISO string, opcional - default: 30 dias atrás)
 * @param endDate - Data final (ISO string, opcional - default: hoje)
 */
export function useDailyBalances(startDate?: string, endDate?: string) {
  // Definir período padrão: últimos 30 dias
  const today = new Date();
  const thirtyDaysAgo = new Date(today);
  thirtyDaysAgo.setDate(today.getDate() - 30);

  const effectiveStartDate = startDate || thirtyDaysAgo.toISOString().split('T')[0];
  const effectiveEndDate = endDate || today.toISOString().split('T')[0];

  return useQuery({
    queryKey: [QUERY_KEYS.DAILY_BALANCES, effectiveStartDate, effectiveEndDate],
    queryFn: () => dashboardService.getDailyBalances(effectiveStartDate, effectiveEndDate),
  });
}
