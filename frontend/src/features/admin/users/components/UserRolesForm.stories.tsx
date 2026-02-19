import type { Meta, StoryObj } from '@storybook/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { UserRolesForm } from './UserRolesForm';

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

const meta: Meta<typeof UserRolesForm> = {
  title: 'Admin/UserRolesForm',
  component: UserRolesForm,
  tags: ['autodocs'],
  parameters: {
    layout: 'centered',
    docs: {
      description: {
        component:
          'Formulário de checkboxes para gerenciar roles de um usuário ativo. ' +
          'Roles disponíveis: Admin, Leitura, Financeiro. ' +
          'Validações: mínimo 1 role, change detection para habilitar botão Salvar. ' +
          'Usa useUpdateUserRoles hook com toast de feedback.',
      },
    },
  },
  argTypes: {
    userId: {
      control: 'text',
      description: 'ID do usuário cujas roles serão editadas',
    },
    currentRoles: {
      control: 'object',
      description: 'Array de roles atuais do usuário',
    },
    onSuccess: {
      action: 'onSuccess',
      description: 'Callback executado após atualização bem-sucedida',
    },
  },
  decorators: [
    (Story) => (
      <QueryClientProvider client={createMockQueryClient()}>
        <div style={{ width: 350 }}>
          <Story />
        </div>
      </QueryClientProvider>
    ),
  ],
};

export default meta;
type Story = StoryObj<typeof meta>;

/**
 * Usuário com role Leitura — cenário padrão de usuário comum.
 */
export const SingleRole: Story = {
  args: {
    userId: 'usr-003',
    currentRoles: ['Leitura'],
  },
};

/**
 * Usuário Admin com acesso total — duas roles atribuídas.
 */
export const AdminUser: Story = {
  args: {
    userId: 'usr-001',
    currentRoles: ['Admin', 'Financeiro'],
  },
};

/**
 * Todas as roles selecionadas — cenário de super-usuário.
 */
export const AllRoles: Story = {
  args: {
    userId: 'usr-001',
    currentRoles: ['Admin', 'Leitura', 'Financeiro'],
  },
};

/**
 * Apenas role Financeiro — usuário com permissão de escrita financeira.
 */
export const FinanceiroOnly: Story = {
  args: {
    userId: 'usr-004',
    currentRoles: ['Financeiro'],
  },
};
