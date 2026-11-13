using System.Collections.Generic;
using System.Linq;
using MediHub.Domain.Entities;
using MediHub.Domain.Interfaces;
using MediHub.Infrastructure.Data;

namespace MediHub.Infrastructure.Repositories
{
    public class ProveedorRepository : IProveedorRepository
    {
        private readonly MediHubContext _contexto;

        public ProveedorRepository(MediHubContext contexto)
        {
            _contexto = contexto;
        }

        public IReadOnlyList<Proveedor> ObtenerTodos()
        {
            return _contexto.Proveedores.AsNoTracking().OrderBy(p => p.Nombre).ToList();
        }

        public Proveedor ObtenerPorId(int idProveedor)
        {
            return _contexto.Proveedores.Find(idProveedor);
        }

        public bool ExisteIdentificacion(string identificacion)
        {
            return _contexto.Proveedores.Any(p => p.Identificacion == identificacion);
        }

        public void Agregar(Proveedor proveedor)
        {
            _contexto.Proveedores.Add(proveedor);
            _contexto.SaveChanges();
        }
    }
}
