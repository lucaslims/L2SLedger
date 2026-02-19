import { AppLayout } from '@/shared/components/layout/AppLayout';
import { TransactionForm } from '../components/TransactionForm';
import { useParams, useNavigate } from 'react-router-dom';
import { useTransaction } from '../hooks/useTransaction';
import { useCreateTransaction } from '../hooks/useCreateTransaction';
import { useUpdateTransaction } from '../hooks/useUpdateTransaction';
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from '@/shared/components/ui/card';
import { Button } from '@/shared/components/ui/button';
import { ArrowLeft } from 'lucide-react';
import { Skeleton } from '@/shared/components/ui/skeleton';
import type { CreateTransactionRequest } from '../types/transaction.types';

export default function TransactionFormPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const isEdit = !!id;

  const { data: transaction, isLoading } = useTransaction(id);
  const { mutate: createTransaction, isPending: isCreating } = useCreateTransaction();
  const { mutate: updateTransaction, isPending: isUpdating } = useUpdateTransaction();

  const handleSubmit = (data: CreateTransactionRequest) => {
    if (isEdit && id) {
      updateTransaction(
        { id, data },
        {
          onSuccess: () => navigate('/transactions'),
        }
      );
    } else {
      createTransaction(data, {
        onSuccess: () => navigate('/transactions'),
      });
    }
  };

  if (isEdit && isLoading) {
    return (
      <AppLayout>
        <div className="mx-auto max-w-2xl space-y-6">
          <Skeleton className="h-10 w-48" />
          <Skeleton className="h-96 w-full" />
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
            onClick={() => navigate('/transactions')}
            aria-label="Voltar para transações"
          >
            <ArrowLeft className="h-5 w-5" />
          </Button>
          <h1 className="text-3xl font-bold">
            {isEdit ? 'Editar Transação' : 'Nova Transação'}
          </h1>
        </div>

        <Card>
          <CardHeader>
            <CardTitle>Dados da Transação</CardTitle>
          </CardHeader>
          <CardContent>
            <TransactionForm
              initialValues={transaction ?? undefined}
              onSubmit={handleSubmit}
              isPending={isCreating || isUpdating}
            />
          </CardContent>
        </Card>
      </div>
    </AppLayout>
  );
}
