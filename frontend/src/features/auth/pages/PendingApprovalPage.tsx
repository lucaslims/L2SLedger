/**
 * PendingApprovalPage
 */
export default function PendingApprovalPage() {
  return (
    <div className="flex min-h-screen items-center justify-center bg-background">
      <div className="w-full max-w-md space-y-6 rounded-lg border bg-card p-8 shadow-sm">
        <div className="text-center">
          <h1 className="text-3xl font-bold text-primary">L2SLedger</h1>
          <h2 className="mt-2 text-xl font-semibold">Aguardando Aprovação</h2>
          <p className="mt-4 text-sm text-muted-foreground">
            Seu cadastro está aguardando aprovação do administrador.
          </p>
          <p className="mt-2 text-sm text-muted-foreground">
            Você receberá um email quando sua conta for aprovada.
          </p>
        </div>
      </div>
    </div>
  );
}
