using System.Collections.Generic;
using MediHub.Domain.Entities;

namespace MediHub.Domain.Interfaces
{
    public interface ILicitacionRepository
    {
        IReadOnlyList<Licitacion> ObtenerTodas();
        Licitacion ObtenerPorId(int idLicitacion);
        bool ExisteTitulo(string titulo);
        void Agregar(Licitacion licitacion);
        IReadOnlyList<Oferta> ObtenerOfertas(int idLicitacion);
        Oferta ObtenerOferta(int idOferta);
        void ActualizarOferta(Oferta oferta);
    }
}
