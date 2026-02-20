import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Button } from '@/shared/components/ui/button';
import { Input } from '@/shared/components/ui/input';
import {
  Form,
  FormField,
  FormItem,
  FormLabel,
  FormControl,
  FormMessage,
} from '@/shared/components/ui/form';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/shared/components/ui/select';

const categorySchema = z.object({
  name: z.string().min(1, 'Nome é obrigatório').max(100, 'Nome deve ter no máximo 100 caracteres'),
  description: z.string().optional(),
  parentCategoryId: z.string().optional(),
  type: z.enum(['Income', 'Expense'], {
    required_error: 'Selecione o tipo',
  }),
});

type CategoryFormData = z.infer<typeof categorySchema>;

interface CategoryFormProps {
  initialValues?: CategoryFormData;
  onSubmit: (data: CategoryFormData) => void;
  isPending?: boolean;
}

/**
 * Formulário de Categoria (Create/Edit)
 *
 * Validação via Zod + React Hook Form.
 * Suporta modo criação (sem initialValues) e edição (com initialValues).
 */
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
          name="description"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Descrição (opcional)</FormLabel>
              <FormControl>
                <Input placeholder="Ex: Despesas com alimentação" {...field} />
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

        <FormField
          control={form.control}
          name="parentCategoryId"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Categoria Pai (opcional)</FormLabel>
              <FormControl>
                <Input placeholder="ID da categoria pai" {...field} value={field.value ?? ''} />
              </FormControl>
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
