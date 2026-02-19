import { apiClient } from '@/shared/lib/api/client';
import { API_ENDPOINTS } from '@/shared/lib/api/endpoints';
import type {
  TransactionDto,
  GetTransactionsResponse,
  CreateTransactionRequest,
  UpdateTransactionRequest,
  TransactionFilters,
} from '../types/transaction.types';

/**
 * Service de Transações
 *
 * Centraliza chamadas de API relacionadas a transações.
 * Não contém lógica financeira — apenas transporte de dados.
 */
export const transactionService = {
  /**
   * Listar transações com filtros e paginação
   * Backend retorna envelope: { transactions: [...], totalCount, page, pageSize, totalPages, totalIncome, totalExpense, balance }
   */
  async getAll(filters: TransactionFilters = {}): Promise<GetTransactionsResponse> {
    return apiClient.get<GetTransactionsResponse>(API_ENDPOINTS.TRANSACTIONS, {
      params: filters as Record<string, string | number | boolean | undefined | null>,
    });
  },

  /**
   * Buscar transação por ID
   */
  async getById(id: string): Promise<TransactionDto> {
    return apiClient.get<TransactionDto>(API_ENDPOINTS.TRANSACTION_BY_ID(id));
  },

  /**
   * Criar nova transação
   * Backend retorna { id: string } com status 201
   */
  async create(data: CreateTransactionRequest): Promise<{ id: string }> {
    return apiClient.post<{ id: string }>(API_ENDPOINTS.TRANSACTIONS, data);
  },

  /**
   * Atualizar transação existente
   * Backend retorna 204 No Content
   */
  async update(id: string, data: UpdateTransactionRequest): Promise<void> {
    return apiClient.put<void>(API_ENDPOINTS.TRANSACTION_BY_ID(id), data);
  },

  /**
   * Excluir transação (soft delete)
   * Backend retorna 204 No Content
   */
  async delete(id: string): Promise<void> {
    return apiClient.delete<void>(API_ENDPOINTS.TRANSACTION_BY_ID(id));
  },
};
