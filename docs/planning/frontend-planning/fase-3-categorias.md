# Fase 3: Categorias — Frontend L2SLedger

> **Estimativa:** 10 horas  
> **Dependência:** Fase 2 (Dashboard) completa  
> **Status:** Pendente

---

## 🎯 Objetivo

Implementar CRUD completo de categorias financeiras com:
- Listagem de categorias
- Criação e edição de categorias
- Exclusão com confirmação
- Validação de formulários
- Tratamento de erros semânticos

---

## 📋 Estrutura de Arquivos

```
features/categories/
├── components/
│   ├── CategoryList.tsx            # Lista/Tabela de categorias
│   ├── CategoryForm.tsx            # Formulário (create/edit)
│   ├── CategoryCard.tsx            # Card individual
│   └── CategoryDeleteDialog.tsx    # Confirmação de exclusão
├── hooks/
│   ├── useCategories.ts            # Lista com cache (SPEC.md 4.0)
│   ├── useCategory.ts              # Detalhe individual
│   ├── useCreateCategory.ts        # Mutation create (SPEC.md 4.0)
│   ├── useUpdateCategory.ts        # Mutation update (SPEC.md 4.0)
│   └── useDeleteCategory.ts        # Mutation delete (SPEC.md 4.0)
├── pages/
│   ├── CategoriesPage.tsx          # Lista principal
│   └── CategoryFormPage.tsx        # Criar/Editar
├── services/
│   └── categoryService.ts          # API calls
├── types/
│   └── category.types.ts           # DTOs
└── __tests__/
    ├── useCategories.test.ts
    ├── CategoryForm.test.tsx
    └── CategoryList.test.tsx
```

---

## 📋 Tasks Detalhadas

### 3.1 Types

```typescript
// features/categories/types/category.types.ts

export interface CategoryDto {
  id: string;
  name: string;
  type: 'Income' | 'Expense';
  createdAt: string;
  updatedAt: string | null;
}

export interface CreateCategoryRequest {
  name: string;
  type: 'Income' | 'Expense';
}

export interface UpdateCategoryRequest {
  name: string;
  type: 'Income' | 'Expense';
}
```

### 3.2 Service

```typescript
// features/categories/services/categoryService.ts

import { apiClient } from '@/shared/lib/api/client';
import { API_ENDPOINTS } from '@/shared/lib/api/endpoints';
import type { CategoryDto, CreateCategoryRequest, UpdateCategoryRequest } from '../types/category.types';

export const categoryService = {
  async getAll(): Promise<CategoryDto[]> {
    return apiClient.get<CategoryDto[]>(API_ENDPOINTS.CATEGORIES);
  },

  async getById(id: string): Promise<CategoryDto> {
    return apiClient.get<CategoryDto>(API_ENDPOINTS.CATEGORY_BY_ID(id));
  },

  async create(data: CreateCategoryRequest): Promise<CategoryDto> {
    return apiClient.post<CategoryDto>(API_ENDPOINTS.CATEGORIES, data);
  },

  async update(id: string, data: UpdateCategoryRequest): Promise<CategoryDto> {
    return apiClient.put<CategoryDto>(API_ENDPOINTS.CATEGORY_BY_ID(id), data);
  },

  async delete(id: string): Promise<void> {
    return apiClient.delete<void>(API_ENDPOINTS.CATEGORY_BY_ID(id));
  },
};
```

### 3.3 Hooks

```typescript
// useCategories.ts
import { useQuery } from '@tanstack/react-query';
import { QUERY_KEYS } from '@/shared/lib/utils/constants';
import { categoryService } from '../services/categoryService';

export function useCategories(type?: 'Income' | 'Expense') {
  return useQuery({
    queryKey: [QUERY_KEYS.CATEGORIES, type],
    queryFn: async () => {
      const categories = await categoryService.getAll();
      return type ? categories.filter(c => c.type === type) : categories;
    },
  });
}

// useCreateCategory.ts
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { QUERY_KEYS } from '@/shared/lib/utils/constants';
import { categoryService } from '../services/categoryService';
import { toast } from 'sonner';

export function useCreateCategory() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: categoryService.create,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.CATEGORIES] });
      toast.success('Categoria criada com sucesso!');
    },
    onError: (error: any) => {
      toast.error(error.message || 'Erro ao criar categoria');
    },
  });
}

// useDeleteCategory.ts (similar pattern)
```

