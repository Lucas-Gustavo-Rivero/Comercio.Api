using Comercio.Api.DTOs.Paginacion;
using Comercio.Api.DTOs.Producto;
using Comercio.Api.Models;

namespace Comercio.Api.Repository
{
    public interface IProductoRepository
    {
        Task<ResultadoPaginado<Producto>> ObtenerProductosAsync(ProductoQueryParametros parametros);
        Task<Producto?> ObtenerProductoPorIdAsync(int id);
        Task CrearProductoAsync(Producto producto);
        Task GuardarProductoAsync(Producto producto, byte[]? version = null);
        Task EliminarProductoAsync(Producto producto);
        Task<bool> ExisteProductoPorNombreAsync(string nombre);
    }
}
