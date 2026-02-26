import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useDailyBalances } from '../hooks/useDailyBalances';
import { dashboardService } from '../services/dashboardService';
import type { DailyBalance } from '../services/dashboardService';

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

const mockDailyBalances: DailyBalance[] = [
  { date: '2026-02-01', openingBalance: 0, income: 1000, expense: 500, closingBalance: 500 },
  { date: '2026-02-02', openingBalance: 500, income: 2000, expense: 800, closingBalance: 1700 },
  { date: '2026-02-03', openingBalance: 1700, income: 500, expense: 1200, closingBalance: 1000 },
];

describe('useDailyBalances', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('deve buscar saldos diários com sucesso', async () => {
    vi.mocked(dashboardService.getDailyBalances).mockResolvedValue(mockDailyBalances);

    const { result } = renderHook(() => useDailyBalances(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(result.current.data).toEqual(mockDailyBalances);
    expect(result.current.data).toHaveLength(3);
  });

  it('deve passar parâmetros de data para o service', async () => {
    vi.mocked(dashboardService.getDailyBalances).mockResolvedValue(mockDailyBalances);

    const { result } = renderHook(() => useDailyBalances('2026-02-01', '2026-02-28'), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(dashboardService.getDailyBalances).toHaveBeenCalledWith('2026-02-01', '2026-02-28');
  });

  it('deve tratar erro na busca', async () => {
    vi.mocked(dashboardService.getDailyBalances).mockRejectedValue(new Error('Server error'));

    const { result } = renderHook(() => useDailyBalances(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isError).toBe(true));
    expect(result.current.data).toBeUndefined();
  });

  it('deve retornar array vazio quando não há dados', async () => {
    vi.mocked(dashboardService.getDailyBalances).mockResolvedValue([]);

    const { result } = renderHook(() => useDailyBalances(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.data).toEqual([]);
    expect(result.current.data).toHaveLength(0);
  });
});
