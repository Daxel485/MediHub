using System.Globalization;
using MediHub.Domain.Entities;

namespace MediHub.Application.Notificaciones
{
    // Productos concretos del patrón Factory Method. Cada uno arma el asunto y el cuerpo de su tipo de correo.
    public abstract class NotificacionCorreoBase : INotificacionCorreo
    {
        private static readonly CultureInfo CulturaCostaRica = CultureInfo.GetCultureInfo("es-CR");

        protected NotificacionCorreoBase(Licitacion licitacion, Proveedor proveedor)
        {
            Licitacion = licitacion;
            Proveedor = proveedor;
        }

        protected Licitacion Licitacion { get; }
        protected Proveedor Proveedor { get; }

        public abstract TipoNotificacion Tipo { get; }
        public string Destinatario => Proveedor.CorreoElectronico;
        public abstract string Asunto { get; }
        public abstract string Cuerpo { get; }

        protected string Saludo => "Estimado proveedor " + Proveedor.Nombre + ":";

        protected const string Firma =
            "\n\nAtentamente,\nDepartamento de Proveeduría\nClínica Integral San Rafael · MediHub";

        protected static string Fecha(System.DateTime fecha)
        {
            return fecha.ToString("dd/MM/yyyy", CulturaCostaRica);
        }

        protected static string Monto(decimal monto)
        {
            return "₡" + monto.ToString("N2", CulturaCostaRica);
        }
    }

    public sealed class NotificacionPublicacion : NotificacionCorreoBase
    {
        public NotificacionPublicacion(Licitacion licitacion, Proveedor proveedor) : base(licitacion, proveedor)
        {
        }

        public override TipoNotificacion Tipo => TipoNotificacion.Publicacion;
        public override string Asunto => "Nueva licitación publicada: " + Licitacion.Titulo;

        public override string Cuerpo =>
            Saludo + "\n\n" +
            "Se publicó la licitación \"" + Licitacion.Titulo + "\" y puede enviar su oferta en MediHub.\n\n" +
            "Descripción: " + (Licitacion.Descripcion ?? "(sin descripción)") + "\n" +
            "Fecha de cierre: " + Fecha(Licitacion.FechaDeCierre) +
            Firma;
    }

    public sealed class NotificacionAdjudicacion : NotificacionCorreoBase
    {
        private readonly Oferta _oferta;

        public NotificacionAdjudicacion(Licitacion licitacion, Proveedor proveedor, Oferta oferta)
            : base(licitacion, proveedor)
        {
            _oferta = oferta;
        }

        public override TipoNotificacion Tipo => TipoNotificacion.Adjudicacion;
        public override string Asunto => "Licitación adjudicada: " + Licitacion.Titulo;

        public override string Cuerpo =>
            Saludo + "\n\n" +
            "Nos complace informarle que su oferta por " + Monto(_oferta.Monto) +
            " resultó adjudicada en la licitación \"" + Licitacion.Titulo + "\".\n\n" +
            "En los próximos días el departamento de proveeduría se comunicará con usted para coordinar la entrega." +
            Firma;
    }

    public sealed class NotificacionRechazo : NotificacionCorreoBase
    {
        public NotificacionRechazo(Licitacion licitacion, Proveedor proveedor) : base(licitacion, proveedor)
        {
        }

        public override TipoNotificacion Tipo => TipoNotificacion.Rechazo;
        public override string Asunto => "Resultado de la licitación: " + Licitacion.Titulo;

        public override string Cuerpo =>
            Saludo + "\n\n" +
            "Le agradecemos su participación en la licitación \"" + Licitacion.Titulo + "\". " +
            "Lamentamos informarle que su oferta no fue seleccionada en esta ocasión.\n\n" +
            "Le invitamos a participar en nuestras próximas licitaciones." +
            Firma;
    }
}
