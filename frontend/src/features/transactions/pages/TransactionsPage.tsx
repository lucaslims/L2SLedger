import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { AppLayout } from '@/shared/components/layout/AppLayout';
import { Button } from '@/shared/components/ui/button';
import { Plus } from 'lucide-react';
import { useTransactions } from '../hooks/useTransactions';
import { TransactionList } from '../components/TransactionList';
import { TransactionFilters } from '../components/TransactionFilters';
import { TransactionDeleteDialog } from '../components/TransactionDeleteDialog';
import { TransactionSummaryCards } from '../components/TransactionSummaryCards';
import { Pagination } from '@/shared/components/data-display/Pagination';
import { PAGINATION } from '@/shared/lib/utils/constants';
import type { TransactionFilters as FiltersType, TransactionDto } from '../types/transaction.types';

export default function TransactionsPage() {
  const navigate = useNavigate();
  const [filters, setFilters] = useState<FiltersType>({
    page: 1,
    pageSize: PAGINATION.DEFAULT_PAGE_SIZE,
  });
  const [deleteTarget, setDeleteTarget] = useState<TransactionDto | null>(null);

  const { data, isLoading } = useTransactions(filters);

  return (
    <AppLayout>
      <div className="space-y-6">
        {/* Header */}
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-3xl font-bold">Transações</h1>
            <p className="text-muted-foreground">
              Gerencie suas receitas e despesas
            </p>
          </div>
          <Button onClick={() => navigate('/transactions/new')}>
            <Plus className="mr-2 h-4 w-4" />
            Nova Transação
          </Button>
        </div>

        {/* Summary Cards */}
        {data && (
          <TransactionSummaryCards
            totalIncome={data.totalIncome}
            totalExpense={data.totalExpense}
            balance={data.balance}
          />
        )}

        {/* Filters */}
        <TransactionFilters filters={filters} onFilterChange={setFilters} />

        {/* Transaction List */}
        <TransactionList
          transactions={data?.transactions ?? []}
          isLoading={isLoading}
          onEdit={(id) => navigate(`/transactions/${id}/edit`)}
          onDelete={(transaction) => setDeleteTarget(transaction)}
        />

        {/* Pagination */}
        {data && data.totalPages > 1 && (
          <Pagination
            currentPage={data.page}
            totalPages={data.totalPages}
            totalItems={data.totalCount}
            onPageChange={(page) => setFilters((prev) => ({ ...prev, page }))}
          />
        )}

        {/* Delete Dialog */}
        {deleteTarget && (
          <TransactionDeleteDialog
            transaction={deleteTarget}
            open={!!deleteTarget}
            onOpenChange={(open) => {
              if (!open) setDeleteTarget(null);
            }}
          />
        )}
      </div>
    </AppLayout>
  );
}
