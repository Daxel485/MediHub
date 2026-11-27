using MediHub.Application.Interfaces;
using MediHub.Application.Notificaciones;

namespace MediHub.Tests.Comun
{
    // Notificador real con las tres fábricas y un servicio de correo simulado.
    internal static class Notificador
    {
        public static NotificadorLicitacion Crear(IServicioCorreo correo, IBitacoraService bitacora)
        {
            return new NotificadorLicitacion(new CreadorDeNotificacion[]
            {
                new CreadorNotificacionPublicacion(correo),
                new CreadorNotificacionAdjudicacion(correo),
                new CreadorNotificacionRechazo(correo)
            }, bitacora);
        }
    }
}
