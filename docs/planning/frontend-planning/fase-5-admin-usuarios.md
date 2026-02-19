# Fase 5: Admin - Gestão de Usuários — Frontend L2SLedger

> **Estimativa:** 14 horas  
> **Dependência:** Fase 1 (Autenticação) completa  
> **Status:** Implementado

---

## 🎯 Objetivo

Implementar painel administrativo para gestão de usuários com:
- Listagem de usuários com filtros por status
- Aprovação e rejeição de novos cadastros
- Suspensão e reativação de usuários
- Gestão de roles (Admin, Leitura, Escrita)
- Alerta de usuários pendentes no header
- Proteção de rota por role Admin

---

## 📋 Estrutura de Arquivos

```
features/admin/users/
├── components/
│   ├── UserList.tsx                # Tabela de usuários
│   ├── UserStatusBadge.tsx         # Badge por status
│   ├── UserApprovalDialog.tsx      # Aprovar/Rejeitar
│   ├── UserRolesForm.tsx           # Gerenciar roles
│   ├── UserSuspendDialog.tsx       # Suspender usuário
│   └── PendingUsersAlert.tsx       # Alerta no header
├── hooks/
│   ├── useUsers.ts                 # Lista com filtros
│   ├── usePendingUsers.ts          # Contagem pendentes
│   ├── useApproveUser.ts           # Mutation aprovar
│   ├── useRejectUser.ts            # Mutation rejeitar
│   ├── useSuspendUser.ts           # Mutation suspender
│   ├── useReactivateUser.ts        # Mutation reativar
│   └── useUpdateUserRoles.ts       # Mutation roles
├── pages/
│   ├── UsersPage.tsx               # Lista principal
│   └── UserDetailPage.tsx          # Detalhes + ações
├── services/
│   └── userService.ts              # API calls
└── types/
    └── user.types.ts               # DTOs
```

---

## 📋 Tasks Detalhadas

### 5.1 Types

```typescript
// features/admin/users/types/user.types.ts

import type { UserStatus, UserRole } from '@/shared/types/common.types';

export interface UserDto {
  id: string;
  email: string;
  displayName: string;
  status: UserStatus;
  emailVerified: boolean;
  roles: UserRole[];
  createdAt: string;
  updatedAt: string | null;
}

export interface UpdateUserStatusRequest {
  status: UserStatus;
  reason: string;
}

export interface UpdateUserRolesRequest {
  roles: UserRole[];
}
```

### 5.2 Service

```typescript
import { apiClient } from '@/shared/lib/api/client';
import { API_ENDPOINTS } from '@/shared/lib/api/endpoints';
import type { UserDto, UpdateUserStatusRequest, UpdateUserRolesRequest } from '../types/user.types';

export const userService = {
  async getAll(status?: string): Promise<UserDto[]> {
    return apiClient.get<UserDto[]>(API_ENDPOINTS.USERS, {
      params: { status },
    });
  },

  async getById(id: string): Promise<UserDto> {
    return apiClient.get<UserDto>(API_ENDPOINTS.USER_BY_ID(id));
  },

  async getPendingCount(): Promise<number> {
    const users = await apiClient.get<UserDto[]>(API_ENDPOINTS.USERS, {
      params: { status: 'Pending' },
    });
    return users.length;
  },

  async updateStatus(id: string, data: UpdateUserStatusRequest): Promise<UserDto> {
    return apiClient.put<UserDto>(API_ENDPOINTS.USER_STATUS(id), data);
  },

  async updateRoles(id: string, data: UpdateUserRolesRequest): Promise<UserDto> {
    return apiClient.put<UserDto>(API_ENDPOINTS.USER_BY_ID(id), data);
  },
};
```

### 5.3 Hooks

