using System;

namespace MediHub.Application.Interfaces
{
    // Desacopla la fecha actual para poder probar los servicios con un reloj fijo.
    public interface IClock
    {
        DateTime Ahora { get; }
    }
}
