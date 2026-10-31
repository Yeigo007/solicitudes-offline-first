
import { database } from '../../../infrastructure/database/app.database';
import type {
  LocalRequest,
  RequestType
} from '../types/request.types';

export class RequestLocalService {

  // Crear una solicitud local
  async create(
    name: string,
    payload: string,
    type: RequestType
  ): Promise<number> {

    const request: LocalRequest = {
      clientRequestId: crypto.randomUUID(),
      name,
      payload,
      type,
      syncStatus: 'PENDING',
      createdAt: new Date().toISOString()
    };

    return await database.requests.add(request);
  }

  // Consultar todas las solicitudes
  async getAll(): Promise<LocalRequest[]> {
    return await database.requests
      .orderBy('createdAt')
      .reverse()
      .toArray();
  }

  // Consultar por identificador local
  async getById(
    localId: number
  ): Promise<LocalRequest | undefined> {
    return await database.requests.get(localId);
  }

  // Consultar solicitudes pendientes
  async getPending(): Promise<LocalRequest[]> {
    return await database.requests
      .where('syncStatus')
      .equals('PENDING')
      .toArray();
  }

  // Actualizar una solicitud local
  async update(
    localId: number,
    changes: Partial<LocalRequest>
  ): Promise<number> {
    return await database.requests.update(localId, changes);
  }
}

export const requestLocalService = new RequestLocalService();
