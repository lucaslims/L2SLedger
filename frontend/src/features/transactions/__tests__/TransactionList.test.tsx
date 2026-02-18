import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import '@testing-library/jest-dom';
import { TransactionList } from '../components/TransactionList';
import type { TransactionDto } from '../types/transaction.types';

const mockTransactions: TransactionDto[] = [
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
];

describe('TransactionList', () => {
  const mockOnEdit = vi.fn();
  const mockOnDelete = vi.fn();

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('deve exibir loading quando isLoading', () => {
    render(
      <TransactionList
        transactions={[]}
        isLoading={true}
        onEdit={mockOnEdit}
        onDelete={mockOnDelete}
      />
    );

    const skeletons = document.querySelectorAll('[class*="animate-pulse"]');
    expect(skeletons.length).toBeGreaterThan(0);
  });

  it('deve exibir mensagem quando lista está vazia', () => {
    render(
      <TransactionList
        transactions={[]}
        isLoading={false}
        onEdit={mockOnEdit}
        onDelete={mockOnDelete}
      />
    );

    expect(screen.getByText('Nenhuma transação encontrada')).toBeInTheDocument();
  });

  it('deve exibir transações após carregamento', () => {
    render(
      <TransactionList
        transactions={mockTransactions}
        onEdit={mockOnEdit}
        onDelete={mockOnDelete}
      />
    );

    // Both desktop table and mobile cards render in jsdom (no media queries)
    expect(screen.getAllByText('Supermercado').length).toBeGreaterThanOrEqual(1);
    expect(screen.getAllByText('Salário Janeiro').length).toBeGreaterThanOrEqual(1);
  });

  it('deve exibir categorias corretas', () => {
    render(
      <TransactionList
        transactions={mockTransactions}
        onEdit={mockOnEdit}
        onDelete={mockOnDelete}
      />
    );

    expect(screen.getAllByText('Alimentação').length).toBeGreaterThanOrEqual(1);
    expect(screen.getAllByText('Salário').length).toBeGreaterThanOrEqual(1);
  });

  it('deve exibir badges de tipo corretos', () => {
    render(
      <TransactionList
        transactions={mockTransactions}
        onEdit={mockOnEdit}
        onDelete={mockOnDelete}
      />
    );

    expect(screen.getAllByText('Despesa').length).toBeGreaterThanOrEqual(1);
    expect(screen.getAllByText('Receita').length).toBeGreaterThanOrEqual(1);
  });

  it('deve chamar onEdit ao clicar em editar', async () => {
    const user = userEvent.setup();
    render(
      <TransactionList
        transactions={mockTransactions}
        onEdit={mockOnEdit}
        onDelete={mockOnDelete}
      />
    );

    // Multiple elements due to desktop + mobile rendering
    const editButtons = screen.getAllByLabelText('Editar Supermercado');
    await user.click(editButtons[0]);
    expect(mockOnEdit).toHaveBeenCalledWith('1');
  });

  it('deve chamar onDelete ao clicar em excluir', async () => {
    const user = userEvent.setup();
    render(
      <TransactionList
        transactions={mockTransactions}
        onEdit={mockOnEdit}
        onDelete={mockOnDelete}
      />
    );

    const deleteButtons = screen.getAllByLabelText('Excluir Supermercado');
    await user.click(deleteButtons[0]);
    expect(mockOnDelete).toHaveBeenCalledWith(mockTransactions[0]);
  });
});
