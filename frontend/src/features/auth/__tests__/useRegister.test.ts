import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useRegister } from '../hooks/useRegister';
import * as firebaseAuth from '@/shared/lib/firebase/auth';

// Mock Firebase
vi.mock('@/shared/lib/firebase/auth', () => ({
  signUpWithEmail: vi.fn(),
  sendVerificationEmail: vi.fn(),
}));

const createWrapper = () => {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false },
      mutations: { retry: false },
    },
  });
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  return ({ children }: any) => {
    return QueryClientProvider({ client: queryClient, children });
  };
};

describe('useRegister', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('deve registrar usuário com sucesso', async () => {
    const mockFirebaseUser = { uid: '123', email: 'newuser@test.com' };

    vi.mocked(firebaseAuth.signUpWithEmail).mockResolvedValue(mockFirebaseUser as any);
    vi.mocked(firebaseAuth.sendVerificationEmail).mockResolvedValue();

    const { result } = renderHook(() => useRegister(), { wrapper: createWrapper() });

    result.current.mutate({
      email: 'newuser@test.com',
      password: 'password123',
      displayName: 'New User',
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(firebaseAuth.signUpWithEmail).toHaveBeenCalledWith('newuser@test.com', 'password123');
    expect(firebaseAuth.sendVerificationEmail).toHaveBeenCalledWith(mockFirebaseUser);
    expect(result.current.data).toEqual({ email: 'newuser@test.com' });
  });

  it('deve tratar erro de Firebase ao criar usuário', async () => {
    vi.mocked(firebaseAuth.signUpWithEmail).mockRejectedValue(
      new Error('Firebase error: email already in use')
    );

    const { result } = renderHook(() => useRegister(), { wrapper: createWrapper() });

    result.current.mutate({
      email: 'existing@test.com',
      password: 'password123',
      displayName: 'Existing User',
    });

    await waitFor(() => expect(result.current.isError).toBe(true));

    expect(result.current.error).toBeInstanceOf(Error);
  });

  it('deve tratar erro ao enviar email de verificação', async () => {
    const mockFirebaseUser = { uid: '123', email: 'newuser@test.com' };

    vi.mocked(firebaseAuth.signUpWithEmail).mockResolvedValue(mockFirebaseUser as any);
    vi.mocked(firebaseAuth.sendVerificationEmail).mockRejectedValue(
      new Error('Firebase error: failed to send email')
    );

    const { result } = renderHook(() => useRegister(), { wrapper: createWrapper() });

    result.current.mutate({
      email: 'newuser@test.com',
      password: 'password123',
      displayName: 'New User',
    });

    await waitFor(() => expect(result.current.isError).toBe(true));

    expect(result.current.error).toBeInstanceOf(Error);
  });
});
