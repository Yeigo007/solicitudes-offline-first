
import { requestApiService } from './request.api.service';
import { requestLocalService } from './request.local.service';

export class RequestStatusSyncService {

  async synchronizeStatuses(): Promise<void> {
    const requests = await requestLocalService.getAll();

    // Solo consultamos solicitudes que ya fueron recibidas
    // por el backend y tienen un identificador de servidor.
    const synchronizedRequests = requests.filter(
      request =>
        request.syncStatus === 'SYNCED' &&
        request.serverId !== undefined
    );

    for (const request of synchronizedRequests) {
      if (request.localId === undefined || !request.serverId) {
        continue;
      }

      try {
        const response = await requestApiService.getById(
          request.serverId
        );

        await requestLocalService.update(request.localId, {
          serverStatus: response.status,
          processedPayload: response.processedPayload
        });

      } catch (error) {
        console.error(
          `No se pudo consultar la solicitud ${request.serverId}`,
          error
        );
      }
    }
  }
}

export const requestStatusSyncService =
  new RequestStatusSyncService();
