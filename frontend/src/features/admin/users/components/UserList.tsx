import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/shared/components/ui/table';
import { Button } from '@/shared/components/ui/button';
import { UserStatusBadge } from './UserStatusBadge';
import { DateDisplay } from '@/shared/components/data-display';
import { useNavigate } from 'react-router-dom';
import { Eye } from 'lucide-react';
import type { UserSummaryDto } from '../types/user.types';

interface UserListProps {
  users: UserSummaryDto[];
}

export function UserList({ users }: UserListProps) {
  const navigate = useNavigate();

  if (users.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center rounded-lg border border-dashed p-8 text-center">
        <p className="text-lg font-medium text-muted-foreground">Nenhum usuário encontrado</p>
        <p className="text-sm text-muted-foreground">Tente alterar os filtros de busca.</p>
      </div>
    );
  }

  return (
    <div className="rounded-md border">
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>Nome</TableHead>
            <TableHead>Email</TableHead>
            <TableHead>Status</TableHead>
            <TableHead>Roles</TableHead>
            <TableHead>Criado em</TableHead>
            <TableHead className="w-[80px]">Ações</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {users.map((user) => (
            <TableRow key={user.id}>
              <TableCell className="font-medium">{user.displayName}</TableCell>
              <TableCell>{user.email}</TableCell>
              <TableCell>
                <UserStatusBadge status={user.status} />
              </TableCell>
              <TableCell>
                <div className="flex flex-wrap gap-1">
                  {user.roles.map((role) => (
                    <span
                      key={role}
                      className="inline-flex items-center rounded-full bg-muted px-2 py-0.5 text-xs font-medium"
                    >
                      {role}
                    </span>
                  ))}
                </div>
              </TableCell>
              <TableCell>
                <DateDisplay date={user.createdAt} />
              </TableCell>
              <TableCell>
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() => navigate(`/admin/users/${user.id}`)}
                >
                  <Eye className="h-4 w-4" />
                </Button>
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  );
}
