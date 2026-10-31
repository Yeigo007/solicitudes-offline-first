
import { apiClient } from '../../../infrastructure/api/api.client';
import type {
  CreateRequestApi,
  RequestApiResponse,
  ProcessPendingRequestsResponse
} from '../types/request-api.types';

export class RequestApiService {

  async create(
    request: CreateRequestApi
  ): Promise<RequestApiResponse> {
    const response = await apiClient.post<RequestApiResponse>(
      '/requests',
      request
    );

    return response.data;
  }

  async getAll(): Promise<RequestApiResponse[]> {
    const response = await apiClient.get<RequestApiResponse[]>(
      '/requests'
    );

    return response.data;
  }

  async getById(id: string): Promise<RequestApiResponse> {
    const response = await apiClient.get<RequestApiResponse>(
      `/requests/${id}`
    );

    return response.data;
  }

  
  async process(id: string): Promise<RequestApiResponse> {
    const response = await apiClient.post<RequestApiResponse>(
        `/requests/${id}/process`
    );

    return response.data;
    }

        
    async processPending(): Promise<ProcessPendingRequestsResponse> {
    const response = await apiClient.post<ProcessPendingRequestsResponse>(
        '/requests/process-pending'
    );

    return response.data;
    }



}

export const requestApiService = new RequestApiService();
