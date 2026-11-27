using System.Collections.Generic;
using MediHub.Domain.Entities;

namespace MediHub.Domain.Interfaces
{
    public interface ILicitacionRepository
    {
        IReadOnlyList<Licitacion> ObtenerTodas();
        Licitacion ObtenerPorId(int idLicitacion);

        // Devuelve la licitación con seguimiento de cambios, sus ofertas y el proveedor de cada oferta.
        Licitacion ObtenerParaActualizar(int idLicitacion);
        bool ExisteTitulo(string titulo);
        void Agregar(Licitacion licitacion);
        void Actualizar(Licitacion licitacion);
        IReadOnlyList<Oferta> ObtenerOfertas(int idLicitacion);
        Oferta ObtenerOferta(int idOferta);
        void ActualizarOferta(Oferta oferta);
    }
}
