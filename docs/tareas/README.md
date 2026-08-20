# Tareas de desarrollo de OpoMatic-3000

Este directorio divide el MVP en tareas pequeñas y ordenadas. Cada tarea tiene
un identificador estable para poder solicitarla, revisarla y documentarla sin
tener que ejecutar un bloque funcional completo de una vez.

## Estados

- `[ ]` Pendiente.
- `[~]` En curso.
- `[x]` Terminada y verificada.
- `[!]` Bloqueada; debe indicarse el motivo.

El estado se actualizará en este índice y en el archivo correspondiente cuando
cambie de forma efectiva.

## Orden de ejecución

| Bloque | Archivo | Tareas | Estado |
|---|---|---:|---|
| Fundamentos técnicos | [01-fundamentos.md](01-fundamentos.md) | T001-T006 | Completado (6/6) |
| Base de datos | [02-base-datos.md](02-base-datos.md) | T010-T017 | Completado (8/8) |
| Gestión de temas | [03-temas.md](03-temas.md) | T020-T025 | Pendiente |
| Gestión de preguntas | [04-preguntas.md](04-preguntas.md) | T030-T036 | Pendiente |
| Importación JSON | [05-importacion-json.md](05-importacion-json.md) | T040-T045 | Pendiente |
| Generación de tests | [06-generacion-tests.md](06-generacion-tests.md) | T050-T055 | Pendiente |
| Realización y corrección | [07-realizacion-tests.md](07-realizacion-tests.md) | T060-T067 | Pendiente |
| Histórico y revisión | [08-historico.md](08-historico.md) | T070-T074 | Pendiente |
| Estadísticas | [09-estadisticas.md](09-estadisticas.md) | T080-T085 | Pendiente |
| Cierre del MVP | [10-cierre-mvp.md](10-cierre-mvp.md) | T090-T096 | Pendiente |

Las tareas deben ejecutarse en orden salvo que sus dependencias indiquen que
pueden adelantarse.

## Cómo trabajar una tarea

1. Leer la tarea y los documentos enlazados.
2. Confirmar que sus dependencias están terminadas.
3. Implementar solo el alcance indicado.
4. Ejecutar las comprobaciones descritas.
5. Revisar manualmente el comportamiento cuando exista interfaz.
6. Actualizar OpenAPI y documentación si el contrato real cambia.
7. Marcar la tarea como terminada únicamente si no queda trabajo requerido.

Las instrucciones operativas completas para agentes están en
[`../../AGENTS.md`](../../AGENTS.md).

## Registro de inicio y cierre

Al iniciar una tarea se cambia su encabezado de `[ ]` a `[~]`. Al terminar se
cambia a `[x]` y se añade, antes de la tarea siguiente:

```markdown
**Cierre — AAAA-MM-DD**

- Implementado: resumen concreto.
- Verificación: `comando` — OK.
- Documentación: archivos actualizados o "Sin cambios adicionales".
```

Una tarea bloqueada usa `[!]` y debe indicar motivo y requisito para continuar.
No puede marcarse `[x]` si falla una comprobación o queda algún criterio sin
cumplir.

La columna Estado de la tabla se mantiene así:

- `Pendiente`: 0 tareas completadas y ninguna iniciada.
- `En curso (n/total)`: bloque parcialmente completado.
- `Completado (total/total)`: todas sus tareas están cerradas.

## Definición común de terminado

Una tarea se considera terminada cuando:

- Cumple todos sus criterios de comprobación.
- Backend y frontend afectados compilan.
- Las pruebas existentes siguen pasando.
- Se han añadido pruebas proporcionales al cambio.
- No contiene secretos ni datos sensibles.
- No deja código de ejemplo, comentarios pendientes ni errores ignorados.
- La documentación afectada coincide con el comportamiento implementado.
- El encabezado, la evidencia de cierre y el progreso del índice están
  actualizados en los archivos Markdown.

## Situación actual

La preparación del repositorio, la solución .NET, los proyectos, el endpoint
`GET /health` y el frontend inicial ya existen. La primera tarea pendiente es
**T001 - Configurar Problem Details**.
