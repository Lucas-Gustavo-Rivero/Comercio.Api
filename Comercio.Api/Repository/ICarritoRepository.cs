using Comercio.Api.Models;

namespace Comercio.Api.Repository
{
    public interface ICarritoRepository
    {
        Task<Carrito?> GetCarritoAsync(int userId);
        void CrearCarrito(Carrito carrito);
        void EliminarCarrito(Carrito carrito);
        Task GuardarCambiosAsync();
    }
}
