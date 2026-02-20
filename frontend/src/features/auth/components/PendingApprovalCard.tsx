import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/shared/components/ui/card';
import { Clock, AlertCircle } from 'lucide-react';
import { Alert, AlertDescription, AlertTitle } from '@/shared/components/ui/alert';

/**
 * Card exibido quando usuário está com status Pending
 * Aguardando aprovação do administrador
 */
export function PendingApprovalCard() {
  return (
    <Card className="w-full max-w-md">
      <CardHeader className="text-center">
        <div className="mx-auto mb-4 flex h-16 w-16 items-center justify-center rounded-full bg-yellow-500/10">
          <Clock className="h-8 w-8 text-yellow-600" />
        </div>
        <CardTitle>Aguardando aprovação</CardTitle>
        <CardDescription>Sua conta foi criada com sucesso!</CardDescription>
      </CardHeader>
      <CardContent className="space-y-4">
        <Alert>
          <AlertCircle className="h-4 w-4" />
          <AlertTitle>Conta em análise</AlertTitle>
          <AlertDescription>
            Um administrador precisa aprovar sua conta antes que você possa acessar o sistema. Você
            receberá um email assim que sua conta for aprovada.
          </AlertDescription>
        </Alert>

        <div className="rounded-lg border bg-muted/50 p-4 text-sm text-muted-foreground">
          <p className="font-semibold text-foreground">O que acontece agora?</p>
          <ul className="mt-2 list-inside list-disc space-y-1">
            <li>Nossa equipe irá revisar sua solicitação</li>
            <li>Você receberá um email com o resultado</li>
            <li>Este processo pode levar até 24 horas</li>
          </ul>
        </div>

        <p className="text-center text-xs text-muted-foreground">
          Não feche sua conta! Aguarde o email de confirmação.
        </p>
      </CardContent>
    </Card>
  );
}
