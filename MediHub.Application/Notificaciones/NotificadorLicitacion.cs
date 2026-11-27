using System;
using System.Collections.Generic;
using System.Linq;
using MediHub.Application.Interfaces;
using MediHub.Domain.Entities;

namespace MediHub.Application.Notificaciones
{
    // Decide a quién se notifica en cada transición y usa la fábrica de cada tipo (patrón Factory Method).
    // Lo comparten LicitacionService (publicar, cerrar) y PortalOfertasService (adjudicar).
    public class NotificadorLicitacion
    {
        private const string TablaNotificacion = "Notificacion";

        private readonly IReadOnlyDictionary<TipoNotificacion, CreadorDeNotificacion> _creadores;
        private readonly IBitacoraService _bitacora;

        public NotificadorLicitacion(IEnumerable<CreadorDeNotificacion> creadores, IBitacoraService bitacora)
        {
            _creadores = creadores.ToDictionary(c => c.Tipo);
            _bitacora = bitacora;
        }

        // Aviso de publicación a los proveedores activos.
        public ResumenDeEnvio NotificarPublicacion(Licitacion licitacion, IEnumerable<Proveedor> proveedores)
        {
            return Notificar(TipoNotificacion.Publicacion, licitacion, proveedores.Where(p => p.Estado));
        }

        // Adjudicación al ganador y rechazo al resto de oferentes (un correo por proveedor).
        public ResumenDeEnvio NotificarAdjudicacion(Licitacion licitacion, Oferta ganadora, IEnumerable<Oferta> ofertas)
        {
            var resumen = Notificar(TipoNotificacion.Adjudicacion, licitacion, new[] { ganadora.Proveedor }, ganadora);
            var rechazados = Oferentes(ofertas).Where(p => p.IdProveedor != ganadora.IdProveedor);
            return resumen.Sumar(Notificar(TipoNotificacion.Rechazo, licitacion, rechazados));
        }

        // Licitación cerrada sin adjudicar: rechazo a todos los oferentes.
        public ResumenDeEnvio NotificarDesierta(Licitacion licitacion, IEnumerable<Oferta> ofertas)
        {
            return Notificar(TipoNotificacion.Rechazo, licitacion, Oferentes(ofertas));
        }

        private static IEnumerable<Proveedor> Oferentes(IEnumerable<Oferta> ofertas)
        {
            return ofertas
                .Where(o => o.Proveedor != null)
                .GroupBy(o => o.IdProveedor)
                .Select(g => g.First().Proveedor);
        }

        // Un correo que falla no revierte la transición: se registra en la bitácora y se informa al usuario.
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
                    _bitacora.RegistrarError(TablaNotificacion, ex);
                    resumen = resumen.Sumar(new ResumenDeEnvio(0, 1));
                }
            }
            return resumen;
        }
    }

    public sealed class ResumenDeEnvio
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

        // Texto que se agrega al mensaje de la transición.
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
