namespace Comercio.Api.DTOs.Carrito
{
    public class CarritoResponseDto
    {
        public int CarritoId { get; set; }
        public ICollection<CarritoItemDto> Items { get; set; } = [];
        public Decimal Total => Items.Sum(i => i.SubTotal);
    }
}
