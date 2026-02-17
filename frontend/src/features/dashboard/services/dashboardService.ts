import { apiClient } from '@/shared/lib/api/client';
import { API_ENDPOINTS } from '@/shared/lib/api/endpoints';

/**
 * Resposta de saldos consolidados
 */
export interface BalancesResponse {
  totalIncome: number;
  totalExpense: number;
  currentBalance: number;
  period: {
    start: string;
    end: string;
  };
}

/**
 * Saldo diário para gráficos
 */
export interface DailyBalance {
  date: string;
  income: number;
  expense: number;
  balance: number;
}

/**
 * Transação recente (preview)
 */
export interface RecentTransaction {
  id: string;
  description: string;
  amount: number;
  type: 'Income' | 'Expense';
  categoryName: string;
  date: string;
}

/**
 * Service do Dashboard
 * 
 * Centraliza chamadas de API relacionadas ao dashboard.
 * Não contém lógica financeira — apenas transporte de dados.
 */
export const dashboardService = {
  /**
   * Buscar saldos consolidados do período atual
   */
  async getBalances(): Promise<BalancesResponse> {
    return apiClient.get<BalancesResponse>(API_ENDPOINTS.BALANCES);
  },

  /**
   * Buscar saldos diários para gráficos
   */
  async getDailyBalances(
    startDate?: string,
    endDate?: string
  ): Promise<DailyBalance[]> {
    return apiClient.get<DailyBalance[]>(API_ENDPOINTS.DAILY_BALANCES, {
      params: { startDate, endDate },
    });
  },

  /**
   * Buscar transações recentes (últimas 5)
   */
  async getRecentTransactions(): Promise<RecentTransaction[]> {
    const response = await apiClient.get<{ data: RecentTransaction[] }>(
      API_ENDPOINTS.TRANSACTIONS,
      {
        params: { pageSize: 5, page: 1 },
      }
    );
    return response.data;
  },
};
