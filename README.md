# Sistema de Gestión de Solicitudes Offline-First

## 1. Descripción del proyecto

Aplicación Fullstack para la gestión de solicitudes bajo un enfoque **Offline-First**. Permite registrar solicitudes localmente, conservarlas aunque se cierre el navegador y sincronizarlas posteriormente con un backend centralizado cuando exista conexión.

La solución está dividida en dos servicios independientes:

- **Frontend:** aplicación web desarrollada con React y TypeScript, con persistencia local mediante IndexedDB.
- **Backend:** API REST desarrollada con ASP.NET Core y .NET 10, encargada de recibir, almacenar, consultar y procesar las solicitudes.

La arquitectura está organizada para facilitar la incorporación de nuevos tipos de solicitudes y reglas de procesamiento sin modificar la estructura principal del sistema.

## 2. Funcionalidades implementadas

### Gestión de solicitudes

- Crear solicitudes y almacenarlas localmente.
- Consultar las solicitudes registradas.
- Identificar cuáles están pendientes de sincronización, sincronizadas o fallidas.
- Consultar el estado de procesamiento en el backend.
- Consultar el resultado del procesamiento.

### Funcionamiento Offline-First

- Las solicitudes se guardan inicialmente en IndexedDB.
- Cada solicitud recibe un identificador único de cliente (`clientRequestId`).
- La información permanece disponible al cerrar y volver a abrir el navegador.
- Las solicitudes pendientes pueden sincronizarse cuando se recupera la conexión.
- La sincronización actualiza el estado local según la respuesta del backend.

### Procesamiento por tipo

El backend permite procesar solicitudes según su tipo:

- **TEXT:** transforma el contenido a mayúsculas.
- **JSON:** valida y procesa el contenido JSON, conservando una representación estructurada y formateada.

La separación mediante procesadores permite incorporar nuevos tipos de procesamiento sin concentrar todas las reglas en un único componente.

### Agrupación de solicitudes

- Crear grupos y subgrupos.
- Asociar solicitudes a grupos.
- Consultar la estructura jerárquica.
- Obtener el total de solicitudes contenidas en un grupo, incluyendo sus subgrupos.
- Renombrar y eliminar grupos cuando cumplen las condiciones establecidas.
- Sincronizar y procesar solicitudes de un grupo y sus descendientes.

### Backend

- Recibir solicitudes desde el frontend.
- Registrar las solicitudes en PostgreSQL.
- Devolver confirmación de recepción.
- Consultar solicitudes individuales y listados.
- Procesar solicitudes individualmente o por lotes.
- Mantener los estados de procesamiento: pendiente, procesada y fallida.
- Controlar solicitudes duplicadas mediante `clientRequestId`.

## 3. Arquitectura de la solución

### Backend – Clean Architecture

La solución backend está organizada en los siguientes proyectos:

| Proyecto | Responsabilidad |
|---|---|
| `Solicitudes.Domain` | Entidades, enumeraciones y reglas fundamentales del dominio. |
| `Solicitudes.Application` | Casos de uso, contratos, validaciones y lógica de aplicación. |
| `Solicitudes.Infrastructure` | Persistencia, Entity Framework Core, PostgreSQL y repositorios. |
| `Solicitudes.Api` | Endpoints HTTP, configuración de la API y manejo de excepciones. |
| `Solicitudes.Tests` | Pruebas unitarias de dominio y casos de uso. |

Esta separación permite mantener las reglas de negocio independientes de los detalles de infraestructura y transporte.

### Frontend

El frontend organiza sus responsabilidades en servicios y componentes:

- **Presentación:** interfaz de gestión de solicitudes y grupos.
- **Servicios de aplicación:** creación, sincronización, procesamiento y administración de grupos.
- **Infraestructura:** cliente HTTP y base de datos local.
- **Persistencia local:** IndexedDB mediante Dexie.

## 4. Tecnologías utilizadas

| Tecnología | Uso |
|---|---|
| C# | Lenguaje del backend. |
| .NET 10 / ASP.NET Core | Construcción de la API REST. |
| Entity Framework Core | Acceso y persistencia de datos. |
| PostgreSQL | Base de datos centralizada. |
| Npgsql | Proveedor de PostgreSQL para Entity Framework Core. |
| FluentValidation | Validación de solicitudes. |
| React | Construcción de la interfaz web. |
| TypeScript | Tipado estático del frontend. |
| Node.js y npm | Entorno de ejecución y administración de dependencias del frontend. |
| Vite | Herramienta de desarrollo y compilación del frontend. |
| Axios | Comunicación HTTP con el backend. |
| Dexie / IndexedDB | Persistencia local de solicitudes y grupos. |
| xUnit | Pruebas unitarias del backend. |
| Moq | Simulación de dependencias en pruebas. |

