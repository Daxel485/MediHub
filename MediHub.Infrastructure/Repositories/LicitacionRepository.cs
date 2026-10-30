using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using MediHub.Domain.Entities;
using MediHub.Domain.Interfaces;
using MediHub.Infrastructure.Data;

namespace MediHub.Infrastructure.Repositories
{
    public class LicitacionRepository : ILicitacionRepository
    {
        private readonly MediHubContext _contexto;

        public LicitacionRepository(MediHubContext contexto)
        {
            _contexto = contexto;
        }

        public IReadOnlyList<Licitacion> ObtenerTodas()
        {
            return _contexto.Licitaciones.AsNoTracking().Include(l => l.Ofertas)
                .OrderByDescending(l => l.FechaDeRegistro)
                .ToList();
        }

        public Licitacion ObtenerPorId(int idLicitacion)
        {
            return _contexto.Licitaciones.AsNoTracking().Include(l => l.Ofertas)
                .SingleOrDefault(l => l.IdLicitacion == idLicitacion);
        }

        public bool ExisteTitulo(string titulo)
        {
            return _contexto.Licitaciones.Any(l => l.Titulo == titulo);
        }

        public void Agregar(Licitacion licitacion)
        {
            _contexto.Licitaciones.Add(licitacion);
            _contexto.SaveChanges();
        }

        public IReadOnlyList<Oferta> ObtenerOfertas(int idLicitacion)
        {
            return _contexto.Ofertas.AsNoTracking().Include(o => o.Proveedor)
                .Where(o => o.IdLicitacion == idLicitacion)
                .OrderBy(o => o.FechaDeEnvio)
                .ToList();
        }

        public Oferta ObtenerOferta(int idOferta)
        {
            return _contexto.Ofertas.Find(idOferta);
        }

        public void ActualizarOferta(Oferta oferta)
        {
            _contexto.SaveChanges();
        }
    }
}
