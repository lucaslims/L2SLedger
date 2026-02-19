import type { Meta, StoryObj } from '@storybook/react';
import { TransactionList } from './TransactionList';
import type { TransactionDto } from '../types/transaction.types';

// Mock data
const mockTransactions: TransactionDto[] = [
  {
    id: 'txn-001',
    description: 'Almoço restaurante',
    amount: 45.9,
    type: 2,
    categoryId: 'cat-001',
    categoryName: 'Alimentação',
    transactionDate: '2026-03-15T00:00:00Z',
    notes: 'Almoço de negócios',
    isRecurring: false,
    recurringDay: null,
    userId: 'user-001',
    createdAt: '2026-03-15T10:00:00Z',
    updatedAt: '2026-03-15T10:00:00Z',
  },
  {
    id: 'txn-002',
    description: 'Salário mensal',
    amount: 8500.0,
    type: 1,
    categoryId: 'cat-002',
    categoryName: 'Salário',
    transactionDate: '2026-03-05T00:00:00Z',
    notes: null,
    isRecurring: true,
    recurringDay: 5,
    userId: 'user-001',
    createdAt: '2026-03-05T08:00:00Z',
    updatedAt: '2026-03-05T08:00:00Z',
  },
  {
    id: 'txn-003',
    description: 'Uber para o trabalho',
    amount: 32.5,
    type: 2,
    categoryId: 'cat-003',
    categoryName: 'Transporte',
    transactionDate: '2026-03-14T00:00:00Z',
    notes: null,
    isRecurring: false,
    recurringDay: null,
    userId: 'user-001',
    createdAt: '2026-03-14T09:00:00Z',
    updatedAt: '2026-03-14T09:00:00Z',
  },
  {
    id: 'txn-004',
    description: 'Projeto website cliente X',
    amount: 3200.0,
    type: 1,
    categoryId: 'cat-004',
    categoryName: 'Freelance',
    transactionDate: '2026-03-10T00:00:00Z',
    notes: 'Entrega final do projeto',
    isRecurring: false,
    recurringDay: null,
    userId: 'user-001',
    createdAt: '2026-03-10T14:00:00Z',
    updatedAt: '2026-03-11T08:30:00Z',
  },
  {
    id: 'txn-005',
    description: 'Aluguel apartamento',
    amount: 2200.0,
    type: 2,
    categoryId: 'cat-005',
    categoryName: 'Moradia',
    transactionDate: '2026-03-01T00:00:00Z',
    notes: 'Aluguel mensal',
    isRecurring: true,
    recurringDay: 1,
    userId: 'user-001',
    createdAt: '2026-03-01T07:00:00Z',
    updatedAt: '2026-03-01T07:00:00Z',
  },
];

const meta: Meta<typeof TransactionList> = {
  title: 'Transactions/TransactionList',
  component: TransactionList,
  tags: ['autodocs'],
  parameters: {
    layout: 'centered',
    docs: {
      description: {
        component:
          'Tabela/lista de transações financeiras com data, descrição, categoria, ' +
          'tipo (badge), valor e ações (editar/excluir). ' +
          'Integra skeleton loading e empty state. ' +
          'Layout responsivo: tabela no desktop, cards no mobile. ' +
          'Não contém lógica financeira — apenas exibição.',
      },
    },
  },
  argTypes: {
    onEdit: {
      action: 'onEdit',
      description: 'Callback ao clicar em editar — recebe o ID da transação',
    },
    onDelete: {
      action: 'onDelete',
      description: 'Callback ao clicar em excluir — recebe o TransactionDto completo',
    },
  },
  decorators: [
    (Story) => (
      <div style={{ width: 900 }}>
        <Story />
      </div>
    ),
  ],
};

export default meta;
type Story = StoryObj<typeof meta>;

/**
 * Estado com transações carregadas — mix de receitas e despesas.
 */
export const WithData: Story = {
  args: {
    transactions: mockTransactions,
    isLoading: false,
  },
};

/**
 * Estado vazio — nenhuma transação encontrada.
 * Exibe mensagem convidando o usuário a criar a primeira transação.
 */
export const Empty: Story = {
  args: {
    transactions: [],
    isLoading: false,
  },
};

/**
 * Estado de carregamento — exibe skeleton placeholders.
 */
export const Loading: Story = {
  args: {
    transactions: [],
    isLoading: true,
  },
};

/**
 * Apenas receitas — todas as transações são do tipo Income.
 */
export const OnlyIncome: Story = {
  args: {
    transactions: mockTransactions.filter((t) => t.type === 1),
    isLoading: false,
  },
};

/**
 * Apenas despesas — todas as transações são do tipo Expense.
 */
export const OnlyExpenses: Story = {
  args: {
    transactions: mockTransactions.filter((t) => t.type === 2),
    isLoading: false,
  },
};
