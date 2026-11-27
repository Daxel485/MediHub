using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AutoMapper;
using MediHub.Application.Common;
using MediHub.Application.DTOs;
using MediHub.Application.Interfaces;
using MediHub.Application.Notificaciones;
using MediHub.Domain.Entities;
using MediHub.Domain.Enums;
using MediHub.Domain.Interfaces;

namespace MediHub.Application.Services
{
    public class PortalOfertasService : ServicioBase, IPortalOfertasService
    {
        private readonly IPortalOfertasRepository _repositorio;
        private readonly IMapper _mapper;
        private readonly IClock _clock;
        private readonly ITerminosLicitacion _terminos;
        private readonly NotificadorLicitacion _notificador;

        public PortalOfertasService(IPortalOfertasRepository repositorio, IMapper mapper, IClock clock,
            ITerminosLicitacion terminos, IBitacoraService bitacora, NotificadorLicitacion notificador) : base(bitacora)
        {
            _repositorio = repositorio;
            _mapper = mapper;
            _clock = clock;
            _terminos = terminos;
            _notificador = notificador;
        }

        public ComparacionOfertasDto Comparar(int idLicitacion)
        {
            var licitacion = _repositorio.ObtenerLicitacion(idLicitacion);
            if (licitacion == null) return null;

            var cerrada = _clock.Ahora >= licitacion.FechaDeCierre;
            var ganadora = _repositorio.ObtenerOfertaGanadora(idLicitacion);
            IReadOnlyList<OfertaDto> ofertas = Array.Empty<OfertaDto>();
            if (cerrada)
                ofertas = _mapper.Map<List<OfertaDto>>(_repositorio.ObtenerOfertas(idLicitacion));

            var estado = licitacion.EstadoActual;
            return new ComparacionOfertasDto
            {
                Licitacion = _mapper.Map<LicitacionVigenteDto>(licitacion),
                RecepcionCerrada = cerrada,
                PuedeAdjudicar = cerrada && licitacion.Estado && !ganadora.HasValue && estado.PuedeAdjudicar,
                IdOfertaGanadora = ganadora,
                Ofertas = ofertas,
                EstadoDelProceso = licitacion.EstadoDelProceso,
                PuedePublicar = estado.PuedePublicar,
                PuedeIniciarEvaluacion = estado.PuedeIniciarEvaluacion && cerrada,
                PuedeCerrar = estado.PuedeCerrar,
                PermiteSincronizarOfertas = estado.PermiteSincronizarOfertas && cerrada
            };
        }

        public Resultado Adjudicar(int idLicitacion, int idOferta, Guid idComprador)
        {
            if (idComprador == Guid.Empty) return Resultado.Fallido("Se requiere la identidad del comprador.");

            Licitacion adjudicada = null;
            var resultado = Modificar(idLicitacion, (licitacion, ahora) =>
            {
                if (!licitacion.Estado || ahora < licitacion.FechaDeCierre)
                    return Resultado.Fallido("La licitación debe estar activa y haber terminado la recepción de ofertas.");
                if (_repositorio.ObtenerOfertaGanadora(idLicitacion).HasValue)
                    return Resultado.Fallido("La licitación ya tiene una oferta adjudicada.");
                var ganadora = _repositorio.ObtenerOferta(idLicitacion, idOferta);
                if (ganadora == null)
                    return Resultado.Fallido("La oferta seleccionada no pertenece a la licitación.");

                // El estado valida la transición (solo En evaluación y con oferta sincronizada) y pasa a Adjudicada.
                try
                {
                    licitacion.Adjudicar(ganadora, ahora);
                }
                catch (InvalidOperationException ex)
                {
                    return Resultado.Fallido(ex.Message);
                }

                _repositorio.AgregarAdjudicacion(new Adjudicacion
                {
                    IdLicitacion = idLicitacion,
                    IdOferta = idOferta,
                    IdComprador = idComprador,
                    FechaDeAdjudicacion = ahora
                });
                _repositorio.AgregarEvento(BitacoraService.CrearEvento("Licitacion", TipoEvento.Adjudicar,
                    $"Se adjudicó la licitación {idLicitacion} a la oferta {idOferta}.", ahora, null,
                    new { IdLicitacion = idLicitacion, IdOferta = idOferta, ganadora.IdProveedor,
                        IdComprador = idComprador, FechaDeAdjudicacion = ahora }));
                adjudicada = licitacion;
                return Resultado.Correcto("La oferta fue adjudicada.");
            });

            // Los correos se envían después de confirmar la transacción, para no avisar una adjudicación revertida.
            return resultado.Exito ? NotificarAdjudicacion(adjudicada, idOferta, resultado) : resultado;
        }

