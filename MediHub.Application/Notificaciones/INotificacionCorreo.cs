namespace MediHub.Application.Notificaciones
{
    // Producto del patrón Factory Method: un correo listo para enviarse.
    public interface INotificacionCorreo
    {
        TipoNotificacion Tipo { get; }
        string Destinatario { get; }
        string Asunto { get; }
        string Cuerpo { get; }
    }
}
