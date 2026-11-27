using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using MediHub.Domain.Entities;
using MediHub.Domain.Enums;
using MediHub.Domain.Interfaces;
using MediHub.Infrastructure.Data;

namespace MediHub.Infrastructure.Repositories
{
    public class PortalOfertasRepository : IPortalOfertasRepository
    {
        private readonly MediHubContext _contexto;

        public PortalOfertasRepository(MediHubContext contexto)
        {
            _contexto = contexto;
        }

        public Licitacion ObtenerLicitacion(int idLicitacion)
        {
            return _contexto.Licitaciones.AsNoTracking().SingleOrDefault(l => l.IdLicitacion == idLicitacion);
        }

        // Vigente = publicada (patrón State), activa y con la recepción abierta.
        private IQueryable<Licitacion> ConsultarVigentes(DateTime ahora)
        {
            return _contexto.Licitaciones.AsNoTracking()
                .Where(l => l.Estado && l.EstadoDelProceso == EstadoLicitacion.Publicada && l.FechaDeCierre > ahora
                    && !_contexto.Adjudicaciones.Any(a => a.IdLicitacion == l.IdLicitacion));
        }

        public IReadOnlyList<Licitacion> ObtenerVigentes(DateTime ahora)
        {
            return ConsultarVigentes(ahora).OrderBy(l => l.FechaDeCierre).ToList();
        }

        public Licitacion ObtenerVigente(int idLicitacion, DateTime ahora)
        {
            return ConsultarVigentes(ahora).SingleOrDefault(l => l.IdLicitacion == idLicitacion);
        }

        public T EjecutarEnLicitacion<T>(int idLicitacion, Func<Licitacion, T> operacion)
        {
            _contexto.Database.Initialize(false);
            using (var transaccion = _contexto.Database.BeginTransaction())
            {
                // Serializa los envíos y la selección del ganador para una misma licitación.
                var id = _contexto.Database.SqlQuery<int?>(
                    "SELECT IdLicitacion FROM dbo.Licitacion WITH (UPDLOCK, HOLDLOCK) WHERE IdLicitacion = @p0",
                    idLicitacion).SingleOrDefault();
                var licitacion = id.HasValue ? _contexto.Licitaciones.Find(id.Value) : null;
                var resultado = operacion(licitacion);
                _contexto.SaveChanges();
                transaccion.Commit();
                return resultado;
            }
        }

        public IReadOnlyList<Oferta> ObtenerOfertas(int idLicitacion)
        {
            return _contexto.Ofertas.AsNoTracking().Include(o => o.Proveedor).Include(o => o.Detalles)
                .Where(o => o.IdLicitacion == idLicitacion).OrderBy(o => o.FechaDeEnvio).ToList();
        }

        public Oferta ObtenerOferta(int idLicitacion, int idOferta)
        {
            return _contexto.Ofertas.SingleOrDefault(o => o.IdOferta == idOferta && o.IdLicitacion == idLicitacion);
        }

        public bool TieneOferta(int idLicitacion, int idProveedor)
        {
            return _contexto.Ofertas.Any(o => o.IdLicitacion == idLicitacion && o.IdProveedor == idProveedor);
        }

        public int? ObtenerProveedorAutorizado(Guid idNetUser)
        {
            var proveedores = _contexto.Usuarios.AsNoTracking()
                .Where(u => u.IdNetUser == idNetUser && u.Estado && u.Proveedor.Estado)
                .Select(u => u.IdProveedor).Distinct().Take(2).ToList();
            return proveedores.Count == 1 ? (int?)proveedores[0] : null;
        }

        public void AgregarOferta(Oferta oferta) => _contexto.Ofertas.Add(oferta);
        public void AgregarAdjudicacion(Adjudicacion adjudicacion) => _contexto.Adjudicaciones.Add(adjudicacion);
        public void AgregarEvento(BitacoraEvento evento) => _contexto.Bitacora.Add(evento);

        public int? ObtenerOfertaGanadora(int idLicitacion)
        {
            return _contexto.Adjudicaciones.AsNoTracking()
                .Where(a => a.IdLicitacion == idLicitacion).Select(a => (int?)a.IdOferta).SingleOrDefault();
        }
    }
}
