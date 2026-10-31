
import axios from 'axios';

import { requestApiService } from './request.api.service';
import { requestLocalService } from './request.local.service';

export interface SyncResult {
  total: number;
  synced: number;
  pending: number;
  failed: number;
}

export class RequestSyncService {

  async synchronize(): Promise<SyncResult> {
    const pendingRequests = await requestLocalService.getPending();

    const result: SyncResult = {
      total: pendingRequests.length,
      synced: 0,
      pending: 0,
      failed: 0
    };

    for (const request of pendingRequests) {

      if (request.localId === undefined) {
        result.failed++;
        continue;
      }

      try {
        const response = await requestApiService.create({
          clientRequestId: request.clientRequestId,
          name: request.name,
          payload: request.payload,
          type: request.type
        });

        await requestLocalService.update(request.localId, {
          serverId: response.id,
          syncStatus: 'SYNCED',
          serverStatus: response.status,
          processedPayload: response.processedPayload,
          syncedAt: new Date().toISOString(),
          lastSyncError: undefined
        });

        result.synced++;

      } catch (error: unknown) {

        const message = axios.isAxiosError(error)
          ? error.response?.data?.message ?? error.message
          : error instanceof Error
            ? error.message
            : 'Error desconocido durante la sincronización';

        const statusCode = axios.isAxiosError(error)
          ? error.response?.status
          : undefined;

        if (statusCode === 400 || statusCode === 422) {
          await requestLocalService.update(request.localId, {
            syncStatus: 'FAILED',
            lastSyncError: String(message)
          });

          result.failed++;
        } else {
          await requestLocalService.update(request.localId, {
            lastSyncError: String(message)
          });

          result.pending++;
        }
      }
    }

    return result;
  }
}

export const requestSyncService = new RequestSyncService();
