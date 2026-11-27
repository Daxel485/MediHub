using System;
using System.IO;
using System.Net.Mail;
using System.Text;
using MediHub.Application.Interfaces;

namespace MediHub.Infrastructure.Servicios
{
    // Envía los correos con SmtpClient, que toma servidor, puerto, remitente y modo de entrega de
    // <system.net><mailSettings> en Web.config. En desarrollo se usa SpecifiedPickupDirectory: cada correo
    // queda como archivo .eml en App_Data\Correos en lugar de salir a un servidor real.
    public class ServicioCorreoSmtp : IServicioCorreo
    {
        private const string CarpetaPorDefecto = @"App_Data\Correos";

        public void Enviar(string destinatario, string asunto, string cuerpo)
        {
            using (var mensaje = new MailMessage())
            using (var cliente = new SmtpClient())
            {
                mensaje.To.Add(destinatario);
                mensaje.Subject = asunto;
                mensaje.Body = cuerpo;
                mensaje.SubjectEncoding = Encoding.UTF8;
                mensaje.BodyEncoding = Encoding.UTF8;
                mensaje.IsBodyHtml = false;

                if (cliente.DeliveryMethod == SmtpDeliveryMethod.SpecifiedPickupDirectory)
                    cliente.PickupDirectoryLocation = ResolverCarpeta(cliente.PickupDirectoryLocation);

                // Si el mensaje no trae remitente, SmtpClient usa el atributo "from" de mailSettings.
                cliente.Send(mensaje);
            }
        }

        // SmtpClient exige una ruta absoluta; una ruta relativa se resuelve contra la carpeta de la aplicación.
        private static string ResolverCarpeta(string ruta)
        {
            if (string.IsNullOrWhiteSpace(ruta)) ruta = CarpetaPorDefecto;
            if (!Path.IsPathRooted(ruta))
                ruta = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ruta.TrimStart('~', '/', '\\'));

            Directory.CreateDirectory(ruta);
            return ruta;
        }
    }
}
