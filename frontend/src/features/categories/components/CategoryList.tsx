import { useState } from 'react';
import { useCategories } from '../hooks/useCategories';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/shared/components/ui/table';
import { Button } from '@/shared/components/ui/button';
import { Badge } from '@/shared/components/ui/badge';
import { Edit, Trash } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import { CategoryDeleteDialog } from './CategoryDeleteDialog';
import { Skeleton } from '@/shared/components/ui/skeleton';
import type { CategoryDto } from '../types/category.types';

/**
 * Lista de Categorias
 *
 * Exibe tabela com nome, tipo e ações (editar/excluir).
 * Integra diálogo de confirmação de exclusão.
 */
export function CategoryList() {
  const { data: categories, isLoading } = useCategories();
  const navigate = useNavigate();
  const [deleteTarget, setDeleteTarget] = useState<CategoryDto | null>(null);

  if (isLoading) {
    return (
      <div className="space-y-3">
        {Array.from({ length: 5 }).map((_, i) => (
          <Skeleton key={i} className="h-12 w-full" />
        ))}
      </div>
    );
  }

  if (!categories?.length) {
    return (
      <div className="flex flex-col items-center justify-center py-12 text-center">
        <p className="text-lg font-medium text-muted-foreground">Nenhuma categoria cadastrada</p>
        <p className="text-sm text-muted-foreground">
          Crie sua primeira categoria para começar a organizar seus lançamentos.
        </p>
      </div>
    );
  }

  return (
    <>
      {/* Desktop table */}
      <div className="hidden md:block">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Nome</TableHead>
              <TableHead>Descrição</TableHead>
              <TableHead>Tipo</TableHead>
              <TableHead>Categoria Pai</TableHead>
              <TableHead className="text-right">Ações</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {categories.map((category) => (
              <TableRow key={category.id}>
                <TableCell className="font-medium">{category.name}</TableCell>
                <TableCell>{category.description}</TableCell>
                <TableCell>
                  <Badge variant={category.type === 'Income' ? 'default' : 'destructive'}>
                    {category.type === 'Income' ? 'Receita' : 'Despesa'}
                  </Badge>
                </TableCell>
                <TableCell>{category.parentCategoryName ?? '—'}</TableCell>
                <TableCell className="text-right">
                  <Button
                    variant="ghost"
                    size="icon"
                    onClick={() => navigate(`/categories/${category.id}/edit`)}
                    aria-label={`Editar ${category.name}`}
                  >
                    <Edit className="h-4 w-4" />
                  </Button>
                  <Button
                    variant="ghost"
                    size="icon"
                    onClick={() => setDeleteTarget(category)}
                    aria-label={`Excluir ${category.name}`}
                  >
                    <Trash className="h-4 w-4" />
                  </Button>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </div>

      {/* Mobile cards */}
      <div className="space-y-3 md:hidden">
        {categories.map((category) => (
          <CategoryCard
            key={category.id}
            category={category}
            onEdit={() => navigate(`/categories/${category.id}/edit`)}
            onDelete={() => setDeleteTarget(category)}
          />
        ))}
      </div>

      {deleteTarget && (
        <CategoryDeleteDialog
          categoryId={deleteTarget.id}
          categoryName={deleteTarget.name}
          open={!!deleteTarget}
          onOpenChange={(open) => {
            if (!open) setDeleteTarget(null);
          }}
        />
      )}
    </>
  );
}

// Inline mobile card component
function CategoryCard({
  category,
  onEdit,
  onDelete,
}: {
  category: CategoryDto;
  onEdit: () => void;
  onDelete: () => void;
}) {
  return (
    <div className="flex items-start justify-between rounded-lg border p-4">
      <div className="min-w-0 flex-1">
        <div className="flex items-center gap-2">
          <p className="truncate font-medium">{category.name}</p>
          <Badge
            variant={category.type === 'Income' ? 'default' : 'destructive'}
            className="shrink-0"
          >
            {category.type === 'Income' ? 'Receita' : 'Despesa'}
          </Badge>
        </div>
        {category.description && (
          <p className="mt-1 text-sm text-muted-foreground">{category.description}</p>
        )}
        {category.parentCategoryName && (
          <p className="mt-1 text-xs text-muted-foreground">Pai: {category.parentCategoryName}</p>
        )}
      </div>
      <div className="ml-4 flex shrink-0 items-center gap-1">
        <Button variant="ghost" size="icon" onClick={onEdit} aria-label={`Editar ${category.name}`}>
          <Edit className="h-4 w-4" />
        </Button>
        <Button
          variant="ghost"
          size="icon"
          onClick={onDelete}
          aria-label={`Excluir ${category.name}`}
        >
          <Trash className="h-4 w-4" />
        </Button>
      </div>
    </div>
  );
}
