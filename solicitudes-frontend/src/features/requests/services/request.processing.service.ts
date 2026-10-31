
import { requestApiService } from './request.api.service';
import { requestLocalService } from './request.local.service';

export class RequestProcessingService {

  async process(localId: number): Promise<void> {
    const localRequest = await requestLocalService.getById(localId);

    if (!localRequest) {
      throw new Error('No se encontró la solicitud local.');
    }

    if (localRequest.syncStatus !== 'SYNCED' || !localRequest.serverId) {
      throw new Error('La solicitud debe estar sincronizada antes de procesarse.');
    }

    const response = await requestApiService.process(localRequest.serverId);

    await requestLocalService.update(localId, {
      serverStatus: response.status,
      processedPayload: response.processedPayload
    });
  }
}

export const requestProcessingService =
  new RequestProcessingService();
