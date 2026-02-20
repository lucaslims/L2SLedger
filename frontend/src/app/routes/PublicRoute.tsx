import { Navigate } from 'react-router-dom';
import { useAuth } from '@/app/providers/useAuth';
import { ROUTES } from '@/shared/lib/utils/constants';

interface PublicRouteProps {
  children: React.ReactNode;
}

/**
 * PublicRoute
 *
 * Guard para rotas públicas (login, register)
 * - Redireciona para dashboard se já autenticado
 */
export function PublicRoute({ children }: PublicRouteProps) {
  const { isAuthenticated, isLoading } = useAuth();

  if (isLoading) {
    return null; // Aguardar verificação
  }

  if (isAuthenticated) {
    return <Navigate to={ROUTES.DASHBOARD} replace />;
  }

  return <>{children}</>;
}
