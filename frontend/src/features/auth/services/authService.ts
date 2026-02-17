import { apiClient } from '@/shared/lib/api/client';
import { API_ENDPOINTS } from '@/shared/lib/api/endpoints';
import type { LoginRequest, LoginResponse, CurrentUserResponse } from '@/shared/types/api.types';

/**
 * Serviço de autenticação
 * Gerencia comunicação com backend para login/logout/sessão
 */
export const authService = {
  /**
   * Login com Firebase token
   * @param firebaseToken Token obtido do Firebase após signIn
   * @returns Dados do usuário + sessão
   */
  async login(firebaseIdToken: string): Promise<LoginResponse> {
    return apiClient.post<LoginResponse>(API_ENDPOINTS.AUTH_LOGIN, {
      firebaseIdToken,
    } as LoginRequest);
  },

  /**
   * Logout (limpa sessão no backend)
   * Cookie HttpOnly é removido pelo backend
   */
  async logout(): Promise<void> {
    return apiClient.post<void>(API_ENDPOINTS.AUTH_LOGOUT);
  },

  /**
   * Verificar sessão atual
   * Usado pelo AuthProvider para validar sessão ao carregar app
   * @returns Dados do usuário logado
   */
  async me(): Promise<CurrentUserResponse> {
    return apiClient.get<CurrentUserResponse>(API_ENDPOINTS.AUTH_ME);
  },
};
