using System;
using MediHub.Application.Interfaces;
using MediHub.Domain.Entities;

namespace MediHub.Application.Notificaciones
{
    // Creador del patrón Factory Method. Define el algoritmo común (crear y enviar el correo) y deja a cada
    // subclase decidir qué notificación concreta se crea mediante el método fábrica CrearNotificacion.
    public abstract class CreadorDeNotificacion
    {
        private readonly IServicioCorreo _correo;

        protected CreadorDeNotificacion(IServicioCorreo correo)
        {
            _correo = correo;
        }

        public abstract TipoNotificacion Tipo { get; }

        // Método fábrica.
        protected abstract INotificacionCorreo CrearNotificacion(Licitacion licitacion, Proveedor proveedor, Oferta oferta);

        public INotificacionCorreo Enviar(Licitacion licitacion, Proveedor proveedor, Oferta oferta = null)
        {
            if (licitacion == null) throw new ArgumentNullException(nameof(licitacion));
            if (proveedor == null) throw new ArgumentNullException(nameof(proveedor));

            var notificacion = CrearNotificacion(licitacion, proveedor, oferta);
            _correo.Enviar(notificacion.Destinatario, notificacion.Asunto, notificacion.Cuerpo);
            return notificacion;
        }
    }

    public sealed class CreadorNotificacionPublicacion : CreadorDeNotificacion
    {
        public CreadorNotificacionPublicacion(IServicioCorreo correo) : base(correo)
        {
        }

        public override TipoNotificacion Tipo => TipoNotificacion.Publicacion;

        protected override INotificacionCorreo CrearNotificacion(Licitacion licitacion, Proveedor proveedor, Oferta oferta)
        {
            return new NotificacionPublicacion(licitacion, proveedor);
        }
    }

    public sealed class CreadorNotificacionAdjudicacion : CreadorDeNotificacion
    {
        public CreadorNotificacionAdjudicacion(IServicioCorreo correo) : base(correo)
        {
        }

        public override TipoNotificacion Tipo => TipoNotificacion.Adjudicacion;

        protected override INotificacionCorreo CrearNotificacion(Licitacion licitacion, Proveedor proveedor, Oferta oferta)
        {
            if (oferta == null)
                throw new ArgumentNullException(nameof(oferta), "La notificación de adjudicación requiere la oferta ganadora.");
            return new NotificacionAdjudicacion(licitacion, proveedor, oferta);
        }
    }

    public sealed class CreadorNotificacionRechazo : CreadorDeNotificacion
    {
        public CreadorNotificacionRechazo(IServicioCorreo correo) : base(correo)
        {
        }

        public override TipoNotificacion Tipo => TipoNotificacion.Rechazo;

        protected override INotificacionCorreo CrearNotificacion(Licitacion licitacion, Proveedor proveedor, Oferta oferta)
        {
            return new NotificacionRechazo(licitacion, proveedor);
        }
    }
}
