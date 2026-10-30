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

### Configuración de proveedor (30 pts)

Se accede desde la lista de proveedores (botón **Configuración**) o desde el menú.

- **Tipo de configuración:** 1 Plataforma (las ofertas se sincronizan desde MediHub), 2 Externa (el proveedor consulta por API) o 3 Ambas.
- **Listar:** muestra nombre del proveedor, tipo en prosa, comisión, fechas de registro y modificación, estado y la acción Editar.
- **Registrar:** solo se permite una configuración por proveedor. Se valida antes de guardar y, si ya existe, se muestra el mensaje y se vuelve a la vista de crear. Además hay un índice único en `IdProveedor`. Las fechas y el estado no se piden al usuario.
- **Editar:** solo cambia tipo, comisión y estado, y actualiza `FechaDeModificacion` automáticamente.

### Reportes mensuales (30 pts)

- **Listar:** muestra proveedor, cantidad de licitaciones, monto ofertado, cantidad de ofertas, comisión y fecha del reporte.
- **Generar reportes:** un único método (`ReporteMensualService.GenerarReportes`) calcula el mes actual de todos los proveedores con una sola consulta agrupada. Si el proveedor ya tiene reporte del mes, lo actualiza en lugar de crear otro.
- `MontoTotalComision = MontoTotalOfertado × Comision / 100` (una comisión de 20 equivale a 20 %). Si el proveedor no tiene configuración o la tiene inactiva, la comisión es 0.

### Usuarios (20 pts)

- **Listar:** muestra nombres, apellidos, identificación, correo, fechas, estado y Editar.
- **Registrar:** no permite dos usuarios con la misma identificación (validación previa e índice único). `IdNetUser` queda en `null` hasta que se integre ASP.NET Identity.
- **Editar:** solo cambia nombres, apellidos, identificación, correo y estado, y actualiza `FechaDeModificacion`.

### Licitaciones: sincronizar ofertas (10 pts)

En **Licitaciones → Ver ofertas**, cada oferta no sincronizada tiene el botón **Sincronizar**, que cambia su estado a sincronizada.
Una oferta ya sincronizada no se puede volver a sincronizar. El título de la licitación es único.

### Bitácora (10 pts)

La tabla `BITACORA_EVENTOS` registra cada evento de Registrar, Editar o Error con la tabla afectada, la fecha,
la descripción y los datos anteriores y posteriores en JSON. Los errores guardan también el `StackTrace`.

- Los servicios capturan las excepciones, las registran y devuelven un mensaje controlado a la vista.
- `BitacoraExceptionFilter` registra cualquier excepción que llegue sin controlar al controlador.
- La bitácora usa su propio `DbContext`, para que un error en el contexto de la solicitud no impida registrarlo.

## Tablas

`Proveedor`, `ConfiguracionProveedor`, `ReporteMensual`, `Usuario`, `Licitacion`, `Oferta` y `BITACORA_EVENTOS`.
Las columnas de `ConfiguracionProveedor`, `ReporteMensual` y `Usuario` siguen los tipos del enunciado:
`int` para la comisión, `decimal` para los montos, `varchar` con las longitudes indicadas y `bit` para el estado.
