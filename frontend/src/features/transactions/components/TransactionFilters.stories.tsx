import type { Meta, StoryObj } from '@storybook/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { TransactionFilters } from './TransactionFilters';
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

/**
 * Cria um QueryClient com categorias pré-carregadas.
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

  queryClient.setQueryData([QUERY_KEYS.CATEGORIES, undefined], mockCategories);

  return queryClient;
}

const meta: Meta<typeof TransactionFilters> = {
  title: 'Transactions/TransactionFilters',
  component: TransactionFilters,
  tags: ['autodocs'],
  parameters: {
    layout: 'centered',
    docs: {
      description: {
        component:
          'Filtros de transações com seleção de tipo (Receita/Despesa) e categoria. ' +
          'Exibe botão para limpar filtros ativos. ' +
          'Categorias são carregadas via hook useCategories. ' +
          'Não contém lógica financeira — apenas filtragem de dados.',
      },
    },
  },
  argTypes: {
    onFilterChange: {
      action: 'onFilterChange',
      description: 'Callback disparado ao alterar qualquer filtro',
    },
  },
  decorators: [
    (Story) => (
      <QueryClientProvider client={createMockQueryClient()}>
        <div style={{ width: 600 }}>
          <Story />
        </div>
      </QueryClientProvider>
    ),
  ],
};

export default meta;
type Story = StoryObj<typeof meta>;

/**
 * Estado padrão — nenhum filtro ativo.
 */
export const Default: Story = {
  args: {
    filters: {
      page: 1,
      pageSize: 10,
    },
  },
};

/**
 * Filtro por tipo ativo — exibindo apenas receitas.
 */
export const WithTypeFilter: Story = {
  args: {
    filters: {
      type: 1,
      page: 1,
      pageSize: 10,
    },
  },
};

/**
 * Filtro por categoria ativo — exibindo apenas transações da categoria Alimentação.
 */
export const WithCategoryFilter: Story = {
  args: {
    filters: {
      categoryId: 'cat-001',
      page: 1,
      pageSize: 10,
    },
  },
};
