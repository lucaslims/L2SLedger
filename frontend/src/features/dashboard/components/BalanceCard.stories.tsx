import type { Meta, StoryObj } from '@storybook/react';
import { BalanceCard } from './BalanceCard';

const meta: Meta<typeof BalanceCard> = {
  title: 'Dashboard/BalanceCard',
  component: BalanceCard,
  tags: ['autodocs'],
  parameters: {
    layout: 'centered',
    docs: {
      description: {
        component:
          'Card para exibição de valores financeiros no dashboard. ' +
          'Suporta 3 tipos: income (receita), expense (despesa), balance (saldo). ' +
          'Não contém lógica financeira — apenas formatação e exibição.',
      },
    },
  },
  argTypes: {
    type: {
      control: 'select',
      options: ['income', 'expense', 'balance'],
      description: 'Tipo do card: receita, despesa ou saldo',
    },
    value: {
      control: 'number',
      description: 'Valor monetário em centavos/unidade',
    },
    label: {
      control: 'text',
      description: 'Label exibido no header do card',
    },
  },
  decorators: [
    (Story) => (
      <div style={{ width: 320 }}>
        <Story />
      </div>
    ),
  ],
};

export default meta;
type Story = StoryObj<typeof meta>;

/**
 * Card de receitas com valor positivo.
 * Exibe ícone ↗ verde e valor em cor de receita.
 */
export const Income: Story = {
  args: {
    type: 'income',
    value: 15250.75,
    label: 'Receitas',
  },
};

/**
 * Card de despesas com valor de gastos.
 * Exibe ícone ↘ vermelho e valor em cor de despesa.
 */
export const Expense: Story = {
  args: {
    type: 'expense',
    value: 8430.2,
    label: 'Despesas',
  },
};

/**
 * Card de saldo com valor líquido.
 * Exibe ícone de carteira e valor em cor primária.
 */
export const Balance: Story = {
  args: {
    type: 'balance',
    value: 6820.55,
    label: 'Saldo',
  },
};

/**
 * Card com valor zero — edge case.
 */
export const ZeroValue: Story = {
  args: {
    type: 'balance',
    value: 0,
    label: 'Saldo',
  },
};

/**
 * Card com valor grande para verificar formatação.
 */
export const LargeValue: Story = {
  args: {
    type: 'income',
    value: 1234567.89,
    label: 'Receitas',
  },
};

/**
 * Todos os 3 tipos lado a lado.
 */
export const AllVariants: Story = {
  render: () => (
    <div className="grid grid-cols-3 gap-4" style={{ width: 960 }}>
      <BalanceCard type="income" value={15250.75} label="Receitas" />
      <BalanceCard type="expense" value={8430.2} label="Despesas" />
      <BalanceCard type="balance" value={6820.55} label="Saldo" />
    </div>
  ),
};
