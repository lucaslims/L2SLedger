import { LoginForm } from '../components/LoginForm';
import { Link } from 'react-router-dom';
import { ROUTES } from '@/shared/lib/utils/constants';

/**
 * Página de Login
 * Permite usuário fazer login com Firebase + Backend
 */
export default function LoginPage() {
  return (
    <div className="flex min-h-screen items-center justify-center bg-background px-4">
      <div className="w-full max-w-md space-y-6 rounded-lg border bg-card p-8 shadow-sm">
        <div className="text-center">
          <h1 className="text-3xl font-bold text-primary">L2SLedger</h1>
          <h2 className="mt-2 text-xl font-semibold">Bem-vindo de volta</h2>
          <p className="mt-2 text-sm text-muted-foreground">
            Entre com suas credenciais para acessar sua conta
          </p>
        </div>

        <LoginForm />

        <div className="text-center text-sm">
          <span className="text-muted-foreground">Não tem uma conta? </span>
          <Link to={ROUTES.REGISTER} className="text-primary hover:underline font-medium">
            Cadastre-se
          </Link>
        </div>
      </div>
    </div>
  );
}
