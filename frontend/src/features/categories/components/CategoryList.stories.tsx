import type { Meta, StoryObj } from '@storybook/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { MemoryRouter } from 'react-router-dom';
import { CategoryList } from './CategoryList';
import type { CategoryDto } from '../types/category.types';
import { QUERY_KEYS } from '@/shared/lib/utils/constants';

// Mock data
const mockCategories: CategoryDto[] = [
  {
    id: 'cat-001',
    name: 'Alimentação',
    description: 'Despesas com alimentação e refeições',
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
    updatedAt: '2026-02-01T09:30:00Z',
  },
  {
    id: 'cat-003',
    name: 'Transporte',
    description: 'Despesas com transporte e combustível',
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
    name: 'Restaurantes',
    description: 'Refeições em restaurantes',
    type: 'Expense',
    isActive: true,
    parentCategoryId: 'cat-001',
    parentCategoryName: 'Alimentação',
    createdAt: '2026-02-01T09:00:00Z',
    updatedAt: null,
  },
];

/**
 * Creates a QueryClient with pre-set categories data.
 */
function createMockQueryClient(data?: CategoryDto[] | null) {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: {
        retry: false,
        staleTime: Infinity,
      },
    },
  });

  if (data !== undefined) {
    queryClient.setQueryData([QUERY_KEYS.CATEGORIES, undefined], data);
  }

  return queryClient;
}

const meta: Meta<typeof CategoryList> = {
  title: 'Categories/CategoryList',
  component: CategoryList,
  tags: ['autodocs'],
  parameters: {
    layout: 'centered',
    docs: {
      description: {
        component:
          'Tabela de categorias financeiras com nome, descrição, tipo (badge), ' +
          'categoria pai e ações (editar/excluir). ' +
          'Integra skeleton loading e empty state. ' +
          'Não contém lógica financeira — apenas exibição e navegação.',
      },
    },
  },
  decorators: [
    (Story) => (
      <MemoryRouter initialEntries={['/categories']}>
        <div style={{ width: 800 }}>
          <Story />
        </div>
      </MemoryRouter>
    ),
  ],
};

export default meta;
type Story = StoryObj<typeof meta>;

/**
 * Estado com categorias carregadas — mix de receitas, despesas e subcategorias.
 */
export const WithData: Story = {
  decorators: [
    (Story) => (
      <QueryClientProvider client={createMockQueryClient(mockCategories)}>
        <Story />
      </QueryClientProvider>
    ),
  ],
};

/**
 * Estado vazio — nenhuma categoria cadastrada.
 * Exibe mensagem convidando o usuário a criar a primeira categoria.
 */
export const Empty: Story = {
  decorators: [
    (Story) => (
      <QueryClientProvider client={createMockQueryClient([])}>
        <Story />
      </QueryClientProvider>
    ),
  ],
};

/**
 * Estado de carregamento — exibe skeleton placeholders.
 */
export const Loading: Story = {
  decorators: [
    (Story) => {
      const qc = new QueryClient({
        defaultOptions: {
          queries: {
            retry: false,
            enabled: false,
          },
        },
      });
      return (
        <QueryClientProvider client={qc}>
          <Story />
        </QueryClientProvider>
      );
    },
  ],
};

/**
 * Apenas receitas — todas as categorias são do tipo Income.
 */
export const OnlyIncome: Story = {
  decorators: [
    (Story) => {
      const incomeCategories = mockCategories.filter((c) => c.type === 'Income');
      return (
        <QueryClientProvider client={createMockQueryClient(incomeCategories)}>
          <Story />
        </QueryClientProvider>
      );
    },
  ],
};

/**
 * Apenas despesas — todas as categorias são do tipo Expense.
 */
export const OnlyExpenses: Story = {
  decorators: [
    (Story) => {
      const expenseCategories = mockCategories.filter((c) => c.type === 'Expense');
      return (
        <QueryClientProvider client={createMockQueryClient(expenseCategories)}>
          <Story />
        </QueryClientProvider>
      );
    },
  ],
};
