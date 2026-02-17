import { QueryClient } from '@tanstack/react-query';

/**
 * Configuração do React Query Client
 */
export const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: 1,
      refetchOnWindowFocus: false,
      staleTime: 5 * 60 * 1000, // 5 minutos
      gcTime: 10 * 60 * 1000, // 10 minutos (cacheTime no v4)
    },
    mutations: {
      retry: 0,
    },
  },
});
