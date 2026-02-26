import { Header } from './Header';
import { Sidebar } from './Sidebar';
import { MobileNav } from './MobileNav';
import { useMediaQuery } from '@/shared/hooks/useMediaQuery';

interface AppLayoutProps {
  children: React.ReactNode;
}

/**
 * AppLayout
 *
 * Layout principal para usuários autenticados.
 * Inclui Sidebar (desktop), Header, e MobileNav (mobile).
 *
 * Responsivo:
 * - Desktop: Sidebar fixa + conteúdo principal
 * - Mobile: Bottom navigation + Header
 */
export function AppLayout({ children }: AppLayoutProps) {
  const isMobile = useMediaQuery('(max-width: 768px)');

  return (
    <div className="flex min-h-screen bg-background">
      {/* Sidebar Desktop */}
      {!isMobile && <Sidebar />}

      {/* Main Content */}
      <div className="flex flex-1 flex-col">
        <Header />

        <main className={`flex-1 overflow-y-auto p-4 md:p-6 lg:p-8 ${isMobile ? 'pb-24' : ''}`}>
          {children}
        </main>
      </div>

      {/* Mobile Navigation */}
      {isMobile && <MobileNav />}
    </div>
  );
}
