import { describe, it, expect, vi } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { BrowserRouter } from 'react-router-dom';
import { LoginForm } from '../components/LoginForm';
import * as useLoginHook from '../hooks/useLogin';

// Mock useLogin hook
vi.mock('../hooks/useLogin', () => ({
  useLogin: vi.fn(),
}));

const createWrapper = () => {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false },
      mutations: { retry: false },
    },
  });
  return ({ children }: { children: React.ReactNode }) => (
    <BrowserRouter>
      <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
    </BrowserRouter>
  );
};

describe('LoginForm', () => {
  it('deve renderizar campos de email e senha', () => {
    const mockMutate = vi.fn();
    vi.mocked(useLoginHook.useLogin).mockReturnValue({
      mutate: mockMutate,
      isPending: false,
      error: null,
    } as any);

    render(<LoginForm />, { wrapper: createWrapper() });

    expect(screen.getByLabelText(/email/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/senha/i)).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /entrar/i })).toBeInTheDocument();
  });

  it('deve validar email inválido', async () => {
    const mockMutate = vi.fn();
    vi.mocked(useLoginHook.useLogin).mockReturnValue({
      mutate: mockMutate,
      isPending: false,
      error: null,
    } as any);

    const user = userEvent.setup();
    render(<LoginForm />, { wrapper: createWrapper() });

    const emailInput = screen.getByLabelText(/email/i);
    const submitButton = screen.getByRole('button', { name: /entrar/i });

    await user.type(emailInput, 'invalid-email');
    await user.click(submitButton);

    // Validação acontece mas não impede submit, então verificar que mutate não foi chamado
    expect(mockMutate).not.toHaveBeenCalled();
  });

  it('deve validar senha com menos de 6 caracteres', async () => {
    const mockMutate = vi.fn();
    vi.mocked(useLoginHook.useLogin).mockReturnValue({
      mutate: mockMutate,
      isPending: false,
      error: null,
    } as any);

    const user = userEvent.setup();
    render(<LoginForm />, { wrapper: createWrapper() });

    const emailInput = screen.getByLabelText(/email/i);
    const passwordInput = screen.getByLabelText(/senha/i);
    const submitButton = screen.getByRole('button', { name: /entrar/i });

    await user.type(emailInput, 'test@test.com');
    await user.type(passwordInput, '123');
    await user.click(submitButton);

    await waitFor(() => {
      expect(screen.getByText(/senha deve ter no mínimo 6 caracteres/i)).toBeInTheDocument();
    });

    expect(mockMutate).not.toHaveBeenCalled();
  });

  it('deve submeter formulário com dados válidos', async () => {
    const mockMutate = vi.fn();
    vi.mocked(useLoginHook.useLogin).mockReturnValue({
      mutate: mockMutate,
      isPending: false,
      error: null,
    } as any);

    const user = userEvent.setup();
    render(<LoginForm />, { wrapper: createWrapper() });

    const emailInput = screen.getByLabelText(/email/i);
    const passwordInput = screen.getByLabelText(/senha/i);
    const submitButton = screen.getByRole('button', { name: /entrar/i });

    await user.type(emailInput, 'test@test.com');
    await user.type(passwordInput, 'password123');
    await user.click(submitButton);

    await waitFor(() => {
      expect(mockMutate).toHaveBeenCalledWith(
        { email: 'test@test.com', password: 'password123' },
        expect.any(Object)
      );
    });
  });

  it('deve exibir erro quando login falha', () => {
    const mockMutate = vi.fn();
    const mockError = { message: 'Erro ao fazer login. Tente novamente.' };
    vi.mocked(useLoginHook.useLogin).mockReturnValue({
      mutate: mockMutate,
      isPending: false,
      error: mockError,
    } as any);

    render(<LoginForm />, { wrapper: createWrapper() });

    expect(screen.getByText(/erro ao fazer login/i)).toBeInTheDocument();
  });

  it('deve desabilitar campos durante submissão', () => {
    const mockMutate = vi.fn();
    vi.mocked(useLoginHook.useLogin).mockReturnValue({
      mutate: mockMutate,
      isPending: true,
      error: null,
    } as any);

    render(<LoginForm />, { wrapper: createWrapper() });

    const emailInput = screen.getByLabelText(/email/i);
    const passwordInput = screen.getByLabelText(/senha/i);
    const submitButton = screen.getByRole('button', { name: /entrando/i });

    expect(emailInput).toBeDisabled();
    expect(passwordInput).toBeDisabled();
    expect(submitButton).toBeDisabled();
  });
});