        private Resultado NotificarAdjudicacion(Licitacion licitacion, int idOferta, Resultado resultado)
        {
            try
            {
                var ofertas = _repositorio.ObtenerOfertas(licitacion.IdLicitacion);
                var ganadora = ofertas.Single(o => o.IdOferta == idOferta);
                var envio = _notificador.NotificarAdjudicacion(licitacion, ganadora, ofertas);
                return Resultado.Correcto(resultado.Mensaje + envio.Describir());
            }
            catch (Exception ex)
            {
                Bitacora.RegistrarError("Notificacion", ex);
                return Resultado.Correcto(resultado.Mensaje +
                    " No se pudieron enviar las notificaciones; el detalle quedó registrado en la bitácora.");
            }
        }

        public Resultado Sincronizar(int idLicitacion, int idOferta)
        {
            return Modificar(idLicitacion, (licitacion, ahora) =>
            {
                if (ahora < licitacion.FechaDeCierre)
                    return Resultado.Fallido("La recepción de ofertas sigue abierta.");
                if (!licitacion.EstadoActual.PermiteSincronizarOfertas)
                    return Resultado.Fallido("No se pueden sincronizar ofertas de una licitación en estado "
                                             + licitacion.EstadoActual.Nombre + ".");
                var oferta = _repositorio.ObtenerOferta(idLicitacion, idOferta);
                if (oferta == null) return Resultado.Fallido("La oferta no pertenece a esta licitación.");
                if (oferta.Sincronizada) return Resultado.Fallido("La oferta ya se encuentra sincronizada.");

                oferta.Sincronizar();
                _repositorio.AgregarEvento(BitacoraService.CrearEvento("Oferta", TipoEvento.Editar,
                    $"Se sincronizó la oferta {idOferta}.", ahora, new { idOferta, Sincronizada = false },
                    new { idOferta, Sincronizada = true }));
                return Resultado.Correcto("Oferta sincronizada correctamente.");
            });
        }

        public IReadOnlyList<LicitacionVigenteDto> ObtenerVigentes(Guid idNetUser)
        {
            if (!ProveedorAutorizado(idNetUser).HasValue) return null;
            return _mapper.Map<List<LicitacionVigenteDto>>(_repositorio.ObtenerVigentes(_clock.Ahora));
        }

        public PortalProveedorDto ObtenerPortal(int idLicitacion, Guid idNetUser)
        {
            var proveedor = ProveedorAutorizado(idNetUser);
            if (!proveedor.HasValue) return null;

            var licitacion = _repositorio.ObtenerVigente(idLicitacion, _clock.Ahora);
            if (licitacion == null) return null;
            return new PortalProveedorDto
            {
                Licitacion = _mapper.Map<LicitacionVigenteDto>(licitacion),
                Items = _terminos.ObtenerItems(licitacion),
                TienePdf = _terminos.TienePdf(idLicitacion),
                TieneOfertaPropia = _repositorio.TieneOferta(idLicitacion, proveedor.Value)
            };
        }

        public Stream AbrirPdf(int idLicitacion, Guid idNetUser)
        {
            if (!ProveedorAutorizado(idNetUser).HasValue ||
                _repositorio.ObtenerVigente(idLicitacion, _clock.Ahora) == null)
                return null;
            return _terminos.AbrirPdf(idLicitacion);
        }

