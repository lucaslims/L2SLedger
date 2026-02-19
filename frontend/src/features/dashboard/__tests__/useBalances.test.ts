import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useBalances } from '../hooks/useBalances';
import { dashboardService } from '../services/dashboardService';
import type { BalancesResponse } from '../services/dashboardService';

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
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  return ({ children }: any) => {
    return QueryClientProvider({ client: queryClient, children });
  };
};

const mockBalances: BalancesResponse = {
  totalIncome: 5000,
  totalExpense: 3200,
  currentBalance: 1800,
  period: {
    start: '2026-02-01',
    end: '2026-02-28',
  },
};

describe('useBalances', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('deve buscar saldos com sucesso', async () => {
    vi.mocked(dashboardService.getBalances).mockResolvedValue(mockBalances);

    const { result } = renderHook(() => useBalances(), {
      wrapper: createWrapper(),
    });

    // Inicialmente loading
    expect(result.current.isLoading).toBe(true);

    // Após resolução
    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(result.current.data).toEqual(mockBalances);
    expect(dashboardService.getBalances).toHaveBeenCalledTimes(1);
  });

  it('deve tratar erro na busca de saldos', async () => {
    vi.mocked(dashboardService.getBalances).mockRejectedValue(
      new Error('Network error')
    );

    const { result } = renderHook(() => useBalances(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isError).toBe(true));

    expect(result.current.error).toBeInstanceOf(Error);
    expect(result.current.data).toBeUndefined();
  });

  it('deve retornar dados com os valores corretos', async () => {
    vi.mocked(dashboardService.getBalances).mockResolvedValue(mockBalances);

    const { result } = renderHook(() => useBalances(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(result.current.data?.totalIncome).toBe(5000);
    expect(result.current.data?.totalExpense).toBe(3200);
    expect(result.current.data?.currentBalance).toBe(1800);
  });
});