### 3.4 Componentes

#### CategoryForm.tsx

```typescript
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Button } from '@/shared/components/ui/button';
import { Input } from '@/shared/components/ui/input';
import { Form, FormField, FormItem, FormLabel, FormControl, FormMessage } from '@/shared/components/ui/form';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/shared/components/ui/select';

const categorySchema = z.object({
  name: z.string().min(1, 'Nome é obrigatório').max(100, 'Nome muito longo'),
  type: z.enum(['Income', 'Expense'], { required_error: 'Selecione o tipo' }),
});

type CategoryFormData = z.infer<typeof categorySchema>;

interface CategoryFormProps {
  initialValues?: CategoryFormData;
  onSubmit: (data: CategoryFormData) => void;
  isPending?: boolean;
}

export function CategoryForm({ initialValues, onSubmit, isPending }: CategoryFormProps) {
  const form = useForm<CategoryFormData>({
    resolver: zodResolver(categorySchema),
    defaultValues: initialValues || {
      name: '',
      type: 'Expense',
    },
  });

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
        <FormField
          control={form.control}
          name="name"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Nome da Categoria</FormLabel>
              <FormControl>
                <Input placeholder="Ex: Alimentação" {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <FormField
          control={form.control}
          name="type"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Tipo</FormLabel>
              <Select onValueChange={field.onChange} defaultValue={field.value}>
                <FormControl>
                  <SelectTrigger>
                    <SelectValue placeholder="Selecione o tipo" />
                  </SelectTrigger>
                </FormControl>
                <SelectContent>
                  <SelectItem value="Income">Receita</SelectItem>
                  <SelectItem value="Expense">Despesa</SelectItem>
                </SelectContent>
              </Select>
              <FormMessage />
            </FormItem>
          )}
        />

        <Button type="submit" disabled={isPending} className="w-full">
          {isPending ? 'Salvando...' : initialValues ? 'Atualizar' : 'Criar'}
        </Button>
      </form>
    </Form>
  );
}
```

#### CategoryList.tsx

```typescript
import { useCategories } from '../hooks/useCategories';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/shared/components/ui/table';
import { Button } from '@/shared/components/ui/button';
import { Edit, Trash } from 'lucide-react';
import { Badge } from '@/shared/components/ui/badge';
import { useNavigate } from 'react-router-dom';

export function CategoryList() {
  const { data: categories, isLoading } = useCategories();
  const navigate = useNavigate();

  if (isLoading) return <div>Carregando...</div>;

  return (
    <Table>
      <TableHeader>
        <TableRow>
          <TableHead>Nome</TableHead>
          <TableHead>Tipo</TableHead>
          <TableHead className="text-right">Ações</TableHead>
        </TableRow>
      </TableHeader>
      <TableBody>
        {categories?.map((category) => (
          <TableRow key={category.id}>
            <TableCell>{category.name}</TableCell>
            <TableCell>
              <Badge variant={category.type === 'Income' ? 'default' : 'destructive'}>
                {category.type === 'Income' ? 'Receita' : 'Despesa'}
              </Badge>
            </TableCell>
            <TableCell className="text-right">
              <Button
                variant="ghost"
                size="icon"
                onClick={() => navigate(`/categories/${category.id}/edit`)}
              >
                <Edit className="h-4 w-4" />
              </Button>
              <Button variant="ghost" size="icon">
                <Trash className="h-4 w-4" />
              </Button>
            </TableCell>
          </TableRow>
        ))}
      </TableBody>
    </Table>
  );
}
```