        public Resultado EnviarOferta(int idLicitacion, Guid idNetUser, int plazoEntregaDias,
            string descripcion, IReadOnlyList<DetalleOfertaDto> detalles)
        {
            return Modificar(idLicitacion, (licitacion, ahora) =>
            {
                var proveedor = ProveedorAutorizado(idNetUser);
                if (!proveedor.HasValue) return Resultado.Fallido("Su cuenta no está asociada a un proveedor activo.");
                if (!licitacion.Estado || !licitacion.EstadoActual.PermiteRecibirOfertas ||
                    ahora >= licitacion.FechaDeCierre || _repositorio.ObtenerOfertaGanadora(idLicitacion).HasValue)
                    return Resultado.Fallido("La licitación ya no recibe ofertas.");
                if (_repositorio.TieneOferta(idLicitacion, proveedor.Value))
                    return Resultado.Fallido("Su proveedor ya presentó una oferta para esta licitación.");

                descripcion = descripcion?.Trim();
                if (plazoEntregaDias < 1 || plazoEntregaDias > (DateTime.MaxValue.Date - ahora.Date).Days || (descripcion?.Length ?? 0) > 500)
                    return Resultado.Fallido("Indique un plazo de entrega válido y una descripción de hasta 500 caracteres.");

                var items = _terminos.ObtenerItems(licitacion);
                if (items.Count == 0 || detalles == null || detalles.Count != items.Count)
                    return Resultado.Fallido("Indique precio y vencimiento para todos los ítems de la licitación.");

                var oferta = new Oferta
                {
                    IdLicitacion = idLicitacion,
                    IdProveedor = proveedor.Value,
                    PlazoEntregaDias = plazoEntregaDias,
                    Descripcion = descripcion,
                    FechaDeEnvio = ahora
                };
                var fechaEntrega = ahora.Date.AddDays(plazoEntregaDias);
                const decimal maximo = 9999999999999999.99m;

                var detallesPorItem = detalles.Where(d => d != null).ToLookup(d => d.IdItemLicitacion);
                foreach (var item in items)
                {
                    var registros = detallesPorItem[item.IdItemLicitacion].ToList();
                    if (registros.Count != 1)
                        return Resultado.Fallido("Presente un único precio para cada ítem.");

                    var detalle = registros[0];
                    if (detalle.PrecioUnitario <= 0 || detalle.PrecioUnitario > maximo ||
                        detalle.PrecioUnitario != decimal.Round(detalle.PrecioUnitario, 2))
                        return Resultado.Fallido("Cada precio debe ser positivo y tener como máximo dos decimales.");

                    var vencimientoMinimo = fechaEntrega.AddMonths(item.VidaUtilMinimaMeses);
                    if (detalle.FechaVencimientoLote.Date <= fechaEntrega || detalle.FechaVencimientoLote.Date < vencimientoMinimo)
                        return Resultado.Fallido($"El lote de «{item.Descripcion}» debe cumplir la vida útil mínima a la entrega.");

                    if (item.Cantidad <= 0 || item.Cantidad >= 100000000000000m ||
                        item.Cantidad != decimal.Round(item.Cantidad, 4) ||
                        detalle.PrecioUnitario > maximo / Math.Max(1m, item.Cantidad))
                        return Resultado.Fallido($"La cantidad o el importe de «{item.Descripcion}» supera el formato permitido.");

                    oferta.Monto += decimal.Round(detalle.PrecioUnitario * item.Cantidad, 2, MidpointRounding.AwayFromZero);
                    if (oferta.Monto > maximo)
                        return Resultado.Fallido("El monto total de la oferta supera el máximo permitido.");

                    oferta.Detalles.Add(new DetalleOferta
                    {
                        IdItemLicitacion = item.IdItemLicitacion,
                        DescripcionItem = item.Descripcion,
                        Cantidad = item.Cantidad,
                        PrecioUnitario = detalle.PrecioUnitario,
                        FechaVencimientoLote = detalle.FechaVencimientoLote.Date
                    });
                }

                if (oferta.Monto <= 0)
                    return Resultado.Fallido("El monto total debe ser positivo.");

                _repositorio.AgregarOferta(oferta);
                return Resultado.Correcto("Su oferta se recibió correctamente.");
            });
        }

        private int? ProveedorAutorizado(Guid idNetUser)
        {
            return idNetUser == Guid.Empty ? null : _repositorio.ObtenerProveedorAutorizado(idNetUser);
        }

        private Resultado Modificar(int idLicitacion, Func<Licitacion, DateTime, Resultado> operacion)
        {
            return Ejecutar("Licitacion", () => _repositorio.EjecutarEnLicitacion(idLicitacion, licitacion =>
                licitacion == null ? Resultado.Fallido("La licitación no existe.") : operacion(licitacion, _clock.Ahora)));
        }
    }
}
