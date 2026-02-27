import { L2SLogo } from '@/assets/logo';

/**
 * LoadingScreen
 *
 * Exibido durante verificação inicial de sessão
 * Parte do bundle inicial (main.js)
 */
export function LoadingScreen() {
  return (
    <div className="flex min-h-screen items-center justify-center bg-background">
      <div className="text-center">
        <div className="mb-4 flex justify-center">
          <L2SLogo variant="full" width={200} />
        </div>

        <div className="flex items-center justify-center space-x-2">
          <div className="h-2 w-2 animate-pulse rounded-full bg-primary"></div>
          <div
            className="h-2 w-2 animate-pulse rounded-full bg-primary"
            style={{ animationDelay: '0.2s' }}
          ></div>
          <div
            className="h-2 w-2 animate-pulse rounded-full bg-primary"
            style={{ animationDelay: '0.4s' }}
          ></div>
        </div>

        <p className="mt-4 text-sm text-muted-foreground">Verificando sessão...</p>
      </div>
    </div>
  );
}
