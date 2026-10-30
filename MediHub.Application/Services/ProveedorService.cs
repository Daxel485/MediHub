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
    public class ProveedorService : ServicioBase, IProveedorService
    {
        private const string Tabla = "Proveedor";

        private readonly IProveedorRepository _repositorio;
        private readonly IValidator<ProveedorDto> _validador;
        private readonly IMapper _mapper;
        private readonly IClock _clock;

        public ProveedorService(IProveedorRepository repositorio, IValidator<ProveedorDto> validador,
            IMapper mapper, IClock clock, IBitacoraService bitacora) : base(bitacora)
        {
            _repositorio = repositorio;
            _validador = validador;
            _mapper = mapper;
            _clock = clock;
        }

        public IReadOnlyList<ProveedorDto> ObtenerTodos()
        {
            return _mapper.Map<List<ProveedorDto>>(_repositorio.ObtenerTodos());
        }

        public ProveedorDto ObtenerPorId(int idProveedor)
        {
            var proveedor = _repositorio.ObtenerPorId(idProveedor);
            return proveedor == null ? null : _mapper.Map<ProveedorDto>(proveedor);
        }

        public Resultado Registrar(ProveedorDto dto)
        {
            return Ejecutar(Tabla, () =>
            {
                var validacion = _validador.Validate(dto);
                if (!validacion.IsValid) return Resultado.Invalido(validacion);

                if (_repositorio.ExisteIdentificacion(dto.Identificacion))
                    return Resultado.Fallido("Ya existe un proveedor registrado con esa cédula.");

                var proveedor = _mapper.Map<Proveedor>(dto);
                proveedor.FechaDeRegistro = _clock.Ahora;
                proveedor.FechaDeModificacion = null;
                proveedor.Estado = true;
                _repositorio.Agregar(proveedor);

                Bitacora.RegistrarEvento(Tabla, TipoEvento.Registrar,
                    "Se registró el proveedor " + proveedor.Nombre + ".",
                    datosPosteriores: _mapper.Map<ProveedorDto>(proveedor));
                return Resultado.Correcto("Proveedor registrado correctamente.");
            });
        }
    }
}
