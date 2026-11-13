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
    public class ConfiguracionProveedorService : ServicioBase, IConfiguracionProveedorService
    {
        private const string Tabla = "ConfiguracionProveedor";

        private readonly IConfiguracionProveedorRepository _repositorio;
        private readonly IProveedorRepository _proveedores;
        private readonly IValidator<ConfiguracionProveedorDto> _validador;
        private readonly IMapper _mapper;
        private readonly IClock _clock;

        public ConfiguracionProveedorService(IConfiguracionProveedorRepository repositorio,
            IProveedorRepository proveedores, IValidator<ConfiguracionProveedorDto> validador,
            IMapper mapper, IClock clock, IBitacoraService bitacora) : base(bitacora)
        {
            _repositorio = repositorio;
            _proveedores = proveedores;
            _validador = validador;
            _mapper = mapper;
            _clock = clock;
        }

        public IReadOnlyList<ConfiguracionProveedorDto> ObtenerTodas(int? idProveedor = null)
        {
            return _mapper.Map<List<ConfiguracionProveedorDto>>(_repositorio.ObtenerTodas(idProveedor));
        }

        public ConfiguracionProveedorDto ObtenerPorId(int idConfiguracion)
        {
            var configuracion = _repositorio.ObtenerPorId(idConfiguracion);
            return configuracion == null ? null : _mapper.Map<ConfiguracionProveedorDto>(configuracion);
        }

        public Resultado Registrar(ConfiguracionProveedorDto dto)
        {
            return Ejecutar(Tabla, () =>
            {
                var validacion = _validador.Validate(dto);
                if (!validacion.IsValid) return Resultado.Invalido(validacion);

                if (_proveedores.ObtenerPorId(dto.IdProveedor) == null)
                    return Resultado.Fallido("El proveedor seleccionado no existe.");

                if (_repositorio.ExisteParaProveedor(dto.IdProveedor))
                    return Resultado.Fallido("El proveedor ya tiene una configuración registrada. Solo se permite una por proveedor.");

                var configuracion = _mapper.Map<ConfiguracionProveedor>(dto);
                configuracion.FechaDeRegistro = _clock.Ahora;
                configuracion.FechaDeModificacion = null;
                configuracion.Estado = true;
                _repositorio.Agregar(configuracion);

                Bitacora.RegistrarEvento(Tabla, TipoEvento.Registrar,
                    "Se registró la configuración del proveedor " + dto.IdProveedor + ".",
                    datosPosteriores: Resumir(configuracion));
                return Resultado.Correcto("Configuración registrada correctamente.");
            });
        }

        public Resultado Editar(ConfiguracionProveedorDto dto)
        {
            return Ejecutar(Tabla, () =>
            {
                var configuracion = _repositorio.ObtenerPorId(dto.IdConfiguracion);
                if (configuracion == null)
                    return Resultado.Fallido("La configuración no existe.");

                // El proveedor no se puede cambiar al editar; se valida con el valor guardado.
                dto.IdProveedor = configuracion.IdProveedor;
                var validacion = _validador.Validate(dto);
                if (!validacion.IsValid) return Resultado.Invalido(validacion);

                var anterior = Resumir(configuracion);
                configuracion.TipoConfiguracion = dto.TipoConfiguracion;
                configuracion.Comision = dto.Comision;
                configuracion.Estado = dto.Estado;
                configuracion.FechaDeModificacion = _clock.Ahora;
                _repositorio.Actualizar(configuracion);

                Bitacora.RegistrarEvento(Tabla, TipoEvento.Editar,
                    "Se editó la configuración " + configuracion.IdConfiguracion + ".",
                    anterior, Resumir(configuracion));
                return Resultado.Correcto("Configuración actualizada correctamente.");
            });
        }

        private static object Resumir(ConfiguracionProveedor c)
        {
            return new
            {
                c.IdConfiguracion,
                c.IdProveedor,
                TipoConfiguracion = (int)c.TipoConfiguracion,
                c.Comision,
                c.FechaDeRegistro,
                c.FechaDeModificacion,
                c.Estado
            };
        }
    }
}
