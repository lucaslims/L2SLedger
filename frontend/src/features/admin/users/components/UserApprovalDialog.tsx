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
      approve(
        { userId, reason },
        {
          onSuccess: () => {
            onOpenChange(false);
            setReason('');
            setAction(null);
          },
        }
      );
    } else if (action === 'reject') {
      reject(
        { userId, reason },
        {
          onSuccess: () => {
            onOpenChange(false);
            setReason('');
            setAction(null);
          },
        }
      );
    }
  };

  const handleClose = (isOpen: boolean) => {
    if (!isOpen) {
      setAction(null);
      setReason('');
    }
    onOpenChange(isOpen);
  };

  return (
    <Dialog open={open} onOpenChange={handleClose}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>
            {action === 'approve'
              ? 'Aprovar Usuário'
              : action === 'reject'
                ? 'Rejeitar Usuário'
                : 'Aprovar ou Rejeitar Usuário'}
          </DialogTitle>
          <DialogDescription>
            {action
              ? `Você está prestes a ${action === 'approve' ? 'aprovar' : 'rejeitar'} o usuário ${userName}. Informe o motivo:`
              : `Escolha uma ação para o usuário ${userName}.`}
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-4">
          {!action && (
            <div className="flex gap-2">
              <Button onClick={() => setAction('approve')} className="flex-1" variant="default">
                Aprovar
              </Button>
              <Button onClick={() => setAction('reject')} className="flex-1" variant="destructive">
                Rejeitar
              </Button>
            </div>
          )}

          {action && (
            <Textarea
              placeholder="Ex: Cadastro aprovado após verificação de documentos"
              value={reason}
              onChange={(e) => setReason(e.target.value)}
              rows={4}
            />
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
