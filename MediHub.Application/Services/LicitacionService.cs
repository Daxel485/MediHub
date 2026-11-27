using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using FluentValidation;
using MediHub.Application.Common;
using MediHub.Application.DTOs;
using MediHub.Application.Interfaces;
using MediHub.Application.Notificaciones;
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
        private readonly IProveedorRepository _proveedores;
        private readonly IValidator<LicitacionDto> _validador;
        private readonly IMapper _mapper;
        private readonly IClock _clock;
        private readonly NotificadorLicitacion _notificador;

        public LicitacionService(ILicitacionRepository repositorio, IProveedorRepository proveedores,
            IValidator<LicitacionDto> validador, IMapper mapper, IClock clock, IBitacoraService bitacora,
            NotificadorLicitacion notificador) : base(bitacora)
        {
            _repositorio = repositorio;
            _proveedores = proveedores;
            _validador = validador;
            _mapper = mapper;
            _clock = clock;
            _notificador = notificador;
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
                licitacion.EstadoDelProceso = EstadoLicitacion.Borrador;
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
                        licitacion.Estado,
                        licitacion.EstadoDelProceso
                    });
                return Resultado.Correcto("Licitación registrada como borrador. Publíquela para notificar a los proveedores.");
            });
        }

        public Resultado Publicar(int idLicitacion)
        {
            return CambiarEstado(idLicitacion, "publicó",
                licitacion => licitacion.Publicar(_clock.Ahora),
                (licitacion, estadoAnterior) =>
                    _notificador.NotificarPublicacion(licitacion, _proveedores.ObtenerTodos()));
        }

        public Resultado IniciarEvaluacion(int idLicitacion)
        {
            return CambiarEstado(idLicitacion, "pasó a evaluación",
                licitacion => licitacion.IniciarEvaluacion(_clock.Ahora),
                (licitacion, estadoAnterior) => ResumenDeEnvio.Vacio);
        }

        // La adjudicación la registra PortalOfertasService.Adjudicar, que también pasa por el estado.
        public Resultado Cerrar(int idLicitacion)
        {
            return CambiarEstado(idLicitacion, "cerró",
                licitacion => licitacion.Cerrar(_clock.Ahora),
                (licitacion, estadoAnterior) =>
                {
                    // Si se cierra sin adjudicar mientras recibía o evaluaba ofertas, la licitación queda desierta
                    // y se avisa a todos los oferentes. Cerrar un borrador o una adjudicada no genera correos.
                    var quedoDesierta = estadoAnterior == EstadoLicitacion.Publicada
                                        || estadoAnterior == EstadoLicitacion.EnEvaluacion;
                    return quedoDesierta
                        ? _notificador.NotificarDesierta(licitacion, licitacion.Ofertas)
                        : ResumenDeEnvio.Vacio;
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

                if (oferta.Licitacion != null && !oferta.Licitacion.EstadoActual.PermiteSincronizarOfertas)
                    return Resultado.Fallido("No se pueden sincronizar ofertas de una licitación en estado "
                                             + oferta.Licitacion.EstadoActual.Nombre + ".");

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

        // Plantilla común de las transiciones: cargar, delegar al estado (patrón State), guardar,
        // registrar en la bitácora y enviar las notificaciones que correspondan.
        private Resultado CambiarEstado(int idLicitacion, string verbo, Action<Licitacion> transicion,
            Func<Licitacion, EstadoLicitacion, ResumenDeEnvio> notificar)
        {
            return Ejecutar(TablaLicitacion, () =>
            {
                var licitacion = _repositorio.ObtenerParaActualizar(idLicitacion);
                if (licitacion == null)
                    return Resultado.Fallido("La licitación no existe.");

                var estadoAnterior = licitacion.EstadoDelProceso;
                try
                {
                    transicion(licitacion);
                }
                catch (InvalidOperationException ex)
                {
                    return Resultado.Fallido(ex.Message);
                }

                _repositorio.Actualizar(licitacion);
                Bitacora.RegistrarEvento(TablaLicitacion, TipoEvento.Editar,
                    "Se " + verbo + " la licitación " + licitacion.Titulo + ".",
                    new { licitacion.IdLicitacion, EstadoDelProceso = estadoAnterior.ToString() },
                    new
                    {
                        licitacion.IdLicitacion,
                        EstadoDelProceso = licitacion.EstadoDelProceso.ToString(),
                        licitacion.FechaDeModificacion
                    });

                var envio = notificar(licitacion, estadoAnterior);
                return Resultado.Correcto("La licitación quedó en estado " + licitacion.EstadoActual.Nombre + "."
                                          + envio.Describir());
            });
        }
    }
}
