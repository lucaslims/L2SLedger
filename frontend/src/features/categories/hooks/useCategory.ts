import { useQuery } from '@tanstack/react-query';
import { categoryService } from '../services/categoryService';
import { QUERY_KEYS } from '@/shared/lib/utils/constants';

/**
 * Hook para buscar uma categoria individual por ID
 *
 * @param id - ID da categoria. Query desabilitada se undefined.
 */
export function useCategory(id?: string) {
  return useQuery({
    queryKey: [QUERY_KEYS.CATEGORY, id],
    queryFn: () => categoryService.getById(id!),
    enabled: !!id,
  });
}