**Nota sobre la versión de .NET:** el enunciado de la evaluación solicita .NET 8. La implementación de este proyecto se desarrolló con .NET 10, por decisión tomada durante el desarrollo.

## 5. Requisitos previos

Para ejecutar el proyecto localmente se requiere:

- Windows 10 u 11.
- .NET SDK 10.0.
- Node.js y npm.
- PostgreSQL.
- Visual Studio Code u otro editor compatible.

Verificar las instalaciones desde una terminal:

```bash
dotnet --version
node --version
npm --version
```

## 6. Configuración de la base de datos

El backend utiliza PostgreSQL como almacenamiento centralizado.

Crear una base de datos denominada:

```sql
CREATE DATABASE "SolicitudesDb";
```

Configurar la cadena de conexión en el archivo:

`Solicitudes.Api/appsettings.Development.json`

Ejemplo:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=SolicitudesDb;Username=postgres;Password=TU_PASSWORD"
  }
}
```

Reemplazar `TU_PASSWORD` por la contraseña configurada para el usuario de PostgreSQL.

No publicar contraseñas reales ni cadenas de conexión privadas en el repositorio.

## 7. Ejecución del backend

Ubicarse en la carpeta raíz de la solución:

```bash
cd C:\Proyectos
```

Restaurar las dependencias:

```bash
dotnet restore Solicitudes.sln
```

Compilar la solución:

```bash
dotnet build Solicitudes.sln
```

Aplicar las migraciones para crear o actualizar la estructura de la base de datos:

```bash
dotnet ef database update --project Solicitudes.Infrastructure --startup-project Solicitudes.Api
```

Ejecutar la API:

```bash
dotnet run --project Solicitudes.Api --urls http://localhost:5000
```

La API estará disponible en:

```text
http://localhost:5000
```

El backend se ejecuta de manera independiente del frontend.

## 8. Ejecución del frontend

Abrir una segunda terminal y ubicarse en la carpeta del frontend:

```bash
cd C:\Proyectos\solicitudes-frontend
```

Instalar las dependencias:

```bash
npm install
```

Crear el archivo `.env` en la raíz del frontend:

```env
VITE_API_URL=http://localhost:5000/api
```

Iniciar el servidor de desarrollo:

```bash
npm run dev
```

Vite mostrará la dirección local de la aplicación. Por defecto:

```text
http://localhost:5173
```

El frontend y el backend deben estar ejecutándose para realizar operaciones que requieran comunicación con el servidor. La creación y consulta de información local no dependen de que el backend esté disponible.

## 9. Variables y configuraciones necesarias

### Backend

| Configuración | Descripción | Ejemplo |
|---|---|---|
| `ConnectionStrings:DefaultConnection` | Cadena de conexión a PostgreSQL. | `Host=localhost;Port=5432;Database=SolicitudesDb;Username=postgres;Password=...` |
| URL de ejecución | Dirección y puerto de la API. | `http://localhost:5000` |
| `ASPNETCORE_ENVIRONMENT` | Entorno de ejecución de ASP.NET Core. | `Development` |

La cadena de conexión debe ajustarse al servidor, usuario, contraseña y puerto de PostgreSQL de cada entorno.

### Frontend

| Variable | Descripción | Ejemplo |
|---|---|---|
| `VITE_API_URL` | URL base de la API REST. | `http://localhost:5000/api` |

El frontend utiliza esta variable para configurar el cliente HTTP de Axios.

## 10. Endpoints principales de la API

La API expone las siguientes operaciones:

| Método | Endpoint | Descripción |
|---|---|---|
| `POST` | `/api/requests` | Crear y registrar una solicitud. |
| `GET` | `/api/requests` | Consultar todas las solicitudes. |
| `GET` | `/api/requests/{id}` | Consultar una solicitud por identificador. |
| `POST` | `/api/requests/{id}/process` | Procesar una solicitud individual. |
| `POST` | `/api/requests/process-pending` | Procesar las solicitudes pendientes en lote. |

