import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useCreateCategory } from '../hooks/useCreateCategory';
import { useUpdateCategory } from '../hooks/useUpdateCategory';
import { useDeleteCategory } from '../hooks/useDeleteCategory';
import { categoryService } from '../services/categoryService';
import type { CategoryDto } from '../types/category.types';
import { ApiError } from '@/shared/types/errors.types';

// Mock categoryService
vi.mock('../services/categoryService', () => ({
  categoryService: {
    getAll: vi.fn(),
    getById: vi.fn(),
    create: vi.fn(),
    update: vi.fn(),
    delete: vi.fn(),
  },
}));

// Mock sonner toast
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

const mockCategory: CategoryDto = {
  id: '1',
  name: 'Alimentação',
  type: 'Expense',
  isActive: true,
  createdAt: '2026-01-01T00:00:00Z',
  updatedAt: null,
};

describe('useCreateCategory', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('deve criar categoria com sucesso', async () => {
    vi.mocked(categoryService.create).mockResolvedValue(mockCategory);

    const { result } = renderHook(() => useCreateCategory(), {
      wrapper: createWrapper(),
    });

    result.current.mutate({ name: 'Alimentação', type: 'Expense' });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(categoryService.create).toHaveBeenCalledWith(
      expect.objectContaining({
        name: 'Alimentação',
        type: 'Expense',
      }),
      expect.anything()
    );
  });

  it('deve tratar erro ao criar categoria', async () => {
    vi.mocked(categoryService.create).mockRejectedValue(
      new ApiError('VAL_DUPLICATE_VALUE', 'Nome já existe')
    );

    const { result } = renderHook(() => useCreateCategory(), {
      wrapper: createWrapper(),
    });

    result.current.mutate({ name: 'Duplicada', type: 'Expense' });

    await waitFor(() => expect(result.current.isError).toBe(true));

    expect(result.current.error).toBeInstanceOf(ApiError);
  });
});

describe('useUpdateCategory', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('deve atualizar categoria com sucesso', async () => {
    const updated = { ...mockCategory, name: 'Alimentação Atualizada' };
    vi.mocked(categoryService.update).mockResolvedValue(updated);

    const { result } = renderHook(() => useUpdateCategory(), {
      wrapper: createWrapper(),
    });

    result.current.mutate({
      id: '1',
      data: { name: 'Alimentação Atualizada', type: 'Expense' },
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(categoryService.update).toHaveBeenCalledWith('1', {
      name: 'Alimentação Atualizada',
      type: 'Expense',
    });
  });

  it('deve tratar erro ao atualizar categoria', async () => {
    vi.mocked(categoryService.update).mockRejectedValue(
      new ApiError('FIN_CATEGORY_NOT_FOUND', 'Categoria não encontrada')
    );

    const { result } = renderHook(() => useUpdateCategory(), {
      wrapper: createWrapper(),
    });

    result.current.mutate({
      id: '999',
      data: { name: 'Inexistente', type: 'Income' },
    });

    await waitFor(() => expect(result.current.isError).toBe(true));

    expect(result.current.error).toBeInstanceOf(ApiError);
  });
});

describe('useDeleteCategory', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('deve excluir categoria com sucesso', async () => {
    vi.mocked(categoryService.delete).mockResolvedValue(undefined);

    const { result } = renderHook(() => useDeleteCategory(), {
      wrapper: createWrapper(),
    });

    result.current.mutate('1');

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(categoryService.delete).toHaveBeenCalledWith('1');
  });

  it('deve tratar erro FIN_CATEGORY_HAS_TRANSACTIONS', async () => {
    vi.mocked(categoryService.delete).mockRejectedValue(
      new ApiError('FIN_CATEGORY_HAS_TRANSACTIONS', 'Categoria possui lançamentos')
    );

    const { result } = renderHook(() => useDeleteCategory(), {
      wrapper: createWrapper(),
    });

    result.current.mutate('1');

    await waitFor(() => expect(result.current.isError).toBe(true));

    expect(result.current.error).toBeInstanceOf(ApiError);
    expect((result.current.error as ApiError).code).toBe('FIN_CATEGORY_HAS_TRANSACTIONS');
  });

  it('deve tratar erro FIN_CATEGORY_ALREADY_DELETED', async () => {
    vi.mocked(categoryService.delete).mockRejectedValue(
      new ApiError('FIN_CATEGORY_ALREADY_DELETED', 'Categoria já excluída')
    );

    const { result } = renderHook(() => useDeleteCategory(), {
      wrapper: createWrapper(),
    });

    result.current.mutate('1');

    await waitFor(() => expect(result.current.isError).toBe(true));

    expect((result.current.error as ApiError).code).toBe('FIN_CATEGORY_ALREADY_DELETED');
  });
});
