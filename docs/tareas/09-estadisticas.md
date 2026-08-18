# 09. Estadísticas

Objetivo del bloque: completar HU-09 y HU-10 calculando datos desde intentos
finalizados, sin tablas acumuladas.

Referencias: `../requisitos.txt`, HU-09/HU-10 y RN-08/RN-09;
`../diseno-api.txt`, sección 12.

## [ ] T080 - Calcular el resumen global

**Depende de:** T074.

**Objetivo:** obtener volumen y precisión total.

**Qué hacer:** agregar intentos, contestadas, aciertos, fallos y blancos; calcular
`A/(A+F)*100`; devolver null si no hay contestadas; evitar cargar filas completas.

**Comprobación:** pruebas cubren base vacía, solo blancos y mezcla de intentos.

## [ ] T081 - Calcular estadísticas y ranking por tema

**Depende de:** T080.

**Objetivo:** detectar temas fuertes y débiles.

**Qué hacer:** agrupar snapshots por `OriginalTopicId`, incluir nombres
históricos/inactivos, calcular volumen y precisión; ordenar por porcentaje,
contestadas y nombre; excluir del ranking temas sin contestadas.

**Comprobación:** empates y temas sin datos producen exactamente el orden y nulls
documentados.

## [ ] T082 - Publicar los endpoints de estadísticas

**Depende de:** T081.

**Objetivo:** implementar `/api/statistics/summary` y `/topics`.

**Qué hacer:** contratos, controladores, porcentajes numéricos, OpenAPI y consultas
de solo lectura sin tracking.

**Comprobación:** respuestas coinciden con la sección 12 de `diseno-api.txt`.

## [ ] T083 - Crear el resumen del panel principal

**Depende de:** T082.

**Objetivo:** mostrar en `/` los indicadores más útiles y actividad reciente.

**Qué hacer:** tarjetas de tests, contestadas y precisión; accesos a nuevo test,
preguntas e historial; estados vacío/carga/error.

**Comprobación:** una base vacía guía al primer paso y no muestra ceros engañosos
como porcentajes reales.

## [ ] T084 - Crear la pantalla de estadísticas por tema

**Depende de:** T082 y T083.

**Objetivo:** comparar temas en `/statistics`.

**Qué hacer:** tabla/ranking accesible con volumen, aciertos, fallos, blancos y
porcentaje; distinguir resultados con poca muestra; permitir ordenar mejor/peor.

**Comprobación:** la información sigue siendo comprensible sin depender del
color y en una pantalla estrecha.

## [ ] T085 - Cerrar criterios de estadísticas

**Depende de:** T084.

**Objetivo:** verificar HU-09/HU-10.

**Qué hacer:** pruebas de consultas y empates, endpoints, estados frontend y
actualización tras completar un test; revisar rendimiento con datos de ejemplo.

**Comprobación:** ambas historias quedan cubiertas y todas las suites pasan.

