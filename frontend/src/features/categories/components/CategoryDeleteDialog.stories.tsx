import type { Meta, StoryObj } from '@storybook/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { CategoryDeleteDialog } from './CategoryDeleteDialog';

/**
 * Cria um QueryClient mock para isolar o Storybook do backend real.
 */
function createMockQueryClient() {
  return new QueryClient({
    defaultOptions: {
      queries: {
        retry: false,
        staleTime: Infinity,
      },
      mutations: {
        retry: false,
      },
    },
  });
}

const meta: Meta<typeof CategoryDeleteDialog> = {
  title: 'Categories/CategoryDeleteDialog',
  component: CategoryDeleteDialog,
  tags: ['autodocs'],
  parameters: {
    layout: 'centered',
    docs: {
      description: {
        component:
          'Diálogo de confirmação de exclusão de categoria. ' +
          'Exibe nome da categoria e botões Cancelar/Excluir. ' +
          'Trata erros semânticos (FIN_CATEGORY_HAS_TRANSACTIONS) via hook. ' +
          'Não contém lógica financeira — apenas confirmação de ação.',
      },
    },
  },
  argTypes: {
    open: {
      control: 'boolean',
      description: 'Controla a visibilidade do diálogo',
    },
    categoryName: {
      control: 'text',
      description: 'Nome da categoria a ser excluída',
    },
    categoryId: {
      control: 'text',
      description: 'ID da categoria a ser excluída',
    },
    onOpenChange: {
      action: 'onOpenChange',
      description: 'Callback ao alterar estado de abertura do diálogo',
    },
  },
  decorators: [
    (Story) => (
      <QueryClientProvider client={createMockQueryClient()}>
        <Story />
      </QueryClientProvider>
    ),
  ],
};

export default meta;
type Story = StoryObj<typeof meta>;

/**
 * Diálogo aberto — confirmação de exclusão padrão.
 */
export const Open: Story = {
  args: {
    open: true,
    categoryId: 'cat-001',
    categoryName: 'Alimentação',
  },
};

/**
 * Diálogo fechado — não renderiza conteúdo visível.
 */
export const Closed: Story = {
  args: {
    open: false,
    categoryId: 'cat-001',
    categoryName: 'Alimentação',
  },
};

/**
 * Nome longo — verifica quebra de texto no diálogo.
 */
export const LongCategoryName: Story = {
  args: {
    open: true,
    categoryId: 'cat-002',
    categoryName: 'Despesas Extraordinárias com Manutenção Predial e Reformas',
  },
};
