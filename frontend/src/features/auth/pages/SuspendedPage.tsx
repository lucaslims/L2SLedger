import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { XCircle } from 'lucide-react';
import { Link } from 'react-router-dom';
import { ROUTES } from '@/shared/lib/utils/constants';
import { Button } from '@/shared/components/ui/button';

/**
 * Página exibida quando usuário tem status Suspended
 * Conta suspensa temporariamente
 */
export default function SuspendedPage() {
  return (
    <div className="flex min-h-screen items-center justify-center bg-background px-4">
      <Card className="w-full max-w-md">
        <CardHeader className="text-center">
          <div className="mx-auto mb-4 flex h-16 w-16 items-center justify-center rounded-full bg-orange-500/10">
            <XCircle className="h-8 w-8 text-orange-600" />
          </div>
          <CardTitle>Conta Suspensa</CardTitle>
          <CardDescription>
            Sua conta foi temporariamente suspensa
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="rounded-lg border bg-muted/50 p-4 text-sm text-muted-foreground">
            <p className="font-semibold text-foreground">O que isso significa?</p>
            <p className="mt-2">
              Sua conta foi suspensa devido a uma violação dos termos de uso ou por motivos de segurança.
            </p>
            <p className="mt-2">
              Entre em contato com nossa equipe de suporte para mais informações sobre como reativar sua conta.
            </p>
          </div>

          <div className="rounded-lg border-l-4 border-orange-500 bg-orange-50 p-4 text-sm dark:bg-orange-950/30">
            <p className="font-semibold text-orange-800 dark:text-orange-300">
              Precisa de ajuda?
            </p>
            <p className="mt-1 text-orange-700 dark:text-orange-400">
              Email: suporte@l2sledger.com
            </p>
          </div>

          <Button asChild variant="outline" className="w-full">
            <Link to={ROUTES.LOGIN}>
              Voltar para login
            </Link>
          </Button>
        </CardContent>
      </Card>
    </div>
  );
}
