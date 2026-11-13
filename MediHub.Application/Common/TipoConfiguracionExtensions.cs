using MediHub.Domain.Enums;

namespace MediHub.Application.Common
{
    public static class TipoConfiguracionExtensions
    {
        public static string Describir(this TipoConfiguracion tipo)
        {
            switch (tipo)
            {
                case TipoConfiguracion.Plataforma: return "1 – Plataforma";
                case TipoConfiguracion.Externa: return "2 – Externa";
                case TipoConfiguracion.Ambas: return "3 – Ambas";
                default: return tipo.ToString();
            }
        }
    }
}
