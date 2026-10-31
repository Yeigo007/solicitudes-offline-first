
import Dexie, { type Table } from 'dexie';

import type { LocalRequest } from '../../features/requests/types/request.types';
import type { LocalGroup } from '../../features/groups/types/group.types';

class AppDatabase extends Dexie {
  requests!: Table<LocalRequest, number>;
  groups!: Table<LocalGroup, number>;

  constructor() {
    super('SolicitudesOfflineDB');

    // Versión 1: almacenamiento inicial
    this.version(1).stores({
      requests: '++localId, serverId, syncStatus, createdAt'
    });

    // Versión 2: identificador único para sincronización
    this.version(2).stores({
      requests: '++localId, clientRequestId, serverId, syncStatus, createdAt'
    });

    // Versión 3: grupos y relación con solicitudes
    this.version(3).stores({
      requests: '++localId, clientRequestId, serverId, syncStatus, createdAt, groupId',
      groups: '++localId, parentGroupId, name, createdAt'
    });
  }
}

export const database = new AppDatabase();
