import { useQuery } from '@tanstack/react-query';
import { categoryService } from '../services/categoryService';
import { QUERY_KEYS } from '@/shared/lib/utils/constants';

/**
 * Hook para listar categorias
 *
 * Usa React Query para:
 * - Cache automático (5min stale)
 * - Refetch em reconexão
 * - Loading e error states
 *
 * @param type - Filtrar por tipo (Income/Expense). Omitir para todas.
 */
export function useCategories(type?: 'Income' | 'Expense') {
  return useQuery({
    queryKey: [QUERY_KEYS.CATEGORIES, type],
    queryFn: async () => {
      const categories = await categoryService.getAll();
      return type ? categories.filter((c) => c.type === type) : categories;
    },
  });
}
