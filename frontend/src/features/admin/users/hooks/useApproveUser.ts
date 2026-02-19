import { useMutation, useQueryClient } from '@tanstack/react-query';
import { QUERY_KEYS } from '@/shared/lib/utils/constants';
import { userService } from '../services/userService';
import { getErrorMessage } from '@/shared/lib/api/errors';
import { toast } from 'sonner';
import type { ApiError } from '@/shared/types/errors.types';

export function useApproveUser() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ userId, reason }: { userId: string; reason: string }) =>
      userService.updateStatus(userId, { status: 'Active', reason }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.USERS] });
      toast.success('Usuário aprovado com sucesso!');
    },
    onError: (error: ApiError) => {
      toast.error(getErrorMessage(error.code));
    },
  });
}
