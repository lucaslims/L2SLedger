import { createContext, useEffect, useState } from 'react';
import { User as FirebaseUser, onAuthStateChanged } from 'firebase/auth';
import { auth } from '@/shared/lib/firebase';
import type { UserDto } from '@/shared/types/api.types';
import { apiClient } from '@/shared/lib/api/client';
import { API_ENDPOINTS } from '@/shared/lib/api/endpoints';
import type { CurrentUserResponse } from '@/shared/types/api.types';

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
 * AuthProvider
 *
 * Responsável por:
 * 1. Monitorar estado do Firebase Auth
 * 2. Verificar sessão no backend (/auth/me)
 * 3. Prover contexto de autenticação
 *
 * Segurança:
 * - Não carrega código protegido até sessão confirmada
 * - Usa cookies HttpOnly (não armazena tokens)
 */
export function AuthProvider({ children }: AuthProviderProps) {
  const [firebaseUser, setFirebaseUser] = useState<FirebaseUser | null>(null);
  const [currentUser, setCurrentUser] = useState<UserDto | null>(null);
  const [isLoading, setIsLoading] = useState(true);

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
        } catch (error) {
          console.error('Erro ao verificar sessão:', error);
          setCurrentUser(null);
        }
      } else {
        setCurrentUser(null);
      }

      setIsLoading(false);
    });

    return () => unsubscribe();
  }, []);

  const value: AuthContextValue = {
    firebaseUser,
    currentUser,
    isLoading,
    isAuthenticated: !!currentUser,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
