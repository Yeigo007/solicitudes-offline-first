
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

import './App.css';

type RequestTypeOption = 'TEXT' | 'JSON';

interface GroupNodeProps {
  group: GroupTreeNode;
  onRename: (id: number, name: string) => void;
  onDelete: (id: number) => void;
  onSync: (id: number) => void;
  onProcess: (id: number) => void;
}

function GroupNode({
  group,
  onRename,
  onDelete,
  onSync,
  onProcess
}: GroupNodeProps) {
  return (
    <div className="group-node">
      <div className="group-heading">
        <div>
          <span className="eyebrow">GRUPO</span>
          <strong>{group.name}</strong>
        </div>
        <span className="count-pill">
          {group.totalRequestCount} solicitudes
        </span>
      </div>

      <div className="group-meta">
        <span>
          Solicitudes directas: <strong>{group.directRequestCount}</strong>
        </span>
        <span>
          Total con subgrupos: <strong>{group.totalRequestCount}</strong>
        </span>
      </div>

      <div className="group-actions">
        <button
          type="button"
          className="button button-light"
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

        <button
          type="button"
          className="button button-danger-light"
          onClick={() => {
            if (group.localId !== undefined) {
              onDelete(group.localId);
            }
          }}
        >
          Eliminar
        </button>

        <button
          type="button"
          className="button button-light"
          onClick={() => {
            if (group.localId !== undefined) {
              onSync(group.localId);
            }
          }}
        >
          Sincronizar grupo
        </button>

        <button
          type="button"
          className="button button-success-light"
          onClick={() => {
            if (group.localId !== undefined) {
              onProcess(group.localId);
            }
          }}
        >
          Procesar grupo
        </button>
      </div>

      {group.children.length > 0 && (
        <div className="group-children">
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
  const [requestType, setRequestType] =
    useState<RequestTypeOption>('TEXT');

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
      window.alert('Completa el nombre y el contenido de la solicitud.');
      return;
    }

    // Validar JSON antes de guardarlo localmente
    if (requestType === 'JSON') {
      try {
        JSON.parse(payload);
      } catch {
        window.alert('El contenido no es un JSON válido.');
        return;
      }
    }

    try {
      await requestLocalService.create(
        name.trim(),
        payload,
        requestType
      );

      setName('');
      setPayload('');
      setRequestType('TEXT');

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
      window.alert('Escribe el nombre del grupo.');
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

  // Sincronizar un grupo y todos sus descendientes
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

  // Procesar un grupo y todos sus descendientes
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

  function getStatusLabel(status: string) {
    switch (status) {
      case 'PENDING':
        return 'Pendiente';
      case 'SYNCED':
        return 'Sincronizada';
      case 'FAILED':
        return 'Fallida';
      default:
        return status;
    }
  }

  return (
    <main className="app-shell">

      {/* ENCABEZADO */}
      <header className="app-header">
        <div>
          <span className="eyebrow">PLATAFORMA DE GESTIÓN</span>
          <h1>Gestión de solicitudes</h1>
          <p>
            Administración, procesamiento y sincronización offline-first
          </p>
        </div>

        <span className="header-tag">
          Sistema Offline-First
        </span>
      </header>

      {/* CREACIÓN DE SOLICITUDES */}
      <section className="section-card">
        <div className="section-title">
          <div>
            <h2>Nueva solicitud</h2>
            <p className="section-description">
              Registra solicitudes localmente y sincronízalas cuando tengas conexión.
            </p>
          </div>
        </div>

        <div className="form-grid">
          <div className="form-group">
            <label htmlFor="request-name">
              Nombre de la solicitud
            </label>

            <input
              id="request-name"
              placeholder="Ej. Actualización de información"
              value={name}
              onChange={event => setName(event.target.value)}
            />
          </div>

          <div className="form-group">
            <label htmlFor="request-type">
              Tipo de solicitud
            </label>

            <select
              id="request-type"
              value={requestType}
              onChange={event =>
                setRequestType(event.target.value as RequestTypeOption)
              }
            >
              <option value="TEXT">Texto (TEXT)</option>
              <option value="JSON">Estructura JSON</option>
            </select>
          </div>

          <div className="form-group full">
            <label htmlFor="request-payload">
              Contenido
            </label>

            <textarea
              id="request-payload"
              placeholder={
                requestType === 'JSON'
                  ? '{\n  "nombre": "Carlos",\n  "edad": 30,\n  "activo": true\n}'
                  : 'Escribe aquí el contenido de la solicitud...'
              }
              value={payload}
              onChange={event => setPayload(event.target.value)}
            />

            <p className="subtle-text">
              {requestType === 'JSON'
                ? 'Se validará la estructura JSON antes de guardar.'
                : 'El contenido se procesará en el backend según las reglas configuradas.'}
            </p>
          </div>
        </div>

        <button
          type="button"
          className="button button-primary"
          onClick={() => void createRequest()}
        >
          + Guardar localmente
        </button>
      </section>

      {/* SINCRONIZACIÓN GENERAL */}
      <section className="section-card">
        <div className="section-title">
          <div>
            <h2>Estado de sincronización</h2>
            <p className="section-description">
              Resumen del estado de tus solicitudes.
            </p>
          </div>
        </div>

        <div className="stats-grid">
          <div className="stat-card pending">
            <span>Solicitudes pendientes</span>
            <strong>{pendingCount}</strong>
          </div>

          <div className="stat-card synced">
            <span>Solicitudes sincronizadas</span>
            <strong>{syncedCount}</strong>
          </div>

          <div className="stat-card failed">
            <span>Solicitudes fallidas</span>
            <strong>{failedCount}</strong>
          </div>
        </div>

        <div className="actions-row">
          <button
            type="button"
            className="button button-primary"
            onClick={() => void synchronizeRequests()}
            disabled={isSyncing || pendingCount === 0}
          >
            {isSyncing ? 'Sincronizando...' : 'Sincronizar todas'}
          </button>

          <button
            type="button"
            className="button"
            onClick={() => void refreshServerStatuses()}
          >
            Actualizar estados del servidor
          </button>

          <button
            type="button"
            className="button button-success-light"
            onClick={() => void processPendingRequests()}
          >
            Procesar todas las pendientes
          </button>
        </div>

        {syncMessage && (
          <p className="status-message" role="status">
            {syncMessage}
          </p>
        )}
      </section>

      {/* ADMINISTRACIÓN DE GRUPOS */}
      <section className="section-card">
        <div className="section-title">
          <div>
            <h2>Grupos y subgrupos</h2>
            <p className="section-description">
              Organiza tus solicitudes en estructuras jerárquicas.
            </p>
          </div>
        </div>

        <div className="form-grid">
          <div className="form-group">
            <label htmlFor="group-name">
              Nombre del grupo
            </label>

            <input
              id="group-name"
              placeholder="Ej. Solicitudes comerciales"
              value={groupName}
              onChange={event => setGroupName(event.target.value)}
            />
          </div>

          <div className="form-group">
            <label htmlFor="parent-group">
              Grupo padre
            </label>

            <select
              id="parent-group"
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
          </div>
        </div>

        <button
          type="button"
          className="button button-primary"
          onClick={() => void createGroup()}
        >
          + Crear grupo
        </button>

        <div className="groups-container">
          <h3>Estructura de grupos</h3>

          {groups.length === 0 ? (
            <div className="empty-state">
              No existen grupos registrados.
              Crea uno para comenzar a organizar tus solicitudes.
            </div>
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
        </div>
      </section>

      {/* LISTADO DE SOLICITUDES */}
      <section className="section-card">
        <div className="section-title">
          <div>
            <h2>Solicitudes registradas</h2>
            <p className="section-description">
              Consulta el contenido y administra el estado de cada solicitud.
            </p>
          </div>

          <span className="count-pill">
            {requests.length} en total
          </span>
        </div>

        {requests.length === 0 ? (
          <div className="empty-state">
            No hay solicitudes registradas.
          </div>
        ) : (
          <div className="request-list">
            {requests.map(request => (
              <article
                key={request.localId}
                className="request-card"
              >
                <div className="request-top">
                  <div>
                    <span className="eyebrow">
                      {request.type ?? 'SOLICITUD'}
                    </span>

                    <h3 className="request-name">
                      {request.name}
                    </h3>
                  </div>

                  <span
                    className={`status-badge ${request.syncStatus.toLowerCase()}`}
                  >
                    {getStatusLabel(request.syncStatus)}
                  </span>
                </div>

                <pre className="request-content">
                  {request.payload}
                </pre>

                <div className="request-details">
                  <span>
                    Estado del servidor:{' '}
                    <strong>
                      {request.serverStatus === 0
                        ? 'Pendiente'
                        : request.serverStatus === 1
                          ? 'Procesada'
                          : request.serverStatus === 2
                            ? 'Fallida'
                            : 'No sincronizada'}
                    </strong>
                  </span>

                  {request.serverId && (
                    <span>
                      ID servidor: <strong>{request.serverId}</strong>
                    </span>
                  )}
                </div>

                <div className="form-group group-select">
                  <label htmlFor={`group-${request.localId}`}>
                    Grupo asignado
                  </label>

                  <select
                    id={`group-${request.localId}`}
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
                </div>

                {request.syncStatus === 'SYNCED' &&
                  request.serverStatus === 0 && (
                    <button
                      type="button"
                      className="button button-success-light"
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
                  <p className="result-success">
                    Solicitud procesada correctamente.
                  </p>
                )}

                {request.serverStatus === 2 && (
                  <p className="result-error">
                    El procesamiento de esta solicitud falló.
                  </p>
                )}

                {request.processedPayload && (
                  <div className="server-result">
                    <strong>Resultado del procesamiento</strong>
                    <pre>{request.processedPayload}</pre>
                  </div>
                )}
              </article>
            ))}
          </div>
        )}
      </section>

      <footer className="app-footer">
        Gestión de solicitudes · Arquitectura Offline-First
      </footer>
    </main>
  );
}

export default App;
