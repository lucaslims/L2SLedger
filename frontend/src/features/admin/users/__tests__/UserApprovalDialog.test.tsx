import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { BrowserRouter } from 'react-router-dom';
import type { ReactNode } from 'react';
import { UserApprovalDialog } from '../components/UserApprovalDialog';

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

describe('UserApprovalDialog', () => {
  const defaultProps = {
    userId: '1',
    userName: 'Test User',
    open: true,
    onOpenChange: vi.fn(),
  };

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('deve renderizar botões Aprovar e Rejeitar inicialmente', () => {
    const Wrapper = createWrapper();
    render(
      <Wrapper>
        <UserApprovalDialog {...defaultProps} />
      </Wrapper>
    );

    expect(screen.getByRole('button', { name: /aprovar/i })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /rejeitar/i })).toBeInTheDocument();
  });

  it('deve exibir nome do usuário', () => {
    const Wrapper = createWrapper();
    render(
      <Wrapper>
        <UserApprovalDialog {...defaultProps} />
      </Wrapper>
    );

    expect(screen.getByText(/Test User/)).toBeInTheDocument();
  });

  it('deve mostrar campo de motivo ao clicar em Aprovar', async () => {
    const user = userEvent.setup();
    const Wrapper = createWrapper();
    render(
      <Wrapper>
        <UserApprovalDialog {...defaultProps} />
      </Wrapper>
    );

    await user.click(screen.getByRole('button', { name: /^aprovar$/i }));

    expect(screen.getByPlaceholderText(/verificação de documentos/i)).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /confirmar aprovação/i })).toBeInTheDocument();
  });

  it('deve mostrar campo de motivo ao clicar em Rejeitar', async () => {
    const user = userEvent.setup();
    const Wrapper = createWrapper();
    render(
      <Wrapper>
        <UserApprovalDialog {...defaultProps} />
      </Wrapper>
    );

    await user.click(screen.getByRole('button', { name: /^rejeitar$/i }));

    expect(screen.getByPlaceholderText(/verificação de documentos/i)).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /confirmar rejeição/i })).toBeInTheDocument();
  });

  it('deve desabilitar botão de confirmar quando motivo está vazio', async () => {
    const user = userEvent.setup();
    const Wrapper = createWrapper();
    render(
      <Wrapper>
        <UserApprovalDialog {...defaultProps} />
      </Wrapper>
    );

    await user.click(screen.getByRole('button', { name: /^aprovar$/i }));

    expect(screen.getByRole('button', { name: /confirmar aprovação/i })).toBeDisabled();
  });

  it('deve habilitar botão de confirmar quando motivo é preenchido', async () => {
    const user = userEvent.setup();
    const Wrapper = createWrapper();
    render(
      <Wrapper>
        <UserApprovalDialog {...defaultProps} />
      </Wrapper>
    );

    await user.click(screen.getByRole('button', { name: /^aprovar$/i }));
    await user.type(
      screen.getByPlaceholderText(/verificação de documentos/i),
      'Documentação verificada'
    );

    expect(screen.getByRole('button', { name: /confirmar aprovação/i })).not.toBeDisabled();
  });

  it('deve chamar approve com dados corretos', async () => {
    vi.mocked(userService.updateStatus).mockResolvedValue({
      id: '1',
      email: 'test@test.com',
      displayName: 'Test User',
      status: 'Active',
      emailVerified: true,
      roles: ['Leitura'],
      createdAt: '2026-01-01T00:00:00Z',
      updatedAt: null,
      lastLoginAt: null,
    });

    const user = userEvent.setup();
    const Wrapper = createWrapper();
    render(
      <Wrapper>
        <UserApprovalDialog {...defaultProps} />
      </Wrapper>
    );

    await user.click(screen.getByRole('button', { name: /^aprovar$/i }));
    await user.type(screen.getByPlaceholderText(/verificação de documentos/i), 'Documentação ok');
    await user.click(screen.getByRole('button', { name: /confirmar aprovação/i }));

    expect(userService.updateStatus).toHaveBeenCalledWith('1', {
      status: 'Active',
      reason: 'Documentação ok',
    });
  });

  it('deve ter botão Voltar para retornar à escolha inicial', async () => {
    const user = userEvent.setup();
    const Wrapper = createWrapper();
    render(
      <Wrapper>
        <UserApprovalDialog {...defaultProps} />
      </Wrapper>
    );

    await user.click(screen.getByRole('button', { name: /^aprovar$/i }));
    expect(screen.getByRole('button', { name: /voltar/i })).toBeInTheDocument();

    await user.click(screen.getByRole('button', { name: /voltar/i }));

    // Deve voltar a mostrar botões Aprovar/Rejeitar
    expect(screen.getByRole('button', { name: /^aprovar$/i })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /^rejeitar$/i })).toBeInTheDocument();
  });
});