```typescript
// usePendingUsers.ts
import { useQuery } from '@tanstack/react-query';
import { QUERY_KEYS } from '@/shared/lib/utils/constants';
import { userService } from '../services/userService';

export function usePendingUsers() {
  return useQuery({
    queryKey: [QUERY_KEYS.USERS, 'pending-count'],
    queryFn: userService.getPendingCount,
    refetchInterval: 60000, // Atualizar a cada 1 minuto
  });
}

// useApproveUser.ts
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { QUERY_KEYS } from '@/shared/lib/utils/constants';
import { userService } from '../services/userService';
import { toast } from 'sonner';

export function useApproveUser() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ userId, reason }: { userId: string; reason: string }) =>
      userService.updateStatus(userId, { status: 'Active', reason }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.USERS] });
      toast.success('Usuário aprovado com sucesso!');
    },
    onError: (error: any) => {
      toast.error(error.message || 'Erro ao aprovar usuário');
    },
  });
}

// useRejectUser.ts (similar)
// useSuspendUser.ts (similar)
// useReactivateUser.ts (similar)
```

### 5.4 Componentes

#### UserStatusBadge.tsx

```typescript
import { Badge } from '@/shared/components/ui/badge';
import type { UserStatus } from '@/shared/types/common.types';
import { USER_STATUS_LABELS } from '@/shared/lib/utils/constants';

interface UserStatusBadgeProps {
  status: UserStatus;
}

export function UserStatusBadge({ status }: UserStatusBadgeProps) {
  const variants: Record<UserStatus, any> = {
    Pending: 'secondary',
    Active: 'default',
    Suspended: 'destructive',
    Rejected: 'outline',
  };

  return (
    <Badge variant={variants[status]}>
      {USER_STATUS_LABELS[status]}
    </Badge>
  );
}
```

#### PendingUsersAlert.tsx

```typescript
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
        <Button
          size="sm"
          onClick={() => navigate(`${ROUTES.ADMIN_USERS}?status=Pending`)}
        >
          Ver Pendentes
        </Button>
      </AlertDescription>
    </Alert>
  );
}
```

#### UserApprovalDialog.tsx

```typescript
import { useState } from 'react';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/shared/components/ui/dialog';
import { Button } from '@/shared/components/ui/button';
import { Textarea } from '@/shared/components/ui/textarea';
import { useApproveUser } from '../hooks/useApproveUser';
import { useRejectUser } from '../hooks/useRejectUser';

interface UserApprovalDialogProps {
  userId: string;
  userName: string;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export function UserApprovalDialog({
  userId,
  userName,
  open,
  onOpenChange,
}: UserApprovalDialogProps) {
  const [action, setAction] = useState<'approve' | 'reject' | null>(null);
  const [reason, setReason] = useState('');

  const { mutate: approve, isPending: isApproving } = useApproveUser();
  const { mutate: reject, isPending: isRejecting } = useRejectUser();

  const handleSubmit = () => {
    if (!reason.trim()) return;

    if (action === 'approve') {
      approve({ userId, reason }, {
        onSuccess: () => {
          onOpenChange(false);
          setReason('');
          setAction(null);
        },
      });
    } else if (action === 'reject') {
      reject({ userId, reason }, {
        onSuccess: () => {
          onOpenChange(false);
          setReason('');
          setAction(null);
        },
      });
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>
            {action === 'approve' ? 'Aprovar' : 'Rejeitar'} Usuário
          </DialogTitle>
          <DialogDescription>
            Você está prestes a {action === 'approve' ? 'aprovar' : 'rejeitar'} o
            usuário <strong>{userName}</strong>. Informe o motivo:
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-4">
          <Textarea
            placeholder="Ex: Cadastro aprovado após verificação de documentos"
            value={reason}
            onChange={(e) => setReason(e.target.value)}
            rows={4}
          />

          {!action && (
            <div className="flex gap-2">
              <Button
                onClick={() => setAction('approve')}
                className="flex-1"
                variant="default"
              >
                Aprovar
              </Button>
              <Button
                onClick={() => setAction('reject')}
                className="flex-1"
                variant="destructive"
              >
                Rejeitar
              </Button>
            </div>
          )}
        </div>

        <DialogFooter>
          {action && (
            <>
              <Button
                variant="outline"
                onClick={() => {
                  setAction(null);
                  setReason('');
                }}
                disabled={isApproving || isRejecting}
              >
                Voltar
              </Button>
              <Button
                onClick={handleSubmit}
                disabled={!reason.trim() || isApproving || isRejecting}
                variant={action === 'approve' ? 'default' : 'destructive'}
              >
                {isApproving || isRejecting
                  ? 'Processando...'
                  : action === 'approve'
                  ? 'Confirmar Aprovação'
                  : 'Confirmar Rejeição'}
              </Button>
            </>
          )}
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
```

