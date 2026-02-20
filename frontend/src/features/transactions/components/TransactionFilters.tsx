import { Button } from '@/shared/components/ui/button';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/shared/components/ui/select';
import { useCategories } from '@/features/categories/hooks/useCategories';
import { X } from 'lucide-react';
import type {
  TransactionFilters as FiltersType,
  TransactionTypeValue,
} from '../types/transaction.types';

interface TransactionFiltersProps {
  filters: FiltersType;
  onFilterChange: (filters: FiltersType) => void;
}

export function TransactionFilters({ filters, onFilterChange }: TransactionFiltersProps) {
  const { data: categories } = useCategories();

  const hasActiveFilters = filters.type !== undefined || filters.categoryId !== undefined;

  return (
    <div className="flex flex-wrap gap-4">
      <Select
        value={filters.type !== undefined ? String(filters.type) : 'all'}
        onValueChange={(value) =>
          onFilterChange({
            ...filters,
            type: value === 'all' ? undefined : (Number(value) as TransactionTypeValue),
            page: 1, // Reset to first page on filter change
          })
        }
      >
        <SelectTrigger className="w-[180px]">
          <SelectValue placeholder="Tipo" />
        </SelectTrigger>
        <SelectContent>
          <SelectItem value="all">Todos os tipos</SelectItem>
          <SelectItem value="1">Receitas</SelectItem>
          <SelectItem value="2">Despesas</SelectItem>
        </SelectContent>
      </Select>

      <Select
        value={filters.categoryId || 'all'}
        onValueChange={(value) =>
          onFilterChange({
            ...filters,
            categoryId: value === 'all' ? undefined : value,
            page: 1,
          })
        }
      >
        <SelectTrigger className="w-[200px]">
          <SelectValue placeholder="Categoria" />
        </SelectTrigger>
        <SelectContent>
          <SelectItem value="all">Todas as categorias</SelectItem>
          {categories?.map((category) => (
            <SelectItem key={category.id} value={category.id}>
              {category.name}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>

      {hasActiveFilters && (
        <Button
          variant="outline"
          size="icon"
          onClick={() => onFilterChange({ page: 1, pageSize: filters.pageSize })}
          title="Limpar filtros"
        >
          <X className="h-4 w-4" />
        </Button>
      )}
    </div>
  );
}
