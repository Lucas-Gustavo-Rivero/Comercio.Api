namespace Comercio.Api.Models
{
    public class CarritoItem
    {
        public int Id { get; set; }
        public int CarritoId { get; set; }
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }

        //Propiedades de navegacion

        public Carrito Carrito { get; set; } = null!;
        public Producto Producto { get; set; } = null!;

    }
}
