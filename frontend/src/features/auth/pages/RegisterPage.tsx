import { RegisterForm } from '../components/RegisterForm';
import { Link } from 'react-router-dom';
import { ROUTES } from '@/shared/lib/utils/constants';

/**
 * Página de Registro
 * Permite criar nova conta no Firebase
 */
export default function RegisterPage() {
  return (
    <div className="flex min-h-screen items-center justify-center bg-background px-4">
      <div className="w-full max-w-md space-y-6 rounded-lg border bg-card p-8 shadow-sm">
        <div className="text-center">
          <h1 className="text-3xl font-bold text-primary">L2SLedger</h1>
          <h2 className="mt-2 text-xl font-semibold">Criar conta</h2>
          <p className="mt-2 text-sm text-muted-foreground">
            Preencha os dados abaixo para começar
          </p>
        </div>

        <RegisterForm />

        <div className="text-center text-sm">
          <span className="text-muted-foreground">Já tem uma conta? </span>
          <Link to={ROUTES.LOGIN} className="font-medium text-primary hover:underline">
            Faça login
          </Link>
        </div>
      </div>
    </div>
  );
}
