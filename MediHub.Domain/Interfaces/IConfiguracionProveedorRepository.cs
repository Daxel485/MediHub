using System.Collections.Generic;
using MediHub.Domain.Entities;

namespace MediHub.Domain.Interfaces
{
    public interface IConfiguracionProveedorRepository
    {
        IReadOnlyList<ConfiguracionProveedor> ObtenerTodas(int? idProveedor = null);
        ConfiguracionProveedor ObtenerPorId(int idConfiguracion);
        bool ExisteParaProveedor(int idProveedor);
        void Agregar(ConfiguracionProveedor configuracion);
        void Actualizar(ConfiguracionProveedor configuracion);
    }
}
