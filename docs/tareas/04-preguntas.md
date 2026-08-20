# 04. Gestión de preguntas

Objetivo del bloque: completar HU-02 y disponer de un banco administrable.

Referencias: `../requisitos.txt`, HU-02; `../diseno-api.txt`, sección 7.

## [x] T030 - Consultar preguntas con filtros y paginación

**Depende de:** T025.

**Objetivo:** implementar lista y detalle de administración.

**Qué hacer:** filtrar por tema, estado y texto; validar `page/pageSize`; ordenar
de forma estable; devolver opciones y respuesta correcta solo en el detalle de
administración.

**Comprobación:** filtros combinados, página vacía y límites producen resultados
y metadatos correctos.

**Cierre — 2026-08-20**

- Implementado: consulta paginada con filtros por tema, estado y texto, orden estable y detalle administrativo.
- Verificación: `dotnet test backend/tests/OpoMatic3000.UnitTests/OpoMatic3000.UnitTests.csproj --no-restore -c Release --no-build` — OK (31/31).
- Documentación: índice de tareas actualizado.

## [x] T031 - Crear preguntas y sus opciones

**Depende de:** T030.

**Objetivo:** guardar una pregunta válida en una sola transacción.

**Qué hacer:** validar tema activo, enunciado, cuatro posiciones únicas, textos
y exactamente una correcta; crear las cuatro opciones conjuntamente.

**Comprobación:** ningún fallo deja una pregunta u opción parcialmente guardada.

**Cierre — 2026-08-20**

- Implementado: alta validada de pregunta y sus cuatro opciones mediante una única unidad de persistencia.
- Verificación: `dotnet test backend/tests/OpoMatic3000.UnitTests/OpoMatic3000.UnitTests.csproj --no-restore -c Release --no-build` — OK (31/31).
- Documentación: índice de tareas actualizado.

## [x] T032 - Editar y cambiar el estado de preguntas

**Depende de:** T031.

**Objetivo:** mantener preguntas sin romper sus referencias históricas.

**Qué hacer:** actualizar opciones por posición conservando IDs; permitir cambio
de tema activo; implementar desactivación/reactivación y prohibir activación si
el tema está inactivo.

**Comprobación:** editar no crea opciones adicionales y el histórico existente
no cambia.

**Cierre — 2026-08-20**

- Implementado: edición por posición conservando IDs, cambio a temas activos y desactivación/reactivación segura.
- Verificación: `dotnet test backend/tests/OpoMatic3000.UnitTests/OpoMatic3000.UnitTests.csproj --no-restore -c Release --no-build` — OK (31/31).
- Documentación: índice de tareas actualizado.

## [x] T033 - Publicar los endpoints de preguntas

**Depende de:** T032.

**Objetivo:** implementar `/api/questions` salvo importación.

**Qué hacer:** crear contratos, controlador, paginación, filtros, mapeos,
Problem Details y OpenAPI para GET/POST/PUT/PATCH.

**Comprobación:** los contratos coinciden con la sección 7 de `diseno-api.txt`.

**Cierre — 2026-08-20**

- Implementado: contratos y endpoints GET/POST/PUT/PATCH con filtros, paginación, Problem Details y OpenAPI.
- Verificación: `dotnet test backend/tests/OpoMatic3000.IntegrationTests/OpoMatic3000.IntegrationTests.csproj --no-restore -c Release --no-build --filter FullyQualifiedName~QuestionEndpointsTests` — OK (3/3).
- Documentación: contrato OpenAPI comprobado; índice actualizado.

## [x] T034 - Crear la lista de preguntas en React

**Depende de:** T033.

**Objetivo:** buscar y administrar el banco desde `/questions`.

**Qué hacer:** lista paginada, búsqueda, filtros por tema/estado, enlaces de
edición, acción de estado y conservación razonable de filtros en la navegación.

**Comprobación:** estados loading/error/empty están presentes y la paginación no
solicita páginas inválidas.

**Cierre — 2026-08-20**

- Implementado: lista paginada con búsqueda, filtros, edición, cambio de estado y conservación de filtros.
- Verificación: `npm.cmd run test:run` — OK (21/21); `npm.cmd run lint` — OK; `npm.cmd run build` — OK.
- Documentación: navegación visible actualizada; índice actualizado.

## [x] T035 - Crear el formulario de pregunta

**Depende de:** T033 y T034.

**Objetivo:** compartir un formulario accesible para alta y edición.

**Qué hacer:** selector de tema, enunciado, cuatro opciones, radio de respuesta
correcta, validación cliente, envío y tratamiento de errores del servidor.

**Comprobación:** el formulario funciona completamente con teclado y no permite
dos respuestas correctas.

**Cierre — 2026-08-20**

- Implementado: formulario compartido de alta/edición con tema, enunciado, cuatro opciones, radio único y errores por campo.
- Verificación: `npm.cmd run test:run` — OK (21/21); `npm.cmd run lint` — OK; `npm.cmd run build` — OK.
- Documentación: navegación visible actualizada; índice actualizado.

## [x] T036 - Cerrar los criterios de aceptación de preguntas

**Depende de:** T035.

**Objetivo:** verificar HU-02.

**Qué hacer:** cubrir reglas con pruebas unitarias, endpoints con integración y
formularios/listas con pruebas de componentes; revisar OpenAPI y mensajes.

**Comprobación:** HU-02 está cubierta y todas las suites pasan.

**Cierre — 2026-08-20**

- Implementado: cobertura de HU-02 en dominio, casos de uso, persistencia, endpoints, lista y formulario React.
- Verificación: `dotnet build OpoMatic3000.sln --no-restore -c Release` — OK; `dotnet test OpoMatic3000.sln --no-restore -c Release --no-build` — OK (58/58); `npm.cmd run test:run` — OK (21/21); `npm.cmd run lint` — OK; `npm.cmd run build` — OK.
- Documentación: `README.md`, `docs/plan-implementacion.txt`, OpenAPI e índice de tareas actualizados.
