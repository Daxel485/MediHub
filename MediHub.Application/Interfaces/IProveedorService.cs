using System.Collections.Generic;
using MediHub.Application.Common;
using MediHub.Application.DTOs;

namespace MediHub.Application.Interfaces
{
    public interface IProveedorService
    {
        IReadOnlyList<ProveedorDto> ObtenerTodos();
        ProveedorDto ObtenerPorId(int idProveedor);
        Resultado Registrar(ProveedorDto proveedor);
    }
}
