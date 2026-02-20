import type { ErrorResponse } from '@/shared/types/errors.types';
import { ApiError } from '@/shared/types/errors.types';
import { getEnv } from '@/shared/lib/env';

const API_BASE_URL = getEnv('VITE_API_BASE_URL') || '/api/v1';

interface FetchOptions extends RequestInit {
  params?: Record<string, string | number | boolean | undefined | null>;
}

/**
 * API Client baseado em Fetch API
 * Usa cookies HttpOnly automáticos (credentials: 'include')
 */
class ApiClient {
  private baseURL: string;

  constructor(baseURL: string) {
    this.baseURL = baseURL;
  }

  /**
   * Método base para fazer requisições
   */
  private async request<T>(endpoint: string, options: FetchOptions = {}): Promise<T> {
    const { params, ...fetchOptions } = options;

    // Construir URL com query params
    const url = new URL(`${this.baseURL}${endpoint}`);
    if (params) {
      Object.entries(params).forEach(([key, value]) => {
        if (value !== undefined && value !== null) {
          url.searchParams.append(key, String(value));
        }
      });
    }

    const config: RequestInit = {
      ...fetchOptions,
      credentials: 'include', // Essencial para cookies HttpOnly
      headers: {
        'Content-Type': 'application/json',
        ...fetchOptions.headers,
      },
    };

    try {
      const response = await fetch(url.toString(), config);

      // Se não for OK, tentar parsear erro
      if (!response.ok) {
        await this.handleErrorResponse(response);
      }

      // Se resposta for 204 No Content, retornar undefined
      if (response.status === 204) {
        return undefined as T;
      }

      const data = await response.json();
      return data;
    } catch (error) {
      if (error instanceof ApiError) {
        throw error;
      }

      // Erro de rede ou parse
      console.error('API Client Error:', error);
      throw new ApiError(
        'SYS_INTERNAL_ERROR',
        'Erro ao se comunicar com o servidor. Verifique sua conexão.'
      );
    }
  }

  /**
   * Tratar resposta de erro da API
   */
  private async handleErrorResponse(response: Response): Promise<never> {
    try {
      const errorData: ErrorResponse = await response.json();
      throw new ApiError(
        errorData.error.code,
        errorData.error.message,
        errorData.error.traceId,
        errorData.error.details
      );
    } catch (error) {
      if (error instanceof ApiError) {
        throw error;
      }

      // Se não conseguiu parsear JSON, lançar erro genérico
      throw new ApiError(
        'SYS_INTERNAL_ERROR',
        `Erro HTTP ${response.status}: ${response.statusText}`
      );
    }
  }

  /**
   * GET request
   */
  async get<T>(endpoint: string, options?: FetchOptions): Promise<T> {
    return this.request<T>(endpoint, { ...options, method: 'GET' });
  }

  /**
   * POST request
   */
  async post<T>(endpoint: string, body?: unknown, options?: FetchOptions): Promise<T> {
    return this.request<T>(endpoint, {
      ...options,
      method: 'POST',
      body: body ? JSON.stringify(body) : undefined,
    });
  }

  /**
   * PUT request
   */
  async put<T>(endpoint: string, body?: unknown, options?: FetchOptions): Promise<T> {
    return this.request<T>(endpoint, {
      ...options,
      method: 'PUT',
      body: body ? JSON.stringify(body) : undefined,
    });
  }

  /**
   * DELETE request
   */
  async delete<T>(endpoint: string, options?: FetchOptions): Promise<T> {
    return this.request<T>(endpoint, { ...options, method: 'DELETE' });
  }
}

// Exportar instância única
export const apiClient = new ApiClient(API_BASE_URL);
