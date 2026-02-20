import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useRecentTransactions } from '../hooks/useRecentTransactions';
import { dashboardService } from '../services/dashboardService';
import type { RecentTransaction } from '../services/dashboardService';

// Mock dashboardService
vi.mock('../services/dashboardService', () => ({
  dashboardService: {
    getBalances: vi.fn(),
    getDailyBalances: vi.fn(),
    getRecentTransactions: vi.fn(),
  },
}));

const createWrapper = () => {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false },
      mutations: { retry: false },
    },
  });
  return ({ children }: any) => {
    return QueryClientProvider({ client: queryClient, children });
  };
};

const mockTransactions: RecentTransaction[] = [
  {
    id: '1',
    description: 'Salário',
    amount: 5000,
    type: 'Income',
    categoryName: 'Renda',
    date: '2026-02-15',
  },
  {
    id: '2',
    description: 'Aluguel',
    amount: 1500,
    type: 'Expense',
    categoryName: 'Moradia',
    date: '2026-02-10',
  },
  {
    id: '3',
    description: 'Supermercado',
    amount: 350,
    type: 'Expense',
    categoryName: 'Alimentação',
    date: '2026-02-12',
  },
];

describe('useRecentTransactions', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('deve buscar transações recentes com sucesso', async () => {
    vi.mocked(dashboardService.getRecentTransactions).mockResolvedValue(
      mockTransactions
    );

    const { result } = renderHook(() => useRecentTransactions(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(result.current.data).toEqual(mockTransactions);
    expect(result.current.data).toHaveLength(3);
  });

  it('deve tratar erro na busca', async () => {
    vi.mocked(dashboardService.getRecentTransactions).mockRejectedValue(
      new Error('API error')
    );

    const { result } = renderHook(() => useRecentTransactions(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isError).toBe(true));
    expect(result.current.data).toBeUndefined();
  });

  it('deve retornar array vazio quando não há transações', async () => {
    vi.mocked(dashboardService.getRecentTransactions).mockResolvedValue([]);

    const { result } = renderHook(() => useRecentTransactions(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.data).toEqual([]);
  });
});
