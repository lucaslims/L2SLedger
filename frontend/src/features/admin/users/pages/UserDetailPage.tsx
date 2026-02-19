import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { AppLayout } from '@/shared/components/layout/AppLayout';
import { Button } from '@/shared/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/shared/components/ui/card';
import { Separator } from '@/shared/components/ui/separator';
import { UserStatusBadge } from '../components/UserStatusBadge';
import { UserApprovalDialog } from '../components/UserApprovalDialog';
import { UserSuspendDialog } from '../components/UserSuspendDialog';
import { UserRolesForm } from '../components/UserRolesForm';
import { DateDisplay } from '@/shared/components/data-display';
import { userService } from '../services/userService';
import { useReactivateUser } from '../hooks/useReactivateUser';
import { QUERY_KEYS, ROUTES } from '@/shared/lib/utils/constants';
import type { UserDetailDto } from '../types/user.types';
import { ArrowLeft, CheckCircle, XCircle, Ban, RotateCcw } from 'lucide-react';

export default function UserDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const [approvalDialogOpen, setApprovalDialogOpen] = useState(false);
  const [suspendDialogOpen, setSuspendDialogOpen] = useState(false);

  const {
    data: user,
    isLoading,
    error,
  } = useQuery<UserDetailDto>({
    queryKey: [QUERY_KEYS.USERS, id],
    queryFn: () => userService.getById(id!),
    enabled: !!id,
  });

  const { mutate: reactivate, isPending: isReactivating } = useReactivateUser();

  const handleReactivate = () => {
    if (!id) return;
    reactivate({ userId: id, reason: 'Reativação por administrador' });
  };

  if (isLoading) {
    return (
      <AppLayout>
        <div className="flex items-center justify-center py-8">
          <div className="text-muted-foreground">Carregando dados do usuário...</div>
        </div>
      </AppLayout>
    );
  }

  if (error || !user) {
    return (
      <AppLayout>
        <div className="flex flex-col items-center justify-center py-8 space-y-4">
          <p className="text-lg font-medium text-destructive">
            Usuário não encontrado
          </p>
          <Button variant="outline" onClick={() => navigate(ROUTES.ADMIN_USERS)}>
            <ArrowLeft className="mr-2 h-4 w-4" />
            Voltar para Lista
          </Button>
        </div>
      </AppLayout>
    );
  }

  return (
    <AppLayout>
      <div className="space-y-6">
        {/* Header */}
        <div className="flex items-center gap-4">
          <Button
            variant="ghost"
            size="sm"
            onClick={() => navigate(ROUTES.ADMIN_USERS)}
          >
            <ArrowLeft className="mr-2 h-4 w-4" />
            Voltar
          </Button>
        </div>

        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-3xl font-bold">{user.displayName}</h1>
            <p className="text-muted-foreground">{user.email}</p>
          </div>
          <UserStatusBadge status={user.status} />
        </div>

        <div className="grid gap-6 md:grid-cols-2">
          {/* Informações do Usuário */}
          <Card>
            <CardHeader>
              <CardTitle>Informações</CardTitle>
              <CardDescription>Dados cadastrais do usuário</CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <p className="text-sm font-medium text-muted-foreground">Nome</p>
                  <p className="text-sm">{user.displayName}</p>
                </div>
                <div>
                  <p className="text-sm font-medium text-muted-foreground">Email</p>
                  <p className="text-sm">{user.email}</p>
                </div>
                <div>
                  <p className="text-sm font-medium text-muted-foreground">Email Verificado</p>
                  <p className="text-sm">{user.emailVerified ? 'Sim' : 'Não'}</p>
                </div>
                <div>
                  <p className="text-sm font-medium text-muted-foreground">Status</p>
                  <UserStatusBadge status={user.status} />
                </div>
                <div>
                  <p className="text-sm font-medium text-muted-foreground">Criado em</p>
                  <p className="text-sm">
                    <DateDisplay date={user.createdAt} />
                  </p>
                </div>
                {user.updatedAt && (
                  <div>
                    <p className="text-sm font-medium text-muted-foreground">Atualizado em</p>
                    <p className="text-sm">
                      <DateDisplay date={user.updatedAt} />
                    </p>
                  </div>
                )}
              </div>
            </CardContent>
          </Card>

          {/* Ações */}
          <Card>
            <CardHeader>
              <CardTitle>Ações</CardTitle>
              <CardDescription>Gerenciar status e permissões</CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              {/* Ações por status */}
              {user.status === 'Pending' && (
                <div className="space-y-2">
                  <p className="text-sm text-muted-foreground">
                    Este usuário está aguardando aprovação.
                  </p>
                  <div className="flex gap-2">
                    <Button
                      onClick={() => setApprovalDialogOpen(true)}
                      className="flex-1"
                    >
                      <CheckCircle className="mr-2 h-4 w-4" />
                      Aprovar / Rejeitar
                    </Button>
                  </div>
                </div>
              )}

              {user.status === 'Active' && (
                <div className="space-y-2">
                  <p className="text-sm text-muted-foreground">
                    Usuário ativo. Você pode suspendê-lo se necessário.
                  </p>
                  <Button
                    variant="destructive"
                    onClick={() => setSuspendDialogOpen(true)}
                  >
                    <Ban className="mr-2 h-4 w-4" />
                    Suspender
                  </Button>
                </div>
              )}

              {user.status === 'Suspended' && (
                <div className="space-y-2">
                  <p className="text-sm text-muted-foreground">
                    Usuário suspenso. Você pode reativá-lo.
                  </p>
                  <Button
                    onClick={handleReactivate}
                    disabled={isReactivating}
                  >
                    <RotateCcw className="mr-2 h-4 w-4" />
                    {isReactivating ? 'Reativando...' : 'Reativar'}
                  </Button>
                </div>
              )}

              {user.status === 'Rejected' && (
                <div className="space-y-2">
                  <p className="text-sm text-muted-foreground">
                    Cadastro rejeitado. Nenhuma ação disponível.
                  </p>
                  <Button variant="outline" disabled>
                    <XCircle className="mr-2 h-4 w-4" />
                    Rejeitado
                  </Button>
                </div>
              )}

              <Separator />

              {/* Gestão de Roles */}
              <div className="space-y-2">
                <h3 className="text-sm font-semibold">Roles do Usuário</h3>
                {user.status === 'Active' ? (
                  <UserRolesForm
                    userId={user.id}
                    currentRoles={user.roles}
                  />
                ) : (
                  <p className="text-sm text-muted-foreground">
                    Roles só podem ser alteradas para usuários ativos.
                  </p>
                )}
              </div>
            </CardContent>
          </Card>
        </div>
      </div>

      {/* Dialogs */}
      <UserApprovalDialog
        userId={user.id}
        userName={user.displayName}
        open={approvalDialogOpen}
        onOpenChange={setApprovalDialogOpen}
      />
      <UserSuspendDialog
        userId={user.id}
        userName={user.displayName}
        open={suspendDialogOpen}
        onOpenChange={setSuspendDialogOpen}
      />
    </AppLayout>
  );
}
