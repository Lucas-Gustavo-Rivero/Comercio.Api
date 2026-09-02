using Comercio.Api.DTOs.Paginacion;

namespace Comercio.Api.DTOs.Producto
{
    public enum ProductoOrdenarPor
    {
        Precio,
        Nombre,
        Id
    }
    public class ProductoQueryParametros:PaginacionParams
    {
        public string? Busqueda {  get; set; }
        public decimal? PrecioMin { get; set; }
        public decimal? PrecioMax { get; set; }
        public ProductoOrdenarPor OrdenarPor { get; set; } = ProductoOrdenarPor.Id;
        public bool Descendente { get; set; } = false;
    }
}
