import type { Meta, StoryObj } from '@storybook/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { UserApprovalDialog } from './UserApprovalDialog';

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

const meta: Meta<typeof UserApprovalDialog> = {
  title: 'Admin/UserApprovalDialog',
  component: UserApprovalDialog,
  tags: ['autodocs'],
  parameters: {
    layout: 'centered',
    docs: {
      description: {
        component:
          'Diálogo de duas etapas para aprovar ou rejeitar um usuário pendente. ' +
          'Etapa 1: Escolher ação (Aprovar/Rejeitar). ' +
          'Etapa 2: Informar motivo obrigatório e confirmar. ' +
          'Usa useApproveUser/useRejectUser hooks com toast de feedback. ' +
          'Não contém lógica financeira — apenas interação administrativa.',
      },
    },
  },
  argTypes: {
    open: {
      control: 'boolean',
      description: 'Controla a visibilidade do diálogo',
    },
    userId: {
      control: 'text',
      description: 'ID do usuário a ser aprovado/rejeitado',
    },
    userName: {
      control: 'text',
      description: 'Nome do usuário exibido no diálogo',
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
 * Diálogo aberto — etapa inicial com botões Aprovar e Rejeitar.
 */
export const Open: Story = {
  args: {
    open: true,
    userId: 'usr-002',
    userName: 'Maria Oliveira',
  },
};

/**
 * Diálogo fechado — nenhum conteúdo visível.
 */
export const Closed: Story = {
  args: {
    open: false,
    userId: 'usr-002',
    userName: 'Maria Oliveira',
  },
};

/**
 * Nome longo — verifica layout com nomes extensos.
 */
export const LongName: Story = {
  args: {
    open: true,
    userId: 'usr-006',
    userName: 'José Carlos de Almeida Ferreira Neto',
  },
};