#### CategoryDeleteDialog.tsx

```typescript
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
            Tem certeza que deseja excluir a categoria <strong>{categoryName}</strong>?
            Esta ação não pode ser desfeita.
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
```

### 3.5 Pages

#### CategoriesPage.tsx

```typescript
import { AppLayout } from '@/shared/components/layout/AppLayout';
import { CategoryList } from '../components/CategoryList';
import { Button } from '@/shared/components/ui/button';
import { Plus } from 'lucide-react';
import { useNavigate } from 'react-router-dom';

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
```

#### CategoryFormPage.tsx

```typescript
import { AppLayout } from '@/shared/components/layout/AppLayout';
import { CategoryForm } from '../components/CategoryForm';
import { useParams, useNavigate } from 'react-router-dom';
import { useCategory } from '../hooks/useCategory';
import { useCreateCategory } from '../hooks/useCreateCategory';
import { useUpdateCategory } from '../hooks/useUpdateCategory';
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/components/ui/card';

export default function CategoryFormPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const isEdit = !!id;

  const { data: category } = useCategory(id);
  const { mutate: createCategory, isPending: isCreating } = useCreateCategory();
  const { mutate: updateCategory, isPending: isUpdating } = useUpdateCategory();

  const handleSubmit = (data: any) => {
    if (isEdit && id) {
      updateCategory({ id, data }, {
        onSuccess: () => navigate('/categories'),
      });
    } else {
      createCategory(data, {
        onSuccess: () => navigate('/categories'),
      });
    }
  };

  return (
    <AppLayout>
      <div className="mx-auto max-w-2xl space-y-6">
        <div>
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
              initialValues={category}
              onSubmit={handleSubmit}
              isPending={isCreating || isUpdating}
            />
          </CardContent>
        </Card>
      </div>
    </AppLayout>
  );
}
```

---

## 🧪 Testes

### Teste Unitário - useCategories

```typescript
import { describe, it, expect } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import { useCategories } from './useCategories';

describe('useCategories', () => {
  it('should fetch categories', async () => {
    const { result } = renderHook(() => useCategories());
    
    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.data).toBeDefined();
  });

  it('should filter by type', async () => {
    const { result } = renderHook(() => useCategories('Income'));
    
    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.data?.every(c => c.type === 'Income')).toBe(true);
  });
});
```

### Teste E2E

```typescript
import { test, expect } from '@playwright/test';

test.describe('Categories CRUD', () => {
  test('should create a new category', async ({ page }) => {
    await page.goto('/categories');
    await page.click('text=Nova Categoria');
    
    await page.fill('input[name="name"]', 'Test Category');
    await page.selectOption('select[name="type"]', 'Expense');
    await page.click('button[type="submit"]');
    
    await expect(page.locator('text=Test Category')).toBeVisible();
  });

  test('should delete a category', async ({ page }) => {
    await page.goto('/categories');
    await page.click('button[aria-label="Delete"]').first();
    await page.click('text=Excluir');
    
    await expect(page.locator('text=excluída com sucesso')).toBeVisible();
  });
});
```

---

## ✅ Critérios de Aceite (conforme approval-checklist.md)

### Funcionalidade
- [ ] CRUD completo funcionando
- [ ] Validações de formulário (Zod)
- [ ] Confirmação de exclusão
- [ ] Feedback de sucesso/erro (toast)
- [ ] Navegação funcional
- [ ] Filtro por tipo (receita/despesa)

### Testes (OBRIGATÓRIO)
- [ ] Testes unitários criados (≥85% cobertura)
- [ ] Testes de componentes criados
- [ ] Testes E2E passando
- [ ] Pipeline CI passou sem falhas

### ADRs Respeitados
- [ ] ADR-021-A: Erros FIN_CATEGORY_* tratados
- [ ] ADR-040: Testes de comportamento implementados

---

**Próximo passo:** Fase 4 — Transações
