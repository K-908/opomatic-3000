# AGENTS.md — OpoMatic-3000

Estas instrucciones se aplican a todo el repositorio. Su objetivo es ejecutar
las tareas de `docs/tareas` con poco contexto, cambios pequeños y un cierre
verificable.

## 1. Unidad de trabajo

- Trabaja en una sola tarea `Txxx` por petición, salvo que el usuario pida
  explícitamente agrupar varias.
- No implementes tareas posteriores por adelantado.
- Se permite hacer el cambio mínimo de una dependencia técnica imprescindible,
  pero debe explicarse y documentarse.
- No hagas refactors, cambios visuales o actualizaciones de paquetes ajenos a la
  tarea.
- No crees commits, ramas ni tags salvo petición explícita del usuario.
- Conserva los cambios existentes del usuario y no limpies el worktree.

## 2. Fuentes de verdad

Usa este orden cuando exista una discrepancia:

1. La petición más reciente del usuario.
2. `docs/requisitos.txt`: comportamiento y reglas de negocio.
3. `docs/diseno-api.txt`: contrato HTTP.
4. `docs/diseno-base-datos.txt`: modelo de persistencia.
5. `docs/arquitectura.txt`: estructura y decisiones técnicas.
6. La tarea concreta de `docs/tareas`.
7. `docs/plan-implementacion.txt` y `docs/funcionalidades.txt` como contexto.

No resuelvas silenciosamente una contradicción material. Explica el conflicto y
actualiza los documentos afectados cuando la decisión quede clara.

## 3. Lectura eficiente antes de implementar

Evita cargar toda la documentación en cada tarea.

1. Localiza la tarea:

   ```powershell
   rg -n "^## \[[ x~!]\] Txxx" docs/tareas
   ```

2. Lee únicamente desde su encabezado hasta el siguiente encabezado `##`.
3. Lee solo las secciones de referencia indicadas por esa tarea.
4. Comprueba sus dependencias buscando sus IDs; no releas los archivos completos.
5. Inspecciona el código relacionado con `rg` y abre solo los archivos necesarios.
6. Revisa `git status --short` antes de editar para no pisar trabajo existente.

No navegues por Internet si el repositorio y la documentación ya contienen la
respuesta. Si hace falta verificar una versión o API cambiante, usa únicamente
documentación oficial y registra la decisión relevante.

## 4. Inicio de una tarea

Antes del primer cambio funcional:

- Confirma que todas sus dependencias están `[x]`.
- Cambia el encabezado de la tarea de `[ ]` a `[~]` mediante `apply_patch`.
- Actualiza la fila del bloque en `docs/tareas/README.md` a
  `En curso (completadas/total)` si todavía figuraba como pendiente.
- Comunica en una frase el resultado que se pretende obtener.
- Usa un plan solo cuando existan varias operaciones dependientes; mantenlo corto.

Si una dependencia no está cerrada, no marques la tarea como iniciada. Informa
del problema o ejecuta primero la dependencia si el usuario lo autoriza.

## 5. Implementación

- Usa `apply_patch` para editar archivos manualmente.
- Usa los generadores oficiales solo para scaffolding, migraciones o código
  mecánico que corresponda a la tarea.
