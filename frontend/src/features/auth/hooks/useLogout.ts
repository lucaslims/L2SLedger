import { useMutation } from '@tanstack/react-query';
import { signOutUser } from '@/shared/lib/firebase/auth';
import { authService } from '../services/authService';
import { queryClient } from '@/shared/lib/queryClient';
import { useNavigate } from 'react-router-dom';
import { ROUTES } from '@/shared/lib/utils/constants';

/**
 * Hook para logout de usuário
 * Fluxo:
 * 1. Logout no backend (limpa sessão/cookie)
 * 2. Logout no Firebase
 * 3. Limpar cache do React Query
 * 4. Redirecionar para login
 */
export function useLogout() {
  const navigate = useNavigate();

  return useMutation({
    mutationFn: async () => {
      // 1. Logout no backend
      await authService.logout();

      // 2. Logout no Firebase
      await signOutUser();
    },
    onSuccess: () => {
      // 3. Limpar cache completo
      queryClient.clear();

      // 4. Redirecionar para login
      navigate(ROUTES.LOGIN);
    },
  });
}
