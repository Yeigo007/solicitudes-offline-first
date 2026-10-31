# Solicitudes Offline-First

Aplicación fullstack para la gestión y procesamiento de solicitudes, diseñada bajo un enfoque **Offline-First**, que permite crear, almacenar, consultar y gestionar solicitudes incluso cuando no existe conexión con el servidor.

El sistema sincroniza automáticamente las solicitudes pendientes cuando se recupera la conectividad y permite procesarlas individualmente o por lotes, manteniendo la información persistida localmente.

## Características principales

- **Arquitectura Clean Architecture:** separación de responsabilidades entre dominio, aplicación, infraestructura y presentación.
- **Funcionamiento Offline-First:** permite crear y consultar solicitudes sin conexión a Internet.
- **Persistencia local:** almacenamiento de solicitudes mediante IndexedDB.
- **Sincronización:** envío de solicitudes pendientes al backend cuando existe conectividad.
- **Idempotencia:** evita registrar duplicados cuando una solicitud se reintenta.
- **Procesamiento extensible:** permite procesar solicitudes según su tipo.
- **Procesamiento individual y por lotes:** permite ejecutar operaciones sobre solicitudes individuales o múltiples solicitudes.
- **Gestión jerárquica:** permite crear grupos y subgrupos para organizar solicitudes.
- **Acciones por grupo:** permite sincronizar y procesar solicitudes pertenecientes a un grupo y sus descendientes.
- **Seguimiento de estados:** permite identificar solicitudes pendientes, sincronizadas, procesadas o fallidas.

## Arquitectura

El proyecto está organizado en dos aplicaciones principales: backend y frontend.

### Backend

Desarrollado con .NET y Clean Architecture.

```text
Solicitudes/
│
├── Solicitudes.Domain/
│   ├── Entities/
│   ├── Enums/
│   └── Exceptions/
│
├── Solicitudes.Application/
│   ├── UseCases/
│   ├── Interfaces/
│   └── Validators/
│
├── Solicitudes.Infrastructure/
│   ├── Persistence/
│   ├── Repositories/
│   └── DependencyInjection.cs
│
├── Solicitudes.Api/
│   ├── Middleware/
│   ├── Program.cs
│   └── appsettings.json
│
├── Solicitudes.Tests/
│
└── Solicitudes.sln
```

**Responsabilidades:**

| Proyecto | Responsabilidad |
|---|---|
| Domain | Entidades, reglas de negocio y enumeraciones. |
| Application | Casos de uso, valid


**Como probar**
levantar backend
dotnet run --project Solicitudes.Api --urls http://localhost:5000

levantar frontEnd
npm run buid
npm run dev
http://localhost:5173/

BBDD
solucitudesDB

