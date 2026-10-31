
import { useCallback, useEffect, useState } from 'react';

import { requestLocalService } from './features/requests/services/request.local.service';
import { requestSyncService } from './features/requests/services/request.sync.service';
import { requestStatusSyncService } from './features/requests/services/request.status.sync.service';
import { requestProcessingService } from './features/requests/services/request.processing.service';
import { requestBatchProcessingService } from './features/requests/services/request.batch.processing.service';

import { requestGroupService } from './features/groups/services/request.group.service';
import { groupTreeService } from './features/groups/services/group.tree.service';
import { groupActionService } from './features/groups/services/group.action.service';

import type { LocalRequest } from './features/requests/types/request.types';
import type { GroupTreeNode } from './features/groups/types/group.types';

// Propiedades del componente de grupos
interface GroupNodeProps {
  group: GroupTreeNode;
  onRename: (id: number, name: string) => void;
  onDelete: (id: number) => void;
  onSync: (id: number) => void;
  onProcess: (id: number) => void;
}

// Componente recursivo para representar grupos y subgrupos
function GroupNode({
  group,
  onRename,
  onDelete,
  onSync,
  onProcess
}: GroupNodeProps) {
  return (
    <div
      style={{
        border: '1px solid #ccc',
        borderRadius: '6px',
        padding: '12px',
        marginBottom: '10px',
        marginLeft: '20px'
      }}
    >
      <strong>{group.name}</strong>

      <p>
        Solicitudes directas: {group.directRequestCount}
      </p>

      <p>
        Total incluyendo subgrupos:{' '}
        <strong>{group.totalRequestCount}</strong>
      </p>

      <button
        onClick={() => {
          const newName = window.prompt(
            'Nuevo nombre del grupo:',
            group.name
          );

          if (newName?.trim() && group.localId !== undefined) {
            onRename(group.localId, newName);
          }
        }}
      >
        Renombrar
      </button>

      {' '}

      <button
        onClick={() => {
          if (group.localId !== undefined) {
            onDelete(group.localId);
          }
        }}
      >
        Eliminar
      </button>

      <br /><br />

      <button
        onClick={() => {
          if (group.localId !== undefined) {
            onSync(group.localId);
          }
        }}
      >
        Sincronizar grupo
      </button>

      {' '}

      <button
        onClick={() => {
          if (group.localId !== undefined) {
            onProcess(group.localId);
          }
        }}
      >
        Procesar grupo
      </button>

      {group.children.length > 0 && (
        <div style={{ marginTop: '12px' }}>
          {group.children.map(child => (
            <GroupNode
              key={child.localId}
              group={child}
              onRename={onRename}
              onDelete={onDelete}
              onSync={onSync}
              onProcess={onProcess}
            />
          ))}
        </div>
      )}
    </div>
  );
}

