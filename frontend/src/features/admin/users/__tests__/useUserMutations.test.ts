import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import React from 'react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import type { ReactNode } from 'react';
import { useApproveUser } from '../hooks/useApproveUser';
import { useRejectUser } from '../hooks/useRejectUser';
import { useSuspendUser } from '../hooks/useSuspendUser';
import { useReactivateUser } from '../hooks/useReactivateUser';
import { useUpdateUserRoles } from '../hooks/useUpdateUserRoles';
import { ApiError } from '@/shared/types/errors.types';
import type { UserDetailDto } from '../types/user.types';

vi.mock('sonner', () => ({
  toast: {
    success: vi.fn(),
    error: vi.fn(),
  },
}));

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
import { toast } from 'sonner';

const mockUser: UserDetailDto = {
  id: '1',
  email: 'test@test.com',
  displayName: 'Test User',
  status: 'Active',
  emailVerified: true,
  roles: ['Leitura'],
  createdAt: '2026-01-01T00:00:00Z',
  updatedAt: null,
  lastLoginAt: null,
};

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

describe('useApproveUser', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('deve aprovar usuário com sucesso', async () => {
    vi.mocked(userService.updateStatus).mockResolvedValue({
      ...mockUser,
      status: 'Active',
    });

    const { result } = renderHook(() => useApproveUser(), {
      wrapper: createWrapper(),
    });

    result.current.mutate({ userId: '1', reason: 'Aprovado' });

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(userService.updateStatus).toHaveBeenCalledWith('1', {
      status: 'Active',
      reason: 'Aprovado',
    });
    expect(toast.success).toHaveBeenCalledWith('Usuário aprovado com sucesso!');
  });

  it('deve tratar erro ao aprovar', async () => {
    vi.mocked(userService.updateStatus).mockRejectedValue(
      new ApiError('USER_CANNOT_MODIFY_OWN_STATUS', 'Não pode alterar próprio status')
    );

    const { result } = renderHook(() => useApproveUser(), {
      wrapper: createWrapper(),
    });

    result.current.mutate({ userId: '1', reason: 'Test' });

    await waitFor(() => {
      expect(result.current.isError).toBe(true);
    });

    expect(toast.error).toHaveBeenCalled();
  });
});

describe('useRejectUser', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('deve rejeitar usuário com sucesso', async () => {
    vi.mocked(userService.updateStatus).mockResolvedValue({
      ...mockUser,
      status: 'Rejected',
    });

    const { result } = renderHook(() => useRejectUser(), {
      wrapper: createWrapper(),
    });

    result.current.mutate({ userId: '2', reason: 'Documentação insuficiente' });

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(userService.updateStatus).toHaveBeenCalledWith('2', {
      status: 'Rejected',
      reason: 'Documentação insuficiente',
    });
    expect(toast.success).toHaveBeenCalledWith('Usuário rejeitado.');
  });
});

describe('useSuspendUser', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('deve suspender usuário com sucesso', async () => {
    vi.mocked(userService.updateStatus).mockResolvedValue({
      ...mockUser,
      status: 'Suspended',
    });

    const { result } = renderHook(() => useSuspendUser(), {
      wrapper: createWrapper(),
    });

    result.current.mutate({ userId: '1', reason: 'Violação de termos' });

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(userService.updateStatus).toHaveBeenCalledWith('1', {
      status: 'Suspended',
      reason: 'Violação de termos',
    });
    expect(toast.success).toHaveBeenCalledWith('Usuário suspenso com sucesso.');
  });

  it('deve tratar erro USER_INVALID_STATUS_TRANSITION', async () => {
    vi.mocked(userService.updateStatus).mockRejectedValue(
      new ApiError('USER_INVALID_STATUS_TRANSITION', 'Transição inválida')
    );

    const { result } = renderHook(() => useSuspendUser(), {
      wrapper: createWrapper(),
    });

    result.current.mutate({ userId: '2', reason: 'Test' });

    await waitFor(() => {
      expect(result.current.isError).toBe(true);
    });

    expect(toast.error).toHaveBeenCalled();
  });
});

describe('useReactivateUser', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('deve reativar usuário com sucesso', async () => {
    vi.mocked(userService.updateStatus).mockResolvedValue({
      ...mockUser,
      status: 'Active',
    });

    const { result } = renderHook(() => useReactivateUser(), {
      wrapper: createWrapper(),
    });

    result.current.mutate({ userId: '3', reason: 'Reativação solicitada' });

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(userService.updateStatus).toHaveBeenCalledWith('3', {
      status: 'Active',
      reason: 'Reativação solicitada',
    });
    expect(toast.success).toHaveBeenCalledWith('Usuário reativado com sucesso!');
  });
});

describe('useUpdateUserRoles', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('deve atualizar roles com sucesso', async () => {
    vi.mocked(userService.updateRoles).mockResolvedValue({
      ...mockUser,
      roles: ['Admin', 'Financeiro'],
    });

    const { result } = renderHook(() => useUpdateUserRoles(), {
      wrapper: createWrapper(),
    });

    result.current.mutate({ userId: '1', roles: ['Admin', 'Financeiro'] });

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(userService.updateRoles).toHaveBeenCalledWith('1', {
      roles: ['Admin', 'Financeiro'],
    });
    expect(toast.success).toHaveBeenCalledWith('Roles atualizadas com sucesso!');
  });

  it('deve tratar erro USER_CANNOT_REMOVE_OWN_ADMIN', async () => {
    vi.mocked(userService.updateRoles).mockRejectedValue(
      new ApiError('USER_CANNOT_REMOVE_OWN_ADMIN', 'Não pode remover próprio admin')
    );

    const { result } = renderHook(() => useUpdateUserRoles(), {
      wrapper: createWrapper(),
    });

    result.current.mutate({ userId: '1', roles: ['Leitura'] });

    await waitFor(() => {
      expect(result.current.isError).toBe(true);
    });

    expect(toast.error).toHaveBeenCalled();
  });

  it('deve tratar erro USER_LAST_ADMIN', async () => {
    vi.mocked(userService.updateRoles).mockRejectedValue(
      new ApiError('USER_LAST_ADMIN', 'Último admin')
    );

    const { result } = renderHook(() => useUpdateUserRoles(), {
      wrapper: createWrapper(),
    });

    result.current.mutate({ userId: '1', roles: ['Leitura'] });

    await waitFor(() => {
      expect(result.current.isError).toBe(true);
    });

    expect(toast.error).toHaveBeenCalled();
  });
});
