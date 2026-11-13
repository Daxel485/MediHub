using MediHub.Domain.Entities;

namespace MediHub.Domain.Interfaces
{
    public interface IBitacoraRepository
    {
        void Registrar(BitacoraEvento evento);
    }
}
