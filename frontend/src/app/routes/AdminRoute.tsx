import { Navigate } from 'react-router-dom';
import { useAuth } from '@/app/providers/useAuth';
import { ROUTES, ROLES } from '@/shared/lib/utils/constants';
import { LoadingScreen } from '@/shared/components/feedback/LoadingScreen';
import { Suspense } from 'react';

interface AdminRouteProps {
  children: React.ReactNode;
}

/**
 * AdminRoute
 * 
 * Guard para rotas administrativas
 * - Herda verificações de ProtectedRoute
 * - Adiciona verificação de role Admin
 */
export function AdminRoute({ children }: AdminRouteProps) {
  const { currentUser, isLoading } = useAuth();

  if (isLoading) {
    return <LoadingScreen />;
  }

  if (!currentUser) {
    return <Navigate to={ROUTES.LOGIN} replace />;
  }

  // Verificar se é Admin
  const isAdmin = currentUser.roles.includes(ROLES.ADMIN);

  if (!isAdmin) {
    return <Navigate to={ROUTES.DASHBOARD} replace />;
  }

  return <Suspense fallback={<LoadingScreen />}>{children}</Suspense>;
}
