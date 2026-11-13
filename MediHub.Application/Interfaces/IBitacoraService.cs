using System;
using MediHub.Domain.Enums;

namespace MediHub.Application.Interfaces
{
    public interface IBitacoraService
    {
        void RegistrarEvento(string tabla, TipoEvento tipo, string descripcion,
            object datosAnteriores = null, object datosPosteriores = null);

        void RegistrarError(string tabla, Exception excepcion);
    }
}