### Ejemplo de creación de una solicitud TEXT

```json
{
  "name": "Solicitud de prueba",
  "payload": "Hola mundo desde la API",
  "type": "TEXT",
  "clientRequestId": "b6e4e32a-9e32-4f9b-a5d0-1b2c3d4e5f60"
}
```

### Ejemplo de creación de una solicitud JSON

```json
{
  "name": "Solicitud JSON",
  "payload": "{\"nombre\":\"Carlos\",\"edad\":30,\"activo\":true}",
  "type": "JSON",
  "clientRequestId": "a6e4e32a-9e32-4f9b-a5d0-1b2c3d4e5f61"
}
```

El identificador `clientRequestId` permite reconocer una solicitud del cliente y evitar registros duplicados durante los reintentos de sincronización.

## 11. Persistencia y sincronización

La aplicación utiliza dos niveles de persistencia:

**Persistencia local**

IndexedDB almacena las solicitudes y los grupos en el navegador. Esto permite conservar la información entre sesiones y trabajar sin conexión.

**Persistencia centralizada**

PostgreSQL almacena las solicitudes recibidas por el backend y sus estados de procesamiento.

**Flujo de sincronización**

1. El usuario crea una solicitud.
2. El frontend la almacena en IndexedDB con estado `PENDING`.
3. Cuando existe conectividad, el usuario puede ejecutar la sincronización.
4. El frontend envía las solicitudes pendientes a la API.
5. El backend registra la solicitud y devuelve su identificador y estado.
6. El frontend actualiza el registro local a `SYNCED`.
7. El backend procesa las solicitudes individualmente o por lotes.
8. El frontend consulta y actualiza los estados y resultados del servidor.

Los estados locales de sincronización son:

- `PENDING`: pendiente de envío.
- `SYNCED`: recibida y registrada por el backend.
- `FAILED`: error de sincronización.

Los estados de procesamiento del servidor son:

- `0`: pendiente.
- `1`: procesada.
- `2`: fallida.

La sincronización implementada se ejecuta mediante las acciones de la aplicación; no se ha incorporado un mecanismo de sincronización en segundo plano mediante Service Worker.

## 12. Ejecución de pruebas

Desde la raíz de la solución:

```bash
cd C:\Proyectos
dotnet test Solicitudes.sln
```

Durante la validación del proyecto se ejecutaron satisfactoriamente **33 pruebas unitarias del backend**, distribuidas entre:

- Pruebas de la entidad `Request`.
- Pruebas de creación de solicitudes.
- Pruebas de procesamiento individual.
- Pruebas de procesamiento por lotes.

La ejecución de pruebas de integración y pruebas automatizadas del frontend debe realizarse según la configuración de pruebas incorporada al repositorio.

## 13. Estructura general del repositorio

```text
Proyectos/
│
├── Solicitudes.sln
│
├── Solicitudes.Domain/
│
├── Solicitudes.Application/
│
├── Solicitudes.Infrastructure/
│
├── Solicitudes.Api/
│
├── Solicitudes.Tests/
│
└── solicitudes-frontend/
    │
    ├── src/
    │   ├── features/
    │   ├── infrastructure/
    │   └── ...
    │
    ├── public/
    ├── package.json
    ├── vite.config.ts
    └── .env
```

La estructura puede contener archivos adicionales de configuración, migraciones, servicios y componentes.

## 14. Consideraciones y alcance

- El frontend conserva los datos localmente mediante IndexedDB.
- La sincronización con el backend se realiza cuando existe conectividad y se ejecuta la acción correspondiente.
- La idempotencia se apoya en el identificador `clientRequestId`.
- El backend y el frontend se ejecutan de forma independiente.
- PostgreSQL es el motor de persistencia centralizado utilizado en el entorno local.
- La solución está organizada para facilitar la incorporación de nuevos tipos y reglas de procesamiento.

**Configuración de infraestructura:** la ejecución local documentada utiliza PostgreSQL instalado localmente. No se incluye como requisito de ejecución una infraestructura Docker, Azure Storage, Azure Service Bus o Azure Container Apps, ya que estos servicios no forman parte de la configuración local validada de esta implementación.

## 15. Autoría

Proyecto desarrollado como parte de la evaluación técnica para el cargo de Desarrollador Fullstack.

Repositorio: agregar aquí la URL del repositorio Git correspondiente.
