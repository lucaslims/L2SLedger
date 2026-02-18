import { useEffect, useMemo } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { format } from 'date-fns';
import { ptBR } from 'date-fns/locale';
import { CalendarIcon } from 'lucide-react';
import { Button } from '@/shared/components/ui/button';
import { Input } from '@/shared/components/ui/input';
import { Textarea } from '@/shared/components/ui/textarea';
import { Switch } from '@/shared/components/ui/switch';
import {
  Form,
  FormField,
  FormItem,
  FormLabel,
  FormControl,
  FormMessage,
  FormDescription,
} from '@/shared/components/ui/form';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/shared/components/ui/select';
import { Calendar } from '@/shared/components/ui/calendar';
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from '@/shared/components/ui/popover';
import { useCategories } from '@/features/categories/hooks/useCategories';
import { cn } from '@/shared/lib/utils/cn';
import type { TransactionDto, CreateTransactionRequest } from '../types/transaction.types';
import { TransactionTypeMap } from '../types/transaction.types';

const transactionSchema = z.object({
  description: z
    .string()
    .min(1, 'Descrição é obrigatória')
    .max(200, 'Máximo 200 caracteres'),
  amount: z
    .number({ invalid_type_error: 'Informe um valor válido' })
    .positive('Valor deve ser maior que zero'),
  type: z.enum(['Income', 'Expense'], {
    required_error: 'Selecione o tipo',
  }),
  categoryId: z.string().min(1, 'Selecione uma categoria'),
  date: z.date({ required_error: 'Selecione uma data' }),
  notes: z.string().max(1000, 'Máximo 1000 caracteres').optional(),
  isRecurring: z.boolean(),
  recurringDay: z.number().int().min(1).max(31).optional().nullable(),
}).refine(
  (data) => !data.isRecurring || (data.recurringDay && data.recurringDay >= 1 && data.recurringDay <= 31),
  {
    message: 'Dia de recorrência é obrigatório (1-31)',
    path: ['recurringDay'],
  }
);

type TransactionFormData = z.infer<typeof transactionSchema>;

interface TransactionFormProps {
  initialValues?: TransactionDto;
  onSubmit: (data: CreateTransactionRequest) => void;
  isPending?: boolean;
}

