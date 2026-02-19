import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { BrowserRouter } from 'react-router-dom';
import type { ReactNode } from 'react';
import { UserList } from '../components/UserList';
import { UserStatusBadge } from '../components/UserStatusBadge';
import type { UserSummaryDto } from '../types/user.types';

const mockUsers: UserSummaryDto[] = [
  {
    id: '1',
    email: 'admin@test.com',
    displayName: 'Admin User',
    status: 'Active',
    roles: ['Admin'],
    createdAt: '2026-01-01T00:00:00Z',
  },
  {
    id: '2',
    email: 'pending@test.com',
    displayName: 'Pending User',
    status: 'Pending',
    roles: [],
    createdAt: '2026-01-15T00:00:00Z',
  },
  {
    id: '3',
    email: 'suspended@test.com',
    displayName: 'Suspended User',
    status: 'Suspended',
    roles: ['Leitura'],
    createdAt: '2026-01-10T00:00:00Z',
  },
];

function createWrapper() {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false },
      mutations: { retry: false },
    },
  });

  return function Wrapper({ children }: { children: ReactNode }) {
    return (
      <QueryClientProvider client={queryClient}>
        <BrowserRouter>{children}</BrowserRouter>
      </QueryClientProvider>
    );
  };
}

describe('UserStatusBadge', () => {
  it('deve renderizar badge para status Active', () => {
    render(<UserStatusBadge status="Active" />);
    expect(screen.getByText('Ativo')).toBeInTheDocument();
  });

  it('deve renderizar badge para status Pending', () => {
    render(<UserStatusBadge status="Pending" />);
    expect(screen.getByText('Pendente')).toBeInTheDocument();
  });

  it('deve renderizar badge para status Suspended', () => {
    render(<UserStatusBadge status="Suspended" />);
    expect(screen.getByText('Suspenso')).toBeInTheDocument();
  });

  it('deve renderizar badge para status Rejected', () => {
    render(<UserStatusBadge status="Rejected" />);
    expect(screen.getByText('Rejeitado')).toBeInTheDocument();
  });
});

describe('UserList', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('deve renderizar tabela com usuários', () => {
    const Wrapper = createWrapper();
    render(
      <Wrapper>
        <UserList users={mockUsers} />
      </Wrapper>
    );

    expect(screen.getByText('Admin User')).toBeInTheDocument();
    expect(screen.getByText('Pending User')).toBeInTheDocument();
    expect(screen.getByText('Suspended User')).toBeInTheDocument();
  });

  it('deve exibir emails dos usuários', () => {
    const Wrapper = createWrapper();
    render(
      <Wrapper>
        <UserList users={mockUsers} />
      </Wrapper>
    );

    expect(screen.getByText('admin@test.com')).toBeInTheDocument();
    expect(screen.getByText('pending@test.com')).toBeInTheDocument();
    expect(screen.getByText('suspended@test.com')).toBeInTheDocument();
  });

  it('deve exibir status badges', () => {
    const Wrapper = createWrapper();
    render(
      <Wrapper>
        <UserList users={mockUsers} />
      </Wrapper>
    );

    expect(screen.getByText('Ativo')).toBeInTheDocument();
    expect(screen.getByText('Pendente')).toBeInTheDocument();
    expect(screen.getByText('Suspenso')).toBeInTheDocument();
  });

  it('deve exibir roles dos usuários', () => {
    const Wrapper = createWrapper();
    render(
      <Wrapper>
        <UserList users={mockUsers} />
      </Wrapper>
    );

    expect(screen.getByText('Admin')).toBeInTheDocument();
    expect(screen.getByText('Leitura')).toBeInTheDocument();
  });

  it('deve exibir estado vazio quando não há usuários', () => {
    const Wrapper = createWrapper();
    render(
      <Wrapper>
        <UserList users={[]} />
      </Wrapper>
    );

    expect(screen.getByText('Nenhum usuário encontrado')).toBeInTheDocument();
  });

  it('deve renderizar cabeçalhos da tabela', () => {
    const Wrapper = createWrapper();
    render(
      <Wrapper>
        <UserList users={mockUsers} />
      </Wrapper>
    );

    expect(screen.getByText('Nome')).toBeInTheDocument();
    expect(screen.getByText('Email')).toBeInTheDocument();
    expect(screen.getByText('Status')).toBeInTheDocument();
    expect(screen.getByText('Roles')).toBeInTheDocument();
  });

  it('deve ter botão de ação para cada usuário', () => {
    const Wrapper = createWrapper();
    render(
      <Wrapper>
        <UserList users={mockUsers} />
      </Wrapper>
    );

    const actionButtons = screen.getAllByRole('button');
    expect(actionButtons.length).toBe(mockUsers.length);
  });
});
