# 08. Histórico y revisión

Objetivo del bloque: completar HU-07 y HU-08 usando exclusivamente los snapshots
guardados al finalizar cada intento.

Referencias: `../requisitos.txt`, HU-07/HU-08; `../diseno-api.txt`, sección 11.

## [ ] T070 - Consultar el historial paginado

**Depende de:** T067.

**Objetivo:** listar intentos recientes eficientemente.

**Qué hacer:** consulta paginada ordenada por fecha/ID descendentes, temas
seleccionados, contadores y nota; endpoint `GET /api/attempts` y OpenAPI.

**Comprobación:** páginas, total y orden son estables, incluida una lista vacía.

## [ ] T071 - Consultar el detalle histórico

**Depende de:** T070.

**Objetivo:** reconstruir exactamente el test terminado.

**Qué hacer:** consulta por ID con temas, preguntas/opciones en displayOrder,
resultado, correcta y seleccionada; endpoint `GET /api/attempts/{id}`.

**Comprobación:** los textos proceden de snapshots y un ID inexistente devuelve
404 Problem Details.

## [ ] T072 - Crear la pantalla de historial

**Depende de:** T070.

**Objetivo:** navegar por intentos desde `/attempts`.

**Qué hacer:** lista paginada, fecha local, temas, nota con dos decimales,
contadores, estados loading/error/empty y enlace al detalle.

**Comprobación:** el orden coincide con la API y la paginación es accesible.

## [ ] T073 - Crear la revisión del intento

**Depende de:** T071 y T072.

**Objetivo:** revisar aciertos, fallos y blancos en `/attempts/:id`.

**Qué hacer:** resumen, navegación por preguntas, respuesta seleccionada y
correcta, tema y resultado comunicado con texto/icono además de color.

**Comprobación:** la pantalla es de solo lectura y distingue claramente los tres
resultados.

## [ ] T074 - Probar la inmutabilidad histórica

**Depende de:** T073.

**Objetivo:** cerrar HU-07/HU-08 y RN-10.

**Qué hacer:** finalizar un test, editar/desactivar banco y tema, consultar de
nuevo el intento y verificar que textos/orden/corrección no cambian; añadir
pruebas frontend de revisión.

**Comprobación:** criterios de ambas historias cubiertos y todas las suites
pasan.

