import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Check, ChevronsUpDown } from 'lucide-react';
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
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from '@/shared/components/ui/popover';
import { cn } from '@/shared/lib/utils/cn';

const categorySchema = z.object({
  name: z.string().min(1, 'Nome é obrigatório').max(100, 'Nome deve ter no máximo 100 caracteres'),
  description: z.string().optional(),
  parentCategoryId: z.string().optional(),
  type: z.enum(['Income', 'Expense'], {
    required_error: 'Selecione o tipo',
  }),
});

type CategoryFormData = z.infer<typeof categorySchema>;

interface ParentCategoryOption {
  id: string;
  name: string;
}

interface CategoryFormProps {
  initialValues?: CategoryFormData;
  onSubmit: (data: CategoryFormData) => void;
  isPending?: boolean;
  /** Lista de categorias disponíveis para seleção como pai */
  parentCategories?: ParentCategoryOption[];
}

/**
 * Formulário de Categoria (Create/Edit)
 *
 * Validação via Zod + React Hook Form.
 * Suporta modo criação (sem initialValues) e edição (com initialValues).
 */
export function CategoryForm({ initialValues, onSubmit, isPending, parentCategories = [] }: CategoryFormProps) {
  const [parentOpen, setParentOpen] = useState(false);
  const [parentSearch, setParentSearch] = useState('');

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
          render={({ field }) => {
            const filteredParents = parentCategories.filter((c) =>
              c.name.toLowerCase().includes(parentSearch.toLowerCase())
            );
            const selectedName = parentCategories.find((c) => c.id === field.value)?.name;

            return (
              <FormItem>
                <FormLabel>Categoria Pai (opcional)</FormLabel>
                {parentCategories.length > 0 ? (
                  <Popover open={parentOpen} onOpenChange={setParentOpen}>
                    <PopoverTrigger asChild>
                      <FormControl>
                        <Button
                          variant="outline"
                          role="combobox"
                          aria-expanded={parentOpen}
                          aria-label="Selecionar categoria pai"
                          className={cn(
                            'w-full justify-between font-normal',
                            !field.value && 'text-muted-foreground'
                          )}
                        >
                          {selectedName ?? 'Sem categoria pai'}
                          <ChevronsUpDown className="ml-2 h-4 w-4 shrink-0 opacity-50" />
                        </Button>
                      </FormControl>
                    </PopoverTrigger>
                    <PopoverContent className="w-full p-2" align="start">
                      <Input
                        placeholder="Buscar categoria..."
                        value={parentSearch}
                        onChange={(e) => setParentSearch(e.target.value)}
                        className="mb-2"
                        aria-label="Buscar categoria pai"
                      />
                      <div className="max-h-48 overflow-y-auto space-y-1">
                        <button
                          type="button"
                          onClick={() => {
                            field.onChange(undefined);
                            setParentOpen(false);
                            setParentSearch('');
                          }}
                          className={cn(
                            'flex w-full items-center gap-2 rounded px-2 py-1.5 text-sm hover:bg-accent',
                            !field.value && 'font-medium'
                          )}
                        >
                          <Check
                            className={cn('h-4 w-4', field.value ? 'opacity-0' : 'opacity-100')}
                          />
                          Sem categoria pai
                        </button>
                        {filteredParents.length === 0 ? (
                          <p className="px-2 py-4 text-center text-sm text-muted-foreground">
                            Nenhuma categoria encontrada.
                          </p>
                        ) : (
                          filteredParents.map((cat) => (
                            <button
                              key={cat.id}
                              type="button"
                              onClick={() => {
                                field.onChange(cat.id);
                                setParentOpen(false);
                                setParentSearch('');
                              }}
                              className={cn(
                                'flex w-full items-center gap-2 rounded px-2 py-1.5 text-sm hover:bg-accent',
                                field.value === cat.id && 'font-medium'
                              )}
                            >
                              <Check
                                className={cn(
                                  'h-4 w-4',
                                  field.value === cat.id ? 'opacity-100' : 'opacity-0'
                                )}
                              />
                              {cat.name}
                            </button>
                          ))
                        )}
                      </div>
                    </PopoverContent>
                  </Popover>
                ) : (
                  <FormControl>
                    <Input
                      placeholder="ID da categoria pai"
                      {...field}
                      value={field.value ?? ''}
                    />
                  </FormControl>
                )}
                <FormMessage />
              </FormItem>
            );
          }}
        />

        <Button type="submit" disabled={isPending} className="w-full">
          {isPending ? 'Salvando...' : initialValues ? 'Atualizar' : 'Criar'}
        </Button>
      </form>
    </Form>
  );
}
