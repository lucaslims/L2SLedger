import type { Meta, StoryObj } from '@storybook/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { TransactionDeleteDialog } from './TransactionDeleteDialog';
import type { TransactionDto } from '../types/transaction.types';

// Mock transactions
const mockExpenseTransaction: TransactionDto = {
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
};

const mockIncomeTransaction: TransactionDto = {
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
};

/**
 * Cria um QueryClient mock para isolar o Storybook do backend real.
 */
function createMockQueryClient() {
  return new QueryClient({
    defaultOptions: {
      queries: {
        retry: false,
        staleTime: Infinity,
      },
      mutations: {
        retry: false,
      },
    },
  });
}

const meta: Meta<typeof TransactionDeleteDialog> = {
  title: 'Transactions/TransactionDeleteDialog',
  component: TransactionDeleteDialog,
  tags: ['autodocs'],
  parameters: {
    layout: 'centered',
    docs: {
      description: {
        component:
          'Diálogo de confirmação de exclusão de transação. ' +
          'Exibe descrição e valor da transação com botões Cancelar/Excluir. ' +
          'Usa hook useDeleteTransaction para executar a exclusão. ' +
          'Não contém lógica financeira — apenas confirmação de ação.',
      },
    },
  },
  argTypes: {
    open: {
      control: 'boolean',
      description: 'Controla a visibilidade do diálogo',
    },
    onOpenChange: {
      action: 'onOpenChange',
      description: 'Callback ao alterar estado de abertura do diálogo',
    },
  },
  decorators: [
    (Story) => (
      <QueryClientProvider client={createMockQueryClient()}>
        <Story />
      </QueryClientProvider>
    ),
  ],
};

export default meta;
type Story = StoryObj<typeof meta>;

/**
 * Diálogo aberto para exclusão de despesa.
 */
export const OpenExpense: Story = {
  args: {
    open: true,
    transaction: mockExpenseTransaction,
  },
};

/**
 * Diálogo aberto para exclusão de receita.
 */
export const OpenIncome: Story = {
  args: {
    open: true,
    transaction: mockIncomeTransaction,
  },
};

/**
 * Diálogo fechado — não renderiza conteúdo visível.
 */
export const Closed: Story = {
  args: {
    open: false,
    transaction: mockExpenseTransaction,
  },
};

/**
 * Descrição longa — verifica quebra de texto no diálogo.
 */
export const LongDescription: Story = {
  args: {
    open: true,
    transaction: {
      ...mockExpenseTransaction,
      id: 'txn-long',
      description:
        'Pagamento de serviços extraordinários de consultoria em tecnologia da informação e comunicação',
    },
  },
};
