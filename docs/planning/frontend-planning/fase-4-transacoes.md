# Fase 4: Transações — Frontend L2SLedger

> **Estimativa:** 16 horas  
> **Dependência:** Fase 3 (Categorias) completa  
> **Status:** Pendente

---

## 🎯 Objetivo

Implementar CRUD completo de transações financeiras com:
- Listagem com paginação e filtros
- Criação e edição de transações
- Exclusão com confirmação
- Integração com categorias
- Formatação de valores em BRL
- Cards mobile responsivos
- Atualização do dashboard com dados reais

---

## 📋 Estrutura de Arquivos

```
features/transactions/
├── components/
│   ├── TransactionList.tsx             # Tabela/Lista
│   ├── TransactionForm.tsx             # Formulário complexo
│   ├── TransactionCard.tsx             # Card mobile
│   ├── TransactionFilters.tsx          # Filtros (data, tipo, categoria)
│   └── TransactionDeleteDialog.tsx     # Confirmação
├── hooks/
│   ├── useTransactions.ts              # Lista paginada
│   ├── useTransaction.ts               # Detalhe
│   ├── useCreateTransaction.ts         # Create
│   ├── useUpdateTransaction.ts         # Update
│   └── useDeleteTransaction.ts         # Delete
├── pages/
│   ├── TransactionsPage.tsx            # Lista principal
│   └── TransactionFormPage.tsx         # Criar/Editar
├── services/
│   └── transactionService.ts           # API calls
└── types/
    └── transaction.types.ts            # DTOs

shared/components/data-display/
├── AmountDisplay.tsx                   # Formatação de valores
├── DateDisplay.tsx                     # Formatação de datas
└── Pagination.tsx                      # Componente reutilizável
```

---

## 📋 Tasks Detalhadas

### 4.1 Types

```typescript
// features/transactions/types/transaction.types.ts

export interface TransactionDto {
  id: string;
  description: string;
  amount: number;
  type: 'Income' | 'Expense';
  categoryId: string;
  categoryName: string;
  date: string;
  createdAt: string;
  updatedAt: string | null;
}

export interface CreateTransactionRequest {
  description: string;
  amount: number;
  type: 'Income' | 'Expense';
  categoryId: string;
  date: string;
}

export interface TransactionFilters {
  type?: 'Income' | 'Expense';
  categoryId?: string;
  startDate?: string;
  endDate?: string;
  page?: number;
  pageSize?: number;
}
```

### 4.2 Service

```typescript
import { apiClient } from '@/shared/lib/api/client';
import { API_ENDPOINTS } from '@/shared/lib/api/endpoints';
import type { PaginatedResponse } from '@/shared/types/common.types';
import type { TransactionDto, CreateTransactionRequest, TransactionFilters } from '../types/transaction.types';

export const transactionService = {
  async getAll(filters: TransactionFilters): Promise<PaginatedResponse<TransactionDto>> {
    return apiClient.get<PaginatedResponse<TransactionDto>>(
      API_ENDPOINTS.TRANSACTIONS,
      { params: filters }
    );
  },

  async getById(id: string): Promise<TransactionDto> {
    return apiClient.get<TransactionDto>(API_ENDPOINTS.TRANSACTION_BY_ID(id));
  },

  async create(data: CreateTransactionRequest): Promise<TransactionDto> {
    return apiClient.post<TransactionDto>(API_ENDPOINTS.TRANSACTIONS, data);
  },

  async update(id: string, data: CreateTransactionRequest): Promise<TransactionDto> {
    return apiClient.put<TransactionDto>(API_ENDPOINTS.TRANSACTION_BY_ID(id), data);
  },

  async delete(id: string): Promise<void> {
    return apiClient.delete<void>(API_ENDPOINTS.TRANSACTION_BY_ID(id));
  },
};
```

### 4.3 Componentes Auxiliares

#### AmountDisplay.tsx

```typescript
import { formatCurrency } from '@/shared/lib/utils/formatters';
import { cn } from '@/shared/lib/utils/cn';

interface AmountDisplayProps {
  amount: number;
  type: 'Income' | 'Expense';
  className?: string;
}

export function AmountDisplay({ amount, type, className }: AmountDisplayProps) {
  const isIncome = type === 'Income';

  return (
    <span
      className={cn(
        'font-semibold',
        isIncome ? 'text-income' : 'text-expense',
        className
      )}
    >
      {isIncome ? '+' : '-'} {formatCurrency(Math.abs(amount))}
    </span>
  );
}
```