function App() {
  const [requests, setRequests] = useState<LocalRequest[]>([]);
  const [groups, setGroups] = useState<GroupTreeNode[]>([]);

  const [name, setName] = useState('');
  const [payload, setPayload] = useState('');

  const [groupName, setGroupName] = useState('');
  const [parentGroupId, setParentGroupId] = useState('');

  const [isSyncing, setIsSyncing] = useState(false);
  const [syncMessage, setSyncMessage] = useState('');

  // Cargar solicitudes y grupos
  const loadData = useCallback(async () => {
    const [requestData, groupData] = await Promise.all([
      requestLocalService.getAll(),
      groupTreeService.getTree()
    ]);

    setRequests(requestData);
    setGroups(groupData);
  }, []);

  // Crear solicitud local
  async function createRequest() {
    if (!name.trim() || !payload.trim()) {
      alert('Completa todos los campos');
      return;
    }

    try {
      await requestLocalService.create(name, payload, 'TEXT');

      setName('');
      setPayload('');

      await loadData();
      setSyncMessage('Solicitud guardada localmente.');
    } catch (error) {
      console.error('Error creando solicitud:', error);
      setSyncMessage('No fue posible guardar la solicitud.');
    }
  }

  // Crear grupo o subgrupo
  async function createGroup() {
    if (!groupName.trim()) {
      alert('Escribe el nombre del grupo');
      return;
    }

    try {
      const parentId = parentGroupId
        ? Number(parentGroupId)
        : null;

      await requestGroupService.create(groupName, parentId);

      setGroupName('');
      setParentGroupId('');

      await loadData();

      setSyncMessage('Grupo creado correctamente.');
    } catch (error) {
      setSyncMessage(
        error instanceof Error
          ? error.message
          : 'No fue posible crear el grupo.'
      );
    }
  }

  // Renombrar grupo
  async function renameGroup(id: number, newName: string) {
    try {
      await requestGroupService.rename(id, newName);
      await loadData();

      setSyncMessage('Grupo actualizado correctamente.');
    } catch (error) {
      setSyncMessage(
        error instanceof Error
          ? error.message
          : 'No fue posible actualizar el grupo.'
      );
    }
  }

  // Eliminar grupo
  async function deleteGroup(id: number) {
    const confirmed = window.confirm(
      '¿Estás seguro de eliminar este grupo?'
    );

    if (!confirmed) return;

    try {
      await requestGroupService.delete(id);
      await loadData();

      setSyncMessage('Grupo eliminado correctamente.');
    } catch (error) {
      setSyncMessage(
        error instanceof Error
          ? error.message
          : 'No fue posible eliminar el grupo.'
      );
    }
  }

  // Asignar solicitud a un grupo
  async function assignRequest(
    requestId: number,
    groupId: string
  ) {
    try {
      if (groupId) {
        await requestGroupService.addRequest(
          Number(groupId),
          requestId
        );
      } else {
        await requestGroupService.removeRequest(requestId);
      }

      await loadData();
      setSyncMessage('Asignación actualizada correctamente.');
    } catch (error) {
      setSyncMessage(
        error instanceof Error
          ? error.message
          : 'No fue posible asignar la solicitud.'
      );
    }
  }

  // Sincronizar todas las solicitudes pendientes
  async function synchronizeRequests() {
    if (isSyncing) return;

    setIsSyncing(true);
    setSyncMessage('');

    try {
      const result = await requestSyncService.synchronize();

      setSyncMessage(
        `Sincronización finalizada: ${result.synced} enviadas, ` +
        `${result.pending} pendientes y ${result.failed} fallidas.`
      );

      await loadData();
    } catch (error) {
      console.error('Error durante la sincronización:', error);
      setSyncMessage('No fue posible completar la sincronización.');
    } finally {
      setIsSyncing(false);
    }
  }

  // Actualizar estados desde el servidor
  async function refreshServerStatuses() {
    try {
      await requestStatusSyncService.synchronizeStatuses();
      await loadData();

      setSyncMessage('Estados actualizados correctamente.');
    } catch (error) {
      console.error('Error actualizando estados:', error);
      setSyncMessage('No fue posible actualizar los estados.');
    }
  }

  // Procesar una solicitud individual
  async function processRequest(localId: number) {
    try {
      await requestProcessingService.process(localId);

      await loadData();

      setSyncMessage('Solicitud procesada correctamente.');
    } catch (error) {
      console.error('Error procesando solicitud:', error);

      setSyncMessage(
        error instanceof Error
          ? error.message
          : 'No fue posible procesar la solicitud.'
      );

      await loadData();
    }
  }

  // Procesar todas las solicitudes pendientes del servidor
  async function processPendingRequests() {
    try {
      const result =
        await requestBatchProcessingService.processPending();

      await loadData();

      setSyncMessage(
        `Procesamiento por lotes: ${result.processed} procesadas, ` +
        `${result.failed} fallidas de ${result.total} solicitudes.`
      );
    } catch (error) {
      console.error('Error en procesamiento por lotes:', error);
      setSyncMessage(
        'No fue posible procesar las solicitudes pendientes.'
      );
    }
  }

  // Sincronizar solamente un grupo y todos sus descendientes
  async function synchronizeGroup(groupId: number) {
    try {
      const result =
        await groupActionService.synchronizeGroup(groupId);

      await loadData();

      setSyncMessage(
        `Sincronización del grupo: ${result.synced} enviadas, ` +
        `${result.pending} pendientes y ${result.failed} fallidas.`
      );
    } catch (error) {
      console.error('Error sincronizando grupo:', error);

      setSyncMessage(
        error instanceof Error
          ? error.message
          : 'No fue posible sincronizar el grupo.'
      );
    }
  }

  // Procesar solamente un grupo y todos sus descendientes
  async function processGroup(groupId: number) {
    try {
      const result =
        await groupActionService.processGroup(groupId);

      await loadData();

      setSyncMessage(
        `Procesamiento del grupo: ${result.processed} procesadas, ` +
        `${result.failed} fallidas y ${result.skipped} omitidas.`
      );
    } catch (error) {
      console.error('Error procesando grupo:', error);

      setSyncMessage(
        error instanceof Error
          ? error.message
          : 'No fue posible procesar el grupo.'
      );
    }
  }

  // Carga inicial
  useEffect(() => {
    void loadData();
  }, [loadData]);

  // Contadores generales
  const pendingCount = requests.filter(
    request => request.syncStatus === 'PENDING'
  ).length;

  const syncedCount = requests.filter(
    request => request.syncStatus === 'SYNCED'
  ).length;

  const failedCount = requests.filter(
    request => request.syncStatus === 'FAILED'
  ).length;

  // Aplanar el árbol para los select de grupos
  function flattenGroups(
    nodes: GroupTreeNode[]
  ): GroupTreeNode[] {
    return nodes.flatMap(node => [
      node,
      ...flattenGroups(node.children)
    ]);
  }

  const allGroups = flattenGroups(groups);

  return (
    <main style={{ padding: '30px', fontFamily: 'Arial' }}>
      <h1>Gestión de solicitudes</h1>

      {/* CREACIÓN DE SOLICITUDES */}
      <section>
        <h2>Nueva solicitud</h2>

        <input
          placeholder="Nombre"
          value={name}
          onChange={event => setName(event.target.value)}
        />

        <br /><br />

        <textarea
          placeholder="Contenido"
          value={payload}
          onChange={event => setPayload(event.target.value)}
        />

        <br /><br />

        <button onClick={() => void createRequest()}>
          Guardar localmente
        </button>
      </section>

      <hr />

      {/* ADMINISTRACIÓN DE GRUPOS */}
      <section>
        <h2>Grupos y subgrupos</h2>

        <h3>Crear grupo</h3>

        <input
          placeholder="Nombre del grupo"
          value={groupName}
          onChange={event => setGroupName(event.target.value)}
        />

        <br /><br />

        <label>Grupo padre:</label>

        <br />

        <select
          value={parentGroupId}
          onChange={event => setParentGroupId(event.target.value)}
        >
          <option value="">
            Crear como grupo principal
          </option>

          {allGroups.map(group => (
            <option
              key={group.localId}
              value={group.localId}
            >
              {group.name}
            </option>
          ))}
        </select>

        <br /><br />

        <button onClick={() => void createGroup()}>
          Crear grupo
        </button>

        <h3>Estructura de grupos</h3>

        {groups.length === 0 ? (
          <p>No existen grupos registrados.</p>
        ) : (
          groups.map(group => (
            <GroupNode
              key={group.localId}
              group={group}
              onRename={(id, newName) => {
                void renameGroup(id, newName);
              }}
              onDelete={id => {
                void deleteGroup(id);
              }}
              onSync={id => {
                void synchronizeGroup(id);
              }}
              onProcess={id => {
                void processGroup(id);
              }}
            />
          ))
        )}
      </section>

      <hr />

      {/* SINCRONIZACIÓN GENERAL */}
      <section>
        <h2>Estado de sincronización</h2>

        <p>Solicitudes pendientes: {pendingCount}</p>
        <p>Solicitudes sincronizadas: {syncedCount}</p>
        <p>Solicitudes fallidas: {failedCount}</p>

        <button
          onClick={() => void synchronizeRequests()}
          disabled={isSyncing || pendingCount === 0}
        >
          {isSyncing ? 'Sincronizando...' : 'Sincronizar todas'}
        </button>

        <br /><br />

        <button onClick={() => void refreshServerStatuses()}>
          Actualizar estados del servidor
        </button>

        <br /><br />

        <button onClick={() => void processPendingRequests()}>
          Procesar todas las pendientes
        </button>

        {syncMessage && (
          <p role="status">{syncMessage}</p>
        )}
      </section>

      <hr />

      {/* LISTADO DE SOLICITUDES */}
      <section>
        <h2>Solicitudes guardadas: {requests.length}</h2>

        {requests.map(request => (
          <article
            key={request.localId}
            style={{
              border: '1px solid #ccc',
              borderRadius: '6px',
              padding: '12px',
              marginBottom: '10px'
            }}
          >
            <strong>{request.name}</strong>

            <p>{request.payload}</p>

            <p>
              Sincronización:{' '}
              <strong>{request.syncStatus}</strong>
            </p>

            {request.serverId && (
              <small>ID servidor: {request.serverId}</small>
            )}

            <br /><br />

            <label>Grupo asignado:</label>

            <br />

            <select
              value={request.groupId ?? ''}
              onChange={event => {
                if (request.localId !== undefined) {
                  void assignRequest(
                    request.localId,
                    event.target.value
                  );
                }
              }}
            >
              <option value="">Sin grupo</option>

              {allGroups.map(group => (
                <option
                  key={group.localId}
                  value={group.localId}
                >
                  {group.name}
                </option>
              ))}
            </select>

            <br /><br />

            {request.syncStatus === 'SYNCED' &&
              request.serverStatus === 0 && (
                <button
                  onClick={() => {
                    if (request.localId !== undefined) {
                      void processRequest(request.localId);
                    }
                  }}
                >
                  Procesar solicitud
                </button>
              )}

            {request.serverStatus === 1 && (
              <p>Procesada correctamente</p>
            )}

            {request.serverStatus === 2 && (
              <p>El procesamiento falló</p>
            )}

            {request.processedPayload && (
              <div>
                <strong>Resultado del procesamiento:</strong>
                <pre>{request.processedPayload}</pre>
              </div>
            )}
          </article>
        ))}
      </section>
    </main>
  );
}

export default App;
