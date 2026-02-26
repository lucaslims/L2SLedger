import { createContext, useEffect, useRef, useState } from 'react';
import { User as FirebaseUser, onAuthStateChanged } from 'firebase/auth';
import { auth } from '@/shared/lib/firebase';
import type { UserDto } from '@/shared/types/api.types';
import { apiClient } from '@/shared/lib/api/client';
import { API_ENDPOINTS } from '@/shared/lib/api/endpoints';
import type { CurrentUserResponse } from '@/shared/types/api.types';
import { authService } from '@/features/auth/services/authService';

interface AuthContextValue {
  firebaseUser: FirebaseUser | null;
  currentUser: UserDto | null;
  isLoading: boolean;
  isAuthenticated: boolean;
}

// eslint-disable-next-line react-refresh/only-export-components
export const AuthContext = createContext<AuthContextValue | undefined>(undefined);

interface AuthProviderProps {
  children: React.ReactNode;
}

/**
 * TTL do cookie de sessão backend: 1 hora (ADR-045)
 * Refresh silencioso iniciado 5 minutos antes da expiração.
 */
const COOKIE_TTL_MS = 60 * 60 * 1000; // 1 hora
const REFRESH_BEFORE_MS = 5 * 60 * 1000; // 5 minutos antes
const REFRESH_DELAY_MS = COOKIE_TTL_MS - REFRESH_BEFORE_MS; // 55 minutos

/**
 * AuthProvider
 *
 * Responsável por:
 * 1. Monitorar estado do Firebase Auth
 * 2. Verificar sessão no backend (/auth/me)
 * 3. Renovar cookie silenciosamente (ADR-045)
 * 4. Prover contexto de autenticação
 *
 * Segurança:
 * - Não carrega código protegido até sessão confirmada
 * - Usa cookies HttpOnly (não armazena tokens)
 * - Refresh automático 5 min antes da expiração do cookie (1h)
 */
export function AuthProvider({ children }: AuthProviderProps) {
  const [firebaseUser, setFirebaseUser] = useState<FirebaseUser | null>(null);
  const [currentUser, setCurrentUser] = useState<UserDto | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const refreshTimerRef = useRef<ReturnType<typeof setTimeout> | null>(null);
  const isMountedRef = useRef(true);

  /**
   * Agenda o refresh silencioso do cookie (ADR-045).
   * Cancela qualquer timer anterior antes de agendar novo.
   */
  const scheduleRefresh = (fbUser: FirebaseUser) => {
    if (refreshTimerRef.current) {
      clearTimeout(refreshTimerRef.current);
    }

    refreshTimerRef.current = setTimeout(async () => {
      try {
        const freshToken = await fbUser.getIdToken(/* forceRefresh */ true);
        await authService.refresh(freshToken);
        // Reagendar para próximo ciclo apenas se ainda montado
        if (isMountedRef.current) {
          scheduleRefresh(fbUser);
        }
      } catch (error) {
        console.error('Falha no refresh silencioso de sessão (ADR-045):', error);
      }
    }, REFRESH_DELAY_MS);
  };

  /**
   * Cancela o timer de refresh silencioso (chamado no logout ou unmount).
   */
  const cancelRefresh = () => {
    if (refreshTimerRef.current) {
      clearTimeout(refreshTimerRef.current);
      refreshTimerRef.current = null;
    }
  };

  useEffect(() => {
    // Listener do Firebase Auth
    const unsubscribe = onAuthStateChanged(auth, async (user) => {
      setFirebaseUser(user);

      if (user) {
        try {
          // Verificar sessão no backend
          // IMPORTANTE: O cookie HttpOnly deve ter sido criado pelo backend após POST /auth/login
          const response = await apiClient.get<CurrentUserResponse>(API_ENDPOINTS.AUTH_ME);
          setCurrentUser(response.user);

          // Agendar refresh silencioso do cookie (ADR-045)
          scheduleRefresh(user);
        } catch (error) {
          console.error('Erro ao verificar sessão:', error);
          setCurrentUser(null);
          cancelRefresh();
        }
      } else {
        setCurrentUser(null);
        cancelRefresh();
      }

      setIsLoading(false);
    });

    return () => {
      isMountedRef.current = false;
      unsubscribe();
      cancelRefresh();
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const value: AuthContextValue = {
    firebaseUser,
    currentUser,
    isLoading,
    isAuthenticated: !!currentUser,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
