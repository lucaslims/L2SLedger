import { describe, it, expect, vi, beforeEach } from 'vitest';
import { authService } from '../services/authService';
import { apiClient } from '@/shared/lib/api/client';
import { API_ENDPOINTS } from '@/shared/lib/api/endpoints';

// Mock do apiClient
vi.mock('@/shared/lib/api/client', () => ({
  apiClient: {
    post: vi.fn(),
    get: vi.fn(),
  },
}));

describe('authService.refresh (ADR-045)', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('deve chamar POST /auth/refresh com o token Firebase no header Authorization', async () => {
    vi.mocked(apiClient.post).mockResolvedValue(undefined);

    const firebaseToken = 'firebase-id-token-fresco';
    await authService.refresh(firebaseToken);

    expect(apiClient.post).toHaveBeenCalledWith(API_ENDPOINTS.AUTH_REFRESH, undefined, {
      headers: { Authorization: `Bearer ${firebaseToken}` },
    });
  });

  it('deve usar o endpoint correto: AUTH_REFRESH', () => {
    expect(API_ENDPOINTS.AUTH_REFRESH).toBe('/auth/refresh');
  });

  it('deve propagar erro se o apiClient lançar (ex: 401 token expirado)', async () => {
    const apiError = new Error('AUTH_INVALID_TOKEN');
    vi.mocked(apiClient.post).mockRejectedValue(apiError);

    await expect(authService.refresh('token-invalido')).rejects.toThrow('AUTH_INVALID_TOKEN');
  });

  it('deve resolver com undefined em caso de sucesso (204/200 sem body)', async () => {
    vi.mocked(apiClient.post).mockResolvedValue(undefined);

    const result = await authService.refresh('token-valido');

    expect(result).toBeUndefined();
  });
});
