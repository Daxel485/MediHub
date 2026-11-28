using System;
using System.Collections.Generic;
using MediHub.Application.Common;
using MediHub.Application.DTOs;

namespace MediHub.Application.Interfaces
{
    public interface IUsuarioService
    {
        IReadOnlyList<UsuarioDto> ObtenerTodos();
        UsuarioDto ObtenerPorId(int idUsuario);
        // Devuelve el usuario enlazado con la cuenta de Identity (Usuario.IdNetUser), o null si no hay.
        UsuarioDto ObtenerPorIdNetUser(Guid idNetUser);
        Resultado Registrar(UsuarioDto usuario);
        // Enlaza una cuenta de Identity con un usuario de proveedor: crea el registro o enlaza el que ya existe.
        Resultado VincularCuenta(UsuarioDto usuario, Guid idNetUser);
        Resultado Editar(UsuarioDto usuario);
    }
}
