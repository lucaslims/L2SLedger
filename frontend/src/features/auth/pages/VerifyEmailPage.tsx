import { VerifyEmailCard } from '../components/VerifyEmailCard';
import { useLocation, Link } from 'react-router-dom';
import { ROUTES } from '@/shared/lib/utils/constants';
import { ArrowLeft } from 'lucide-react';

/**
 * Página de Verificação de Email
 * Exibida após registro
 */
export default function VerifyEmailPage() {
  const location = useLocation();
  const email = location.state?.email;

  return (
    <div className="flex min-h-screen items-center justify-center bg-background px-4">
      <div className="w-full max-w-md space-y-6">
        <VerifyEmailCard email={email} />

        <div className="text-center">
          <Link
            to={ROUTES.LOGIN}
            className="inline-flex items-center text-sm text-muted-foreground hover:text-primary"
          >
            <ArrowLeft className="mr-2 h-4 w-4" />
            Voltar para login
          </Link>
        </div>
      </div>
    </div>
  );
}
