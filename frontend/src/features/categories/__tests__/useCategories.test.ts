import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useCategories } from '../hooks/useCategories';
import { categoryService } from '../services/categoryService';
import type { CategoryDto } from '../types/category.types';

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

const mockCategories: CategoryDto[] = [
  {
    id: '1',
    name: 'Alimentação',
    type: 'Expense',
    isActive: true,
    createdAt: '2026-01-01T00:00:00Z',
    updatedAt: null,
  },
  {
    id: '2',
    name: 'Salário',
    type: 'Income',
    isActive: true,
    createdAt: '2026-01-01T00:00:00Z',
    updatedAt: null,
  },
  {
    id: '3',
    name: 'Transporte',
    type: 'Expense',
    isActive: true,
    createdAt: '2026-01-02T00:00:00Z',
    updatedAt: null,
  },
];

describe('useCategories', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('deve buscar todas as categorias com sucesso', async () => {
    vi.mocked(categoryService.getAll).mockResolvedValue(mockCategories);

    const { result } = renderHook(() => useCategories(), {
      wrapper: createWrapper(),
    });

    // Inicialmente loading
    expect(result.current.isLoading).toBe(true);

    // Após resolução
    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(result.current.data).toEqual(mockCategories);
    expect(result.current.data).toHaveLength(3);
    expect(categoryService.getAll).toHaveBeenCalledTimes(1);
  });

  it('deve filtrar categorias por tipo Income', async () => {
    vi.mocked(categoryService.getAll).mockResolvedValue(mockCategories);

    const { result } = renderHook(() => useCategories('Income'), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(result.current.data).toHaveLength(1);
    expect(result.current.data?.[0].name).toBe('Salário');
    expect(result.current.data?.every((c: CategoryDto) => c.type === 'Income')).toBe(true);
  });

  it('deve filtrar categorias por tipo Expense', async () => {
    vi.mocked(categoryService.getAll).mockResolvedValue(mockCategories);

    const { result } = renderHook(() => useCategories('Expense'), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(result.current.data).toHaveLength(2);
    expect(result.current.data?.every((c: CategoryDto) => c.type === 'Expense')).toBe(true);
  });

  it('deve tratar erro na busca de categorias', async () => {
    vi.mocked(categoryService.getAll).mockRejectedValue(
      new Error('Network error')
    );

    const { result } = renderHook(() => useCategories(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isError).toBe(true));

    expect(result.current.error).toBeInstanceOf(Error);
    expect(result.current.data).toBeUndefined();
  });

  it('deve retornar lista vazia quando não há categorias', async () => {
    vi.mocked(categoryService.getAll).mockResolvedValue([]);

    const { result } = renderHook(() => useCategories(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(result.current.data).toEqual([]);
    expect(result.current.data).toHaveLength(0);
  });
});
