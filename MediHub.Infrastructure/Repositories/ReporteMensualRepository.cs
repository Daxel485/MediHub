using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using MediHub.Domain.Entities;
using MediHub.Domain.Interfaces;
using MediHub.Infrastructure.Data;

namespace MediHub.Infrastructure.Repositories
{
    public class ReporteMensualRepository : IReporteMensualRepository
    {
        private readonly MediHubContext _contexto;

        public ReporteMensualRepository(MediHubContext contexto)
        {
            _contexto = contexto;
        }

        public IReadOnlyList<ReporteMensual> ObtenerTodos()
        {
            return _contexto.Reportes.AsNoTracking().Include(r => r.Proveedor)
                .OrderByDescending(r => r.FechaDelReporte)
                .ThenBy(r => r.Proveedor.Nombre)
                .ToList();
        }

        // Con seguimiento de cambios: el servicio actualiza estos reportes y luego llama a Guardar.
        public IReadOnlyList<ReporteMensual> ObtenerDelPeriodo(DateTime inicio, DateTime fin)
        {
            return _contexto.Reportes
                .Where(r => r.FechaDelReporte >= inicio && r.FechaDelReporte < fin)
                .ToList();
        }

        // Una sola consulta agrupada por proveedor para todas las ofertas del periodo.
        public IReadOnlyList<ResumenMensualProveedor> ObtenerResumenDeOfertas(DateTime inicio, DateTime fin)
        {
            return _contexto.Ofertas.AsNoTracking()
                .Where(o => o.FechaDeEnvio >= inicio && o.FechaDeEnvio < fin)
                .GroupBy(o => o.IdProveedor)
                .Select(g => new ResumenMensualProveedor
                {
                    IdProveedor = g.Key,
                    CantidadDeLicitaciones = g.Select(o => o.IdLicitacion).Distinct().Count(),
                    MontoTotalOfertado = g.Sum(o => o.Monto),
                    CantidadDeOfertas = g.Count()
                })
                .ToList();
        }

        public void Guardar(IEnumerable<ReporteMensual> nuevos)
        {
            _contexto.Reportes.AddRange(nuevos);
            _contexto.SaveChanges();
        }
    }
}
