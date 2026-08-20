# 02. Base de datos

Objetivo del bloque: trasladar el diseño lógico a entidades, configuraciones de
Entity Framework Core y una migración reproducible sobre SQL Server.

Documentos de referencia: `../diseno-base-datos.txt` completo y
`../arquitectura.txt`, secciones 5, 10 y 13.

## [x] T010 - Añadir Entity Framework Core

**Depende de:** T006.

**Objetivo:** instalar solo los paquetes de persistencia necesarios.

**Qué hacer:**

- Añadir EF Core, el proveedor SQL Server y herramientas de diseño con versiones
  compatibles con .NET 10.
- Colocar cada paquete en el proyecto responsable.
- Verificar o instalar `dotnet-ef` con una versión compatible.
- Documentar las versiones resueltas.

**Comprobación:** restauración y compilación limpias; `dotnet ef --version`
funciona.

**Cierre — 2026-08-18**

- Implementado: EF Core y proveedor SQL Server 10.0.11 añadidos a
  Infrastructure, herramientas de diseño 10.0.11 en el proyecto de inicio API;
  `dotnet-ef` 10.0.11 registrado como herramienta local reproducible.
- Verificación: `dotnet restore OpoMatic3000.sln` — OK;
  `dotnet build OpoMatic3000.sln --no-restore` — OK, sin avisos;
  `dotnet test OpoMatic3000.sln --no-restore` — OK, 9/9 pruebas;
  `dotnet ef --version` — OK, 10.0.11.
- Documentación: versiones registradas en `docs/arquitectura.txt` y restauración
  de herramientas añadida a `README.md`.

## [x] T011 - Crear las entidades del banco de preguntas

**Depende de:** T010.

**Objetivo:** representar `Topic`, `Question` y `QuestionOption` en Domain.

**Qué hacer:**

- Crear propiedades y relaciones definidas en el diseño.
- Usar nombres C# expresivos y colecciones controladas.
- Incorporar `IsActive` y fechas UTC donde corresponda.
- No añadir atributos de Entity Framework a Domain.

**Comprobación:** Domain no referencia EF Core y las entidades permiten expresar
un tema con preguntas y exactamente cuatro opciones.

**Cierre — 2026-08-18**

- Implementado: entidades `Topic`, `Question` y `QuestionOption` sin dependencia
  de EF Core, con colecciones controladas, fechas UTC y construcción válida de
  cuatro opciones con una única respuesta correcta.
- Verificación: `dotnet test backend/tests/OpoMatic3000.UnitTests/OpoMatic3000.UnitTests.csproj --no-restore --filter FullyQualifiedName~TopicTests` — OK, 5/5 pruebas.
- Documentación: tarea e índice de progreso actualizados.

## [x] T012 - Crear las entidades históricas

**Depende de:** T011.

**Objetivo:** representar intentos, temas seleccionados y snapshots históricos.

**Qué hacer:**

- Crear `TestAttempt`, `TestAttemptTopic`, `TestAttemptQuestion` y
  `TestAttemptOption`.
- Crear la enumeración `QuestionResult`.
- Incluir `SubmissionId`, versión de puntuación y contadores.
- Mantener textos snapshot separados de las entidades actuales.

**Comprobación:** el modelo puede conservar un intento completo sin depender de
que posteriormente cambien los textos actuales.

**Cierre — 2026-08-18**

- Implementado: agregado histórico con intento, temas, preguntas, opciones,
  snapshots, `SubmissionId`, versión de puntuación, contadores y
  `QuestionResult`.
- Verificación: `dotnet test backend/tests/OpoMatic3000.UnitTests/OpoMatic3000.UnitTests.csproj --no-restore --filter FullyQualifiedName~TestAttemptTests` — OK, 3/3 pruebas.
- Documentación: tarea e índice de progreso actualizados.

## [x] T013 - Crear DbContext y configurar el banco de preguntas

**Depende de:** T011.

**Objetivo:** mapear Topics, Questions y QuestionOptions mediante Fluent API.

**Qué hacer:**

- Crear `OpoMatic3000DbContext` en Infrastructure.
- Configurar nombres, longitudes, claves, relaciones e índices.
- Restringir posiciones de opciones a 1-4.
- Configurar borrado restrictivo y consultas eficientes por tema/estado.
- Aplicar la política de nombres de tema sin espacios exteriores y sin distinguir
  mayúsculas.

**Comprobación:** el modelo de EF se construye sin advertencias y refleja todas
las restricciones descritas para las tres tablas.

**Cierre — 2026-08-18**

- Implementado: `OpoMatic3000DbContext` y mapeos Fluent API de Topics,
  Questions y QuestionOptions con claves, longitudes, checks, índices,
  collation y borrado restrictivo.
