import { useMutation, useQueryClient } from '@tanstack/react-query';
import { QUERY_KEYS } from '@/shared/lib/utils/constants';
import { userService } from '../services/userService';
import { getErrorMessage } from '@/shared/lib/api/errors';
import { toast } from 'sonner';
import type { ApiError } from '@/shared/types/errors.types';
import type { UserRole } from '@/shared/types/common.types';

export function useUpdateUserRoles() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ userId, roles }: { userId: string; roles: UserRole[] }) =>
      userService.updateRoles(userId, { roles }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.USERS] });
      toast.success('Roles atualizadas com sucesso!');
    },
    onError: (error: ApiError) => {
      toast.error(getErrorMessage(error.code));
    },
  });
}
