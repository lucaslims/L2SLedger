import type { Meta, StoryObj } from '@storybook/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { TransactionForm } from './TransactionForm';
import type { TransactionDto } from '../types/transaction.types';
import type { CategoryDto } from '@/features/categories/types/category.types';
import { QUERY_KEYS } from '@/shared/lib/utils/constants';

// Mock categories
const mockCategories: CategoryDto[] = [
  {
    id: 'cat-001',
    name: 'Alimentação',
    description: 'Despesas com alimentação',
    type: 'Expense',
    isActive: true,
    parentCategoryId: null,
    parentCategoryName: null,
    createdAt: '2026-01-15T10:00:00Z',
    updatedAt: null,
  },
  {
    id: 'cat-002',
    name: 'Salário',
    description: 'Renda mensal principal',
    type: 'Income',
    isActive: true,
    parentCategoryId: null,
    parentCategoryName: null,
    createdAt: '2026-01-10T08:00:00Z',
    updatedAt: null,
  },
  {
    id: 'cat-003',
    name: 'Transporte',
    description: 'Despesas com transporte',
    type: 'Expense',
    isActive: true,
    parentCategoryId: null,
    parentCategoryName: null,
    createdAt: '2026-01-20T14:00:00Z',
    updatedAt: null,
  },
  {
    id: 'cat-004',
    name: 'Freelance',
    description: 'Renda de projetos freelance',
    type: 'Income',
    isActive: true,
    parentCategoryId: null,
    parentCategoryName: null,
    createdAt: '2026-01-22T11:00:00Z',
    updatedAt: null,
  },
  {
    id: 'cat-005',
    name: 'Moradia',
    description: 'Despesas com moradia',
    type: 'Expense',
    isActive: true,
    parentCategoryId: null,
    parentCategoryName: null,
    createdAt: '2026-02-01T09:00:00Z',
    updatedAt: null,
  },
];

// Mock transactions for edit mode
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
 * Cria um QueryClient com categorias pré-carregadas para ambos os tipos.
 */
function createMockQueryClient() {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: {
        retry: false,
        staleTime: Infinity,
      },
    },
  });

  // Seed categories for each type filter
  queryClient.setQueryData(
    [QUERY_KEYS.CATEGORIES, 'Expense'],
    mockCategories.filter((c) => c.type === 'Expense'),
  );
  queryClient.setQueryData(
    [QUERY_KEYS.CATEGORIES, 'Income'],
    mockCategories.filter((c) => c.type === 'Income'),
  );
  queryClient.setQueryData([QUERY_KEYS.CATEGORIES, undefined], mockCategories);

  return queryClient;
}

const meta: Meta<typeof TransactionForm> = {
  title: 'Transactions/TransactionForm',
  component: TransactionForm,
  tags: ['autodocs'],
  parameters: {
    layout: 'centered',
    docs: {
      description: {
        component:
          'Formulário de criação/edição de transações financeiras. ' +
          'Validação via Zod + React Hook Form. ' +
          'Suporta modo criação (sem initialValues) e edição (com initialValues). ' +
          'Usa Calendar/Popover para seleção de data e Switch para recorrência. ' +
          'Não contém lógica financeira — apenas coleta de dados.',
      },
    },
  },
  argTypes: {
    isPending: {
      control: 'boolean',
      description: 'Indica se a submissão está em andamento',
    },
    onSubmit: {
      action: 'onSubmit',
      description: 'Callback disparado ao submeter o formulário',
    },
  },
  decorators: [
    (Story) => (
      <QueryClientProvider client={createMockQueryClient()}>
        <div style={{ width: 500 }}>
          <Story />
        </div>
      </QueryClientProvider>
    ),
  ],
};

export default meta;
type Story = StoryObj<typeof meta>;

/**
 * Modo criação — formulário vazio com tipo padrão "Despesa".
 */
export const Create: Story = {
  args: {
    isPending: false,
  },
};

/**
 * Modo edição de despesa — formulário preenchido com transação existente do tipo Despesa.
 */
export const EditExpense: Story = {
  args: {
    initialValues: mockExpenseTransaction,
    isPending: false,
  },
};

/**
 * Modo edição de receita — formulário preenchido com transação existente do tipo Receita.
 */
export const EditIncome: Story = {
  args: {
    initialValues: mockIncomeTransaction,
    isPending: false,
  },
};

/**
 * Estado de submissão — botão desabilitado com texto "Salvando...".
 */
export const Pending: Story = {
  args: {
    isPending: true,
  },
};

/**
 * Modo edição com recorrência — transação recorrente com dia definido.
 */
export const WithRecurrence: Story = {
  args: {
    initialValues: mockIncomeTransaction,
    isPending: false,
  },
};
