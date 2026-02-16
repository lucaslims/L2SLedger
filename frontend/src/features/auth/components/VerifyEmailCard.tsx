import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { Button } from '@/shared/components/ui/button';
import { useResendVerification } from '../hooks/useResendVerification';
import { CheckCircle2, Mail, AlertCircle } from 'lucide-react';
import { Alert, AlertDescription } from '@/shared/components/ui/alert';
import { ApiError } from '@/shared/types/errors.types';

interface VerifyEmailCardProps {
  email?: string;
}

/**
 * Card exibido na página de verificação de email
 * Permite reenviar email de verificação
 */
export function VerifyEmailCard({ email }: VerifyEmailCardProps) {
  const { mutate: resendEmail, isPending, isSuccess, error } = useResendVerification();

  const handleResend = () => {
    resendEmail();
  };

  const getErrorMessage = (error: unknown): string => {
    if (error instanceof ApiError) {
      return error.message;
    }
    if (error instanceof Error) {
      return error.message;
    }
    return 'Erro ao reenviar email. Tente novamente.';
  };

  return (
    <Card className="w-full max-w-md">
      <CardHeader className="text-center">
        <div className="mx-auto mb-4 flex h-16 w-16 items-center justify-center rounded-full bg-primary/10">
          <Mail className="h-8 w-8 text-primary" />
        </div>
        <CardTitle>Verifique seu email</CardTitle>
        <CardDescription>
          Enviamos um link de verificação para{' '}
          <span className="font-semibold">{email || 'seu email'}</span>
        </CardDescription>
      </CardHeader>
      <CardContent className="space-y-4">
        <div className="rounded-lg border bg-muted/50 p-4 text-sm text-muted-foreground">
          <p>
            Clique no link enviado para verificar seu email e ativar sua conta.
          </p>
          <p className="mt-2">
            Não esqueça de verificar sua caixa de spam!
          </p>
        </div>

        {isSuccess && (
          <Alert>
            <CheckCircle2 className="h-4 w-4" />
            <AlertDescription>
              Email de verificação reenviado com sucesso!
            </AlertDescription>
          </Alert>
        )}

        {error && (
          <Alert variant="destructive">
            <AlertCircle className="h-4 w-4" />
            <AlertDescription>{getErrorMessage(error)}</AlertDescription>
          </Alert>
        )}

        <Button
          onClick={handleResend}
          variant="outline"
          className="w-full"
          disabled={isPending || isSuccess}
        >
          {isPending ? 'Reenviando...' : isSuccess ? 'Email reenviado' : 'Reenviar email de verificação'}
        </Button>
      </CardContent>
    </Card>
  );
}
