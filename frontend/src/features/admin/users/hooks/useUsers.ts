import { useQuery } from '@tanstack/react-query';
import { userService } from '../services/userService';
import { QUERY_KEYS } from '@/shared/lib/utils/constants';
import type { UserSummaryDto } from '../types/user.types';

export function useUsers(status?: string) {
  return useQuery<UserSummaryDto[]>({
    queryKey: [QUERY_KEYS.USERS, status],
    queryFn: () => userService.getAll(status),
  });
}
