/**
 * DTOs de Categorias
 *
 * Espelha os contratos do backend.
 * Não contém lógica financeira — apenas transporte de dados.
 */

export interface CategoryDto {
  id: string;
  name: string;
  description?: string;
  type: 'Income' | 'Expense';
  isActive: boolean;
  parentCategoryId?: string | null;
  parentCategoryName?: string | null;
  createdAt: string;
  updatedAt: string | null;
}

/**
 * Resposta do GET /categories (envelope)
 */
export interface GetCategoriesResponse {
  categories: CategoryDto[];
  totalCount: number;
}

export interface CreateCategoryRequest {
  name: string;
  description?: string;
  parentCategoryId?: string;
  type: 'Income' | 'Expense';
}

export interface UpdateCategoryRequest {
  name: string;
  description?: string;
  parentCategoryId?: string;
  type: 'Income' | 'Expense';
}
