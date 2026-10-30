using System;
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
    public class LicitacionService : ServicioBase, ILicitacionService
    {
        private const string TablaLicitacion = "Licitacion";
        private const string TablaOferta = "Oferta";

        private readonly ILicitacionRepository _repositorio;
        private readonly IValidator<LicitacionDto> _validador;
        private readonly IMapper _mapper;
        private readonly IClock _clock;

        public LicitacionService(ILicitacionRepository repositorio, IValidator<LicitacionDto> validador,
            IMapper mapper, IClock clock, IBitacoraService bitacora) : base(bitacora)
        {
            _repositorio = repositorio;
            _validador = validador;
            _mapper = mapper;
            _clock = clock;
        }

        public IReadOnlyList<LicitacionDto> ObtenerTodas()
        {
            return _mapper.Map<List<LicitacionDto>>(_repositorio.ObtenerTodas());
        }

        public LicitacionDto ObtenerPorId(int idLicitacion)
        {
            var licitacion = _repositorio.ObtenerPorId(idLicitacion);
            return licitacion == null ? null : _mapper.Map<LicitacionDto>(licitacion);
        }

        public Resultado Registrar(LicitacionDto dto)
        {
            return Ejecutar(TablaLicitacion, () =>
            {
                var validacion = _validador.Validate(dto);
                if (!validacion.IsValid) return Resultado.Invalido(validacion);

                if (_repositorio.ExisteTitulo(dto.Titulo))
                    return Resultado.Fallido("Ya existe una licitación con el título \"" + dto.Titulo + "\".");

                var licitacion = _mapper.Map<Licitacion>(dto);
                licitacion.FechaDeRegistro = _clock.Ahora;
                licitacion.Estado = true;
                _repositorio.Agregar(licitacion);

                Bitacora.RegistrarEvento(TablaLicitacion, TipoEvento.Registrar,
                    "Se registró la licitación " + licitacion.Titulo + ".",
                    datosPosteriores: new
                    {
                        licitacion.IdLicitacion,
                        licitacion.Titulo,
                        licitacion.Descripcion,
                        licitacion.FechaDeCierre,
                        licitacion.FechaDeRegistro,
                        licitacion.Estado
                    });
                return Resultado.Correcto("Licitación registrada correctamente.");
            });
        }

        public IReadOnlyList<OfertaDto> ObtenerOfertas(int idLicitacion)
        {
            return _mapper.Map<List<OfertaDto>>(_repositorio.ObtenerOfertas(idLicitacion));
        }

        public Resultado SincronizarOferta(int idOferta)
        {
            return Ejecutar(TablaOferta, () =>
            {
                var oferta = _repositorio.ObtenerOferta(idOferta);
                if (oferta == null)
                    return Resultado.Fallido("La oferta no existe.");

                try
                {
                    oferta.Sincronizar();
                }
                catch (InvalidOperationException ex)
                {
                    return Resultado.Fallido(ex.Message);
                }

                _repositorio.ActualizarOferta(oferta);
                Bitacora.RegistrarEvento(TablaOferta, TipoEvento.Editar,
                    "Se sincronizó la oferta " + oferta.IdOferta + ".",
                    new { oferta.IdOferta, Sincronizada = false },
                    new { oferta.IdOferta, Sincronizada = true });
                return Resultado.Correcto("Oferta sincronizada correctamente.");
            });
        }
    }
}
