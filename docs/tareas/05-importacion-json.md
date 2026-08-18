# 05. Importación JSON

Objetivo del bloque: completar HU-03 mediante una importación atómica y fácil de
corregir cuando el archivo contiene errores.

Referencias: `../requisitos.txt`, HU-03; `../diseno-api.txt`, sección 8.

## [ ] T040 - Formalizar el contrato y el archivo de ejemplo

**Depende de:** T036.

**Objetivo:** convertir el JSON documentado en contratos de código verificables.

**Qué hacer:** crear DTOs de importación, fijar `correctOption` 1-4, publicar un
archivo JSON válido de ejemplo y definir límites de 5 MB/2.000 preguntas.

**Comprobación:** el ejemplo se deserializa y contiene al menos dos temas y casos
representativos.

## [ ] T041 - Validar el documento completo

**Depende de:** T040.

**Objetivo:** informar todos los errores antes de escribir en la base.

**Qué hacer:** validar estructura, longitudes, cuatro opciones, posición
correcta, temas repetidos y conflictos con temas inactivos; devolver rutas como
`topics[0].questions[4].options`.

**Comprobación:** un documento con varios errores devuelve todos de una vez y no
accede a la fase de guardado.

## [ ] T042 - Implementar la importación transaccional

**Depende de:** T041.

**Objetivo:** crear o reutilizar temas y guardar preguntas mediante todo o nada.

**Qué hacer:** planificar operaciones tras validar, reutilizar temas activos,
crear los inexistentes y guardar todo en una única transacción.

**Comprobación:** un error de persistencia revierte temas, preguntas y opciones.

## [ ] T043 - Publicar el endpoint de importación

**Depende de:** T042.

**Objetivo:** implementar `POST /api/questions/import`.

**Qué hacer:** comprobar Content-Type y tamaño, devolver resumen 201, mapear
errores por ruta a Problem Details y documentar el ejemplo en OpenAPI.

**Comprobación:** las respuestas coinciden con la sección 8 de `diseno-api.txt`.

## [ ] T044 - Crear la pantalla de importación

**Depende de:** T043.

**Objetivo:** importar un archivo desde `/questions/import`.

**Qué hacer:** selector `.json`, validación inicial de tamaño, resumen del
archivo, confirmación, errores detallados, resultado de importación y acceso al
ejemplo.

**Comprobación:** después de una importación correcta las preguntas aparecen en
el banco sin recargar manualmente.

## [ ] T045 - Probar atomicidad y experiencia de importación

**Depende de:** T044.

**Objetivo:** cerrar HU-03.

**Qué hacer:** probar documentos válidos, múltiples errores, tema inactivo,
límites, fallo de transacción y presentación de errores en React.

**Comprobación:** ningún escenario inválido deja datos parciales y HU-03 queda
completamente verificada.
