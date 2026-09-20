using Comercio.Api.Data;
using Comercio.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Comercio.Api.Repository
{
    public class CarritoRepository:ICarritoRepository
    {
        private readonly AppDbContext _context;

        public CarritoRepository(AppDbContext context)
        {
            _context = context;
        }

        public void CrearCarrito(Carrito carrito)
        {
            _context.Carritos.Add(carrito);
        }

        public void EliminarCarrito(Carrito carrito)
        {
            _context.Carritos.Remove(carrito);
        }

        public async Task<Carrito?> GetCarritoAsync(int userId)
        {
            var carrito = await _context.Carritos.Include(c => c.CarritoItems).ThenInclude(ci => ci.Producto).FirstOrDefaultAsync(c => c.UsuarioId == userId);
            return carrito;
        }

        public async Task GuardarCambiosAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
