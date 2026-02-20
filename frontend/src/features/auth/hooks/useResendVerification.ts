import { useMutation } from '@tanstack/react-query';
import { auth } from '@/shared/lib/firebase/config';
import { sendVerificationEmail } from '@/shared/lib/firebase/auth';
import { ApiError } from '@/shared/types/errors.types';

/**
 * Hook para reenviar email de verificação
 * Usado na VerifyEmailPage
 */
export function useResendVerification() {
  return useMutation({
    mutationFn: async () => {
      const currentUser = auth.currentUser;

      if (!currentUser) {
        throw new ApiError('AUTH_UNAUTHORIZED', 'Usuário não está logado no Firebase');
      }

      if (currentUser.emailVerified) {
        throw new ApiError('VAL_INVALID_REQUEST', 'Email já está verificado');
      }

      await sendVerificationEmail(currentUser);
      return { success: true };
    },
  });
}
