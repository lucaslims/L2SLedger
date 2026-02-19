import type { UserStatus, UserRole } from '@/shared/types/common.types';

/**
 * DTO resumido retornado na listagem (GET /users)
 */
export interface UserSummaryDto {
  id: string;
  email: string;
  displayName: string;
  status: UserStatus;
  roles: UserRole[];
  createdAt: string;
}

/**
 * DTO detalhado retornado por GET /users/:id, PUT /users/:id/status, PUT /users/:id/roles
 */
export interface UserDetailDto {
  id: string;
  email: string;
  displayName: string;
  emailVerified: boolean;
  status: UserStatus;
  roles: UserRole[];
  createdAt: string;
  updatedAt: string | null;
  lastLoginAt: string | null;
}

/**
 * Resposta paginada de GET /users
 */
export interface GetUsersResponse {
  users: UserSummaryDto[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export interface UpdateUserStatusRequest {
  status: UserStatus;
  reason: string;
}

export interface UpdateUserRolesRequest {
  roles: UserRole[];
}
