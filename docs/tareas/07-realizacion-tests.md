# 07. Realización, corrección y resultado

Objetivo del bloque: completar HU-05 y HU-06, desde responder en memoria hasta
guardar un intento histórico idempotente.

Referencias: `../requisitos.txt`, HU-05/HU-06 y reglas RN-02 a RN-07;
`../diseno-api.txt`, sección 10.

## [ ] T060 - Crear el estado temporal del test

**Depende de:** T055.

**Objetivo:** conservar respuestas solo en memoria durante la sesión.

**Qué hacer:** crear contexto de feature y reducer con token, preguntas,
submissionId, índice actual y respuestas por questionId; definir acciones
tipadas y limpiar el estado al finalizar/abandonar.

**Comprobación:** pruebas del reducer cubren seleccionar, cambiar, limpiar,
navegar y reiniciar sin mutar el estado anterior.

## [ ] T061 - Crear la interfaz para responder

**Depende de:** T060.

**Objetivo:** mostrar una pregunta y cuatro opciones accesibles.

**Qué hacer:** crear `/tests/run`, grupo de radio, tema, progreso, botones
anterior/siguiente y estados seleccionados; no mostrar corrección.

**Comprobación:** se puede completar el test con teclado y volver a una pregunta
sin perder la selección.

## [ ] T062 - Añadir revisión previa y protección al abandonar

**Depende de:** T061.

**Objetivo:** evitar entregas o pérdidas accidentales.

**Qué hacer:** mapa de preguntas contestadas/pendientes, navegación directa,
confirmación final con contador en blanco, bloqueo de navegación y
`beforeunload`; redirigir si `/tests/run` no tiene estado.

**Comprobación:** cerrar, recargar o salir advierte; confirmar abandono elimina
el progreso y cancelar lo conserva.

## [ ] T063 - Implementar y probar la puntuación

**Depende de:** T060.

**Objetivo:** centralizar la fórmula oficial en Domain/Application.

**Qué hacer:** clasificar respuestas y calcular
`(10/N) * (aciertos - 0,25*fallos)` con decimal, sin redondeos intermedios;
permitir negativos y dar formato de dos decimales solo en presentación.

**Comprobación:** pruebas cubren todo correcto, todo fallado (-2,50), todo en
blanco, mezcla documentada (6,50) y tamaños con decimales periódicos.

## [ ] T064 - Guardar el intento y sus snapshots

**Depende de:** T062 y T063.

**Objetivo:** crear el histórico completo en una transacción.

**Qué hacer:** validar token/respuestas, obtener corrección, calcular contadores,
crear intento, temas, preguntas y opciones históricas con orden/textos exactos.

**Comprobación:** cualquier fallo revierte todo y la suma de contadores coincide
con el total.

## [ ] T065 - Hacer idempotente la finalización

**Depende de:** T064.

**Objetivo:** impedir duplicados por doble clic o reintento de red.

**Qué hacer:** usar `submissionId` único, devolver el intento existente al
repetir la misma entrega y 409 si el mismo ID llega con datos incompatibles.

**Comprobación:** dos solicitudes idénticas producen una única fila de intento.

## [ ] T066 - Publicar finalización y mostrar el resultado

**Depende de:** T065.

**Objetivo:** implementar `POST /api/tests/finish` y su consumo desde React.

**Qué hacer:** completar endpoint/Problem Details/OpenAPI; enviar null para
blancos; bloquear envíos repetidos; limpiar estado temporal; navegar a
`/attempts/{id}` y mostrar resumen inicial.

**Comprobación:** respuesta 201 inicial, 200 idempotente, 410 expirado y 400 para
opciones ajenas se representan correctamente.

## [ ] T067 - Cerrar realización y corrección

**Depende de:** T066.

**Objetivo:** verificar HU-05, HU-06 y las reglas de puntuación.

**Qué hacer:** pruebas unitarias, integración de persistencia/idempotencia y
componentes del runner; revisión manual de navegación y pérdida de progreso.

**Comprobación:** todos los criterios de ambas historias están cubiertos y las
suites completas pasan.

