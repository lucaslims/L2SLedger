import type { Meta, StoryObj } from '@storybook/react';
import { MemoryRouter } from 'react-router-dom';
import { UserList } from './UserList';
import type { UserSummaryDto } from '../types/user.types';

const mockUsers: UserSummaryDto[] = [
  {
    id: 'usr-001',
    email: 'lucas@example.com',
    displayName: 'Lucas Silva',
    status: 'Active',
    roles: ['Admin', 'Financeiro'],
    createdAt: '2026-01-05T08:00:00Z',
  },
  {
    id: 'usr-002',
    email: 'maria@exemplo.com',
    displayName: 'Maria Oliveira',
    status: 'Pending',
    roles: [],
    createdAt: '2026-02-15T14:30:00Z',
  },
  {
    id: 'usr-003',
    email: 'joao@exemplo.com',
    displayName: 'João Santos',
    status: 'Active',
    roles: ['Leitura'],
    createdAt: '2026-01-20T10:00:00Z',
  },
  {
    id: 'usr-004',
    email: 'ana@exemplo.com',
    displayName: 'Ana Costa',
    status: 'Suspended',
    roles: ['Financeiro'],
    createdAt: '2026-01-10T09:00:00Z',
  },
  {
    id: 'usr-005',
    email: 'pedro@exemplo.com',
    displayName: 'Pedro Ferreira',
    status: 'Rejected',
    roles: [],
    createdAt: '2026-02-18T16:45:00Z',
  },
];

const meta: Meta<typeof UserList> = {
  title: 'Admin/UserList',
  component: UserList,
  tags: ['autodocs'],
  parameters: {
    layout: 'centered',
    docs: {
      description: {
        component:
          'Tabela de usuários do sistema para a área administrativa. ' +
          'Exibe nome, email, status (badge colorido), roles (pills), data de criação e ação de visualizar. ' +
          'Inclui empty state para quando não há usuários. ' +
          'Não contém lógica financeira — apenas exibição.',
      },
    },
  },
  decorators: [
    (Story) => (
      <MemoryRouter>
        <div style={{ width: 1000 }}>
          <Story />
        </div>
      </MemoryRouter>
    ),
  ],
};

export default meta;
type Story = StoryObj<typeof meta>;

/**
 * Tabela com usuários de todos os status — cenário típico do admin.
 */
export const Default: Story = {
  args: {
    users: mockUsers,
  },
};

/**
 * Lista vazia — exibe empty state com mensagem orientativa.
 */
export const Empty: Story = {
  args: {
    users: [],
  },
};

/**
 * Apenas usuários pendentes — filtro por status Pending aplicado.
 */
export const OnlyPending: Story = {
  args: {
    users: mockUsers.filter((u) => u.status === 'Pending'),
  },
};

/**
 * Apenas usuários ativos — filtro por status Active aplicado.
 */
export const OnlyActive: Story = {
  args: {
    users: mockUsers.filter((u) => u.status === 'Active'),
  },
};

/**
 * Usuário único — cenário mínimo.
 */
export const SingleUser: Story = {
  args: {
    users: [mockUsers[0]],
  },
};
