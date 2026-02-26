import type { Meta, StoryObj } from '@storybook/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { BalanceChart } from './BalanceChart';
import type { DailyBalance } from '../services/dashboardService';

// Mock data — 14 dias de evolução financeira
const mockDailyBalances: DailyBalance[] = [
  { date: '2026-02-01', openingBalance: 0, income: 0, expense: 350, closingBalance: -350 },
  { date: '2026-02-02', openingBalance: -350, income: 8500, expense: 0, closingBalance: 8150 },
  { date: '2026-02-03', openingBalance: 8150, income: 0, expense: 120, closingBalance: 8030 },
  { date: '2026-02-04', openingBalance: 8030, income: 0, expense: 450, closingBalance: 7580 },
  { date: '2026-02-05', openingBalance: 7580, income: 0, expense: 180, closingBalance: 7400 },
  { date: '2026-02-06', openingBalance: 7400, income: 1200, expense: 0, closingBalance: 8600 },
  { date: '2026-02-07', openingBalance: 8600, income: 0, expense: 320, closingBalance: 8280 },
  { date: '2026-02-08', openingBalance: 8280, income: 3200, expense: 0, closingBalance: 11480 },
  { date: '2026-02-09', openingBalance: 11480, income: 0, expense: 90, closingBalance: 11390 },
  { date: '2026-02-10', openingBalance: 11390, income: 0, expense: 2200, closingBalance: 9190 },
  { date: '2026-02-11', openingBalance: 9190, income: 0, expense: 75, closingBalance: 9115 },
  { date: '2026-02-12', openingBalance: 9115, income: 500, expense: 0, closingBalance: 9615 },
  { date: '2026-02-13', openingBalance: 9615, income: 0, expense: 200, closingBalance: 9415 },
  { date: '2026-02-14', openingBalance: 9415, income: 0, expense: 150, closingBalance: 9265 },
];

/**
 * Creates a QueryClient with pre-set daily balance data.
 * Must mirror the date computation in useDailyBalances hook.
 */
function createMockQueryClient(data?: DailyBalance[] | null, error?: boolean) {
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
    // Calcular datas da mesma forma que o hook useDailyBalances
    const today = new Date();
    const thirtyDaysAgo = new Date(today);
    thirtyDaysAgo.setDate(today.getDate() - 30);
    const startDate = thirtyDaysAgo.toISOString().split('T')[0];
    const endDate = today.toISOString().split('T')[0];

    queryClient.setQueryData(['daily-balances', startDate, endDate], data);
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
