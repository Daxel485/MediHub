namespace MediHub.Application.Interfaces
{
    // Envío de correo electrónico. La implementación (SMTP) vive en la capa de infraestructura.
    public interface IServicioCorreo
    {
        void Enviar(string destinatario, string asunto, string cuerpo);
    }
}
