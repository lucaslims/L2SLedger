import { apiClient } from '@/shared/lib/api/client';
import { API_ENDPOINTS } from '@/shared/lib/api/endpoints';
import type {
  CategoryDto,
  GetCategoriesResponse,
  CreateCategoryRequest,
  UpdateCategoryRequest,
} from '../types/category.types';

/**
 * Service de Categorias
 *
 * Centraliza chamadas de API relacionadas a categorias.
 * Não contém lógica financeira — apenas transporte de dados.
 */
export const categoryService = {
  /**
   * Listar todas as categorias
   * Backend retorna envelope: { categories: [...], totalCount: N }
   */
  async getAll(): Promise<CategoryDto[]> {
    const response = await apiClient.get<GetCategoriesResponse>(API_ENDPOINTS.CATEGORIES);
    return response.categories;
  },

  /**
   * Buscar categoria por ID
   */
  async getById(id: string): Promise<CategoryDto> {
    return apiClient.get<CategoryDto>(API_ENDPOINTS.CATEGORY_BY_ID(id));
  },

  /**
   * Criar nova categoria
   */
  async create(data: CreateCategoryRequest): Promise<CategoryDto> {
    return apiClient.post<CategoryDto>(API_ENDPOINTS.CATEGORIES, data);
  },

  /**
   * Atualizar categoria existente
   */
  async update(id: string, data: UpdateCategoryRequest): Promise<CategoryDto> {
    return apiClient.put<CategoryDto>(API_ENDPOINTS.CATEGORY_BY_ID(id), data);
  },

  /**
   * Excluir categoria
   */
  async delete(id: string): Promise<void> {
    return apiClient.delete<void>(API_ENDPOINTS.CATEGORY_BY_ID(id));
  },
};