#### UserRolesForm.tsx

```typescript
import { Checkbox } from '@/shared/components/ui/checkbox';
import { Button } from '@/shared/components/ui/button';
import { useUpdateUserRoles } from '../hooks/useUpdateUserRoles';
import { ROLES } from '@/shared/lib/utils/constants';
import { useState } from 'react';
import type { UserRole } from '@/shared/types/common.types';

interface UserRolesFormProps {
  userId: string;
  currentRoles: UserRole[];
  onSuccess?: () => void;
}

export function UserRolesForm({ userId, currentRoles, onSuccess }: UserRolesFormProps) {
  const [selectedRoles, setSelectedRoles] = useState<UserRole[]>(currentRoles);
  const { mutate: updateRoles, isPending } = useUpdateUserRoles();

  const handleToggleRole = (role: UserRole) => {
    setSelectedRoles((prev) =>
      prev.includes(role)
        ? prev.filter((r) => r !== role)
        : [...prev, role]
    );
  };

  const handleSubmit = () => {
    updateRoles(
      { userId, roles: selectedRoles },
      { onSuccess }
    );
  };

  return (
    <div className="space-y-4">
      <div className="space-y-2">
        {Object.values(ROLES).map((role) => (
          <div key={role} className="flex items-center space-x-2">
            <Checkbox
              id={role}
              checked={selectedRoles.includes(role)}
              onCheckedChange={() => handleToggleRole(role)}
            />
            <label htmlFor={role} className="text-sm font-medium">
              {role}
            </label>
          </div>
        ))}
      </div>

      <Button onClick={handleSubmit} disabled={isPending}>
        {isPending ? 'Salvando...' : 'Salvar Roles'}
      </Button>
    </div>
  );
}
```

### 5.5 Pages

#### UsersPage.tsx

```typescript
import { AppLayout } from '@/shared/components/layout/AppLayout';
import { PendingUsersAlert } from '../components/PendingUsersAlert';
import { UserList } from '../components/UserList';
import { useUsers } from '../hooks/useUsers';
import { useState } from 'react';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/shared/components/ui/select';

export default function UsersPage() {
  const [statusFilter, setStatusFilter] = useState<string | undefined>();
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
          <Select value={statusFilter || 'all'} onValueChange={(v) => setStatusFilter(v === 'all' ? undefined : v)}>
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
          <div>Carregando...</div>
        ) : (
          <UserList users={users || []} />
        )}
      </div>
    </AppLayout>
  );
}
```

---

## ✅ Critérios de Aceite

- [ ] Lista de usuários com filtros por status
- [ ] Aprovação/rejeição funcional
- [ ] Suspensão/reativação funcional
- [ ] Gestão de roles funcional
- [ ] Alerta de pendentes visível para admins
- [ ] Apenas admin acessa área admin (AdminRoute)
- [ ] Proteção contra remoção do próprio Admin
- [ ] Proteção contra remoção do último Admin
- [ ] Cobertura ≥ 85%
- [ ] Testes E2E passando

---

---

## ⚠️ Considerações ADRs e Governança

### ADR-016 — Controle de Acesso (RBAC)
- ✅ Roles: Admin, Leitura, Escrita
- ✅ Proteção de rota AdminRoute
- ✅ Verificação de role no frontend

### ADR-021-A — Códigos de Erro USER_
- ✅ USER_CANNOT_REMOVE_OWN_ADMIN tratado
- ✅ USER_LAST_ADMIN tratado
- ✅ USER_INVALID_STATUS_TRANSITION tratado

### Regras de Segurança
- Admin não pode alterar próprio status
- Sistema deve ter pelo menos 1 Admin ativo
- Motivo obrigatório para mudança de status

---

**Status:** ✅ Todas as fases documentadas  
**Próximo passo:** Iniciar implementação da Fase 1 (Autenticação)