- Verificación: `dotnet test backend/tests/OpoMatic3000.IntegrationTests/OpoMatic3000.IntegrationTests.csproj --no-restore --filter FullyQualifiedName~QuestionBankModelTests` — OK, 3/3 pruebas.
- Documentación: tarea e índice de progreso actualizados.

## [x] T014 - Configurar el histórico de intentos

**Depende de:** T012 y T013.

**Objetivo:** mapear las cuatro tablas históricas sin perder integridad.

**Qué hacer:**

- Configurar precisión de `Score` y unicidad de `SubmissionId`.
- Añadir checks de contadores, nota y resultado.
- Configurar claves compuestas e índices históricos.
- Garantizar que cada pregunta histórica pertenece a un tema del intento.
- Definir cascadas solo entre un intento y su detalle interno.

**Comprobación:** el modelo impide duplicar pregunta, tema u orden dentro del
mismo intento.

**Cierre — 2026-08-18**

- Implementado: mapeos de las cuatro tablas históricas con snapshots, precisión
  de nota, checks, unicidades, claves compuestas y cascadas limitadas al detalle
  propiedad del intento.
- Verificación: `dotnet test backend/tests/OpoMatic3000.IntegrationTests/OpoMatic3000.IntegrationTests.csproj --no-restore --filter FullyQualifiedName~HistoricalModelTests` — OK, 3/3 pruebas.
- Documentación: tarea e índice de progreso actualizados.

## [x] T015 - Configurar SQL Server e inyección de dependencias

**Depende de:** T014.

**Objetivo:** conectar la API con SQL Server sin versionar secretos.

**Qué hacer:**

- Añadir la cadena mediante User Secrets.
- Registrar DbContext desde Infrastructure.
- Añadir métodos de extensión claros para la inyección de dependencias.
- Configurar logging de EF sin mostrar datos sensibles.
- Documentar cómo establecer la conexión local.

**Comprobación:** la API arranca con una conexión válida y falla con un mensaje
diagnosticable cuando falta la configuración.

**Cierre — 2026-08-18**

- Implementado: registro de `OpoMatic3000DbContext` desde Infrastructure,
  conexión `OpoMatic3000` mediante User Secrets, autenticación integrada y
  logging sin datos sensibles; configuración local documentada.
- Verificación: pruebas `InfrastructureConfigurationTests` — OK, 2/2; prueba
  `HealthEndpointTests` — OK, la API arranca con la configuración válida.
- Documentación: instrucciones de SQL Server y User Secrets añadidas a
  `README.md`; tarea e índice actualizados.

## [x] T016 - Crear la migración inicial

**Depende de:** T015.

**Objetivo:** generar el esquema completo de forma reproducible.

**Qué hacer:**

- Crear la migración `InitialCreate`.
- Revisar el SQL generado, claves, checks, índices y tipos.
- Aplicarla sobre una base vacía.
- Comprobar que puede revertirse y aplicarse de nuevo en desarrollo.

**Comprobación:** `dotnet ef database update` crea todas las tablas sin cambios
manuales en SQL Server.

**Cierre — 2026-08-18**

- Implementado: migración `InitialCreate` con siete tablas, claves, relaciones,
  checks, índices, tipos y snapshot de EF Core.
- Verificación: `dotnet ef database update` — OK sobre una base nueva;
  `dotnet ef database update 0` — OK; reaplicación de `database update` — OK.
- Documentación: tarea e índice de progreso actualizados; la base local
  `OpoMatic3000` queda creada y actualizada.

## [x] T017 - Probar restricciones reales de SQL Server

**Depende de:** T016.

**Objetivo:** verificar que la base actúa como última barrera de integridad.

**Qué hacer:**

- Preparar una base aislada para pruebas de integración.
- Probar nombres de tema duplicados y relaciones inválidas.
- Probar posiciones de opción y `SubmissionId` duplicado.
- Probar checks de contadores y nota.
- Limpiar los datos entre pruebas de forma determinista.

**Comprobación:** cada estado inválido previsto provoca la restricción esperada
y las pruebas no afectan a la base de desarrollo.

**Cierre — 2026-08-18**

- Implementado: pruebas contra bases SQL Server aisladas para nombres de tema,
  relación intento-tema, posición de opción, `SubmissionId`, contadores y nota;
  creación y limpieza deterministas con protección por prefijo.
- Verificación: pruebas `SqlServerConstraintTests` — OK, 5/5;
  `dotnet build OpoMatic3000.sln --no-restore` — OK, sin avisos;
  `dotnet test OpoMatic3000.sln --no-build --no-restore` — OK, 30/30 pruebas;
  `dotnet ef migrations has-pending-model-changes` — OK, sin cambios.
- Documentación: configuración de la base de pruebas añadida a `README.md`;
  tarea, índice y plan de implementación actualizados.
