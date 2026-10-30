using System.Collections.Generic;
using MediHub.Application.Common;
using MediHub.Application.DTOs;

namespace MediHub.Application.Interfaces
{
    public interface IUsuarioService
    {
        IReadOnlyList<UsuarioDto> ObtenerTodos();
        UsuarioDto ObtenerPorId(int idUsuario);
        Resultado Registrar(UsuarioDto usuario);
        Resultado Editar(UsuarioDto usuario);
    }
}
