import { Badge } from '@/shared/components/ui/badge';
import { Card, CardContent } from '@/shared/components/ui/card';
import { Button } from '@/shared/components/ui/button';
import { Edit, Trash } from 'lucide-react';
import type { CategoryDto } from '../types/category.types';

interface CategoryCardProps {
  category: CategoryDto;
  onEdit: (id: string) => void;
  onDelete: (category: CategoryDto) => void;
}

/**
 * Card individual de Categoria
 *
 * Exibição compacta para uso mobile ou grid.
 */
export function CategoryCard({ category, onEdit, onDelete }: CategoryCardProps) {
  return (
    <Card>
      <CardContent className="flex items-center justify-between p-4">
        <div className="flex items-center gap-3">
          <div>
            <p className="font-medium">{category.name}</p>
            <Badge
              variant={category.type === 'Income' ? 'default' : 'destructive'}
              className="mt-1"
            >
              {category.type === 'Income' ? 'Receita' : 'Despesa'}
            </Badge>
          </div>
        </div>
        <div className="flex gap-1">
          <Button
            variant="ghost"
            size="icon"
            onClick={() => onEdit(category.id)}
            aria-label={`Editar ${category.name}`}
          >
            <Edit className="h-4 w-4" />
          </Button>
          <Button
            variant="ghost"
            size="icon"
            onClick={() => onDelete(category)}
            aria-label={`Excluir ${category.name}`}
          >
            <Trash className="h-4 w-4" />
          </Button>
        </div>
      </CardContent>
    </Card>
  );
}
