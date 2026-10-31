import { requestApiService } from '../../requests/services/request.api.service';
import { requestLocalService } from '../../requests/services/request.local.service';
import { requestProcessingService } from '../../requests/services/request.processing.service';
import { groupTreeService } from './group.tree.service';

import type { LocalRequest } from '../../requests/types/request.types';
import type { GroupTreeNode } from '../types/group.types';

export interface GroupSyncResult {
  total: number;
  synced: number;
  pending: number;
  failed: number;
}

export interface GroupProcessResult {
  total: number;
  processed: number;
  failed: number;
  skipped: number;
}

export class GroupActionService {

  // Obtiene el ID del grupo y todos sus descendientes.
  private collectGroupIds(
    node: GroupTreeNode,
    ids: number[]
  ): void {
    if (node.localId !== undefined) {
      ids.push(node.localId);
    }

    for (const child of node.children) {
      this.collectGroupIds(child, ids);
    }
  }

  // Recupera las solicitudes pertenecientes al grupo completo.
  private async getGroupRequests(
    groupId: number
  ): Promise<LocalRequest[]> {
    const tree = await groupTreeService.getTree();

    const findGroup = (
      nodes: GroupTreeNode[]
    ): GroupTreeNode | undefined => {
      for (const node of nodes) {
        if (node.localId === groupId) {
          return node;
        }

        const found = findGroup(node.children);

        if (found) {
          return found;
        }
      }

      return undefined;
    };

    const selectedGroup = findGroup(tree);

    if (!selectedGroup) {
      throw new Error('No existe el grupo seleccionado.');
    }

    const groupIds: number[] = [];

    this.collectGroupIds(selectedGroup, groupIds);

    const requests = await requestLocalService.getAll();

    return requests.filter(
      request =>
        request.groupId !== undefined &&
        request.groupId !== null &&
        groupIds.includes(request.groupId)
    );
  }

  // Sincroniza únicamente las solicitudes pendientes del grupo.
  async synchronizeGroup(
    groupId: number
  ): Promise<GroupSyncResult> {
    const requests = await this.getGroupRequests(groupId);

    const pendingRequests = requests.filter(
      request => request.syncStatus === 'PENDING'
    );

    const result: GroupSyncResult = {
      total: pendingRequests.length,
      synced: 0,
      pending: 0,
      failed: 0
    };

    for (const request of pendingRequests) {
      if (request.localId === undefined) {
        result.pending++;
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
      } catch (error) {
        const httpStatus = (
          error as { response?: { status?: number } }
        ).response?.status;

        const errorMessage = error instanceof Error
          ? error.message
          : 'Error desconocido durante la sincronización.';

        if (httpStatus === 400 || httpStatus === 422) {
          await requestLocalService.update(request.localId, {
            syncStatus: 'FAILED',
            lastSyncError: errorMessage
          });

          result.failed++;
        } else {
          await requestLocalService.update(request.localId, {
            lastSyncError: errorMessage
          });

          result.pending++;
        }
      }
    }

    return result;
  }

  // Procesa únicamente solicitudes sincronizadas y pendientes en el servidor.
  async processGroup(
    groupId: number
  ): Promise<GroupProcessResult> {
    const requests = await this.getGroupRequests(groupId);

    const eligibleRequests = requests.filter(
      request =>
        request.syncStatus === 'SYNCED' &&
        request.serverId !== undefined &&
        request.serverStatus === 0
    );

    const result: GroupProcessResult = {
      total: eligibleRequests.length,
      processed: 0,
      failed: 0,
      skipped: 0
    };

    for (const request of eligibleRequests) {
      if (request.localId === undefined || !request.serverId) {
        result.skipped++;
        continue;
      }

      try {
        await requestProcessingService.process(request.localId);
        result.processed++;
      } catch (error) {
        // El backend puede marcar la solicitud como Failed y responder 400.
        // Consultamos el estado real para reflejarlo localmente.
        try {
          const serverRequest = await requestApiService.getById(
            request.serverId
          );

          await requestLocalService.update(request.localId, {
            serverStatus: serverRequest.status,
            processedPayload: serverRequest.processedPayload,
            lastSyncError:
              serverRequest.status === 2 && error instanceof Error
                ? error.message
                : undefined
          });

          if (serverRequest.status === 2) {
            result.failed++;
          } else if (serverRequest.status === 1) {
            result.processed++;
          } else {
            result.skipped++;
          }
        } catch {
          result.failed++;
        }
      }
    }

    return result;
  }
}

export const groupActionService = new GroupActionService();
