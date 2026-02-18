import { AppLayout } from '@/shared/components/layout/AppLayout';
import { CategoryList } from '../components/CategoryList';
import { Button } from '@/shared/components/ui/button';
import { Plus } from 'lucide-react';
import { useNavigate } from 'react-router-dom';

/**
 * Página de Listagem de Categorias
 *
 * Exibe lista de categorias com ações de CRUD.
 * Usa AppLayout para manter navegação consistente.
 */
export default function CategoriesPage() {
  const navigate = useNavigate();

  return (
    <AppLayout>
      <div className="space-y-6">
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-3xl font-bold">Categorias</h1>
            <p className="text-muted-foreground">
              Gerencie as categorias de receitas e despesas
            </p>
          </div>
          <Button onClick={() => navigate('/categories/new')}>
            <Plus className="mr-2 h-4 w-4" />
            Nova Categoria
          </Button>
        </div>

        <CategoryList />
      </div>
    </AppLayout>
  );
}
