import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import '@testing-library/jest-dom';
import { TransactionForm } from '../components/TransactionForm';
import { categoryService } from '@/features/categories/services/categoryService';
import type { TransactionDto } from '../types/transaction.types';
import { CategoryDto } from '@/features/categories/types/category.types';

// Mock category service for the useCategories hook
vi.mock('@/features/categories/services/categoryService', () => ({
  categoryService: {
    getAll: vi.fn(),
    getById: vi.fn(),
    create: vi.fn(),
    update: vi.fn(),
    delete: vi.fn(),
  },
}));

const mockCategories: CategoryDto[] = [
  {
    id: 'cat-1',
    name: 'Alimentação',
    type: 'Expense',
    isActive: true,
    createdAt: '2026-01-01T00:00:00Z',
    updatedAt: null,
  },
  {
    id: 'cat-2',
    name: 'Salário',
    type: 'Income',
    isActive: true,
    createdAt: '2026-01-01T00:00:00Z',
    updatedAt: null,
  },
];

function createWrapper() {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false },
      mutations: { retry: false },
    },
  });

  return ({ children }: any) => (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  );
}

describe('TransactionForm', () => {
  const mockOnSubmit = vi.fn();

  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(categoryService.getAll).mockResolvedValue(mockCategories);
  });

  it('deve renderizar formulário de criação', () => {
    render(<TransactionForm onSubmit={mockOnSubmit} />, { wrapper: createWrapper() });

    expect(screen.getByLabelText('Descrição')).toBeInTheDocument();
    expect(screen.getByLabelText('Valor (R$)')).toBeInTheDocument();
    expect(screen.getByLabelText('Tipo')).toBeInTheDocument();
    expect(screen.getByLabelText('Categoria')).toBeInTheDocument();
    expect(screen.getByLabelText('Data')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Criar Transação' })).toBeInTheDocument();
  });

  it('deve carregar e exibir categorias no dropdown', async () => {
    const user = userEvent.setup();
    render(<TransactionForm onSubmit={mockOnSubmit} />, { wrapper: createWrapper() });

    // Aguarda o carregamento das categorias (queryFn resolve)
    await screen.findByLabelText('Categoria');

    // Clica no select de categoria para abrir o dropdown
    const categoryTrigger = screen.getByLabelText('Categoria');
    await user.click(categoryTrigger);

    // O tipo padrão é Expense, então deve mostrar categorias Expense
    // Radix Select pode renderizar o texto em múltiplos nós DOM
    const items = await screen.findAllByText('Alimentação');
    expect(items.length).toBeGreaterThanOrEqual(1);
  });

  it('deve exibir todas as categorias como fallback quando nenhuma tem o tipo', async () => {
    // Simula categorias sem campo type (backend antigo)
    const categoriesWithoutType = [
      {
        id: 'cat-1',
        name: 'Geral',
        isActive: true,
        createdAt: '2026-01-01T00:00:00Z',
        updatedAt: null,
      },
      {
        id: 'cat-2',
        name: 'Outros',
        isActive: true,
        createdAt: '2026-01-01T00:00:00Z',
        updatedAt: null,
      },
    ];
    vi.mocked(categoryService.getAll).mockResolvedValue(categoriesWithoutType as any);
    const user = userEvent.setup();
    render(<TransactionForm onSubmit={mockOnSubmit} />, { wrapper: createWrapper() });

    await screen.findByLabelText('Categoria');
    const categoryTrigger = screen.getByLabelText('Categoria');
    await user.click(categoryTrigger);

    // Fallback: mostra todas as categorias (findAllByText pois Radix duplica no DOM)
    const geralItems = await screen.findAllByText('Geral');
    expect(geralItems.length).toBeGreaterThanOrEqual(1);
    const outrosItems = await screen.findAllByText('Outros');
    expect(outrosItems.length).toBeGreaterThanOrEqual(1);
  });

  it('deve renderizar botão "Atualizar" quando há initialValues', () => {
    const initial: TransactionDto = {
      id: '1',
      description: 'Supermercado',
      amount: 150.5,
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
    };

    render(<TransactionForm initialValues={initial} onSubmit={mockOnSubmit} />, {
      wrapper: createWrapper(),
    });

    expect(screen.getByRole('button', { name: 'Atualizar' })).toBeInTheDocument();
  });

  it('deve exibir erro quando descrição está vazia', async () => {
    const user = userEvent.setup();
    render(<TransactionForm onSubmit={mockOnSubmit} />, { wrapper: createWrapper() });

    const descInput = screen.getByLabelText('Descrição');
    await user.clear(descInput);
    await user.click(screen.getByRole('button', { name: 'Criar Transação' }));

    expect(await screen.findByText('Descrição é obrigatória')).toBeInTheDocument();
    expect(mockOnSubmit).not.toHaveBeenCalled();
  });

  it('deve desabilitar botão quando isPending', () => {
    render(<TransactionForm onSubmit={mockOnSubmit} isPending />, { wrapper: createWrapper() });

    const button = screen.getByRole('button', { name: 'Salvando...' });
    expect(button).toBeDisabled();
  });

  it('deve exibir switch de recorrência', () => {
    render(<TransactionForm onSubmit={mockOnSubmit} />, { wrapper: createWrapper() });
    expect(screen.getByText('Transação Recorrente')).toBeInTheDocument();
  });

  it('deve exibir campo dia quando recorrência ativada', async () => {
    const user = userEvent.setup();
    render(<TransactionForm onSubmit={mockOnSubmit} />, { wrapper: createWrapper() });

    const switchEl = screen.getByRole('switch');
    await user.click(switchEl);

    expect(await screen.findByLabelText('Dia do mês (1-31)')).toBeInTheDocument();
  });
});
