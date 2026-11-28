using System;
using System.Collections.Generic;
using MediHub.Domain.Entities;

namespace MediHub.Domain.Interfaces
{
    public interface IUsuarioRepository
    {
        IReadOnlyList<Usuario> ObtenerTodos();
        Usuario ObtenerPorId(int idUsuario);
        Usuario ObtenerPorIdentificacion(string identificacion);
        Usuario ObtenerPorIdNetUser(Guid idNetUser);
        bool ExisteIdentificacion(string identificacion, int? excluirIdUsuario = null);
        void Agregar(Usuario usuario);
        void Actualizar(Usuario usuario);
    }
}
