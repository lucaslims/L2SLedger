import type { Meta, StoryObj } from '@storybook/react';
import { CategoryCard } from './CategoryCard';
import type { CategoryDto } from '../types/category.types';

const baseCategoryExpense: CategoryDto = {
  id: 'cat-001',
  name: 'Alimentação',
  description: 'Despesas com alimentação e refeições',
  type: 'Expense',
  isActive: true,
  parentCategoryId: null,
  parentCategoryName: null,
  createdAt: '2026-01-15T10:00:00Z',
  updatedAt: null,
};

const baseCategoryIncome: CategoryDto = {
  id: 'cat-002',
  name: 'Salário',
  description: 'Renda mensal principal',
  type: 'Income',
  isActive: true,
  parentCategoryId: null,
  parentCategoryName: null,
  createdAt: '2026-01-10T08:00:00Z',
  updatedAt: '2026-02-01T09:30:00Z',
};

const meta: Meta<typeof CategoryCard> = {
  title: 'Categories/CategoryCard',
  component: CategoryCard,
  tags: ['autodocs'],
  parameters: {
    layout: 'centered',
    docs: {
      description: {
        component:
          'Card individual de categoria financeira para exibição em grid ou mobile. ' +
          'Exibe nome, badge de tipo (Receita/Despesa) e botões de editar/excluir. ' +
          'Não contém lógica financeira — apenas exibição.',
      },
    },
  },
  argTypes: {
    onEdit: {
      action: 'onEdit',
      description: 'Callback ao clicar em editar — recebe o ID da categoria',
    },
    onDelete: {
      action: 'onDelete',
      description: 'Callback ao clicar em excluir — recebe o CategoryDto completo',
    },
  },
  decorators: [
    (Story) => (
      <div style={{ width: 400 }}>
        <Story />
      </div>
    ),
  ],
};

export default meta;
type Story = StoryObj<typeof meta>;

/**
 * Categoria do tipo Despesa — badge vermelho (destructive).
 */
export const Expense: Story = {
  args: {
    category: baseCategoryExpense,
  },
};

/**
 * Categoria do tipo Receita — badge padrão (verde/azul).
 */
export const Income: Story = {
  args: {
    category: baseCategoryIncome,
  },
};

/**
 * Subcategoria com categoria pai definida.
 */
export const WithParent: Story = {
  args: {
    category: {
      ...baseCategoryExpense,
      id: 'cat-003',
      name: 'Restaurantes',
      description: 'Refeições em restaurantes',
      parentCategoryId: 'cat-001',
      parentCategoryName: 'Alimentação',
    },
  },
};

/**
 * Categoria com nome longo — verifica truncamento/wrap.
 */
export const LongName: Story = {
  args: {
    category: {
      ...baseCategoryExpense,
      id: 'cat-004',
      name: 'Despesas Extraordinárias com Manutenção Predial e Reformas',
      description: 'Categoria para despesas não recorrentes com manutenção',
    },
  },
};

/**
 * Categoria inativa.
 */
export const Inactive: Story = {
  args: {
    category: {
      ...baseCategoryExpense,
      id: 'cat-005',
      name: 'Categoria Inativa',
      isActive: false,
    },
  },
};
