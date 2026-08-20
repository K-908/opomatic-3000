# 03. Gestión de temas

Objetivo del bloque: completar HU-01 de extremo a extremo.

Referencias: `../requisitos.txt`, HU-01; `../diseno-api.txt`, sección 6.

## [x] T020 - Consultar temas desde Application

**Depende de:** T017.

**Objetivo:** implementar lista y detalle sin acoplar Application a HTTP.

**Qué hacer:** crear contratos internos, consulta con `includeInactive`, contador
de preguntas activas y caso de tema inexistente.

**Comprobación:** pruebas unitarias cubren lista vacía, activos, inclusión de
inactivos y detalle inexistente.

**Cierre — 2026-08-20**

- Implementado: contratos, repositorio específico y servicio de consulta de temas con filtro y contador de preguntas activas.
- Verificación: `dotnet test backend/tests/OpoMatic3000.UnitTests/OpoMatic3000.UnitTests.csproj --no-restore` — OK (21/21).
- Documentación: índice de tareas actualizado.

## [x] T021 - Crear y editar temas

**Depende de:** T020.

**Objetivo:** implementar creación y cambio de nombre.

**Qué hacer:** normalizar espacios, validar longitud, detectar duplicados sin
distinguir mayúsculas y actualizar `UpdatedAtUtc` mediante un reloj inyectable.

**Comprobación:** crear devuelve el tema persistido; nombres vacíos o duplicados
producen errores de aplicación específicos.

**Cierre — 2026-08-20**

- Implementado: alta y renombrado con normalización de espacios, límite de longitud, unicidad y reloj inyectable.
- Verificación: `dotnet test backend/tests/OpoMatic3000.UnitTests/OpoMatic3000.UnitTests.csproj --no-restore` — OK (22/22).
- Documentación: índice de tareas actualizado.

## [x] T022 - Desactivar y reactivar temas

**Depende de:** T021.

**Objetivo:** cambiar disponibilidad sin modificar el histórico ni el estado
individual de las preguntas.

**Qué hacer:** implementar cambio idempotente de estado y garantizar que las
preguntas de un tema inactivo no se consideren disponibles.

**Comprobación:** reactivar recupera las preguntas que ya tenían `IsActive=true`.

**Cierre — 2026-08-20**

- Implementado: cambio de estado idempotente y disponibilidad de preguntas condicionada por el estado del tema sin alterar cada pregunta.
- Verificación: `dotnet test backend/tests/OpoMatic3000.UnitTests/OpoMatic3000.UnitTests.csproj --no-restore` — OK (22/22).
- Documentación: índice de tareas actualizado.

## [x] T023 - Publicar los endpoints de temas

**Depende de:** T022.

**Objetivo:** implementar los contratos HTTP de `/api/topics`.

**Qué hacer:** crear DTOs, controlador, mapeos, códigos 200/201/204/400/404/409,
cabecera `Location` y documentación OpenAPI.

**Comprobación:** las respuestas coinciden con la sección 6 de `diseno-api.txt`
y todos los errores usan Problem Details.

**Cierre — 2026-08-20**

- Implementado: DTOs y endpoints GET/POST/PUT/PATCH de `/api/topics` con Location, Problem Details y metadatos OpenAPI.
- Verificación: `dotnet test backend/tests/OpoMatic3000.IntegrationTests/OpoMatic3000.IntegrationTests.csproj --no-restore --filter FullyQualifiedName~TopicEndpointsTests` — OK (3/3).
- Documentación: contrato OpenAPI generado y comprobado; índice actualizado.

## [x] T024 - Crear la administración de temas en React

**Depende de:** T023.

**Objetivo:** administrar temas desde `/topics`.

**Qué hacer:** añadir ruta, lista, formulario de alta/edición, filtro de
inactivos, contadores, confirmación de desactivación y estados de carga/vacío.

**Comprobación:** una persona puede completar todo el flujo sin recargar la
página y los errores de campos aparecen junto al control correspondiente.

**Cierre — 2026-08-20**

- Implementado: pantalla `/topics` con alta, edición, filtro de inactivos, contadores, confirmación y estados de carga, vacío y error.
- Verificación: `npm.cmd run test:run` — OK (13/13); `npm.cmd run lint` — OK; `npm.cmd run build` — OK.
- Documentación: navegación visible actualizada; índice de tareas actualizado.

## [x] T025 - Cerrar los criterios de aceptación de temas

**Depende de:** T024.

**Objetivo:** verificar HU-01 antes de comenzar preguntas.

**Qué hacer:** añadir pruebas de integración de endpoints y pruebas de
componentes; revisar teclado, foco y confirmaciones; actualizar OpenAPI.

**Comprobación:** todos los criterios de HU-01 están cubiertos y las suites
completas de backend y frontend pasan.

**Cierre — 2026-08-20**

- Implementado: cobertura integral de HU-01 en servicios, endpoints y componentes, incluidos teclado, foco, confirmaciones y OpenAPI.
- Verificación: `dotnet build OpoMatic3000.sln --no-restore` — OK; `dotnet test OpoMatic3000.sln --no-restore --no-build` — OK (46/46); `npm.cmd run lint` — OK; `npm.cmd run test:run` — OK (13/13); `npm.cmd run build` — OK.
- Documentación: `README.md`, `docs/plan-implementacion.txt`, OpenAPI e índice de tareas actualizados.
