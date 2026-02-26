import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, act } from '@testing-library/react';
import { AuthProvider, AuthContext } from '@/app/providers/AuthProvider';
import { useContext } from 'react';

vi.mock('@/shared/lib/firebase', () => ({ auth: {} }));

const mockOnAuthStateChanged = vi.fn();
vi.mock('firebase/auth', () => ({
  onAuthStateChanged: (auth: unknown, callback: (u: unknown) => void) => {
    mockOnAuthStateChanged(auth, callback);
    return vi.fn();
  },
}));

const mockApiGet = vi.fn();
vi.mock('@/shared/lib/api/client', () => ({
  apiClient: { get: (...a: unknown[]) => mockApiGet(...a) },
}));

const mockRefresh = vi.fn();
vi.mock('@/features/auth/services/authService', () => ({
  authService: { refresh: (...a: unknown[]) => mockRefresh(...a) },
}));

const TestConsumer = () => {
  const ctx = useContext(AuthContext);
  return <div>{ctx?.isAuthenticated ? 'auth' : 'unauth'}</div>;
};

const makeUser = (token = 'token-123') => ({
  uid: 'uid',
  email: 'u@t.com',
  getIdToken: vi.fn().mockResolvedValue(token),
});

const makeResp = () => ({
  user: { id: '1', email: 'u@t.com', displayName: 'T', roles: [] },
});

describe('AuthProvider - refresh silencioso (ADR-045)', () => {
  beforeEach(() => vi.clearAllMocks());

  it('deve agendar refresh 55 minutos apos login', async () => {
    vi.useFakeTimers();
    const fbUser = makeUser('fresh-token');

    mockOnAuthStateChanged.mockImplementation((_: unknown, cb: (u: unknown) => void) => {
      queueMicrotask(() => cb(fbUser));
      return vi.fn();
    });
    mockApiGet.mockResolvedValue(makeResp());
    mockRefresh.mockResolvedValue(undefined);

    render(
      <AuthProvider>
        <TestConsumer />
      </AuthProvider>
    );

    // deixar microtasks resolverem (queueMicrotask + promise chain)
    await act(async () => {
      await vi.advanceTimersByTimeAsync(0);
    });

    expect(mockApiGet).toHaveBeenCalled();

    // avançar exatamente 55 min (sem ultrapassar para nao re-agendar)
    await act(async () => {
      await vi.advanceTimersByTimeAsync(55 * 60 * 1000);
    });

    expect(fbUser.getIdToken).toHaveBeenCalledWith(true);
    expect(mockRefresh).toHaveBeenCalledWith('fresh-token');

    vi.useRealTimers();
  });

  it('nao deve agendar refresh sem usuario Firebase', async () => {
    vi.useFakeTimers();

    mockOnAuthStateChanged.mockImplementation((_: unknown, cb: (u: unknown) => void) => {
      queueMicrotask(() => cb(null));
      return vi.fn();
    });

    render(
      <AuthProvider>
        <TestConsumer />
      </AuthProvider>
    );

    await act(async () => {
      await vi.advanceTimersByTimeAsync(60 * 60 * 1000);
    });

    expect(mockRefresh).not.toHaveBeenCalled();
    vi.useRealTimers();
  });

  it('deve tolerar falha no refresh sem propagar erro', async () => {
    vi.useFakeTimers();
    const fbUser = makeUser();

    mockOnAuthStateChanged.mockImplementation((_: unknown, cb: (u: unknown) => void) => {
      queueMicrotask(() => cb(fbUser));
      return vi.fn();
    });
    mockApiGet.mockResolvedValue(makeResp());
    mockRefresh.mockRejectedValue(new Error('AUTH_INVALID_TOKEN'));
    const spy = vi.spyOn(console, 'error').mockImplementation(() => {});

    render(
      <AuthProvider>
        <TestConsumer />
      </AuthProvider>
    );

    await act(async () => {
      await vi.advanceTimersByTimeAsync(0);
    });

    expect(mockApiGet).toHaveBeenCalled();

    await act(async () => {
      await vi.advanceTimersByTimeAsync(55 * 60 * 1000);
    });

    expect(mockRefresh).toHaveBeenCalled();
    expect(spy).toHaveBeenCalledWith(
      expect.stringContaining('Falha no refresh silencioso'),
      expect.any(Error)
    );

    spy.mockRestore();
    vi.useRealTimers();
  });

  it('deve cancelar timer ao desmontar', async () => {
    vi.useFakeTimers();
    const clrSpy = vi.spyOn(globalThis, 'clearTimeout');
    const fbUser = makeUser();

    mockOnAuthStateChanged.mockImplementation((_: unknown, cb: (u: unknown) => void) => {
      queueMicrotask(() => cb(fbUser));
      return vi.fn();
    });
    mockApiGet.mockResolvedValue(makeResp());
    mockRefresh.mockResolvedValue(undefined);

    const { unmount } = render(
      <AuthProvider>
        <TestConsumer />
      </AuthProvider>
    );

    await act(async () => {
      await vi.advanceTimersByTimeAsync(0);
    });

    expect(mockApiGet).toHaveBeenCalled();

    unmount();

    expect(clrSpy).toHaveBeenCalled();
    clrSpy.mockRestore();
    vi.useRealTimers();
  });
});
