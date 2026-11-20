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
        private const string TablaNotificacion = "Notificacion";

        private readonly ILicitacionRepository _repositorio;
        private readonly IProveedorRepository _proveedores;
        private readonly IValidator<LicitacionDto> _validador;
        private readonly IMapper _mapper;
        private readonly IClock _clock;
        private readonly IReadOnlyDictionary<TipoNotificacion, CreadorDeNotificacion> _creadores;

        public LicitacionService(ILicitacionRepository repositorio, IProveedorRepository proveedores,
            IValidator<LicitacionDto> validador, IMapper mapper, IClock clock, IBitacoraService bitacora,
            IEnumerable<CreadorDeNotificacion> creadores) : base(bitacora)
        {
            _repositorio = repositorio;
            _proveedores = proveedores;
            _validador = validador;
            _mapper = mapper;
            _clock = clock;
            _creadores = creadores.ToDictionary(c => c.Tipo);
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
                licitacion.IdOfertaAdjudicada = null;
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
                licitacion =>
                {
                    licitacion.Publicar(_clock.Ahora);
                    return null;
                },
                (licitacion, estadoAnterior, ganadora) => NotificarPublicacion(licitacion));
        }

        public Resultado IniciarEvaluacion(int idLicitacion)
        {
            return CambiarEstado(idLicitacion, "pasó a evaluación",
                licitacion =>
                {
                    licitacion.IniciarEvaluacion(_clock.Ahora);
                    return null;
                },
                (licitacion, estadoAnterior, ganadora) => ResumenDeEnvio.Vacio);
        }

        public Resultado Adjudicar(int idLicitacion, int idOferta)
        {
            return CambiarEstado(idLicitacion, "adjudicó",
                licitacion => licitacion.Adjudicar(idOferta, _clock.Ahora),
                (licitacion, estadoAnterior, ganadora) => NotificarAdjudicacion(licitacion, ganadora));
        }

        public Resultado Cerrar(int idLicitacion)
        {
            return CambiarEstado(idLicitacion, "cerró",
                licitacion =>
                {
                    licitacion.Cerrar(_clock.Ahora);
                    return null;
                },
                (licitacion, estadoAnterior, ganadora) =>
                {
                    // Si se cierra sin adjudicar mientras recibía o evaluaba ofertas, la licitación queda desierta
                    // y se avisa a todos los oferentes. Cerrar un borrador o una adjudicada no genera correos.
                    var quedoDesierta = estadoAnterior == EstadoLicitacion.Publicada
                                        || estadoAnterior == EstadoLicitacion.EnEvaluacion;
                    return quedoDesierta
                        ? Notificar(TipoNotificacion.Rechazo, licitacion, ProveedoresQueOfertaron(licitacion))
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
        private Resultado CambiarEstado(int idLicitacion, string verbo, Func<Licitacion, Oferta> transicion,
            Func<Licitacion, EstadoLicitacion, Oferta, ResumenDeEnvio> notificar)
        {
            return Ejecutar(TablaLicitacion, () =>
            {
                var licitacion = _repositorio.ObtenerParaActualizar(idLicitacion);
                if (licitacion == null)
                    return Resultado.Fallido("La licitación no existe.");

                var estadoAnterior = licitacion.EstadoDelProceso;
                Oferta ganadora;
                try
                {
                    ganadora = transicion(licitacion);
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
                        licitacion.IdOfertaAdjudicada,
                        licitacion.FechaDeModificacion
                    });

                var envio = notificar(licitacion, estadoAnterior, ganadora);
                return Resultado.Correcto("La licitación quedó en estado " + licitacion.EstadoActual.Nombre + "."
                                          + envio.Describir());
            });
        }

        private ResumenDeEnvio NotificarPublicacion(Licitacion licitacion)
        {
            var activos = _proveedores.ObtenerTodos().Where(p => p.Estado);
            return Notificar(TipoNotificacion.Publicacion, licitacion, activos);
        }

        private ResumenDeEnvio NotificarAdjudicacion(Licitacion licitacion, Oferta ganadora)
        {
            var resumen = Notificar(TipoNotificacion.Adjudicacion, licitacion, new[] { ganadora.Proveedor }, ganadora);
            var rechazados = ProveedoresQueOfertaron(licitacion).Where(p => p.IdProveedor != ganadora.IdProveedor);
            return resumen.Sumar(Notificar(TipoNotificacion.Rechazo, licitacion, rechazados));
        }

        private static IEnumerable<Proveedor> ProveedoresQueOfertaron(Licitacion licitacion)
        {
            return licitacion.Ofertas
                .Where(o => o.Proveedor != null)
                .GroupBy(o => o.IdProveedor)
                .Select(g => g.First().Proveedor);
        }

        // Usa la fábrica del tipo pedido (patrón Factory Method). Un correo que falla no revierte la transición:
        // se registra en la bitácora y se informa en el mensaje al usuario.
        private ResumenDeEnvio Notificar(TipoNotificacion tipo, Licitacion licitacion,
            IEnumerable<Proveedor> destinatarios, Oferta oferta = null)
        {
            var creador = _creadores[tipo];
            var resumen = ResumenDeEnvio.Vacio;
            foreach (var proveedor in destinatarios.Where(p => p != null && !string.IsNullOrWhiteSpace(p.CorreoElectronico)))
            {
                try
                {
                    creador.Enviar(licitacion, proveedor, oferta);
                    resumen = resumen.Sumar(new ResumenDeEnvio(1, 0));
                }
                catch (Exception ex)
                {
                    Bitacora.RegistrarError(TablaNotificacion, ex);
                    resumen = resumen.Sumar(new ResumenDeEnvio(0, 1));
                }
            }
            return resumen;
        }

        private sealed class ResumenDeEnvio
        {
            public static readonly ResumenDeEnvio Vacio = new ResumenDeEnvio(0, 0);

            public ResumenDeEnvio(int enviadas, int fallidas)
            {
                Enviadas = enviadas;
                Fallidas = fallidas;
            }

            public int Enviadas { get; }
            public int Fallidas { get; }

            public ResumenDeEnvio Sumar(ResumenDeEnvio otro)
            {
                return new ResumenDeEnvio(Enviadas + otro.Enviadas, Fallidas + otro.Fallidas);
            }

            public string Describir()
            {
                var texto = "";
                if (Enviadas > 0)
                    texto += " Se " + (Enviadas == 1 ? "envió 1 notificación" : "enviaron " + Enviadas + " notificaciones")
                             + " por correo.";
                if (Fallidas > 0)
                    texto += " " + Fallidas + (Fallidas == 1 ? " notificación no se pudo enviar" : " notificaciones no se pudieron enviar")
                             + "; el detalle quedó registrado en la bitácora.";
                return texto;
            }
        }
    }
}
