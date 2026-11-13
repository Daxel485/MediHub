using MediHub.Domain.Entities;
using MediHub.Domain.Interfaces;
using MediHub.Infrastructure.Data;

namespace MediHub.Infrastructure.Repositories
{
    public class BitacoraRepository : IBitacoraRepository
    {
        // Usa su propio contexto: si el contexto de la solicitud quedó con cambios inválidos
        // tras un error, el registro de la bitácora no debe intentar guardarlos de nuevo.
        public void Registrar(BitacoraEvento evento)
        {
            using (var contexto = new MediHubContext())
            {
                contexto.Bitacora.Add(evento);
                contexto.SaveChanges();
            }
        }
    }
}
