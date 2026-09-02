using System.ComponentModel.DataAnnotations;

namespace Comercio.Api.DTOs.Producto
{
    public class ProductoCrearDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public string? UrlImagen { get; set; }
    }
}
