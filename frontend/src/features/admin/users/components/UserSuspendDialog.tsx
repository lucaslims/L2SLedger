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
import { useSuspendUser } from '../hooks/useSuspendUser';

interface UserSuspendDialogProps {
  userId: string;
  userName: string;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export function UserSuspendDialog({
  userId,
  userName,
  open,
  onOpenChange,
}: UserSuspendDialogProps) {
  const [reason, setReason] = useState('');
  const { mutate: suspend, isPending } = useSuspendUser();

  const handleSubmit = () => {
    if (!reason.trim()) return;

    suspend(
      { userId, reason },
      {
        onSuccess: () => {
          onOpenChange(false);
          setReason('');
        },
      }
    );
  };

  const handleClose = (isOpen: boolean) => {
    if (!isOpen) {
      setReason('');
    }
    onOpenChange(isOpen);
  };

  return (
    <Dialog open={open} onOpenChange={handleClose}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Suspender Usuário</DialogTitle>
          <DialogDescription>
            Você está prestes a suspender o usuário <strong>{userName}</strong>.
            O usuário perderá acesso ao sistema até ser reativado.
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-4">
          <Textarea
            placeholder="Informe o motivo da suspensão"
            value={reason}
            onChange={(e) => setReason(e.target.value)}
            rows={4}
          />
        </div>

        <DialogFooter>
          <Button
            variant="outline"
            onClick={() => handleClose(false)}
            disabled={isPending}
          >
            Cancelar
          </Button>
          <Button
            variant="destructive"
            onClick={handleSubmit}
            disabled={!reason.trim() || isPending}
          >
            {isPending ? 'Suspendendo...' : 'Confirmar Suspensão'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
