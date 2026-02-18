import { useMutation, useQueryClient } from '@tanstack/react-query';
import { categoryService } from '../services/categoryService';
import { QUERY_KEYS } from '@/shared/lib/utils/constants';
import { getErrorMessage } from '@/shared/lib/api/errors';
import { toast } from 'sonner';
import type { UpdateCategoryRequest } from '../types/category.types';
import type { ApiError } from '@/shared/types/errors.types';

/**
 * Hook para atualizar categoria
 *
 * Invalida cache de categorias e categoria individual após sucesso.
 * Exibe toast de sucesso/erro.
 */
export function useUpdateCategory() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateCategoryRequest }) =>
      categoryService.update(id, data),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.CATEGORIES] });
      queryClient.invalidateQueries({
        queryKey: [QUERY_KEYS.CATEGORY, variables.id],
      });
      toast.success('Categoria atualizada com sucesso!');
    },
    onError: (error: ApiError) => {
      toast.error(getErrorMessage(error.code));
    },
  });
}
