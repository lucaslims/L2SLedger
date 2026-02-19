/**
 * DTOs de Transações
 *
 * Espelha os contratos do backend.
 * Não contém lógica financeira — apenas transporte de dados.
 */

/**
 * Tipo de transação conforme backend (int enum)
 * 1 = Income, 2 = Expense
 */
export type TransactionTypeValue = 1 | 2;

export const TransactionTypeMap = {
  Income: 1 as TransactionTypeValue,
  Expense: 2 as TransactionTypeValue,
} as const;

export const TransactionTypeLabelMap: Record<TransactionTypeValue, string> = {
  1: 'Receita',
  2: 'Despesa',
};

export const TransactionTypeNameMap: Record<TransactionTypeValue, 'Income' | 'Expense'> = {
  1: 'Income',
  2: 'Expense',
};

export interface TransactionDto {
  id: string;
  description: string;
  amount: number;
  type: TransactionTypeValue;
  transactionDate: string;
  categoryId: string;
  categoryName: string;
  userId: string;
  notes: string | null;
  isRecurring: boolean;
  recurringDay: number | null;
  createdAt: string;
  updatedAt: string;
}

export interface CreateTransactionRequest {
  description: string;
  amount: number;
  type: TransactionTypeValue;
  transactionDate: string;
  categoryId: string;
  notes?: string | null;
  isRecurring: boolean;
  recurringDay?: number | null;
}

export type UpdateTransactionRequest = CreateTransactionRequest;

export interface GetTransactionsResponse {
  transactions: TransactionDto[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  totalIncome: number;
  totalExpense: number;
  balance: number;
}

export interface TransactionFilters {
  type?: TransactionTypeValue;
  categoryId?: string;
  startDate?: string;
  endDate?: string;
  page?: number;
  pageSize?: number;
}
