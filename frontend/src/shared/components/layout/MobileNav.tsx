import { NavLink } from 'react-router-dom';
import { ROUTES } from '@/shared/lib/utils/constants';
import { Home, CreditCard, FolderOpen } from 'lucide-react';
import { cn } from '@/shared/lib/utils/cn';

/**
 * Item de navegação mobile
 */
interface MobileNavItem {
  to: string;
  label: string;
  icon: React.ComponentType<{ className?: string }>;
}

const mobileNavItems: MobileNavItem[] = [
  { to: ROUTES.DASHBOARD, label: 'Dashboard', icon: Home },
  { to: ROUTES.TRANSACTIONS, label: 'Transações', icon: CreditCard },
  { to: ROUTES.CATEGORIES, label: 'Categorias', icon: FolderOpen },
];

/**
 * MobileNav
 * 
 * Barra de navegação inferior para dispositivos móveis.
 * Fixa na parte inferior da tela com ícones e labels.
 */
export function MobileNav() {
  return (
    <nav
      className="fixed bottom-0 left-0 right-0 z-50 border-t bg-background/95 backdrop-blur supports-[backdrop-filter]:bg-background/60"
      aria-label="Navegação mobile"
    >
      <div className="flex h-16 items-center justify-around px-2">
        {mobileNavItems.map((item) => (
          <NavLink
            key={item.to}
            to={item.to}
            className={({ isActive }) =>
              cn(
                'flex flex-col items-center gap-1 rounded-lg px-3 py-1.5 text-xs font-medium transition-colors',
                isActive
                  ? 'text-primary'
                  : 'text-muted-foreground hover:text-foreground'
              )
            }
          >
            <item.icon className="h-5 w-5" />
            <span>{item.label}</span>
          </NavLink>
        ))}
      </div>
    </nav>
  );
}
