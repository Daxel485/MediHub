using AutoMapper;
using MediHub.Application.DTOs;
using MediHub.Domain.Enums;
using MediHub.Web.ViewModels;

namespace MediHub.Web.Mappings
{
    // Mapeos entre los ViewModels de los formularios y los DTOs de la capa de aplicación.
    public class WebProfile : Profile
    {
        public WebProfile()
        {
            CreateMap<ProveedorFormViewModel, ProveedorDto>()
                .ForMember(d => d.FechaDeRegistro, o => o.Ignore())
                .ForMember(d => d.FechaDeModificacion, o => o.Ignore())
                .ForMember(d => d.Estado, o => o.Ignore());

            // Los campos vacíos llegan como null y se envían con valores que el validador rechaza.
            CreateMap<ConfiguracionFormViewModel, ConfiguracionProveedorDto>()
                .ForMember(d => d.IdProveedor, o => o.MapFrom(s => s.IdProveedor ?? 0))
                .ForMember(d => d.TipoConfiguracion, o => o.MapFrom(s => (TipoConfiguracion)(s.TipoConfiguracion ?? 0)))
                .ForMember(d => d.Comision, o => o.MapFrom(s => s.Comision ?? -1))
                .ForMember(d => d.NombreProveedor, o => o.Ignore())
                .ForMember(d => d.TipoConfiguracionDescripcion, o => o.Ignore())
                .ForMember(d => d.FechaDeRegistro, o => o.Ignore())
                .ForMember(d => d.FechaDeModificacion, o => o.Ignore());
            CreateMap<ConfiguracionProveedorDto, ConfiguracionFormViewModel>()
                .ForMember(d => d.TipoConfiguracion, o => o.MapFrom(s => (int)s.TipoConfiguracion))
                .ForMember(d => d.Proveedores, o => o.Ignore())
                .ForMember(d => d.Tipos, o => o.Ignore());

            CreateMap<UsuarioFormViewModel, UsuarioDto>()
                .ForMember(d => d.IdProveedor, o => o.MapFrom(s => s.IdProveedor ?? 0))
                .ForMember(d => d.NombreProveedor, o => o.Ignore())
                .ForMember(d => d.IdNetUser, o => o.Ignore())
                .ForMember(d => d.FechaDeRegistro, o => o.Ignore())
                .ForMember(d => d.FechaDeModificacion, o => o.Ignore());
            CreateMap<UsuarioDto, UsuarioFormViewModel>()
                .ForMember(d => d.Proveedores, o => o.Ignore());

            CreateMap<LicitacionFormViewModel, LicitacionDto>()
                .ForMember(d => d.IdLicitacion, o => o.Ignore())
                .ForMember(d => d.FechaDeRegistro, o => o.Ignore())
                .ForMember(d => d.Estado, o => o.Ignore())
                .ForMember(d => d.CantidadDeOfertas, o => o.Ignore())
                .ForMember(d => d.OfertasPendientes, o => o.Ignore());
        }
    }
}
