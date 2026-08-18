# OpoMatic-3000

Aplicación personal para practicar tests de oposición, guardar intentos y
detectar los temas con más dificultades.

## Situación actual

El primer bloque de tareas de fundamentos técnicos, de T001 a T006, está
completado. La Entrega 1 incluye gestión global de errores mediante Problem
Details, CORS de desarrollo, un cliente HTTP tipado, pruebas automatizadas de
backend y frontend, y la comunicación visible entre React y el endpoint
`GET /health`.

El siguiente bloque comenzará con T010 para incorporar Entity Framework Core,
definir el modelo de datos y preparar la primera migración de SQL Server.
Todavía no se ha implementado la persistencia ni ninguna funcionalidad del
dominio.

## Tecnologías previstas

- ASP.NET Core Web API sobre .NET 10 LTS.
- React 19 con TypeScript y Vite.
- Entity Framework Core con SQL Server.

## Estructura

```text
backend/
├── src/
│   ├── OpoMatic3000.Api/
│   ├── OpoMatic3000.Application/
│   ├── OpoMatic3000.Domain/
│   └── OpoMatic3000.Infrastructure/
└── tests/
    ├── OpoMatic3000.UnitTests/
    └── OpoMatic3000.IntegrationTests/

frontend/                    React + TypeScript + Vite
docs/                        Documentación funcional y técnica
OpoMatic3000.sln             Solución .NET
```

## Preparación inicial

Desde la raíz del repositorio:

```powershell
dotnet restore OpoMatic3000.sln
Set-Location frontend
npm.cmd install
```

Se utiliza `npm.cmd` en los ejemplos de PowerShell porque algunos equipos
bloquean la ejecución del wrapper `npm.ps1`.

## Ejecutar en desarrollo

Backend, desde la raíz:

```powershell
dotnet run --project backend/src/OpoMatic3000.Api/OpoMatic3000.Api.csproj --launch-profile http
```

La API escucha inicialmente en `http://localhost:5151`. Comprobación:

```text
GET http://localhost:5151/health
```

Frontend, desde otra terminal:

```powershell
Set-Location frontend
npm.cmd run dev
```

Vite muestra en la terminal la dirección local, normalmente
`http://localhost:5173`.

## Compilar y probar

```powershell
dotnet build OpoMatic3000.sln
dotnet test OpoMatic3000.sln
Set-Location frontend
npm.cmd run lint
npm.cmd run test:run
npm.cmd run build
```

## Documentación

- [Funcionalidades](docs/funcionalidades.txt): alcance y prioridades.
- [Requisitos](docs/requisitos.txt): historias de usuario, criterios de
  aceptación y reglas de negocio.
- [Diseño de base de datos](docs/diseno-base-datos.txt): tablas, relaciones e
  histórico.
- [Diseño de la API](docs/diseno-api.txt): endpoints, contratos JSON,
  validaciones y errores.
- [Arquitectura](docs/arquitectura.txt): estructura técnica, responsabilidades y
  estrategia de pruebas.
- [Plan de implementación](docs/plan-implementacion.txt): entregas ordenadas
  para construir el MVP.
- [Tareas de desarrollo](docs/tareas/README.md): backlog detallado, dependencias y
  criterios de comprobación para cada paso pequeño.
- [Instrucciones para agentes](AGENTS.md): flujo eficiente para implementar,
  verificar y cerrar documentalmente cada tarea.

## Alcance de seguridad

El MVP no tiene autenticación. Está pensado para ejecutarse localmente o en una
red privada controlada y no debe exponerse públicamente a Internet.

## Próximo paso

Comenzar la Entrega 2 de `docs/plan-implementacion.txt` con la tarea T010:
añadir Entity Framework Core para preparar el modelo de datos y la primera
migración de SQL Server.
