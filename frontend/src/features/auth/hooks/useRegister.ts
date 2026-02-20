import { useMutation } from '@tanstack/react-query';
import { signUpWithEmail, sendVerificationEmail } from '@/shared/lib/firebase/auth';

interface RegisterCredentials {
  email: string;
  password: string;
  displayName: string;
}

interface RegisterResult {
  email: string;
}

/**
 * Hook para registro de novo usuário
 * Fluxo:
 * 1. Criar usuário no Firebase
 * 2. Enviar email de verificação
 * 3. Retornar email para exibir mensagem
 */
export function useRegister() {
  return useMutation({
    mutationFn: async ({
      email,
      password,
      displayName: _displayName,
    }: RegisterCredentials): Promise<RegisterResult> => {
      // 1. Criar usuário no Firebase
      const firebaseUser = await signUpWithEmail(email, password);

      // 2. Enviar email de verificação
      await sendVerificationEmail(firebaseUser);

      // 3. Retornar email para exibir mensagem
      return { email: firebaseUser.email || email };
    },
  });
}
