using AutoMapper;
using Comercio.Api.DTOs.Producto;
using Comercio.Api.Models;

namespace Comercio.Api.Mappings
{
    public class ProductoProfile:Profile
    {
        public ProductoProfile()
        {
            CreateMap<Producto, ProductoDto>();
            CreateMap<ProductoCrearDto, Producto>();
            CreateMap<ProductoModificarDto, Producto>();
        }
    }
}
