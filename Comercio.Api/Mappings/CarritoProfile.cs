using AutoMapper;
using Comercio.Api.DTOs.Carrito;
using Comercio.Api.Models;

namespace Comercio.Api.Mappings
{
    public class CarritoProfile: Profile
    {
        public CarritoProfile()
        {
            CreateMap<CarritoItem, CarritoItemDto>()
                .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.Producto.Nombre))
                .ForMember(dest => dest.UrlImagen, opt => opt.MapFrom(src => src.Producto.UrlImagen));

            CreateMap<Carrito, CarritoResponseDto>()
                .ForMember(dest => dest.CarritoId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.CarritoItems));
        }
    }
}
