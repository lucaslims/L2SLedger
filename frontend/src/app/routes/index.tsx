import { lazy, Suspense } from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { ROUTES } from '@/shared/lib/utils/constants';
import { ProtectedRoute } from './ProtectedRoute';
import { PublicRoute } from './PublicRoute';
import { LoadingScreen } from '@/shared/components/feedback/LoadingScreen';
import { AdminRoute } from './AdminRoute';

// Public pages (loaded in public bundle)
const LoginPage = lazy(() => import('@/features/auth/pages/LoginPage'));
const RegisterPage = lazy(() => import('@/features/auth/pages/RegisterPage'));
const VerifyEmailPage = lazy(() => import('@/features/auth/pages/VerifyEmailPage'));
const PendingApprovalPage = lazy(() => import('@/features/auth/pages/PendingApprovalPage'));
const SuspendedPage = lazy(() => import('@/features/auth/pages/SuspendedPage'));
const RejectedPage = lazy(() => import('@/features/auth/pages/RejectedPage'));

// Protected pages (lazy loaded after auth confirmation)
const DashboardPage = lazy(() => import('@/features/dashboard/pages/DashboardPage'));
const CategoriesPage = lazy(() => import('@/features/categories/pages/CategoriesPage'));
const CategoryFormPage = lazy(() => import('@/features/categories/pages/CategoryFormPage'));
const TransactionsPage = lazy(() => import('@/features/transactions/pages/TransactionsPage'));
const TransactionFormPage = lazy(() => import('@/features/transactions/pages/TransactionFormPage'));

// Admin pages (lazy loaded for admin users)
const UsersPage = lazy(() => import('@/features/admin/users/pages/UsersPage'));
const UserDetailPage = lazy(() => import('@/features/admin/users/pages/UserDetailPage'));

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
      <Suspense fallback={<LoadingScreen />}>
        <Routes>
          {/* Home - redirect to dashboard or login */}
          <Route path={ROUTES.HOME} element={<Navigate to={ROUTES.DASHBOARD} replace />} />

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
          <Route path={ROUTES.SUSPENDED} element={<SuspendedPage />} />
          <Route path={ROUTES.REJECTED} element={<RejectedPage />} />

          {/* Protected routes */}
          <Route
            path={ROUTES.DASHBOARD}
            element={
              <ProtectedRoute>
                <DashboardPage />
              </ProtectedRoute>
            }
          />

          {/* Categories routes */}
          <Route
            path={ROUTES.CATEGORIES}
            element={
              <ProtectedRoute>
                <CategoriesPage />
              </ProtectedRoute>
            }
          />
          <Route
            path={ROUTES.CATEGORIES_NEW}
            element={
              <ProtectedRoute>
                <CategoryFormPage />
              </ProtectedRoute>
            }
          />
          <Route
            path={ROUTES.CATEGORIES_EDIT}
            element={
              <ProtectedRoute>
                <CategoryFormPage />
              </ProtectedRoute>
            }
          />

          {/* Transactions routes */}
          <Route
            path={ROUTES.TRANSACTIONS}
            element={
              <ProtectedRoute>
                <TransactionsPage />
              </ProtectedRoute>
            }
          />
          <Route
            path={ROUTES.TRANSACTIONS_NEW}
            element={
              <ProtectedRoute>
                <TransactionFormPage />
              </ProtectedRoute>
            }
          />
          <Route
            path={ROUTES.TRANSACTIONS_EDIT}
            element={
              <ProtectedRoute>
                <TransactionFormPage />
              </ProtectedRoute>
            }
          />

          {/* Admin routes */}
          <Route
            path={ROUTES.ADMIN_USERS}
            element={
              <AdminRoute>
                <UsersPage />
              </AdminRoute>
            }
          />
          <Route
            path={ROUTES.ADMIN_USER_DETAIL}
            element={
              <AdminRoute>
                <UserDetailPage />
              </AdminRoute>
            }
          />

          {/* 404 */}
          <Route path="*" element={<Navigate to={ROUTES.HOME} replace />} />
        </Routes>
      </Suspense>
    </BrowserRouter>
  );
}
