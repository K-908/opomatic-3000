# 06. Generación de tests

Objetivo del bloque: completar HU-04 sin persistir tests en curso ni revelar las
respuestas correctas.

Referencias: `../requisitos.txt`, HU-04; `../diseno-api.txt`, sección 9.

## [ ] T050 - Implementar el token temporal protegido

**Depende de:** T045.

**Objetivo:** representar de forma opaca el test generado.

**Qué hacer:** crear contrato interno versionado, incluir caducidad, temas,
preguntas y orden de opciones; protegerlo con Data Protection y caducidad de 12
horas; diferenciar token inválido y expirado.

**Comprobación:** un token válido se recupera sin pérdida; uno alterado o
caducado se rechaza sin revelar su contenido.

## [ ] T051 - Seleccionar y barajar preguntas

**Depende de:** T050.

**Objetivo:** escoger preguntas activas de los temas solicitados sin repetidos.

**Qué hacer:** validar temas/cantidad, consultar disponibilidad, seleccionar
aleatoriamente, barajar opciones y producir órdenes consecutivos.

**Comprobación:** ejecuciones repetidas pueden variar, pero siempre respetan
temas, cantidad, unicidad y cuatro opciones.

## [ ] T052 - Crear el caso de uso de generación

**Depende de:** T051.

**Objetivo:** coordinar selección, contratos públicos y token.

**Qué hacer:** devolver temas y textos necesarios, excluir `IsCorrect`, producir
el token y mapear preguntas insuficientes con requested/available.

**Comprobación:** ninguna propiedad o estructura serializada permite conocer la
respuesta correcta.

## [ ] T053 - Publicar `POST /api/tests/generate`

**Depende de:** T052.

**Objetivo:** exponer la generación según el contrato documentado.

**Qué hacer:** crear DTOs/controlador, códigos 200/400/404/409, Problem Details y
OpenAPI con ejemplos.

**Comprobación:** la respuesta coincide con la sección 9 de `diseno-api.txt`.

## [ ] T054 - Crear la configuración del test en React

**Depende de:** T053.

**Objetivo:** iniciar un test desde `/tests/new`.

**Qué hacer:** selección múltiple de temas, cantidad, disponibilidad,
validaciones, error por insuficiencia y creación del estado temporal antes de
navegar a `/tests/run`.

**Comprobación:** no se puede empezar sin tema ni cantidad válida y los errores
del backend se entienden sin conocimientos técnicos.

## [ ] T055 - Probar generación y confidencialidad

**Depende de:** T054.

**Objetivo:** cerrar HU-04.

**Qué hacer:** probar selección, barajado, límites, token, endpoint, formulario y
ausencia de respuesta correcta en JSON.

**Comprobación:** HU-04 queda cubierta por pruebas y revisión manual.

