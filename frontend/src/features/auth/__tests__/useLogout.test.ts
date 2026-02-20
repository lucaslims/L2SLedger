import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { BrowserRouter } from 'react-router-dom';
import { useLogout } from '../hooks/useLogout';
import * as firebaseAuth from '@/shared/lib/firebase/auth';
import * as authService from '../services/authService';

// Mock Firebase
vi.mock('@/shared/lib/firebase/auth', () => ({
  signOutUser: vi.fn(),
}));

// Mock authService
vi.mock('../services/authService', () => ({
  authService: {
    logout: vi.fn(),
  },
}));

// Mock useNavigate
const mockNavigate = vi.fn();
vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom');
  return {
    ...actual,
    useNavigate: () => mockNavigate,
  };
});

const createWrapper = () => {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false },
      mutations: { retry: false },
    },
  });
  return ({ children }: any) => {
    const qcp = QueryClientProvider({ client: queryClient, children });
    return BrowserRouter({ children: qcp });
  };
};

describe('useLogout', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('deve fazer logout com sucesso', async () => {
    vi.mocked(authService.authService.logout).mockResolvedValue();
    vi.mocked(firebaseAuth.signOutUser).mockResolvedValue();

    const { result } = renderHook(() => useLogout(), { wrapper: createWrapper() });

    result.current.mutate();

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(authService.authService.logout).toHaveBeenCalled();
    expect(firebaseAuth.signOutUser).toHaveBeenCalled();
    expect(mockNavigate).toHaveBeenCalledWith('/login');
  });

  it('deve tratar erro do backend ao fazer logout', async () => {
    vi.mocked(authService.authService.logout).mockRejectedValue(
      new Error('Backend error')
    );

    const { result } = renderHook(() => useLogout(), { wrapper: createWrapper() });

    result.current.mutate();

    await waitFor(() => expect(result.current.isError).toBe(true));

    expect(result.current.error).toBeInstanceOf(Error);
    expect(firebaseAuth.signOutUser).not.toHaveBeenCalled();
  });

  it('deve tratar erro do Firebase ao fazer logout', async () => {
    vi.mocked(authService.authService.logout).mockResolvedValue();
    vi.mocked(firebaseAuth.signOutUser).mockRejectedValue(
      new Error('Firebase error')
    );

    const { result } = renderHook(() => useLogout(), { wrapper: createWrapper() });

    result.current.mutate();

    await waitFor(() => expect(result.current.isError).toBe(true));

    expect(result.current.error).toBeInstanceOf(Error);
  });
});