#### Pagination.tsx

```typescript
import { Button } from '@/shared/components/ui/button';
import { ChevronLeft, ChevronRight } from 'lucide-react';

interface PaginationProps {
  currentPage: number;
  totalPages: number;
  onPageChange: (page: number) => void;
}

export function Pagination({ currentPage, totalPages, onPageChange }: PaginationProps) {
  return (
    <div className="flex items-center justify-between">
      <p className="text-sm text-muted-foreground">
        Página {currentPage} de {totalPages}
      </p>

      <div className="flex gap-2">
        <Button
          variant="outline"
          size="icon"
          onClick={() => onPageChange(currentPage - 1)}
          disabled={currentPage === 1}
        >
          <ChevronLeft className="h-4 w-4" />
        </Button>
        <Button
          variant="outline"
          size="icon"
          onClick={() => onPageChange(currentPage + 1)}
          disabled={currentPage === totalPages}
        >
          <ChevronRight className="h-4 w-4" />
        </Button>
      </div>
    </div>
  );
}
```

### 4.4 TransactionForm

```typescript
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Button } from '@/shared/components/ui/button';
import { Input } from '@/shared/components/ui/input';
import { Form, FormField, FormItem, FormLabel, FormControl, FormMessage } from '@/shared/components/ui/form';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/shared/components/ui/select';
import { useCategories } from '@/features/categories/hooks/useCategories';
import { Calendar } from '@/shared/components/ui/calendar';
import { Popover, PopoverContent, PopoverTrigger } from '@/shared/components/ui/popover';
import { CalendarIcon } from 'lucide-react';
import { format } from 'date-fns';
import { cn } from '@/shared/lib/utils/cn';

const transactionSchema = z.object({
  description: z.string().min(1, 'Descrição é obrigatória').max(200),
  amount: z.number().positive('Valor deve ser maior que zero'),
  type: z.enum(['Income', 'Expense']),
  categoryId: z.string().min(1, 'Selecione uma categoria'),
  date: z.date(),
});

type TransactionFormData = z.infer<typeof transactionSchema>;

interface TransactionFormProps {
  initialValues?: Partial<TransactionFormData>;
  onSubmit: (data: TransactionFormData) => void;
  isPending?: boolean;
}

export function TransactionForm({ initialValues, onSubmit, isPending }: TransactionFormProps) {
  const form = useForm<TransactionFormData>({
    resolver: zodResolver(transactionSchema),
    defaultValues: {
      description: '',
      amount: 0,
      type: 'Expense',
      categoryId: '',
      date: new Date(),
      ...initialValues,
    },
  });

  const selectedType = form.watch('type');
  const { data: categories } = useCategories(selectedType);

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
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
                <FormLabel>Valor</FormLabel>
                <FormControl>
                  <Input
                    type="number"
                    step="0.01"
                    placeholder="0.00"
                    {...field}
                    onChange={(e) => field.onChange(parseFloat(e.target.value))}
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
                      <SelectValue />
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
                      <SelectValue placeholder="Selecione..." />
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
                        {field.value ? format(field.value, 'dd/MM/yyyy') : 'Selecione...'}
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
                    />
                  </PopoverContent>
                </Popover>
                <FormMessage />
              </FormItem>
            )}
          />
        </div>

        <Button type="submit" disabled={isPending} className="w-full">
          {isPending ? 'Salvando...' : initialValues ? 'Atualizar' : 'Criar'}
        </Button>
      </form>
    </Form>
  );
}
```

### 4.5 TransactionList

