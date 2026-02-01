import { lazy } from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { ROUTES } from '@/shared/lib/utils/constants';
import { ProtectedRoute } from './ProtectedRoute';
import { PublicRoute } from './PublicRoute';
// import { AdminRoute } from './AdminRoute';

// Public pages (loaded in public bundle)
const LoginPage = lazy(() => import('@/features/auth/pages/LoginPage'));
const RegisterPage = lazy(() => import('@/features/auth/pages/RegisterPage'));
const VerifyEmailPage = lazy(() => import('@/features/auth/pages/VerifyEmailPage'));
const PendingApprovalPage = lazy(() => import('@/features/auth/pages/PendingApprovalPage'));

// Protected pages (lazy loaded after auth confirmation)
const DashboardPage = lazy(() => import('@/features/dashboard/pages/DashboardPage'));

// Admin pages (lazy loaded for admin users)
// const UsersPage = lazy(() => import('@/features/admin/users/pages/UsersPage'));

/**
 * Configuração de rotas da aplicação
 * 
 * SEGURANÇA:
 * - Rotas públicas: public bundle
 * - Rotas protegidas: lazy load após autenticação
 * - Rotas admin: lazy load após verificação de role
 */
export function AppRoutes() {
  return (
    <BrowserRouter>
      <Routes>
        {/* Home - redirect to dashboard or login */}
        <Route
          path={ROUTES.HOME}
          element={<Navigate to={ROUTES.DASHBOARD} replace />}
        />

        {/* Public routes */}
        <Route
          path={ROUTES.LOGIN}
          element={
            <PublicRoute>
              <LoginPage />
            </PublicRoute>
          }
        />
        <Route
          path={ROUTES.REGISTER}
          element={
            <PublicRoute>
              <RegisterPage />
            </PublicRoute>
          }
        />
        <Route
          path={ROUTES.VERIFY_EMAIL}
          element={
            <PublicRoute>
              <VerifyEmailPage />
            </PublicRoute>
          }
        />

        {/* Status pages (accessible by authenticated users with specific status) */}
        <Route path={ROUTES.PENDING_APPROVAL} element={<PendingApprovalPage />} />
        {/* TODO: SuspendedPage, RejectedPage */}

        {/* Protected routes */}
        <Route
          path={ROUTES.DASHBOARD}
          element={
            <ProtectedRoute>
              <DashboardPage />
            </ProtectedRoute>
          }
        />

        {/* Admin routes */}
        {/* <Route
          path={ROUTES.ADMIN_USERS}
          element={
            <AdminRoute>
              <UsersPage />
            </AdminRoute>
          }
        /> */}

        {/* 404 */}
        <Route path="*" element={<Navigate to={ROUTES.HOME} replace />} />
      </Routes>
    </BrowserRouter>
  );
}
