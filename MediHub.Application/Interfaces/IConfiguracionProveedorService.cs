using System.Collections.Generic;
using MediHub.Application.Common;
using MediHub.Application.DTOs;

namespace MediHub.Application.Interfaces
{
    public interface IConfiguracionProveedorService
    {
        IReadOnlyList<ConfiguracionProveedorDto> ObtenerTodas(int? idProveedor = null);
        ConfiguracionProveedorDto ObtenerPorId(int idConfiguracion);
        Resultado Registrar(ConfiguracionProveedorDto configuracion);
        Resultado Editar(ConfiguracionProveedorDto configuracion);
    }
}
