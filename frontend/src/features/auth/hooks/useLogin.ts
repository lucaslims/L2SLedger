import { useMutation } from '@tanstack/react-query';
import { signInWithEmail, getIdToken, isEmailVerified } from '@/shared/lib/firebase/auth';
import { authService } from '../services/authService';
import { queryClient } from '@/shared/lib/queryClient';
import { QUERY_KEYS } from '@/shared/lib/utils/constants';
import { ApiError } from '@/shared/types/errors.types';

interface LoginCredentials {
  email: string;
  password: string;
}

interface EmailNotVerifiedError extends ApiError {
  email: string;
}

/**
 * Hook para login de usuário
 * Fluxo:
 * 1. Login no Firebase
 * 2. Verificar se email está verificado
 * 3. Obter ID token
 * 4. Login no backend com token
 */
export function useLogin() {
  return useMutation({
    mutationFn: async ({ email, password }: LoginCredentials) => {
      // 1. Login no Firebase
      const firebaseUser = await signInWithEmail(email, password);

      // 2. Verificar se email está verificado
      if (!isEmailVerified(firebaseUser)) {
        const emailNotVerifiedError = new ApiError(
          'AUTH_EMAIL_NOT_VERIFIED',
          'Por favor, verifique seu email antes de fazer login.'
        ) as EmailNotVerifiedError;
        emailNotVerifiedError.email = firebaseUser.email || email;
        throw emailNotVerifiedError;
      }

      // 3. Obter ID token
      const firebaseToken = await getIdToken(firebaseUser);

      // 4. Login no backend
      return authService.login(firebaseToken);
    },
    onSuccess: () => {
      // Invalidar cache de usuário para forçar re-fetch
      // AuthProvider irá detectar a mudança no Firebase e buscar /auth/me
      queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.AUTH] });
      
      // Aguardar um pouco para garantir que o cookie foi setado
      setTimeout(() => {
        window.location.reload();
      }, 100);
    },
  });
}
