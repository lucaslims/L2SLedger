import { AppLayout } from '@/shared/components/layout/AppLayout';
import { PendingUsersAlert } from '../components/PendingUsersAlert';
import { UserList } from '../components/UserList';
import { useUsers } from '../hooks/useUsers';
import { useState } from 'react';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/shared/components/ui/select';
import { useSearchParams } from 'react-router-dom';

export default function UsersPage() {
  const [searchParams] = useSearchParams();
  const initialStatus = searchParams.get('status') || undefined;
  const [statusFilter, setStatusFilter] = useState<string | undefined>(initialStatus);
  const { data: users, isLoading } = useUsers(statusFilter);

  return (
    <AppLayout>
      <div className="space-y-6">
        <div>
          <h1 className="text-3xl font-bold">Gestão de Usuários</h1>
          <p className="text-muted-foreground">
            Aprovar, suspender e gerenciar usuários do sistema
          </p>
        </div>

        <PendingUsersAlert />

        <div className="flex items-center gap-4">
          <Select
            value={statusFilter || 'all'}
            onValueChange={(v) => setStatusFilter(v === 'all' ? undefined : v)}
          >
            <SelectTrigger className="w-[200px]">
              <SelectValue placeholder="Filtrar por status" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">Todos</SelectItem>
              <SelectItem value="Pending">Pendentes</SelectItem>
              <SelectItem value="Active">Ativos</SelectItem>
              <SelectItem value="Suspended">Suspensos</SelectItem>
              <SelectItem value="Rejected">Rejeitados</SelectItem>
            </SelectContent>
          </Select>
        </div>

        {isLoading ? (
          <div className="flex items-center justify-center py-8">
            <div className="text-muted-foreground">Carregando usuários...</div>
          </div>
        ) : (
          <UserList users={users || []} />
        )}
      </div>
    </AppLayout>
  );
}
