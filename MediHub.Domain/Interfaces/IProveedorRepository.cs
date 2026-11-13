using System.Collections.Generic;
using MediHub.Domain.Entities;

namespace MediHub.Domain.Interfaces
{
    public interface IProveedorRepository
    {
        IReadOnlyList<Proveedor> ObtenerTodos();
        Proveedor ObtenerPorId(int idProveedor);
        bool ExisteIdentificacion(string identificacion);
        void Agregar(Proveedor proveedor);
    }
}
