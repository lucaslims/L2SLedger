import type { Meta, StoryObj } from '@storybook/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { MemoryRouter } from 'react-router-dom';
import { RecentTransactions } from './RecentTransactions';
import type { RecentTransaction } from '../services/dashboardService';

// Mock data
const mockTransactions: RecentTransaction[] = [
  {
    id: '1',
    description: 'Salário',
    amount: 8500.0,
    type: 'Income',
    categoryName: 'Renda',
    date: '2026-02-15',
  },
  {
    id: '2',
    description: 'Aluguel',
    amount: 2200.0,
    type: 'Expense',
    categoryName: 'Moradia',
    date: '2026-02-10',
  },
  {
    id: '3',
    description: 'Freelance Design',
    amount: 3200.0,
    type: 'Income',
    categoryName: 'Renda Extra',
    date: '2026-02-08',
  },
  {
    id: '4',
    description: 'Supermercado',
    amount: 450.3,
    type: 'Expense',
    categoryName: 'Alimentação',
    date: '2026-02-07',
  },
  {
    id: '5',
    description: 'Conta de Luz',
    amount: 180.9,
    type: 'Expense',
    categoryName: 'Utilidades',
    date: '2026-02-05',
  },
];

/**
 * Creates a QueryClient with pre-set data for Storybook stories.
 */
function createMockQueryClient(data?: RecentTransaction[] | null, error?: boolean) {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: {
        retry: false,
        staleTime: Infinity,
      },
    },
  });

  if (error) {
    // Para simular erro, configuramos um queryFn que sempre rejeita
    const errorQueryFn = () => Promise.reject(new Error('Erro ao carregar transações recentes'));
    queryClient.setDefaultOptions({
      queries: {
        retry: false,
        queryFn: errorQueryFn,
      },
    });
  } else if (data !== undefined) {
    // Usar a mesma queryKey que o hook useRecentTransactions
    queryClient.setQueryData(['transactions', 'recent'], data);
  }

  return queryClient;
}

const meta: Meta<typeof RecentTransactions> = {
  title: 'Dashboard/RecentTransactions',
  component: RecentTransactions,
  tags: ['autodocs'],
  parameters: {
    layout: 'centered',
    docs: {
      description: {
        component:
          'Preview das últimas transações no dashboard. ' +
          'Exibe até 5 transações recentes com tipo, valor e data. ' +
          'Não contém lógica financeira — apenas exibição.',
      },
    },
  },
  decorators: [
    (Story) => (
      <MemoryRouter initialEntries={['/dashboard']}>
        <div style={{ width: 600 }}>
          <Story />
        </div>
      </MemoryRouter>
    ),
  ],
};

export default meta;
type Story = StoryObj<typeof meta>;

/**
 * Estado com transações carregadas — mix de receitas e despesas.
 */
export const WithData: Story = {
  decorators: [
    (Story) => (
      <QueryClientProvider client={createMockQueryClient(mockTransactions)}>
        <Story />
      </QueryClientProvider>
    ),
  ],
};

/**
 * Estado vazio — nenhuma transação encontrada.
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
 * Estado de carregamento com skeletons.
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
 * Estado de erro ao carregar transações.
 */
export const ErrorState: Story = {
  decorators: [
    (Story) => (
      <QueryClientProvider client={createMockQueryClient(undefined, true)}>
        <Story />
      </QueryClientProvider>
    ),
  ],
};
