using System.Linq;
using AutoMapper;
using MediHub.Application.Common;
using MediHub.Application.DTOs;
using MediHub.Domain.Entities;

namespace MediHub.Application.Mappings
{
    public class ApplicationProfile : Profile
    {
        public ApplicationProfile()
        {
            CreateMap<Proveedor, ProveedorDto>();
            CreateMap<ProveedorDto, Proveedor>()
                .ForMember(d => d.Ofertas, o => o.Ignore())
                .ForMember(d => d.Usuarios, o => o.Ignore());

            CreateMap<ConfiguracionProveedor, ConfiguracionProveedorDto>()
                .ForMember(d => d.NombreProveedor, o => o.MapFrom(s => s.Proveedor.Nombre))
                .ForMember(d => d.TipoConfiguracionDescripcion,
                    o => o.MapFrom(s => s.TipoConfiguracion.Describir()));
            CreateMap<ConfiguracionProveedorDto, ConfiguracionProveedor>()
                .ForMember(d => d.Proveedor, o => o.Ignore());

            CreateMap<ReporteMensual, ReporteMensualDto>()
                .ForMember(d => d.NombreProveedor, o => o.MapFrom(s => s.Proveedor.Nombre));

            CreateMap<Usuario, UsuarioDto>()
                .ForMember(d => d.NombreProveedor, o => o.MapFrom(s => s.Proveedor.Nombre));
            CreateMap<UsuarioDto, Usuario>()
                .ForMember(d => d.Proveedor, o => o.Ignore());

            CreateMap<Licitacion, LicitacionDto>()
                .ForMember(d => d.CantidadDeOfertas, o => o.MapFrom(s => s.Ofertas.Count))
                .ForMember(d => d.OfertasPendientes,
                    o => o.MapFrom(s => s.Ofertas.Count(of => !of.Sincronizada)));
            CreateMap<LicitacionDto, Licitacion>()
                .ForMember(d => d.FechaDeModificacion, o => o.Ignore())
                .ForMember(d => d.Ofertas, o => o.Ignore());

            CreateMap<Oferta, OfertaDto>()
                .ForMember(d => d.NombreProveedor, o => o.MapFrom(s => s.Proveedor.Nombre));
        }
    }
}
