namespace Comercio.Api.Models
{
    public class Carrito
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();


        //Propiedades de navigacion

        public User User { get; set; } = null!;
        public ICollection<CarritoItem> CarritoItems { get; set; } = [];
    }
}
