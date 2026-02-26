import { apiClient } from '@/shared/lib/api/client';
import { API_ENDPOINTS } from '@/shared/lib/api/endpoints';
import type { GetTransactionsResponse } from '@/features/transactions/types/transaction.types';
import { TransactionTypeNameMap } from '@/features/transactions/types/transaction.types';

/**
 * Resposta de saldos consolidados (contrato do backend - BalanceSummaryDto)
 */
export interface BalancesResponse {
  totalIncome: number;
  totalExpense: number;
  netBalance: number;
  startDate: string;
  endDate: string;
  byCategory: Array<{
    categoryId: string;
    categoryName: string;
    income: number;
    expense: number;
    netBalance: number;
  }>;
}

/**
 * Saldo diário para gráficos (contrato do backend - DailyBalanceDto)
 */
export interface DailyBalance {
  date: string;
  openingBalance: number;
  income: number;
  expense: number;
  closingBalance: number;
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
  async getDailyBalances(startDate?: string, endDate?: string): Promise<DailyBalance[]> {
    return apiClient.get<DailyBalance[]>(API_ENDPOINTS.DAILY_BALANCES, {
      params: { startDate, endDate },
    });
  },

  /**
   * Buscar transações recentes (últimas 5)
   */
  async getRecentTransactions(): Promise<RecentTransaction[]> {
    const response = await apiClient.get<GetTransactionsResponse>(API_ENDPOINTS.TRANSACTIONS, {
      params: { pageSize: 5, page: 1 },
    });

    // Mapear TransactionDto para RecentTransaction
    return response.transactions.map((t) => ({
      id: t.id,
      description: t.description,
      amount: t.amount,
      type: TransactionTypeNameMap[t.type],
      categoryName: t.categoryName,
      date: t.transactionDate,
    }));
  },
};
