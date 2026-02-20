import { Alert, AlertDescription } from '@/shared/components/ui/alert';
import { Button } from '@/shared/components/ui/button';
import { Bell } from 'lucide-react';
import { usePendingUsers } from '../hooks/usePendingUsers';
import { useNavigate } from 'react-router-dom';
import { ROUTES } from '@/shared/lib/utils/constants';

export function PendingUsersAlert() {
  const { data: pendingCount } = usePendingUsers();
  const navigate = useNavigate();

  if (!pendingCount || pendingCount === 0) return null;

  return (
    <Alert className="mb-4">
      <Bell className="h-4 w-4" />
      <AlertDescription className="flex items-center justify-between">
        <span>
          Você tem <strong>{pendingCount}</strong> usuário(s) aguardando aprovação
        </span>
        <Button size="sm" onClick={() => navigate(`${ROUTES.ADMIN_USERS}?status=Pending`)}>
          Ver Pendentes
        </Button>
      </AlertDescription>
    </Alert>
  );
}
