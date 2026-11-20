# MediHub

Plataforma web B2B de abastecimiento y licitaciones para la Clínica Integral San Rafael.
Proyecto del curso Programación Avanzada (Universidad Fidélitas), Grupo 3.

## Tecnologías

- .NET Framework 4.8 y ASP.NET MVC 5, con vistas Razor y Bootstrap 5
- Entity Framework 6 (Code First) sobre SQL Server LocalDB
- Autofac (con un módulo de registro por capa), AutoMapper y FluentValidation
- Pruebas con xUnit, Moq y FluentAssertions

## Estructura de la solución

| Proyecto | Responsabilidad |
|---|---|
| `MediHub.Domain` | Entidades, enumeraciones e interfaces de repositorio. No depende de ningún framework |
| `MediHub.Application` | Servicios, DTOs, validadores, perfiles de AutoMapper y `ApplicationModule` de Autofac |
| `MediHub.Infrastructure` | `MediHubContext` (EF6), migraciones, repositorios, `SystemClock` e `InfrastructureModule` |
| `MediHub.Web` | Controladores, ViewModels, vistas, filtro global de excepciones y `AutofacConfig` |
| `MediHub.Tests` | Pruebas unitarias del dominio y de los servicios de aplicación |

## Cómo ejecutarlo

1. Abrir `MediHub.sln` en Visual Studio 2022 o posterior.
2. Compilar la solución; los paquetes NuGet se restauran automáticamente.
3. Ejecutar `MediHub.Web` con **Ctrl + F5**.

La base de datos `MediHubDb` se crea sola en `(LocalDb)\MSSQLLocalDB` la primera vez que se abre una página,
aplicando la migración `CrearMediHub` y cargando datos de ejemplo (3 proveedores, 3 licitaciones, 5 ofertas y 2 usuarios).
La cadena de conexión `MediHubConnection` está en `MediHub.Web/Web.config`.

Las pruebas se ejecutan desde el Explorador de pruebas de Visual Studio.

## Avance 2: módulos

El enunciado del avance (SINPE Empresarial) se aplicó al dominio de MediHub con esta equivalencia:

| Enunciado | MediHub |
|---|---|
| Comercio | Proveedor |
| Caja | Licitación |
| SINPE | Oferta |
| Monto recaudado | Monto total ofertado |

### Configuración de proveedor

Se accede desde la lista de proveedores (botón **Configuración**) o desde el menú.

- **Tipo de configuración:** 1 Plataforma (las ofertas se sincronizan desde MediHub), 2 Externa (el proveedor consulta por API) o 3 Ambas.
- **Listar:** muestra nombre del proveedor, tipo en prosa, comisión, fechas de registro y modificación, estado y la acción Editar.
- **Registrar:** solo se permite una configuración por proveedor. Se valida antes de guardar y, si ya existe, se muestra el mensaje y se vuelve a la vista de crear. Además hay un índice único en `IdProveedor`. Las fechas y el estado no se piden al usuario.
- **Editar:** solo cambia tipo, comisión y estado, y actualiza `FechaDeModificacion` automáticamente.

### Reportes mensuales

- **Listar:** muestra proveedor, cantidad de licitaciones, monto ofertado, cantidad de ofertas, comisión y fecha del reporte.
- **Generar reportes:** un único método (`ReporteMensualService.GenerarReportes`) calcula el mes actual de todos los proveedores con una sola consulta agrupada. Si el proveedor ya tiene reporte del mes, lo actualiza en lugar de crear otro.
- `MontoTotalComision = MontoTotalOfertado × Comision / 100` (una comisión de 20 equivale a 20 %). Si el proveedor no tiene configuración o la tiene inactiva, la comisión es 0.

### Usuarios

- **Listar:** muestra nombres, apellidos, identificación, correo, fechas, estado y Editar.
- **Registrar:** no permite dos usuarios con la misma identificación (validación previa e índice único). `IdNetUser` queda en `null` hasta que se integre ASP.NET Identity.
- **Editar:** solo cambia nombres, apellidos, identificación, correo y estado, y actualiza `FechaDeModificacion`.

### Licitaciones: sincronizar ofertas

En **Licitaciones → Gestionar**, cada oferta no sincronizada tiene el botón **Sincronizar**, que cambia su estado a sincronizada.
Una oferta ya sincronizada no se puede volver a sincronizar. El título de la licitación es único.
Solo se sincronizan ofertas mientras la licitación está *Publicada* o *En evaluación*.

