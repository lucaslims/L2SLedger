import { NavLink } from 'react-router-dom';
import { ROUTES, ROLES } from '@/shared/lib/utils/constants';
import { Home, CreditCard, FolderOpen, Users } from 'lucide-react';
import { cn } from '@/shared/lib/utils/cn';
import { useAuth } from '@/app/providers/useAuth';
import { Separator } from '@/shared/components/ui/separator';
import { L2SLogo } from '@/assets/logo';

/**
 * Item de navegação
 */
interface NavItem {
  to: string;
  label: string;
  icon: React.ComponentType<{ className?: string }>;
}

const navItems: NavItem[] = [
  { to: ROUTES.DASHBOARD, label: 'Dashboard', icon: Home },
  { to: ROUTES.TRANSACTIONS, label: 'Transações', icon: CreditCard },
  { to: ROUTES.CATEGORIES, label: 'Categorias', icon: FolderOpen },
];

/**
 * Sidebar
 *
 * Navegação lateral (desktop).
 * Exibe links de navegação e seção admin condicional.
 */
export function Sidebar() {
  const { currentUser } = useAuth();
  const isAdmin = currentUser?.roles.includes(ROLES.ADMIN);

  return (
    <aside className="sticky top-0 flex h-screen w-64 flex-col overflow-y-auto border-r bg-card">
      {/* Logo */}
      <div className="flex h-16 shrink-0 items-center border-b px-6">
        <L2SLogo variant="full" width={150} />
      </div>

      {/* Navigation */}
      <nav className="flex-1 space-y-1 p-4" aria-label="Navegação principal">
        {navItems.map((item) => (
          <NavLink
            key={item.to}
            to={item.to}
            className={({ isActive }) =>
              cn(
                'flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium transition-colors',
                isActive
                  ? 'bg-primary text-primary-foreground'
                  : 'text-muted-foreground hover:bg-accent hover:text-accent-foreground'
              )
            }
          >
            <item.icon className="h-5 w-5" />
            {item.label}
          </NavLink>
        ))}

        {/* Admin Section */}
        {isAdmin && (
          <>
            <Separator className="my-4" />
            <p className="px-3 text-xs font-semibold uppercase text-muted-foreground">
              Administração
            </p>
            <NavLink
              to={ROUTES.ADMIN_USERS}
              className={({ isActive }) =>
                cn(
                  'flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium transition-colors',
                  isActive
                    ? 'bg-primary text-primary-foreground'
                    : 'text-muted-foreground hover:bg-accent hover:text-accent-foreground'
                )
              }
            >
              <Users className="h-5 w-5" />
              Usuários
            </NavLink>
          </>
        )}
      </nav>
    </aside>
  );
}
