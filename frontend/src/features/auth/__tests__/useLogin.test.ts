import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useLogin } from '../hooks/useLogin';
import * as firebaseAuth from '@/shared/lib/firebase/auth';
import * as authService from '../services/authService';
import { ApiError } from '@/shared/types/errors.types';

// Mock Firebase
vi.mock('@/shared/lib/firebase/auth', () => ({
  signInWithEmail: vi.fn(),
  getIdToken: vi.fn(),
  isEmailVerified: vi.fn(),
}));

// Mock authService
vi.mock('../services/authService', () => ({
  authService: {
    login: vi.fn(),
  },
}));

const createWrapper = () => {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false },
      mutations: { retry: false },
    },
  });
  return ({ children }: any) => {
    return QueryClientProvider({ client: queryClient, children });
  };
};

describe('useLogin', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('deve fazer login com sucesso', async () => {
    const mockFirebaseUser = { uid: '123', emailVerified: true };
    const mockToken = 'firebase-token-123';
    const mockLoginResponse = {
      user: {
        id: '123',
        email: 'test@test.com',
        displayName: 'Test User',
        status: 'Active',
        emailVerified: true,
        roles: ['Leitura'],
        createdAt: '2026-01-01T00:00:00Z',
        updatedAt: null,
      },
      sessionExpiresAt: '2026-01-02T00:00:00Z',
    };

    vi.mocked(firebaseAuth.signInWithEmail).mockResolvedValue(mockFirebaseUser as any);
    vi.mocked(firebaseAuth.isEmailVerified).mockReturnValue(true);
    vi.mocked(firebaseAuth.getIdToken).mockResolvedValue(mockToken);
    vi.mocked(authService.authService.login).mockResolvedValue(mockLoginResponse);

    const { result } = renderHook(() => useLogin(), { wrapper: createWrapper() });

    result.current.mutate({ email: 'test@test.com', password: 'password123' });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(firebaseAuth.signInWithEmail).toHaveBeenCalledWith('test@test.com', 'password123');
    expect(firebaseAuth.isEmailVerified).toHaveBeenCalledWith(mockFirebaseUser);
    expect(firebaseAuth.getIdToken).toHaveBeenCalledWith(mockFirebaseUser);
    expect(authService.authService.login).toHaveBeenCalledWith(mockToken);
    expect(result.current.data).toEqual(mockLoginResponse);
  });

  it('deve lançar erro se email não está verificado', async () => {
    const mockFirebaseUser = { uid: '123', emailVerified: false };

    vi.mocked(firebaseAuth.signInWithEmail).mockResolvedValue(mockFirebaseUser as any);
    vi.mocked(firebaseAuth.isEmailVerified).mockReturnValue(false);

    const { result } = renderHook(() => useLogin(), { wrapper: createWrapper() });

    result.current.mutate({ email: 'test@test.com', password: 'password123' });

    await waitFor(() => expect(result.current.isError).toBe(true));

    expect(result.current.error).toBeInstanceOf(ApiError);
    expect((result.current.error as ApiError).code).toBe('AUTH_EMAIL_NOT_VERIFIED');
  });

  it('deve tratar erro de Firebase', async () => {
    vi.mocked(firebaseAuth.signInWithEmail).mockRejectedValue(
      new Error('Firebase error: invalid credentials')
    );

    const { result } = renderHook(() => useLogin(), { wrapper: createWrapper() });

    result.current.mutate({ email: 'wrong@test.com', password: 'wrongpass' });

    await waitFor(() => expect(result.current.isError).toBe(true));

    expect(result.current.error).toBeInstanceOf(Error);
  });

  it('deve tratar erro do backend', async () => {
    const mockFirebaseUser = { uid: '123', emailVerified: true };
    const mockToken = 'firebase-token-123';

    vi.mocked(firebaseAuth.signInWithEmail).mockResolvedValue(mockFirebaseUser as any);
    vi.mocked(firebaseAuth.isEmailVerified).mockReturnValue(true);
    vi.mocked(firebaseAuth.getIdToken).mockResolvedValue(mockToken);
    vi.mocked(authService.authService.login).mockRejectedValue(
      new ApiError('AUTH_USER_PENDING', 'Usuário aguardando aprovação')
    );

    const { result } = renderHook(() => useLogin(), { wrapper: createWrapper() });

    result.current.mutate({ email: 'pending@test.com', password: 'password123' });

    await waitFor(() => expect(result.current.isError).toBe(true));

    expect(result.current.error).toBeInstanceOf(ApiError);
    expect((result.current.error as ApiError).code).toBe('AUTH_USER_PENDING');
  });
});