## Avance 3 (Persona 3): ciclo de vida y notificaciones

### Patrón State: estados de la licitación

```
Borrador ──Publicar──▶ Publicada ──Pasar a evaluación──▶ En evaluación ──Adjudicar──▶ Adjudicada
    │                      │                                  │                          │
    └──────────────────────┴──────────────Cerrar──────────────┴──────────────────────────┴──▶ Cerrada
```

- `MediHub.Domain/Estados`: `EstadoLicitacionBase` (estado abstracto) y un estado concreto por etapa
  (`EstadoBorrador`, `EstadoPublicada`, `EstadoEnEvaluacion`, `EstadoAdjudicada`, `EstadoCerrada`).
  La clase base prohíbe todas las transiciones; cada estado sobrescribe solo las que admite.
- `Licitacion` guarda la etapa en `EstadoDelProceso` (enum `EstadoLicitacion`, columna `int`) y expone `EstadoActual`.
  `Publicar`, `IniciarEvaluacion`, `Adjudicar` y `Cerrar` delegan en el estado actual.
- Reglas: solo se publica si la fecha de cierre no ha pasado; solo se evalúa con al menos una oferta;
  solo se adjudica una oferta sincronizada de la misma licitación (queda en `IdOfertaAdjudicada`); *Cerrada* es final.
- Una licitación nueva queda en *Borrador*. La migración `CicloDeVidaLicitacion` deja las existentes como *Publicada*.
- Los botones de la vista **Gestionar** dependen de lo que permite el estado (`PuedePublicar`, `PuedeAdjudicar`, ...).

### Patrón Factory Method: notificaciones por correo

- `MediHub.Application/Notificaciones`: `CreadorDeNotificacion` define `Enviar` (crear y enviar) y el método fábrica
  abstracto `CrearNotificacion`. Cada creador concreto (`CreadorNotificacionPublicacion`, `CreadorNotificacionAdjudicacion`,
  `CreadorNotificacionRechazo`) devuelve su producto (`NotificacionPublicacion`, `NotificacionAdjudicacion`, `NotificacionRechazo`).
- El envío se hace con `IServicioCorreo`, implementado por `ServicioCorreoSmtp` en infraestructura.

| Transición | Correos |
|---|---|
| Publicar | Publicación a todos los proveedores activos |
| Adjudicar | Adjudicación al proveedor ganador y rechazo a los demás oferentes (uno por proveedor) |
| Cerrar desde *Publicada* o *En evaluación* (desierta) | Rechazo a todos los oferentes |
| Pasar a evaluación, cerrar un borrador o una adjudicada | Ninguno |

### Conexión en `LicitacionService`

`Publicar`, `IniciarEvaluacion`, `Adjudicar` y `Cerrar` siguen el mismo flujo: cargar la licitación, delegar la transición
al estado, guardar, registrar el cambio en la bitácora y enviar las notificaciones con la fábrica correspondiente.
Una transición no permitida devuelve el mensaje del estado sin guardar nada. Si un correo falla, la transición se conserva,
el error queda en la bitácora (tabla `Notificacion`) y el mensaje indica cuántos correos no se enviaron.

En desarrollo, `Web.config` (`system.net/mailSettings`) guarda los correos como archivos `.eml` en `MediHub.Web/App_Data/Correos`.
Para enviarlos por un servidor SMTP real, cambie `deliveryMethod` a `Network` y configure el elemento `<network>`.

### Bitácora

La tabla `BITACORA_EVENTOS` registra cada evento de Registrar, Editar o Error con la tabla afectada, la fecha,
la descripción y los datos anteriores y posteriores en JSON. Los errores guardan también el `StackTrace`.

- Los servicios capturan las excepciones, las registran y devuelven un mensaje controlado a la vista.
- `BitacoraExceptionFilter` registra cualquier excepción que llegue sin controlar al controlador.
- La bitácora usa su propio `DbContext`, para que un error en el contexto de la solicitud no impida registrarlo.

## Tablas

`Proveedor`, `ConfiguracionProveedor`, `ReporteMensual`, `Usuario`, `Licitacion`, `Oferta` y `BITACORA_EVENTOS`.
Las columnas de `ConfiguracionProveedor`, `ReporteMensual` y `Usuario` siguen los tipos del enunciado:
`int` para la comisión, `decimal` para los montos, `varchar` con las longitudes indicadas y `bit` para el estado.
