import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { BrowserRouter } from 'react-router-dom';
import type { ReactNode } from 'react';
import { UserRolesForm } from '../components/UserRolesForm';

vi.mock('sonner', () => ({
  toast: {
    success: vi.fn(),
    error: vi.fn(),
  },
}));

vi.mock('../services/userService', () => ({
  userService: {
    getAll: vi.fn(),
    getById: vi.fn(),
    getPendingCount: vi.fn(),
    updateStatus: vi.fn(),
    updateRoles: vi.fn(),
  },
}));

import { userService } from '../services/userService';

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

describe('UserRolesForm', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('deve renderizar todas as roles disponíveis', () => {
    const Wrapper = createWrapper();
    render(
      <Wrapper>
        <UserRolesForm userId="1" currentRoles={['Leitura']} />
      </Wrapper>
    );

    expect(screen.getByText(/admin/i)).toBeInTheDocument();
    expect(screen.getByText(/leitura/i)).toBeInTheDocument();
    expect(screen.getByText(/financeiro/i)).toBeInTheDocument();
  });

  it('deve marcar roles atuais do usuário', () => {
    const Wrapper = createWrapper();
    render(
      <Wrapper>
        <UserRolesForm userId="1" currentRoles={['Admin', 'Leitura']} />
      </Wrapper>
    );

    const adminCheckbox = screen.getByRole('checkbox', { name: /admin/i });
    const leituraCheckbox = screen.getByRole('checkbox', { name: /leitura/i });
    const financeiroCheckbox = screen.getByRole('checkbox', { name: /financeiro/i });

    expect(adminCheckbox).toHaveAttribute('data-state', 'checked');
    expect(leituraCheckbox).toHaveAttribute('data-state', 'checked');
    expect(financeiroCheckbox).toHaveAttribute('data-state', 'unchecked');
  });

  it('deve desabilitar botão salvar quando não há mudanças', () => {
    const Wrapper = createWrapper();
    render(
      <Wrapper>
        <UserRolesForm userId="1" currentRoles={['Leitura']} />
      </Wrapper>
    );

    expect(screen.getByRole('button', { name: /salvar/i })).toBeDisabled();
  });

  it('deve habilitar botão salvar quando roles mudam', async () => {
    const user = userEvent.setup();
    const Wrapper = createWrapper();
    render(
      <Wrapper>
        <UserRolesForm userId="1" currentRoles={['Leitura']} />
      </Wrapper>
    );

    await user.click(screen.getByRole('checkbox', { name: /admin/i }));

    expect(screen.getByRole('button', { name: /salvar/i })).not.toBeDisabled();
  });

  it('deve exibir mensagem quando nenhuma role selecionada', async () => {
    const user = userEvent.setup();
    const Wrapper = createWrapper();
    render(
      <Wrapper>
        <UserRolesForm userId="1" currentRoles={['Leitura']} />
      </Wrapper>
    );

    // Desmarcar a role atual
    await user.click(screen.getByRole('checkbox', { name: /leitura/i }));

    expect(screen.getByText(/pelo menos uma role/i)).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /salvar/i })).toBeDisabled();
  });

  it('deve chamar updateRoles com dados corretos ao salvar', async () => {
    vi.mocked(userService.updateRoles).mockResolvedValue({
      id: '1',
      email: 'test@test.com',
      displayName: 'Test',
      status: 'Active',
      emailVerified: true,
      roles: ['Admin', 'Leitura'],
      createdAt: '2026-01-01T00:00:00Z',
      updatedAt: null,
      lastLoginAt: null,
    });

    const user = userEvent.setup();
    const Wrapper = createWrapper();
    render(
      <Wrapper>
        <UserRolesForm userId="1" currentRoles={['Leitura']} />
      </Wrapper>
    );

    await user.click(screen.getByRole('checkbox', { name: /admin/i }));
    await user.click(screen.getByRole('button', { name: /salvar/i }));

    expect(userService.updateRoles).toHaveBeenCalledWith('1', {
      roles: ['Leitura', 'Admin'],
    });
  });
});
