
namespace Comercio.Api.Models
{
    public class Producto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public string? UrlImagen { get; set; }
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    }
}
