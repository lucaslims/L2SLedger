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
