import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@/shared/components/ui/alert-dialog';
import { useDeleteCategory } from '../hooks/useDeleteCategory';

interface CategoryDeleteDialogProps {
  categoryId: string;
  categoryName: string;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

/**
 * Diálogo de Confirmação de Exclusão de Categoria
 *
 * Exibe confirmação antes de excluir.
 * Trata erro FIN_CATEGORY_HAS_TRANSACTIONS via hook.
 */
export function CategoryDeleteDialog({
  categoryId,
  categoryName,
  open,
  onOpenChange,
}: CategoryDeleteDialogProps) {
  const { mutate: deleteCategory, isPending } = useDeleteCategory();

  const handleDelete = () => {
    deleteCategory(categoryId, {
      onSuccess: () => {
        onOpenChange(false);
      },
    });
  };

  return (
    <AlertDialog open={open} onOpenChange={onOpenChange}>
      <AlertDialogContent>
        <AlertDialogHeader>
          <AlertDialogTitle>Confirmar Exclusão</AlertDialogTitle>
          <AlertDialogDescription>
            Tem certeza que deseja excluir a categoria <strong>{categoryName}</strong>? Esta ação
            não pode ser desfeita.
          </AlertDialogDescription>
        </AlertDialogHeader>
        <AlertDialogFooter>
          <AlertDialogCancel disabled={isPending}>Cancelar</AlertDialogCancel>
          <AlertDialogAction onClick={handleDelete} disabled={isPending}>
            {isPending ? 'Excluindo...' : 'Excluir'}
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  );
}