export function TransactionForm({ initialValues, onSubmit, isPending }: TransactionFormProps) {
  const form = useForm<TransactionFormData>({
    resolver: zodResolver(transactionSchema),
    defaultValues: initialValues
      ? {
          description: initialValues.description,
          amount: initialValues.amount,
          type: initialValues.type === 1 ? 'Income' : 'Expense',
          categoryId: initialValues.categoryId,
          date: new Date(initialValues.transactionDate),
          notes: initialValues.notes ?? '',
          isRecurring: initialValues.isRecurring,
          recurringDay: initialValues.recurringDay,
        }
      : {
          description: '',
          amount: 0,
          type: 'Expense' as const,
          categoryId: '',
          date: new Date(),
          notes: '',
          isRecurring: false,
          recurringDay: null,
        },
  });

  const selectedType = form.watch('type');
  const isRecurring = form.watch('isRecurring');
  const { data: allCategories } = useCategories();

  // Filtrar categorias pelo tipo da transação selecionada (client-side)
  // Se a categoria não tiver tipo (backend antigo), exibe todas
  const categories = useMemo(() => {
    if (!allCategories) return [];
    const filtered = allCategories.filter((c) => c.type === selectedType);
    return filtered.length > 0 ? filtered : allCategories;
  }, [allCategories, selectedType]);

  // Reset categoryId when type changes (only for new transactions)
  useEffect(() => {
    if (!initialValues) {
      form.setValue('categoryId', '');
    }
  }, [selectedType, form, initialValues]);

  const handleFormSubmit = (data: TransactionFormData) => {
    const request: CreateTransactionRequest = {
      description: data.description,
      amount: data.amount,
      type: TransactionTypeMap[data.type],
      transactionDate: data.date.toISOString(),
      categoryId: data.categoryId,
      notes: data.notes || null,
      isRecurring: data.isRecurring,
      recurringDay: data.isRecurring ? data.recurringDay : null,
    };
    onSubmit(request);
  };

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(handleFormSubmit)} className="space-y-4">
        <FormField
          control={form.control}
          name="description"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Descrição</FormLabel>
              <FormControl>
                <Input placeholder="Ex: Compra no supermercado" {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <div className="grid gap-4 md:grid-cols-2">
          <FormField
            control={form.control}
            name="amount"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Valor (R$)</FormLabel>
                <FormControl>
                  <Input
                    type="number"
                    step="0.01"
                    min="0.01"
                    placeholder="0,00"
                    {...field}
                    value={field.value || ''}
                    onChange={(e) => {
                      const val = parseFloat(e.target.value);
                      field.onChange(isNaN(val) ? 0 : val);
                    }}
                  />
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
        </div>

        <div className="grid gap-4 md:grid-cols-2">
          <FormField
            control={form.control}
            name="categoryId"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Categoria</FormLabel>
                <Select onValueChange={field.onChange} value={field.value}>
                  <FormControl>
                    <SelectTrigger>
                      <SelectValue placeholder="Selecione uma categoria" />
                    </SelectTrigger>
                  </FormControl>
                  <SelectContent>
                    {categories?.map((category) => (
                      <SelectItem key={category.id} value={category.id}>
                        {category.name}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
                <FormMessage />
              </FormItem>
            )}
          />

          <FormField
            control={form.control}
            name="date"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Data</FormLabel>
                <Popover>
                  <PopoverTrigger asChild>
                    <FormControl>
                      <Button
                        variant="outline"
                        className={cn(
                          'w-full pl-3 text-left font-normal',
                          !field.value && 'text-muted-foreground'
                        )}
                      >
                        {field.value
                          ? format(field.value, 'dd/MM/yyyy', { locale: ptBR })
                          : 'Selecione uma data'}
                        <CalendarIcon className="ml-auto h-4 w-4 opacity-50" />
                      </Button>
                    </FormControl>
                  </PopoverTrigger>
                  <PopoverContent className="w-auto p-0" align="start">
                    <Calendar
                      mode="single"
                      selected={field.value}
                      onSelect={field.onChange}
                      disabled={(date) => date > new Date()}
                      locale={ptBR}
                      initialFocus
                    />
                  </PopoverContent>
                </Popover>
                <FormMessage />
              </FormItem>
            )}
          />
        </div>

        <FormField
          control={form.control}
          name="notes"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Observações (opcional)</FormLabel>
              <FormControl>
                <Textarea
                  placeholder="Notas adicionais sobre a transação..."
                  className="resize-none"
                  rows={3}
                  {...field}
                  value={field.value ?? ''}
                />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <div className="space-y-4 rounded-lg border p-4">
          <FormField
            control={form.control}
            name="isRecurring"
            render={({ field }) => (
              <FormItem className="flex flex-row items-center justify-between">
                <div className="space-y-0.5">
                  <FormLabel>Transação Recorrente</FormLabel>
                  <FormDescription>
                    Marque se esta transação se repete mensalmente
                  </FormDescription>
                </div>
                <FormControl>
                  <Switch
                    checked={field.value}
                    onCheckedChange={field.onChange}
                  />
                </FormControl>
              </FormItem>
            )}
          />

          {isRecurring && (
            <FormField
              control={form.control}
              name="recurringDay"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Dia do mês (1-31)</FormLabel>
                  <FormControl>
                    <Input
                      type="number"
                      min={1}
                      max={31}
                      placeholder="15"
                      {...field}
                      value={field.value ?? ''}
                      onChange={(e) => {
                        const val = parseInt(e.target.value, 10);
                        field.onChange(isNaN(val) ? null : val);
                      }}
                    />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
          )}
        </div>

        <Button type="submit" disabled={isPending} className="w-full">
          {isPending ? 'Salvando...' : initialValues ? 'Atualizar' : 'Criar Transação'}
        </Button>
      </form>
    </Form>
  );
}
