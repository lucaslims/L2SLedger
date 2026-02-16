import type { UserStatus, UserRole } from './common.types';

/**
 * DTOs da API (espelhando backend)
 */

export interface UserDto {
  id: string;
  email: string;
  displayName: string;
  status: UserStatus;
  emailVerified: boolean;
  roles: UserRole[];
  createdAt: string;
  updatedAt: string | null;
}

export interface CurrentUserResponse {
  user: UserDto;
}

export interface LoginRequest {
  firebaseIdToken: string;
}

export interface LoginResponse {
  user: UserDto;
  sessionExpiresAt: string;
}
