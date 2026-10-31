import { database } from '../../../infrastructure/database/app.database';
import type { LocalGroup } from '../types/group.types';

export class RequestGroupService {

  // Crear un grupo o subgrupo
  async create(
    name: string,
    parentGroupId: number | null = null
  ): Promise<number> {

    if (!name.trim()) {
      throw new Error('El nombre del grupo es obligatorio.');
    }

    if (parentGroupId !== null) {
      const parent = await database.groups.get(parentGroupId);

      if (!parent) {
        throw new Error('No existe el grupo padre.');
      }
    }

    const group: LocalGroup = {
      name: name.trim(),
      parentGroupId,
      createdAt: new Date().toISOString()
    };

    return database.groups.add(group);
  }

  // Consultar todos los grupos
  async getAll(): Promise<LocalGroup[]> {
    return database.groups.orderBy('createdAt').toArray();
  }

  // Consultar un grupo
  async getById(id: number): Promise<LocalGroup | undefined> {
    return database.groups.get(id);
  }

  // Renombrar un grupo
  async rename(id: number, name: string): Promise<number> {
    if (!name.trim()) {
      throw new Error('El nombre del grupo es obligatorio.');
    }

    const group = await database.groups.get(id);

    if (!group) {
      throw new Error('No existe el grupo.');
    }

    return database.groups.update(id, {
      name: name.trim()
    });
  }

  // Asignar una solicitud a un grupo
  async addRequest(
    groupId: number,
    requestId: number
  ): Promise<number> {

    const group = await database.groups.get(groupId);
    const request = await database.requests.get(requestId);

    if (!group) {
      throw new Error('No existe el grupo.');
    }

    if (!request) {
      throw new Error('No existe la solicitud.');
    }

    return database.requests.update(requestId, {
      groupId
    });
  }

  // Quitar una solicitud de su grupo
  async removeRequest(requestId: number): Promise<number> {
    const request = await database.requests.get(requestId);

    if (!request) {
      throw new Error('No existe la solicitud.');
    }

    return database.requests.update(requestId, {
      groupId: null
    });
  }

  // Eliminar un grupo solamente si está vacío
  async delete(id: number): Promise<void> {
    await database.transaction(
      'rw',
      database.groups,
      database.requests,
      async () => {
        const children = await database.groups
          .where('parentGroupId')
          .equals(id)
          .count();

        const requests = await database.requests
          .where('groupId')
          .equals(id)
          .count();

        if (children > 0 || requests > 0) {
          throw new Error(
            'No puedes eliminar un grupo que contiene subgrupos o solicitudes.'
          );
        }

        await database.groups.delete(id);
      }
    );
  }
}

export const requestGroupService = new RequestGroupService();
