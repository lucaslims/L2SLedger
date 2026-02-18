import { useMutation, useQueryClient } from '@tanstack/react-query';
import { categoryService } from '../services/categoryService';
import { QUERY_KEYS } from '@/shared/lib/utils/constants';
import { getErrorMessage } from '@/shared/lib/api/errors';
import { toast } from 'sonner';
import type { ApiError } from '@/shared/types/errors.types';

/**
 * Hook para excluir categoria
 *
 * Invalida cache de categorias após sucesso.
 * Exibe toast de sucesso/erro.
 * Trata erro FIN_CATEGORY_HAS_TRANSACTIONS especificamente.
 */
export function useDeleteCategory() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => categoryService.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.CATEGORIES] });
      toast.success('Categoria excluída com sucesso!');
    },
    onError: (error: ApiError) => {
      toast.error(getErrorMessage(error.code));
    },
  });
}
