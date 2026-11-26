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
        void Actualizar(Licitacion licitacion);
        IReadOnlyList<Oferta> ObtenerOfertas(int idLicitacion);
        Oferta ObtenerOferta(int idOferta);
        void ActualizarOferta(Oferta oferta);

        // Items
        ItemLicitacion ObtenerItem(int idItem);
        void AgregarItem(ItemLicitacion item);
        void EliminarItem(ItemLicitacion item);
    }
}