- Mantén la dirección de dependencias:

  ```text
  Api -> Application -> Domain
   |          ^
   `-> Infrastructure
  ```

- Domain no referencia ASP.NET Core ni Entity Framework Core.
- Los contratos HTTP no son entidades de persistencia.
- Las reglas de puntuación viven en un único punto del backend.
- Toda entrada se valida en el backend; el frontend añade validación para mejorar
  la experiencia, no como barrera de integridad.
- No uses `localStorage` o `sessionStorage` para tests en curso en el MVP.
- No añadas un repositorio genérico sobre EF Core.
- No guardes secretos en archivos versionados.
- Actualiza OpenAPI cuando cambie un endpoint o contrato.
- Actualiza una migración o crea otra cuando cambie el esquema; no ajustes SQL
  manualmente como sustituto.

## 6. Uso eficiente de herramientas y tokens

- Busca primero con `rg`; evita imprimir archivos grandes completos.
- Agrupa lecturas independientes, pero no ejecutes en paralelo herramientas que
  escriban el mismo caché o proyecto.
- Durante la implementación ejecuta la prueba más pequeña relacionada.
- Ejecuta las validaciones completas una sola vez antes del cierre.
- No reinstales ni actualices dependencias si los lockfiles ya satisfacen la
  tarea.
- No repitas en mensajes el contenido de los documentos; informa de resultados,
  decisiones o bloqueos.
- No generes imágenes, diagramas ni artefactos que la tarea no solicite.
- No uses subagentes salvo petición explícita del usuario.

En PowerShell usa `npm.cmd` en lugar de `npm`, porque `npm.ps1` puede estar
bloqueado por la política de ejecución del equipo.

## 7. Validación proporcional

Selecciona primero las comprobaciones específicas de la tarea.

Backend:

```powershell
dotnet build OpoMatic3000.sln --no-restore
dotnet test OpoMatic3000.sln --no-restore
```

Frontend:

```powershell
Set-Location frontend
npm.cmd run lint
npm.cmd run test:run
npm.cmd run build
```

`test:run` será obligatorio desde T004. Antes de que exista ese script, indícalo
como no aplicable en vez de inventar una comprobación.

Además:

- Si cambia un endpoint, prueba su estado, cuerpo y Problem Details.
- Si cambia SQL Server, aplica migraciones desde una base de prueba y verifica
  restricciones reales.
- Si cambia una pantalla, prueba loading, error, empty, teclado y recorrido feliz.
- Si una comprobación no puede ejecutarse, no ocultes el motivo ni la declares
  superada.

## 8. Requisitos obligatorios para cerrar una tarea

Una tarea solo puede pasar a `[x]` cuando:

1. Se ha implementado todo su apartado **Qué hacer**.
2. Se cumple su apartado **Comprobación**.
3. Compilan todos los proyectos afectados.
4. Pasan las pruebas relevantes y no se rompen las existentes.
5. Se añadieron o actualizaron pruebas proporcionales al riesgo.
6. No quedan errores ignorados, código de plantilla, secretos o TODOs creados por
   la tarea.
7. OpenAPI, README y documentos técnicos afectados coinciden con el código.
8. `git diff --check` no informa de errores de whitespace.

No cierres una tarea únicamente porque el código compila.

## 9. Cierre documental obligatorio

En el mismo turno en que se completa el trabajo:

1. Cambia el encabezado de `[~]` a `[x]` en su archivo de `docs/tareas`.
2. Añade al final de la tarea, antes del siguiente encabezado `##`, este bloque:

   ```markdown
   **Cierre — AAAA-MM-DD**

   - Implementado: resumen concreto de una o dos líneas.
   - Verificación: `comando` — OK; `otro comando` — OK.
   - Documentación: archivos actualizados, o "Sin cambios adicionales".
   ```

3. Cuenta las tareas `[x]` del archivo y actualiza su fila en
   `docs/tareas/README.md`:
   - `Pendiente` si hay 0 completadas y ninguna en curso.
   - `En curso (n/total)` si solo una parte está completada.
   - `Completado (total/total)` cuando todas estén `[x]`.
4. Actualiza `docs/plan-implementacion.txt` solo si cambia el estado de una
   entrega completa.
5. Actualiza `README.md` solo si cambian instalación, ejecución, estructura o
   comportamiento visible relevante.

El cierre documental forma parte de la tarea. Si falta, la tarea sigue `[~]`.

## 10. Tareas bloqueadas o incompletas

- Si existe un bloqueo externo real, cambia `[~]` a `[!]` y añade:

  ```markdown
  **Bloqueo — AAAA-MM-DD**

  - Motivo: causa concreta.
  - Necesario para continuar: acción o decisión requerida.
  - Verificación ya realizada: comandos y resultados relevantes.
  ```

- No uses `[!]` porque el trabajo sea difícil o necesite más tiempo.
- Si la tarea queda parcialmente implementada pero no bloqueada, mantenla `[~]`
  y describe con precisión lo que falta en la respuesta final.
- Nunca añadas un bloque de cierre a una tarea fallida o incompleta.

## 11. Respuesta final de una tarea

Debe ser breve y contener:

- Resultado implementado.
- Archivos o áreas principales modificadas.
- Verificaciones ejecutadas y su resultado.
- Estado documental (`Txxx [x]`, `[~]` o `[!]`).
- Próxima tarea desbloqueada, sin empezar a implementarla.

