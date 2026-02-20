import { Navigate, useLocation } from 'react-router-dom';
import { useAuth } from '@/app/providers/useAuth';
import { ROUTES, USER_STATUS } from '@/shared/lib/utils/constants';
import { LoadingScreen } from '@/shared/components/feedback/LoadingScreen';
import { Suspense } from 'react';

interface ProtectedRouteProps {
  children: React.ReactNode;
}

/**
 * ProtectedRoute
 *
 * Guard para rotas protegidas
 * - Verifica autenticação no backend
 * - Verifica status do usuário
 * - Carrega código protegido apenas se autorizado (lazy loading)
 *
 * SEGURANÇA:
 * - Código protegido não é carregado sem autenticação
 * - Redirecionamento baseado em status do usuário
 */
export function ProtectedRoute({ children }: ProtectedRouteProps) {
  const { currentUser, isLoading } = useAuth();
  const location = useLocation();

  // Ainda verificando sessão
  if (isLoading) {
    return <LoadingScreen />;
  }

  // Não autenticado
  if (!currentUser) {
    return <Navigate to={ROUTES.LOGIN} state={{ from: location }} replace />;
  }

  // Verificar status do usuário
  switch (currentUser.status) {
    case USER_STATUS.PENDING:
      return <Navigate to={ROUTES.PENDING_APPROVAL} replace />;

    case USER_STATUS.SUSPENDED:
      return <Navigate to={ROUTES.SUSPENDED} replace />;

    case USER_STATUS.REJECTED:
      return <Navigate to={ROUTES.REJECTED} replace />;

    case USER_STATUS.ACTIVE:
      // Usuário ativo - permitir acesso
      // Código protegido será carregado aqui (lazy)
      return <Suspense fallback={<LoadingScreen />}>{children}</Suspense>;

    default:
      // Status desconhecido - negar acesso
      return <Navigate to={ROUTES.LOGIN} replace />;
  }
}
