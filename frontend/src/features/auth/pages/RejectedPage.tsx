import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { XCircle } from 'lucide-react';
import { Link } from 'react-router-dom';
import { ROUTES } from '@/shared/lib/utils/constants';
import { Button } from '@/shared/components/ui/button';

/**
 * Página exibida quando usuário tem status Rejected
 * Conta rejeitada pelo administrador
 */
export default function RejectedPage() {
  return (
    <div className="flex min-h-screen items-center justify-center bg-background px-4">
      <Card className="w-full max-w-md">
        <CardHeader className="text-center">
          <div className="mx-auto mb-4 flex h-16 w-16 items-center justify-center rounded-full bg-red-500/10">
            <XCircle className="h-8 w-8 text-red-600" />
          </div>
          <CardTitle>Cadastro Não Aprovado</CardTitle>
          <CardDescription>
            Seu cadastro foi analisado e não foi aprovado
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="rounded-lg border bg-muted/50 p-4 text-sm text-muted-foreground">
            <p className="font-semibold text-foreground">O que isso significa?</p>
            <p className="mt-2">
              Após análise, sua solicitação de cadastro não foi aprovada pelos nossos administradores.
            </p>
            <p className="mt-2">
              Isso pode ter ocorrido por diversos motivos, incluindo informações incompletas ou não atendimento aos requisitos do sistema.
            </p>
          </div>

          <div className="rounded-lg border-l-4 border-red-500 bg-red-50 p-4 text-sm dark:bg-red-950/30">
            <p className="font-semibold text-red-800 dark:text-red-300">
              Precisa de esclarecimentos?
            </p>
            <p className="mt-1 text-red-700 dark:text-red-400">
              Entre em contato conosco: suporte@l2sledger.com
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
