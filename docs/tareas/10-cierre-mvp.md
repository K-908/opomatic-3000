# 10. Cierre del MVP

Objetivo del bloque: convertir las funcionalidades terminadas en una versión
inicial coherente, reproducible y utilizable localmente.

Referencias: `../requisitos.txt` completo, `../arquitectura.txt`, secciones 14-17,
y `../plan-implementacion.txt`, Entrega 10.

## [ ] T090 - Unificar estados y mensajes de interfaz

**Depende de:** T085.

**Objetivo:** evitar que cada pantalla resuelva loading/error/empty de forma
distinta.

**Qué hacer:** auditar pantallas, extraer componentes compartidos cuando exista
reutilización real, revisar Problem Details y confirmar acciones destructivas.

**Comprobación:** no hay pantallas en blanco, errores técnicos crudos ni acciones
sin feedback.

## [ ] T091 - Revisar accesibilidad y teclado

**Depende de:** T090.

**Objetivo:** completar los recorridos principales sin ratón.

**Qué hacer:** revisar HTML semántico, encabezados, labels, foco, diálogos,
contraste, anuncios de errores y que el resultado no dependa solo del color.

**Comprobación:** crear pregunta, hacer test y revisar resultado funcionan con
teclado y foco visible.

## [ ] T092 - Revisar comportamiento responsive

**Depende de:** T091.

**Objetivo:** evitar que el MVP quede bloqueado en pantallas estrechas.

**Qué hacer:** probar anchuras representativas, navegación, tablas, formularios,
runner y diálogos; corregir desbordamientos y tamaños de interacción.

**Comprobación:** todas las funciones son utilizables desde 320 px aunque el
pulido móvil completo siga siendo posterior.

## [ ] T093 - Preparar Playwright y el entorno E2E

**Depende de:** T092.

**Objetivo:** ejecutar frontend, API y base de pruebas de forma reproducible.

**Qué hacer:** instalar/configurar Playwright, comandos de arranque, datos
aislados, esperas por salud y limpieza; no depender de la base personal.

**Comprobación:** una prueba mínima abre la portada y detecta ambos servicios.

## [ ] T094 - Automatizar el recorrido principal E2E

**Depende de:** T093.

**Objetivo:** proteger el valor principal del producto.

**Qué hacer:** automatizar crear tema/pregunta, generar test, responder,
finalizar, revisar histórico y confirmar actualización estadística.

**Comprobación:** el recorrido pasa desde una base limpia y falla con un mensaje
útil si se rompe un paso.

## [ ] T095 - Validar instalación desde cero

**Depende de:** T094.

**Objetivo:** asegurar que el repositorio no depende del equipo actual.

**Qué hacer:** restaurar paquetes, configurar User Secrets, crear base mediante
migraciones, compilar, probar y arrancar siguiendo solo README; completar
resolución de problemas habituales.

**Comprobación:** una base vacía y dependencias limpias producen una aplicación
funcional sin cambios manuales en SQL Server.

## [ ] T096 - Auditar requisitos y cerrar el MVP

**Depende de:** T095.

**Objetivo:** confirmar formalmente que la versión inicial cumple lo acordado.

**Qué hacer:** recorrer cada criterio HU-01 a HU-10 y RN/RNF, registrar evidencia,
actualizar OpenAPI/documentos, anotar limitaciones y crear backlog posterior.

**Comprobación:** no queda criterio básico sin cumplir o justificar; build, lint,
pruebas unitarias, integración y E2E pasan antes de etiquetar la versión.

