using Comercio.Api.Data;
using Comercio.Api.DTOs.Paginacion;
using Comercio.Api.DTOs.Producto;
using Comercio.Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Net.Quic;

namespace Comercio.Api.Repository
{
    public class ProductoRepository:IProductoRepository
    {
        private readonly AppDbContext _context;

        public ProductoRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task CrearProductoAsync(Producto producto)
        {
            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();
        }

        public async Task EliminarProductoAsync(Producto producto)
        {
            _context.Productos.Remove(producto);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExisteProductoPorNombreAsync(string nombre)
        {
            var producto = await _context.Productos.AnyAsync(p => p.Nombre == nombre);
            return producto;
        }

        public async Task GuardarProductoAsync(Producto producto, byte[]? version = null)
        {
            if(version != null && version.Length > 0)
            {
                _context.Entry(producto).Property(p => p.RowVersion).OriginalValue = version;
            }

            _context.Productos.Update(producto);
            await _context.SaveChangesAsync();
        }

        public async Task<Producto?> ObtenerProductoPorIdAsync(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            return producto;
        }

        public async Task<ResultadoPaginado<Producto>> ObtenerProductosAsync(ProductoQueryParametros parametros)
        {
            var productoQuery = _context.Productos.AsQueryable();

            productoQuery = AplicarFiltro(productoQuery, parametros);

            var totalRegistros = await productoQuery.CountAsync();

            productoQuery = AplicarOrden(productoQuery, parametros);

            var productos = await productoQuery.Skip((parametros.NumeroPagina - 1) * parametros.TamPagina)
                .Take(parametros.TamPagina).AsNoTracking().ToListAsync();

            return new ResultadoPaginado<Producto>(productos, parametros.NumeroPagina, parametros.TamPagina, totalRegistros);
        }

        private static IQueryable<Producto> AplicarFiltro(IQueryable<Producto> query, ProductoQueryParametros parametros)
        {
            if (!string.IsNullOrWhiteSpace(parametros.Busqueda))
            {
                query = query.Where(p => p.Nombre.Contains(parametros.Busqueda));
            }

            if (parametros.PrecioMin.HasValue)
            {
                query = query.Where(p => p.Precio >= parametros.PrecioMin);
            }
            if (parametros.PrecioMax.HasValue)
            {
                query = query.Where(p => p.Precio <= parametros.PrecioMax);
            }

            return query;
        }

        private static IQueryable<Producto> AplicarOrden(IQueryable<Producto> query, ProductoQueryParametros parametros)
        {
            query = parametros.OrdenarPor switch
            {
                ProductoOrdenarPor.Precio =>
                    parametros.Descendente
                    ? query.OrderByDescending(p => p.Precio)
                    : query.OrderBy(p => p.Precio),

                ProductoOrdenarPor.Nombre => parametros.Descendente
                    ? query.OrderByDescending(p => p.Nombre)
                    : query.OrderBy(p => p.Nombre),
                _ => query.OrderBy(p => p.Id)
            };

            return query;
        }
    }
}
