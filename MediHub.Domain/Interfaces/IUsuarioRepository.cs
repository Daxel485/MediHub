using System.Collections.Generic;
using MediHub.Domain.Entities;

namespace MediHub.Domain.Interfaces
{
    public interface IUsuarioRepository
    {
        IReadOnlyList<Usuario> ObtenerTodos();
        Usuario ObtenerPorId(int idUsuario);
        bool ExisteIdentificacion(string identificacion, int? excluirIdUsuario = null);
        void Agregar(Usuario usuario);
        void Actualizar(Usuario usuario);
    }
}
