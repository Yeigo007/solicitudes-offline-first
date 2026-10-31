
import { requestApiService } from './request.api.service';
import { requestLocalService } from './request.local.service';

export class RequestBatchProcessingService {

  async processPending() {
    // Ejecutar el procesamiento de todas las solicitudes pendientes
    const result = await requestApiService.processPending();

    // Obtener solicitudes locales
    const localRequests = await requestLocalService.getAll();

    for (const localRequest of localRequests) {

      if (
        localRequest.localId === undefined ||
        !localRequest.serverId ||
        localRequest.syncStatus !== 'SYNCED'
      ) {
        continue;
      }

      const failure = result.failures.find(
        item =>
          item.requestId.toLowerCase() ===
          localRequest.serverId?.toLowerCase()
      );

      if (failure) {
        await requestLocalService.update(localRequest.localId, {
          serverStatus: 2,
          lastSyncError: failure.error
        });

        continue;
      }

      // Solo necesitamos consultar solicitudes que localmente
      // aparecen pendientes de procesamiento.
      if (localRequest.serverStatus === 0) {
        const response = await requestApiService.getById(
          localRequest.serverId
        );

        await requestLocalService.update(localRequest.localId, {
          serverStatus: response.status,
          processedPayload: response.processedPayload,
          lastSyncError: undefined
        });
      }
    }

    return result;
  }
}

export const requestBatchProcessingService =
  new RequestBatchProcessingService();
