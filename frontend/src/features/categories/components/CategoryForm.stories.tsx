import type { Meta, StoryObj } from '@storybook/react';
import { CategoryForm } from './CategoryForm';

const meta: Meta<typeof CategoryForm> = {
  title: 'Categories/CategoryForm',
  component: CategoryForm,
  tags: ['autodocs'],
  parameters: {
    layout: 'centered',
    docs: {
      description: {
        component:
          'Formulário de criação/edição de categorias financeiras. ' +
          'Validação via Zod + React Hook Form. ' +
          'Suporta modo criação (sem initialValues) e edição (com initialValues). ' +
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
      <div style={{ width: 400 }}>
        <Story />
      </div>
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
 * Modo edição — formulário preenchido com valores existentes.
 * Botão exibe "Atualizar" em vez de "Criar".
 */
export const Edit: Story = {
  args: {
    initialValues: {
      name: 'Alimentação',
      description: 'Despesas com alimentação e refeições',
      type: 'Expense',
      parentCategoryId: '',
    },
    isPending: false,
  },
};

/**
 * Modo edição de receita — categoria do tipo Income.
 */
export const EditIncome: Story = {
  args: {
    initialValues: {
      name: 'Salário',
      description: 'Renda mensal principal',
      type: 'Income',
      parentCategoryId: '',
    },
    isPending: false,
  },
};

/**
 * Modo edição com categoria pai definida.
 */
export const EditWithParent: Story = {
  args: {
    initialValues: {
      name: 'Restaurantes',
      description: 'Refeições em restaurantes',
      type: 'Expense',
      parentCategoryId: 'cat-alimentacao-001',
    },
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
 * Modo edição com submissão em andamento.
 */
export const EditPending: Story = {
  args: {
    initialValues: {
      name: 'Transporte',
      description: 'Despesas com transporte',
      type: 'Expense',
      parentCategoryId: '',
    },
    isPending: true,
  },
};
