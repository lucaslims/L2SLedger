import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/shared/components/ui/card';
import { Button } from '@/shared/components/ui/button';
import { useResendVerification } from '../hooks/useResendVerification';
import { CheckCircle2, Mail, AlertCircle, Clock } from 'lucide-react';
import { Alert, AlertDescription } from '@/shared/components/ui/alert';
import { ApiError } from '@/shared/types/errors.types';
import { useState, useEffect } from 'react';

interface VerifyEmailCardProps {
  email?: string;
}

/**
 * Card exibido na página de verificação de email
 * Permite reenviar email de verificação
 */
export function VerifyEmailCard({ email }: VerifyEmailCardProps) {
  const { mutate: resendEmail, isPending, isSuccess, error } = useResendVerification();

  // Timer configurável via .env (padrão: 60 segundos)
  const cooldownSeconds = Number(import.meta.env.VITE_EMAIL_VERIFICATION_RESEND_COOLDOWN) || 60;
  const [remainingTime, setRemainingTime] = useState(0);
  const [canResend, setCanResend] = useState(true);

  useEffect(() => {
    if (remainingTime > 0) {
      const timer = setTimeout(() => {
        setRemainingTime(remainingTime - 1);
      }, 1000);
      return () => clearTimeout(timer);
    } else if (remainingTime === 0 && !canResend) {
      setCanResend(true);
    }
  }, [remainingTime, canResend]);

  const handleResend = () => {
    resendEmail();
    setRemainingTime(cooldownSeconds);
    setCanResend(false);
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
          <p>Clique no link enviado para verificar seu email e ativar sua conta.</p>
          <p className="mt-2">Não esqueça de verificar sua caixa de spam!</p>
        </div>

        {isSuccess && (
          <Alert>
            <CheckCircle2 className="h-4 w-4" />
            <AlertDescription>Email de verificação reenviado com sucesso!</AlertDescription>
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
          disabled={isPending || !canResend}
        >
          {isPending ? (
            'Reenviando...'
          ) : !canResend ? (
            <span className="flex items-center gap-2">
              <Clock className="h-4 w-4" />
              Aguarde {remainingTime}s para reenviar
            </span>
          ) : (
            'Reenviar email de verificação'
          )}
        </Button>
      </CardContent>
    </Card>
  );
}
