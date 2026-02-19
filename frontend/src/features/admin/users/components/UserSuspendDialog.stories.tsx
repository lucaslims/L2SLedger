import type { Meta, StoryObj } from '@storybook/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { UserSuspendDialog } from './UserSuspendDialog';

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

const meta: Meta<typeof UserSuspendDialog> = {
  title: 'Admin/UserSuspendDialog',
  component: UserSuspendDialog,
  tags: ['autodocs'],
  parameters: {
    layout: 'centered',
    docs: {
      description: {
        component:
          'Diálogo de confirmação para suspender um usuário ativo. ' +
          'Exibe nome do usuário, aviso de perda de acesso e campo de motivo obrigatório. ' +
          'Usa useSuspendUser hook com toast de feedback. ' +
          'Botão de confirmação é destructive e desabilitado sem motivo.',
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
      description: 'ID do usuário a ser suspenso',
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
 * Diálogo aberto — campo de motivo vazio, botão desabilitado.
 */
export const Open: Story = {
  args: {
    open: true,
    userId: 'usr-003',
    userName: 'João Santos',
  },
};

/**
 * Diálogo fechado — nenhum conteúdo visível.
 */
export const Closed: Story = {
  args: {
    open: false,
    userId: 'usr-003',
    userName: 'João Santos',
  },
};

/**
 * Nome longo — verifica layout com nomes extensos.
 */
export const LongName: Story = {
  args: {
    open: true,
    userId: 'usr-007',
    userName: 'Ana Beatriz Rodrigues de Souza Lima',
  },
};
