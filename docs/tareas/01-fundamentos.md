# 01. Fundamentos técnicos

Objetivo del bloque: terminar la base ejecutable del backend y frontend y
comprobar que React puede comunicarse con la API de forma consistente.

Documentos de referencia: `../arquitectura.txt`, secciones 3, 6, 8, 11 y 12;
`../diseno-api.txt`, secciones 2 y 3.

## [x] T001 - Configurar Problem Details

**Depende de:** estructura inicial existente.

**Objetivo:** devolver todos los errores HTTP con un formato uniforme.

**Qué hacer:**

- Configurar Problem Details en `OpoMatic3000.Api`.
- Añadir un manejador global de excepciones.
- Mapear errores esperados a 400, 404 y 409.
- Incluir `traceId` sin exponer stack traces ni detalles internos.
- Mantener OpenAPI disponible solo en desarrollo.

**Comprobación:** provocar un error controlado y otro inesperado; ambos deben
usar `application/problem+json`, y el segundo no debe revelar una traza.

**Cierre — 2026-08-18**

- Implementado: excepciones esperadas para 400/404/409, manejador global para
  respuestas seguras 500, `traceId` y registro en el pipeline de la API.
- Verificación: pruebas específicas de Problem Details (4/4) — OK;
  `dotnet build OpoMatic3000.sln --no-restore` — OK, 0 advertencias;
  `dotnet test OpoMatic3000.sln --no-build --no-restore` — OK, 6/6 pruebas.
- Documentación: tarea e índice de progreso actualizados; el contrato de
  `diseno-api.txt` ya coincidía con la implementación.

## [x] T002 - Configurar CORS de desarrollo

**Depende de:** T001.

**Objetivo:** permitir que Vite llame a la API durante el desarrollo.

**Qué hacer:**

- Crear una política CORS limitada a `http://localhost:5173`.
- Leer los orígenes permitidos desde configuración.
- Activar la política antes de mapear controladores.
- No utilizar `AllowAnyOrigin`.

**Comprobación:** el origen configurado recibe cabeceras CORS y un origen
distinto no las recibe.

**Cierre — 2026-08-18**

- Implementado: política CORS `Frontend` alimentada por configuración, limitada
  a `http://localhost:5173` en desarrollo y aplicada antes de controladores.
- Verificación: pruebas específicas CORS (3/3) — OK;
  `dotnet test OpoMatic3000.sln --no-restore` — OK, 9/9 pruebas.
- Documentación: tarea e índice de progreso actualizados.

## [x] T003 - Crear el cliente HTTP del frontend

**Depende de:** T001.

**Objetivo:** centralizar todas las llamadas de React a la API.

**Qué hacer:**

- Crear `src/shared/api/apiClient.ts`.
- Definir la URL base con `VITE_API_BASE_URL` y un `.env.example`.
- Gestionar JSON, respuestas sin contenido y cancelación con `AbortSignal`.
- Convertir Problem Details en un error tipado para la interfaz.
- No incluir secretos en variables de Vite.

**Comprobación:** una respuesta correcta se deserializa y un Problem Details
conserva estado, título, detalle, traceId y errores de campos.

**Cierre — 2026-08-18**

- Implementado: `apiClient` centralizado con URL configurable, JSON, respuestas
  204, `AbortSignal` y error tipado que conserva Problem Details.
- Verificación: pruebas de `apiClient` (4/4) — OK; `npm.cmd run lint` — OK;
  `npm.cmd run build` — OK.
- Documentación: `.env.example`, tarea e índice de progreso actualizados.

## [x] T004 - Preparar las pruebas del frontend

**Depende de:** proyecto React existente.

**Objetivo:** poder probar componentes y llamadas HTTP antes de añadir pantallas.

**Qué hacer:**

- Instalar y configurar Vitest, Testing Library y `jsdom`.
- Añadir scripts `test` y `test:run`.
- Crear un archivo común de configuración de pruebas.
- Añadir una primera prueba de renderizado de `App`.

**Comprobación:** `npm.cmd run test:run` encuentra y supera al menos una prueba.

**Cierre — 2026-08-18**

- Implementado: Vitest, Testing Library, jest-dom y jsdom; scripts de ejecución,
  configuración común y primera prueba de renderizado de `App`.
- Verificación: `npm.cmd run test:run` — OK, 5/5 pruebas; auditoría npm —
  0 vulnerabilidades; compilación y lint — OK.
- Documentación: tarea e índice de progreso actualizados.

## [x] T005 - Mostrar el estado real de la API en React

**Depende de:** T002, T003 y T004.

**Objetivo:** completar la primera comunicación visible React -> API.

**Qué hacer:**

- Crear el contrato TypeScript de `GET /health`.
- Consultar el endpoint al cargar la portada.
- Mostrar estados cargando, disponible y no disponible.
- Evitar actualizaciones de estado después de desmontar el componente.
- Añadir pruebas para los tres estados.

**Comprobación:** con ambos procesos activos la portada indica que la API está
disponible; con la API detenida muestra un error comprensible.

**Cierre — 2026-08-18**

- Implementado: contrato TypeScript de salud, consulta cancelable al cargar la
  portada y estados visibles de carga, disponibilidad y error de conexión.
- Verificación: `npm.cmd run test:run` — OK, 8/8 pruebas; compilación y lint — OK.
- Documentación: tarea e índice de progreso actualizados.

## [x] T006 - Probar el endpoint de salud de extremo a extremo

**Depende de:** T005.

**Objetivo:** cerrar la Entrega 1 con una prueba automatizada del backend real.

**Qué hacer:**

- Añadir `Microsoft.AspNetCore.Mvc.Testing` al proyecto de integración.
- Crear una prueba con `WebApplicationFactory<Program>`.
- Verificar estado 200 y el contrato `{ status, application }`.
- Sustituir la prueba temporal de configuración del proyecto de integración.

**Comprobación:** `dotnet test OpoMatic3000.sln` supera la prueba sin levantar
manualmente la API.

**Cierre — 2026-08-18**

- Implementado: paquete `Microsoft.AspNetCore.Mvc.Testing` y prueba de
  integración con `WebApplicationFactory<Program>` para `GET /health`; retirada
  la prueba temporal de configuración.
- Verificación: `dotnet build OpoMatic3000.sln --no-restore` — OK, sin avisos;
  `dotnet test OpoMatic3000.sln --no-build --no-restore` — OK, 9/9 pruebas.
- Documentación: Entrega 1, tarea e índice de progreso actualizados.
