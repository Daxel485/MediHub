using AutoMapper;
using MediHub.Application.Mappings;

namespace MediHub.Tests.Comun
{
    internal static class Mapeador
    {
        public static IMapper Crear()
        {
            return new MapperConfiguration(cfg => cfg.AddProfile<ApplicationProfile>()).CreateMapper();
        }
    }
}
