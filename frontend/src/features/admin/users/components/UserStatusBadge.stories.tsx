import type { Meta, StoryObj } from '@storybook/react';
import { UserStatusBadge } from './UserStatusBadge';

const meta: Meta<typeof UserStatusBadge> = {
  title: 'Admin/UserStatusBadge',
  component: UserStatusBadge,
  tags: ['autodocs'],
  parameters: {
    layout: 'centered',
    docs: {
      description: {
        component:
          'Badge visual que indica o status de um usuário no sistema. ' +
          'Cada status possui variante de cor distinta para identificação rápida. ' +
          'Pendente = cinza, Ativo = verde, Suspenso = vermelho, Rejeitado = outline.',
      },
    },
  },
  argTypes: {
    status: {
      control: 'select',
      options: ['Pending', 'Active', 'Suspended', 'Rejected'],
      description: 'Status do usuário',
    },
  },
};

export default meta;
type Story = StoryObj<typeof meta>;

/**
 * Usuário com cadastro pendente de aprovação — badge cinza (secondary).
 */
export const Pending: Story = {
  args: {
    status: 'Pending',
  },
};

/**
 * Usuário ativo no sistema — badge verde (default).
 */
export const Active: Story = {
  args: {
    status: 'Active',
  },
};

/**
 * Usuário suspenso pelo administrador — badge vermelho (destructive).
 */
export const Suspended: Story = {
  args: {
    status: 'Suspended',
  },
};

/**
 * Cadastro rejeitado pelo administrador — badge outline.
 */
export const Rejected: Story = {
  args: {
    status: 'Rejected',
  },
};

/**
 * Todos os badges lado a lado para comparação visual.
 */
export const AllStatuses: Story = {
  render: () => (
    <div className="flex items-center gap-4">
      <UserStatusBadge status="Pending" />
      <UserStatusBadge status="Active" />
      <UserStatusBadge status="Suspended" />
      <UserStatusBadge status="Rejected" />
    </div>
  ),
};