```typescript
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/shared/components/ui/table';
import { Button } from '@/shared/components/ui/button';
import { Edit, Trash } from 'lucide-react';
import { AmountDisplay } from '@/shared/components/data-display/AmountDisplay';
import { formatDate } from '@/shared/lib/utils/formatters';
import type { TransactionDto } from '../types/transaction.types';

interface TransactionListProps {
  transactions: TransactionDto[];
  onEdit: (id: string) => void;
  onDelete: (id: string) => void;
}

export function TransactionList({ transactions, onEdit, onDelete }: TransactionListProps) {
  return (
    <Table>
      <TableHeader>
        <TableRow>
          <TableHead>Data</TableHead>
          <TableHead>Descrição</TableHead>
          <TableHead>Categoria</TableHead>
          <TableHead className="text-right">Valor</TableHead>
          <TableHead className="text-right">Ações</TableHead>
        </TableRow>
      </TableHeader>
      <TableBody>
        {transactions.map((transaction) => (
          <TableRow key={transaction.id}>
            <TableCell>{formatDate(transaction.date)}</TableCell>
            <TableCell>{transaction.description}</TableCell>
            <TableCell>{transaction.categoryName}</TableCell>
            <TableCell className="text-right">
              <AmountDisplay amount={transaction.amount} type={transaction.type} />
            </TableCell>
            <TableCell className="text-right">
              <Button variant="ghost" size="icon" onClick={() => onEdit(transaction.id)}>
                <Edit className="h-4 w-4" />
              </Button>
              <Button variant="ghost" size="icon" onClick={() => onDelete(transaction.id)}>
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

### 4.6 TransactionFilters

```typescript
import { Button } from '@/shared/components/ui/button';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/shared/components/ui/select';
import { useCategories } from '@/features/categories/hooks/useCategories';
import { X } from 'lucide-react';

interface TransactionFiltersProps {
  filters: {
    type?: string;
    categoryId?: string;
  };
  onFilterChange: (filters: any) => void;
}

export function TransactionFilters({ filters, onFilterChange }: TransactionFiltersProps) {
  const { data: categories } = useCategories();

  return (
    <div className="flex flex-wrap gap-4">
      <Select
        value={filters.type || 'all'}
        onValueChange={(value) =>
          onFilterChange({ ...filters, type: value === 'all' ? undefined : value })
        }
      >
        <SelectTrigger className="w-[180px]">
          <SelectValue placeholder="Tipo" />
        </SelectTrigger>
        <SelectContent>
          <SelectItem value="all">Todos</SelectItem>
          <SelectItem value="Income">Receitas</SelectItem>
          <SelectItem value="Expense">Despesas</SelectItem>
        </SelectContent>
      </Select>

      <Select
        value={filters.categoryId || 'all'}
        onValueChange={(value) =>
          onFilterChange({ ...filters, categoryId: value === 'all' ? undefined : value })
        }
      >
        <SelectTrigger className="w-[200px]">
          <SelectValue placeholder="Categoria" />
        </SelectTrigger>
        <SelectContent>
          <SelectItem value="all">Todas</SelectItem>
          {categories?.map((category) => (
            <SelectItem key={category.id} value={category.id}>
              {category.name}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>

      <Button
        variant="outline"
        size="icon"
        onClick={() => onFilterChange({})}
        disabled={!filters.type && !filters.categoryId}
      >
        <X className="h-4 w-4" />
      </Button>
    </div>
  );
}
```

---

## ✅ Critérios de Aceite

- [ ] CRUD completo de transações
- [ ] Filtros funcionando (tipo, categoria, data)
- [ ] Paginação funcionando
- [ ] Integração com categorias
- [ ] Valores formatados em BRL
- [ ] Dashboard atualizado com dados reais (RecentTransactions)
- [ ] Cards mobile responsivos
- [ ] Cobertura ≥ 85%
- [ ] Testes E2E passando

---

---

## ⚠️ Considerações ADRs e Comerciais

### ADR-015 — Imutabilidade de Períodos
> **IMPORTANTE:** Transações em períodos fechados são imutáveis.
> O frontend deve:
> - Exibir alerta `FIN_PERIOD_CLOSED` ao tentar editar/excluir
> - Desabilitar botões de ação para transações em períodos fechados

### ADR-021-A — Códigos de Erro FIN_
- ✅ FIN_PERIOD_CLOSED tratado
- ✅ FIN_TRANSACTION_NOT_FOUND tratado
- ✅ FIN_CATEGORY_NOT_FOUND tratado
- ✅ FIN_DUPLICATE_ENTRY tratado

### Regra Comercial (plans-and-features.md)
| Plano | Limite Lançamentos/Mês |
|-------|------------------------|
| FREE | 100 |
| PRO | 1.000 |
| BUSINESS | Ilimitado |

> O frontend deve ser preparado para exibir contador de uso após MVP.

---

**Próximo passo:** Fase 5 — Admin (Usuários)
