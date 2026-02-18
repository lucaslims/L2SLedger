import { AppLayout } from '@/shared/components/layout/AppLayout';
import { CategoryForm } from '../components/CategoryForm';
import { useParams, useNavigate } from 'react-router-dom';
import { useCategory } from '../hooks/useCategory';
import { useCreateCategory } from '../hooks/useCreateCategory';
import { useUpdateCategory } from '../hooks/useUpdateCategory';
import {
    Card,
    CardContent,
    CardHeader,
    CardTitle,
} from '@/shared/components/ui/card';
import { Button } from '@/shared/components/ui/button';
import { ArrowLeft } from 'lucide-react';
import { Skeleton } from '@/shared/components/ui/skeleton';


/**
 * Página de Formulário de Categoria (Create/Edit)
 *
 * Detecta modo (criar/editar) via params.
 * Carrega dados existentes no modo edição.
 */
export default function CategoryFormPage() {
    const { id } = useParams();
    const navigate = useNavigate();
    const isEdit = !!id;

    const { data: category, isLoading } = useCategory(id);
    const { mutate: createCategory, isPending: isCreating } =
        useCreateCategory();
    const { mutate: updateCategory, isPending: isUpdating } =
        useUpdateCategory();

    const handleSubmit = (data: { name: string; type: 'Income' | 'Expense'; description?: string; parentCategoryId?: string }) => {
        if (isEdit && id) {
            updateCategory(
                { id, data },
                {
                    onSuccess: () => navigate('/categories'),
                }
            );
        } else {
            createCategory(data, {
                onSuccess: () => navigate('/categories'),
            });
        }
    };

    if (isEdit && isLoading) {
        return (
            <AppLayout>
                <div className="mx-auto max-w-2xl space-y-6">
                    <Skeleton className="h-10 w-48" />
                    <Skeleton className="h-64 w-full" />
                </div>
            </AppLayout>
        );
    }

    return (
        <AppLayout>
            <div className="mx-auto max-w-2xl space-y-6">
                <div className="flex items-center gap-4">
                    <Button
                        variant="ghost"
                        size="icon"
                        onClick={() => navigate('/categories')}
                        aria-label="Voltar para categorias"
                    >
                        <ArrowLeft className="h-5 w-5" />
                    </Button>
                    <h1 className="text-3xl font-bold">
                        {isEdit ? 'Editar Categoria' : 'Nova Categoria'}
                    </h1>
                </div>

                <Card>
                    <CardHeader>
                        <CardTitle>Informações da Categoria</CardTitle>
                    </CardHeader>
                    <CardContent>
                        <CategoryForm
                            initialValues={
                                category
                                    ? {
                                        name: category.name,
                                        type: category.type,
                                        description: category.description ?? '',
                                        parentCategoryId: category.parentCategoryId ?? undefined,
                                    }
                                    : undefined
                            }
                            onSubmit={handleSubmit}
                            isPending={isCreating || isUpdating}
                        />
                    </CardContent>
                </Card>
            </div>
        </AppLayout>
    );
}
