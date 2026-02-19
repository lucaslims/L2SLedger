import { apiClient } from '@/shared/lib/api/client';
import { API_ENDPOINTS } from '@/shared/lib/api/endpoints';
import type {
  UserSummaryDto,
  UserDetailDto,
  GetUsersResponse,
  UpdateUserStatusRequest,
  UpdateUserRolesRequest,
} from '../types/user.types';

export const userService = {
  async getAll(status?: string): Promise<UserSummaryDto[]> {
    const response = await apiClient.get<GetUsersResponse>(API_ENDPOINTS.USERS, {
      params: { status },
    });
    return response.users;
  },

  async getById(id: string): Promise<UserDetailDto> {
    return apiClient.get<UserDetailDto>(API_ENDPOINTS.USER_BY_ID(id));
  },

  async getPendingCount(): Promise<number> {
    const response = await apiClient.get<GetUsersResponse>(API_ENDPOINTS.USERS, {
      params: { status: 'Pending' },
    });
    return response.totalCount;
  },

  async updateStatus(id: string, data: UpdateUserStatusRequest): Promise<UserDetailDto> {
    return apiClient.put<UserDetailDto>(API_ENDPOINTS.USER_STATUS(id), data);
  },

  async updateRoles(id: string, data: UpdateUserRolesRequest): Promise<UserDetailDto> {
    return apiClient.put<UserDetailDto>(API_ENDPOINTS.USER_ROLES(id), data);
  },
};
