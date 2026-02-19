import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import React from 'react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import type { ReactNode } from 'react';
import { useUsers } from '../hooks/useUsers';
import { usePendingUsers } from '../hooks/usePendingUsers';
import type { UserSummaryDto } from '../types/user.types';

vi.mock('../services/userService', () => ({
  userService: {
    getAll: vi.fn(),
    getById: vi.fn(),
    getPendingCount: vi.fn(),
    updateStatus: vi.fn(),
    updateRoles: vi.fn(),
  },
}));

import { userService } from '../services/userService';

const mockUsers: UserSummaryDto[] = [
  {
    id: '1',
    email: 'admin@test.com',
    displayName: 'Admin User',
    status: 'Active',
    roles: ['Admin'],
    createdAt: '2026-01-01T00:00:00Z',
  },
  {
    id: '2',
    email: 'pending@test.com',
    displayName: 'Pending User',
    status: 'Pending',
    roles: [],
    createdAt: '2026-01-15T00:00:00Z',
  },
  {
    id: '3',
    email: 'suspended@test.com',
    displayName: 'Suspended User',
    status: 'Suspended',
    roles: ['Leitura'],
    createdAt: '2026-01-10T00:00:00Z',
  },
];

function createWrapper() {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false },
      mutations: { retry: false },
    },
  });

  return function Wrapper({ children }: { children: ReactNode }) {
    return React.createElement(QueryClientProvider, { client: queryClient }, children);
  };
}

describe('useUsers', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('deve retornar lista de todos os usuários', async () => {
    vi.mocked(userService.getAll).mockResolvedValue(mockUsers);

    const { result } = renderHook(() => useUsers(), {
      wrapper: createWrapper(),
    });

    expect(result.current.isLoading).toBe(true);

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(result.current.data).toEqual(mockUsers);
    expect(userService.getAll).toHaveBeenCalledWith(undefined);
  });

  it('deve filtrar por status quando informado', async () => {
    const pendingUsers = mockUsers.filter((u) => u.status === 'Pending');
    vi.mocked(userService.getAll).mockResolvedValue(pendingUsers);

    const { result } = renderHook(() => useUsers('Pending'), {
      wrapper: createWrapper(),
    });

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(result.current.data).toEqual(pendingUsers);
    expect(userService.getAll).toHaveBeenCalledWith('Pending');
  });

  it('deve tratar erro na listagem', async () => {
    vi.mocked(userService.getAll).mockRejectedValue(new Error('Network error'));

    const { result } = renderHook(() => useUsers(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => {
      expect(result.current.isError).toBe(true);
    });
  });
});

describe('usePendingUsers', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('deve retornar contagem de usuários pendentes', async () => {
    vi.mocked(userService.getPendingCount).mockResolvedValue(3);

    const { result } = renderHook(() => usePendingUsers(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(result.current.data).toBe(3);
  });

  it('deve retornar zero quando não há pendentes', async () => {
    vi.mocked(userService.getPendingCount).mockResolvedValue(0);

    const { result } = renderHook(() => usePendingUsers(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(result.current.data).toBe(0);
  });
});
