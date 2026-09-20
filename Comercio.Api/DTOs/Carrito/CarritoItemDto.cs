namespace Comercio.Api.DTOs.Carrito
{
    public class CarritoItemDto
    {
        public int ProductoId { get; set; }
        public string Nombre { get; set; } = null!;
        public decimal PrecioUnitario { get; set; }
        public int Cantidad { get; set; }
        public decimal SubTotal => Cantidad * PrecioUnitario;
        public string? UrlImagen { get; set; } 
    }
}
