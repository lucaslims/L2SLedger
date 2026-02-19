import type { Meta, StoryObj } from '@storybook/react';
import { TransactionSummaryCards } from './TransactionSummaryCards';

const meta: Meta<typeof TransactionSummaryCards> = {
  title: 'Transactions/TransactionSummaryCards',
  component: TransactionSummaryCards,
  tags: ['autodocs'],
  parameters: {
    layout: 'centered',
    docs: {
      description: {
        component:
          'Cards de resumo financeiro exibindo receitas, despesas e saldo. ' +
          'Formatação monetária em R$ com ícones contextuais. ' +
          'Cor do saldo muda conforme positivo (azul) ou negativo (laranja). ' +
          'Não contém lógica financeira — apenas exibição de totais.',
      },
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
 * Saldo positivo — receitas superam despesas.
 */
export const Positive: Story = {
  args: {
    totalIncome: 11700.0,
    totalExpense: 2278.4,
    balance: 9421.6,
  },
};

/**
 * Saldo negativo — despesas superam receitas.
 */
export const Negative: Story = {
  args: {
    totalIncome: 3200.0,
    totalExpense: 5500.0,
    balance: -2300.0,
  },
};

/**
 * Saldo zerado — receitas iguais às despesas.
 */
export const Zero: Story = {
  args: {
    totalIncome: 4000.0,
    totalExpense: 4000.0,
    balance: 0,
  },
};

/**
 * Valores altos — verifica formatação monetária com valores grandes.
 */
export const HighValues: Story = {
  args: {
    totalIncome: 125750.5,
    totalExpense: 98320.75,
    balance: 27429.75,
  },
};
