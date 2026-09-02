using Comercio.Api.DTOs.Paginacion;
using Comercio.Api.DTOs.Producto;
using Comercio.Api.Models;
using Comercio.Api.Service.Results;

namespace Comercio.Api.Service
{
    public interface IProductoService
    {
        Task<ResultadoPaginado<ProductoDto>> ObtenerProductosAsync(ProductoQueryParametros query);
        Task<ProductoDto?> ObtenerProductoPorIdAsync(int id);
        Task<Result<ProductoDto>> CrearProductoAsync(ProductoCrearDto dto);
        Task<Result> ModificarProductoAsync(int id, ProductoModificarDto dto);
        Task<Result> EliminarProductoAsync(int id);
    }
}
