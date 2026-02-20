import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useCreateTransaction } from '../hooks/useCreateTransaction';
import { useUpdateTransaction } from '../hooks/useUpdateTransaction';
import { useDeleteTransaction } from '../hooks/useDeleteTransaction';
import { transactionService } from '../services/transactionService';
import { ApiError } from '@/shared/types/errors.types';

vi.mock('../services/transactionService', () => ({
  transactionService: {
    getAll: vi.fn(),
    getById: vi.fn(),
    create: vi.fn(),
    update: vi.fn(),
    delete: vi.fn(),
  },
}));

vi.mock('sonner', () => ({
  toast: {
    success: vi.fn(),
    error: vi.fn(),
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

describe('useCreateTransaction', () => {
  beforeEach(() => { vi.clearAllMocks(); });

  it('deve criar transação com sucesso', async () => {
    vi.mocked(transactionService.create).mockResolvedValue({ id: 'new-1' });

    const { result } = renderHook(() => useCreateTransaction(), {
      wrapper: createWrapper(),
    });

    result.current.mutate({
      description: 'Compra supermercado',
      amount: 150.50,
      type: 2,
      transactionDate: '2026-01-15T00:00:00Z',
      categoryId: 'cat-1',
      isRecurring: false,
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(transactionService.create).toHaveBeenCalledWith(
      expect.objectContaining({
        description: 'Compra supermercado',
        amount: 150.50,
        type: 2,
      }),
      expect.anything()
    );
  });

  it('deve tratar erro ao criar transação', async () => {
    vi.mocked(transactionService.create).mockRejectedValue(
      new ApiError('VAL_VALIDATION_FAILED', 'Validação falhou')
    );

    const { result } = renderHook(() => useCreateTransaction(), {
      wrapper: createWrapper(),
    });

    result.current.mutate({
      description: '',
      amount: -1,
      type: 2,
      transactionDate: '2026-01-15T00:00:00Z',
      categoryId: '',
      isRecurring: false,
    });

    await waitFor(() => expect(result.current.isError).toBe(true));
    expect(result.current.error).toBeInstanceOf(ApiError);
  });
});

describe('useUpdateTransaction', () => {
  beforeEach(() => { vi.clearAllMocks(); });

  it('deve atualizar transação com sucesso', async () => {
    vi.mocked(transactionService.update).mockResolvedValue(undefined);

    const { result } = renderHook(() => useUpdateTransaction(), {
      wrapper: createWrapper(),
    });

    result.current.mutate({
      id: '1',
      data: {
        description: 'Supermercado Atualizado',
        amount: 200,
        type: 2,
        transactionDate: '2026-01-15T00:00:00Z',
        categoryId: 'cat-1',
        isRecurring: false,
      },
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(transactionService.update).toHaveBeenCalledWith('1', {
      description: 'Supermercado Atualizado',
      amount: 200,
      type: 2,
      transactionDate: '2026-01-15T00:00:00Z',
      categoryId: 'cat-1',
      isRecurring: false,
    });
  });

  it('deve tratar erro FIN_PERIOD_CLOSED ao atualizar', async () => {
    vi.mocked(transactionService.update).mockRejectedValue(
      new ApiError('FIN_PERIOD_CLOSED', 'Período encerrado')
    );

    const { result } = renderHook(() => useUpdateTransaction(), {
      wrapper: createWrapper(),
    });

    result.current.mutate({
      id: '1',
      data: {
        description: 'Test',
        amount: 100,
        type: 1,
        transactionDate: '2025-01-01T00:00:00Z',
        categoryId: 'cat-1',
        isRecurring: false,
      },
    });

    await waitFor(() => expect(result.current.isError).toBe(true));
    expect((result.current.error as ApiError).code).toBe('FIN_PERIOD_CLOSED');
  });
});

describe('useDeleteTransaction', () => {
  beforeEach(() => { vi.clearAllMocks(); });

  it('deve excluir transação com sucesso', async () => {
    vi.mocked(transactionService.delete).mockResolvedValue(undefined);

    const { result } = renderHook(() => useDeleteTransaction(), {
      wrapper: createWrapper(),
    });

    result.current.mutate('1');

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(transactionService.delete).toHaveBeenCalledWith('1', expect.anything());
  });

  it('deve tratar erro FIN_TRANSACTION_NOT_FOUND', async () => {
    vi.mocked(transactionService.delete).mockRejectedValue(
      new ApiError('FIN_TRANSACTION_NOT_FOUND', 'Transação não encontrada')
    );

    const { result } = renderHook(() => useDeleteTransaction(), {
      wrapper: createWrapper(),
    });

    result.current.mutate('999');

    await waitFor(() => expect(result.current.isError).toBe(true));
    expect((result.current.error as ApiError).code).toBe('FIN_TRANSACTION_NOT_FOUND');
  });

  it('deve tratar erro FIN_PERIOD_CLOSED ao excluir', async () => {
    vi.mocked(transactionService.delete).mockRejectedValue(
      new ApiError('FIN_PERIOD_CLOSED', 'Período encerrado')
    );

    const { result } = renderHook(() => useDeleteTransaction(), {
      wrapper: createWrapper(),
    });

    result.current.mutate('1');

    await waitFor(() => expect(result.current.isError).toBe(true));
    expect((result.current.error as ApiError).code).toBe('FIN_PERIOD_CLOSED');
  });
});
