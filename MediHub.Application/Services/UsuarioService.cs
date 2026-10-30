using System.Collections.Generic;
using AutoMapper;
using FluentValidation;
using MediHub.Application.Common;
using MediHub.Application.DTOs;
using MediHub.Application.Interfaces;
using MediHub.Domain.Entities;
using MediHub.Domain.Enums;
using MediHub.Domain.Interfaces;

namespace MediHub.Application.Services
{
    public class UsuarioService : ServicioBase, IUsuarioService
    {
        private const string Tabla = "Usuario";

        private readonly IUsuarioRepository _repositorio;
        private readonly IProveedorRepository _proveedores;
        private readonly IValidator<UsuarioDto> _validador;
        private readonly IMapper _mapper;
        private readonly IClock _clock;

        public UsuarioService(IUsuarioRepository repositorio, IProveedorRepository proveedores,
            IValidator<UsuarioDto> validador, IMapper mapper, IClock clock,
            IBitacoraService bitacora) : base(bitacora)
        {
            _repositorio = repositorio;
            _proveedores = proveedores;
            _validador = validador;
            _mapper = mapper;
            _clock = clock;
        }

        public IReadOnlyList<UsuarioDto> ObtenerTodos()
        {
            return _mapper.Map<List<UsuarioDto>>(_repositorio.ObtenerTodos());
        }

        public UsuarioDto ObtenerPorId(int idUsuario)
        {
            var usuario = _repositorio.ObtenerPorId(idUsuario);
            return usuario == null ? null : _mapper.Map<UsuarioDto>(usuario);
        }

        public Resultado Registrar(UsuarioDto dto)
        {
            return Ejecutar(Tabla, () =>
            {
                var validacion = _validador.Validate(dto);
                if (!validacion.IsValid) return Resultado.Invalido(validacion);

                if (_proveedores.ObtenerPorId(dto.IdProveedor) == null)
                    return Resultado.Fallido("El proveedor seleccionado no existe.");

                if (_repositorio.ExisteIdentificacion(dto.Identificacion))
                    return Resultado.Fallido("Ya existe un usuario registrado con la identificación " + dto.Identificacion + ".");

                var usuario = _mapper.Map<Usuario>(dto);
                usuario.IdNetUser = null;
                usuario.FechaDeRegistro = _clock.Ahora;
                usuario.FechaDeModificacion = null;
                usuario.Estado = true;
                _repositorio.Agregar(usuario);

                Bitacora.RegistrarEvento(Tabla, TipoEvento.Registrar,
                    "Se registró el usuario " + usuario.Identificacion + ".",
                    datosPosteriores: Resumir(usuario));
                return Resultado.Correcto("Usuario registrado correctamente.");
            });
        }

        public Resultado Editar(UsuarioDto dto)
        {
            return Ejecutar(Tabla, () =>
            {
                var usuario = _repositorio.ObtenerPorId(dto.IdUsuario);
                if (usuario == null)
                    return Resultado.Fallido("El usuario no existe.");

                dto.IdProveedor = usuario.IdProveedor;
                var validacion = _validador.Validate(dto);
                if (!validacion.IsValid) return Resultado.Invalido(validacion);

                if (_repositorio.ExisteIdentificacion(dto.Identificacion, usuario.IdUsuario))
                    return Resultado.Fallido("Ya existe otro usuario con la identificación " + dto.Identificacion + ".");

                var anterior = Resumir(usuario);
                usuario.Nombres = dto.Nombres;
                usuario.PrimerApellido = dto.PrimerApellido;
                usuario.SegundoApellido = dto.SegundoApellido;
                usuario.Identificacion = dto.Identificacion;
                usuario.CorreoElectronico = dto.CorreoElectronico;
                usuario.Estado = dto.Estado;
                usuario.FechaDeModificacion = _clock.Ahora;
                _repositorio.Actualizar(usuario);

                Bitacora.RegistrarEvento(Tabla, TipoEvento.Editar,
                    "Se editó el usuario " + usuario.IdUsuario + ".", anterior, Resumir(usuario));
                return Resultado.Correcto("Usuario actualizado correctamente.");
            });
        }

        private static object Resumir(Usuario u)
        {
            return new
            {
                u.IdUsuario,
                u.IdProveedor,
                u.Nombres,
                u.PrimerApellido,
                u.SegundoApellido,
                u.Identificacion,
                u.CorreoElectronico,
                u.FechaDeRegistro,
                u.FechaDeModificacion,
                u.Estado
            };
        }
    }
}
