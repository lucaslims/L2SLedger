import { NavLink } from 'react-router-dom';
import { ROUTES, ROLES } from '@/shared/lib/utils/constants';
import { Home, CreditCard, FolderOpen, Users } from 'lucide-react';
import { cn } from '@/shared/lib/utils/cn';
import { useAuth } from '@/app/providers/useAuth';

/**
 * Item de navegação mobile
 */
interface MobileNavItem {
  to: string;
  label: string;
  icon: React.ComponentType<{ className?: string }>;
  adminOnly?: boolean;
}

const mobileNavItems: MobileNavItem[] = [
  { to: ROUTES.DASHBOARD, label: 'Dashboard', icon: Home },
  { to: ROUTES.TRANSACTIONS, label: 'Transações', icon: CreditCard },
  { to: ROUTES.CATEGORIES, label: 'Categorias', icon: FolderOpen },
  { to: ROUTES.ADMIN_USERS, label: 'Usuários', icon: Users, adminOnly: true },
];

/**
 * MobileNav
 *
 * Barra de navegação inferior para dispositivos móveis.
 * Fixa na parte inferior da tela com ícones e labels.
 * Exibe item "Usuários" apenas para Admin.
 */
export function MobileNav() {
  const { currentUser } = useAuth();
  const isAdmin = currentUser?.roles.includes(ROLES.ADMIN);

  // Filtrar itens baseado em permissões
  const visibleItems = mobileNavItems.filter((item) => !item.adminOnly || isAdmin);

  return (
    <nav
      className="fixed bottom-0 left-0 right-0 z-50 border-t bg-background/95 backdrop-blur supports-[backdrop-filter]:bg-background/60"
      aria-label="Navegação mobile"
    >
      <div className="flex h-16 items-center justify-around px-2">
        {visibleItems.map((item) => (
          <NavLink
            key={item.to}
            to={item.to}
            className={({ isActive }) =>
              cn(
                'flex flex-col items-center gap-1 rounded-lg px-3 py-1.5 text-xs font-medium transition-colors',
                isActive ? 'text-primary' : 'text-muted-foreground hover:text-foreground'
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
