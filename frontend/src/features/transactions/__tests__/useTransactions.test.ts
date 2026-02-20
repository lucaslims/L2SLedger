import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useTransactions } from '../hooks/useTransactions';
import { transactionService } from '../services/transactionService';
import type { GetTransactionsResponse } from '../types/transaction.types';

vi.mock('../services/transactionService', () => ({
  transactionService: {
    getAll: vi.fn(),
    getById: vi.fn(),
    create: vi.fn(),
    update: vi.fn(),
    delete: vi.fn(),
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

const mockResponse: GetTransactionsResponse = {
  transactions: [
    {
      id: '1',
      description: 'Supermercado',
      amount: 150.50,
      type: 2,
      transactionDate: '2026-01-15T00:00:00Z',
      categoryId: 'cat-1',
      categoryName: 'Alimentação',
      userId: 'user-1',
      notes: null,
      isRecurring: false,
      recurringDay: null,
      createdAt: '2026-01-15T10:00:00Z',
      updatedAt: '2026-01-15T10:00:00Z',
    },
    {
      id: '2',
      description: 'Salário Janeiro',
      amount: 5000.00,
      type: 1,
      transactionDate: '2026-01-05T00:00:00Z',
      categoryId: 'cat-2',
      categoryName: 'Salário',
      userId: 'user-1',
      notes: 'Pagamento mensal',
      isRecurring: true,
      recurringDay: 5,
      createdAt: '2026-01-05T10:00:00Z',
      updatedAt: '2026-01-05T10:00:00Z',
    },
  ],
  totalCount: 2,
  page: 1,
  pageSize: 20,
  totalPages: 1,
  totalIncome: 5000.00,
  totalExpense: 150.50,
  balance: 4849.50,
};

describe('useTransactions', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('deve buscar transações com sucesso', async () => {
    vi.mocked(transactionService.getAll).mockResolvedValue(mockResponse);

    const { result } = renderHook(() => useTransactions(), {
      wrapper: createWrapper(),
    });

    expect(result.current.isLoading).toBe(true);

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(result.current.data).toEqual(mockResponse);
    expect(result.current.data?.transactions).toHaveLength(2);
    expect(transactionService.getAll).toHaveBeenCalledTimes(1);
  });

  it('deve passar filtros para o serviço', async () => {
    vi.mocked(transactionService.getAll).mockResolvedValue(mockResponse);

    const filters = { type: 1 as const, page: 2, pageSize: 10 };
    const { result } = renderHook(() => useTransactions(filters), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(transactionService.getAll).toHaveBeenCalledWith(filters);
  });

  it('deve retornar totais financeiros', async () => {
    vi.mocked(transactionService.getAll).mockResolvedValue(mockResponse);

    const { result } = renderHook(() => useTransactions(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(result.current.data?.totalIncome).toBe(5000.00);
    expect(result.current.data?.totalExpense).toBe(150.50);
    expect(result.current.data?.balance).toBe(4849.50);
  });

  it('deve tratar erro na busca de transações', async () => {
    vi.mocked(transactionService.getAll).mockRejectedValue(
      new Error('Network error')
    );

    const { result } = renderHook(() => useTransactions(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isError).toBe(true));

    expect(result.current.error).toBeInstanceOf(Error);
    expect(result.current.data).toBeUndefined();
  });

  it('deve retornar lista vazia quando não há transações', async () => {
    const emptyResponse: GetTransactionsResponse = {
      transactions: [],
      totalCount: 0,
      page: 1,
      pageSize: 20,
      totalPages: 0,
      totalIncome: 0,
      totalExpense: 0,
      balance: 0,
    };
    vi.mocked(transactionService.getAll).mockResolvedValue(emptyResponse);

    const { result } = renderHook(() => useTransactions(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(result.current.data?.transactions).toEqual([]);
    expect(result.current.data?.totalCount).toBe(0);
  });
});
