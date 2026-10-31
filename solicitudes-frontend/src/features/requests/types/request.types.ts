export type RequestType = 'TEXT' | 'JSON';

export type SyncStatus = 'PENDING' | 'SYNCED' | 'FAILED';

export interface LocalRequest {
  localId?: number;
  clientRequestId: string;
  serverId?: string;

  name: string;
  payload: string;
  type: RequestType;

  syncStatus: SyncStatus;
  serverStatus?: number;
  processedPayload?: string | null;

  // Grupo al que pertenece la solicitud
  groupId?: number | null;

  createdAt: string;
  syncedAt?: string;
  lastSyncError?: string;
}
