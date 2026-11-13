using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using MediHub.Domain.Entities;
using MediHub.Domain.Interfaces;
using MediHub.Infrastructure.Data;

namespace MediHub.Infrastructure.Repositories
{
    public class ConfiguracionProveedorRepository : IConfiguracionProveedorRepository
    {
        private readonly MediHubContext _contexto;

        public ConfiguracionProveedorRepository(MediHubContext contexto)
        {
            _contexto = contexto;
        }

        public IReadOnlyList<ConfiguracionProveedor> ObtenerTodas(int? idProveedor = null)
        {
            var consulta = _contexto.Configuraciones.AsNoTracking().Include(c => c.Proveedor);
            if (idProveedor.HasValue)
                consulta = consulta.Where(c => c.IdProveedor == idProveedor.Value);
            return consulta.OrderBy(c => c.Proveedor.Nombre).ToList();
        }

        public ConfiguracionProveedor ObtenerPorId(int idConfiguracion)
        {
            return _contexto.Configuraciones.Include(c => c.Proveedor)
                .SingleOrDefault(c => c.IdConfiguracion == idConfiguracion);
        }

        public bool ExisteParaProveedor(int idProveedor)
        {
            return _contexto.Configuraciones.Any(c => c.IdProveedor == idProveedor);
        }

        public void Agregar(ConfiguracionProveedor configuracion)
        {
            _contexto.Configuraciones.Add(configuracion);
            _contexto.SaveChanges();
        }

        public void Actualizar(ConfiguracionProveedor configuracion)
        {
            _contexto.SaveChanges();
        }
    }
}
