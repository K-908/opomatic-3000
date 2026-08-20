# OpoMatic-3000

Aplicación personal para practicar tests de oposición, guardar intentos y
detectar los temas con más dificultades.

## Situación actual

Los bloques de fundamentos técnicos (T001-T006), base de datos (T010-T017),
gestión de temas (T020-T025) y gestión manual de preguntas (T030-T036) están
completados. La aplicación dispone de gestión global de errores, persistencia
mediante EF Core y SQL Server, y una API documentada para administrar temas y
preguntas.

La ruta `/topics` permite administrar las materias. La ruta `/questions` permite
buscar, filtrar, paginar, desactivar y reactivar el banco; `/questions/new` y
`/questions/:id/edit` ofrecen el formulario de alta y edición con cuatro opciones
y una única respuesta correcta. El siguiente bloque implementará la importación
masiva desde JSON.

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
dotnet tool restore
Set-Location frontend
npm.cmd install
```

Se utiliza `npm.cmd` en los ejemplos de PowerShell porque algunos equipos
bloquean la ejecución del wrapper `npm.ps1`.

### Configurar SQL Server en desarrollo

La base de datos local se llama `OpoMatic3000`. La configuración recomendada
usa la instancia predeterminada de SQL Server en `localhost` y autenticación
integrada de Windows. La cadena se guarda mediante .NET User Secrets y nunca en
los archivos `appsettings`:

```powershell
dotnet user-secrets set "ConnectionStrings:OpoMatic3000" "Server=localhost;Database=OpoMatic3000;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True" --project backend/src/OpoMatic3000.Api/OpoMatic3000.Api.csproj
```

Si se utiliza SQL Server Express, una instancia con nombre o autenticación SQL,
hay que sustituir el servidor y las credenciales de la cadena. Las contraseñas
deben permanecer exclusivamente en User Secrets.

Las pruebas de restricciones crean y eliminan bases aisladas cuyo nombre empieza
por `OpoMatic3000_IntegrationTests_`. Por defecto usan `localhost` y autenticación
integrada. Otro servidor de pruebas puede indicarse mediante la variable local
`OPOMATIC_SQLSERVER_TEST_CONNECTION`, apuntando al catálogo `master`; nunca debe
apuntar a una base con datos que deban conservarse.

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

Comenzar la Entrega 5 de `docs/plan-implementacion.txt` con la tarea T040:
definir y publicar el esquema de importación JSON.
