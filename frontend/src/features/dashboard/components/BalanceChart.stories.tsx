import type { Meta, StoryObj } from '@storybook/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { BalanceChart } from './BalanceChart';
import type { DailyBalance } from '../services/dashboardService';

// Mock data — 14 dias de evolução financeira
const mockDailyBalances: DailyBalance[] = [
  { date: '2026-02-01', income: 0, expense: 350, balance: -350 },
  { date: '2026-02-02', income: 8500, expense: 0, balance: 8150 },
  { date: '2026-02-03', income: 0, expense: 120, balance: 8030 },
  { date: '2026-02-04', income: 0, expense: 450, balance: 7580 },
  { date: '2026-02-05', income: 0, expense: 180, balance: 7400 },
  { date: '2026-02-06', income: 1200, expense: 0, balance: 8600 },
  { date: '2026-02-07', income: 0, expense: 320, balance: 8280 },
  { date: '2026-02-08', income: 3200, expense: 0, balance: 11480 },
  { date: '2026-02-09', income: 0, expense: 90, balance: 11390 },
  { date: '2026-02-10', income: 0, expense: 2200, balance: 9190 },
  { date: '2026-02-11', income: 0, expense: 75, balance: 9115 },
  { date: '2026-02-12', income: 500, expense: 0, balance: 9615 },
  { date: '2026-02-13', income: 0, expense: 200, balance: 9415 },
  { date: '2026-02-14', income: 0, expense: 150, balance: 9265 },
];

/**
 * Creates a QueryClient with pre-set daily balance data.
 */
function createMockQueryClient(
  data?: DailyBalance[] | null,
  error?: boolean
) {
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
    const errorQueryFn = () => Promise.reject(new Error('Erro ao carregar dados do gráfico'));
    queryClient.setDefaultOptions({
      queries: {
        retry: false,
        queryFn: errorQueryFn,
      },
    });
  } else if (data !== undefined) {
    // Usar a mesma queryKey que o hook useDailyBalances
    queryClient.setQueryData(['daily-balances', undefined, undefined], data);
  }

  return queryClient;
}

const meta: Meta<typeof BalanceChart> = {
  title: 'Dashboard/BalanceChart',
  component: BalanceChart,
  tags: ['autodocs'],
  parameters: {
    layout: 'padded',
    docs: {
      description: {
        component:
          'Gráfico de evolução financeira diária usando Tremor AreaChart. ' +
          'Exibe receitas, despesas e saldo ao longo do tempo. ' +
          'Não contém lógica financeira — apenas visualização.',
      },
    },
  },
  decorators: [
    (Story) => (
      <div style={{ width: 800 }}>
        <Story />
      </div>
    ),
  ],
};

export default meta;
type Story = StoryObj<typeof meta>;

/**
 * Gráfico com dados de 14 dias — cenário típico de uso.
 */
export const WithData: Story = {
  decorators: [
    (Story) => (
      <QueryClientProvider client={createMockQueryClient(mockDailyBalances)}>
        <Story />
      </QueryClientProvider>
    ),
  ],
};

/**
 * Estado sem dados — período vazio.
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
 * Estado de carregamento com skeleton.
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
 * Estado de erro ao carregar dados do gráfico.
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
