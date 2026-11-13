using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using MediHub.Application.Common;
using MediHub.Application.DTOs;
using MediHub.Application.Interfaces;
using MediHub.Domain.Entities;
using MediHub.Domain.Enums;
using MediHub.Domain.Interfaces;

namespace MediHub.Application.Services
{
    public class ReporteMensualService : ServicioBase, IReporteMensualService
    {
        private const string Tabla = "ReporteMensual";

        private readonly IReporteMensualRepository _repositorio;
        private readonly IProveedorRepository _proveedores;
        private readonly IConfiguracionProveedorRepository _configuraciones;
        private readonly IMapper _mapper;
        private readonly IClock _clock;

        public ReporteMensualService(IReporteMensualRepository repositorio,
            IProveedorRepository proveedores, IConfiguracionProveedorRepository configuraciones,
            IMapper mapper, IClock clock, IBitacoraService bitacora) : base(bitacora)
        {
            _repositorio = repositorio;
            _proveedores = proveedores;
            _configuraciones = configuraciones;
            _mapper = mapper;
            _clock = clock;
        }

        public IReadOnlyList<ReporteMensualDto> ObtenerTodos()
        {
            return _mapper.Map<List<ReporteMensualDto>>(_repositorio.ObtenerTodos());
        }

        // Genera o actualiza en una sola operación el reporte del mes actual de todos los proveedores.
        public Resultado GenerarReportes()
        {
            return Ejecutar(Tabla, () =>
            {
                var ahora = _clock.Ahora;
                var inicioDelMes = ahora.Date.AddDays(1 - ahora.Day);
                var inicioDelSiguienteMes = inicioDelMes.AddMonths(1);

                var resumenes = _repositorio.ObtenerResumenDeOfertas(inicioDelMes, inicioDelSiguienteMes)
                    .ToDictionary(r => r.IdProveedor);
                var configuraciones = _configuraciones.ObtenerTodas()
                    .ToDictionary(c => c.IdProveedor);
                var existentes = _repositorio.ObtenerDelPeriodo(inicioDelMes, inicioDelSiguienteMes)
                    .ToDictionary(r => r.IdProveedor);

                var nuevos = new List<ReporteMensual>();
                foreach (var proveedor in _proveedores.ObtenerTodos())
                {
                    ResumenMensualProveedor resumen;
                    if (!resumenes.TryGetValue(proveedor.IdProveedor, out resumen))
                        resumen = new ResumenMensualProveedor { IdProveedor = proveedor.IdProveedor };

                    ReporteMensual reporte;
                    if (!existentes.TryGetValue(proveedor.IdProveedor, out reporte))
                    {
                        reporte = new ReporteMensual { IdProveedor = proveedor.IdProveedor };
                        nuevos.Add(reporte);
                    }

                    reporte.CantidadDeLicitaciones = resumen.CantidadDeLicitaciones;
                    reporte.MontoTotalOfertado = resumen.MontoTotalOfertado;
                    reporte.CantidadDeOfertas = resumen.CantidadDeOfertas;
                    reporte.MontoTotalComision = CalcularComision(configuraciones, resumen);
                    reporte.FechaDelReporte = ahora;
                }

                _repositorio.Guardar(nuevos);

                var actualizados = existentes.Count;
                Bitacora.RegistrarEvento(Tabla, nuevos.Count > 0 ? TipoEvento.Registrar : TipoEvento.Editar,
                    string.Format("Reportes de {0:MM/yyyy}: {1} generados y {2} actualizados.",
                        ahora, nuevos.Count, actualizados));
                return Resultado.Correcto(string.Format(
                    "Reportes de {0:MM/yyyy} listos: {1} nuevos y {2} actualizados.",
                    ahora, nuevos.Count, actualizados));
            });
        }

        private static decimal CalcularComision(
            IDictionary<int, ConfiguracionProveedor> configuraciones, ResumenMensualProveedor resumen)
        {
            ConfiguracionProveedor configuracion;
            if (!configuraciones.TryGetValue(resumen.IdProveedor, out configuracion) || !configuracion.Estado)
                return 0m;
            return configuracion.CalcularComision(resumen.MontoTotalOfertado);
        }
    }
}
