using Comercio.Api.Data;
using Comercio.Api.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Comercio.Api.IntegrationTest.Integration
{
    public class ProductoRepositoryIntegrationTests: IClassFixture<ProductoApiFixture>
    {
        private readonly AppDbContext _context;

        public ProductoRepositoryIntegrationTests(ProductoApiFixture fixture)
        {
            _context = fixture.DbContext;
        }

        [Fact]
        public async Task Insertar_DosProductosConMismoNommbre_TirarExcepcionPorIndiceUnico()
        {
            //ARRANGE

            var nombreUnico = $"Producto-{Guid.NewGuid()}";

            var producto1 = new Producto { Nombre = nombreUnico };
            var producto2 = new Producto { Nombre = nombreUnico };

            _context.Productos.Add(producto1);
            await _context.SaveChangesAsync();

            _context.Productos.Add(producto2);

            //ACT Y ASSERT

            await Assert.ThrowsAsync<DbUpdateException>(() => _context.SaveChangesAsync());
        }

        [Fact]
        public async Task ModificarConcurrente_ConRowVersionDesactualizado_TiraDbUpdateConcurrencyException()
        {
            // ARRANGE

            var nombreUnico = $"Producto-{Guid.NewGuid()}";

            var producto = new Producto { Nombre = nombreUnico, Precio = 100, Stock = 10, RowVersion = Array.Empty<byte>() };

            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();

            // Dos "sesiones" independientes, simulando dos usuarios leyendo al mismo tiempo
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(_context.Database.GetConnectionString()).Options;

            await using var contextoUsuarioA = new AppDbContext(options);
            await using var contextoUsuarioB = new AppDbContext(options);

            var productoUsuarioA = await contextoUsuarioA.Productos.FindAsync(producto.Id);
            var productoUsuarioB = await contextoUsuarioB.Productos.FindAsync(producto.Id);

            //ACT

            productoUsuarioA!.Precio = 150;
            await contextoUsuarioA.SaveChangesAsync();

            productoUsuarioB!.Precio = 200;

            //ASSERT

            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => contextoUsuarioB.SaveChangesAsync());
        }

        [Fact]
        public async Task Insertar_PrecioConMasDeDosDecimales_SeAlmacenaConPrecisionDecimal18_2()
        {
            //ARRANGE
            var nombreUnico = $"Producto-{Guid.NewGuid}";

            var producto = new Producto { Nombre =  nombreUnico , Precio = 100.999m, Stock = 5, RowVersion = Array.Empty<byte>()};

            //ACT

            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();

            var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlServer(_context.Database.GetConnectionString()).Options;

            await using var contextoLectura = new AppDbContext(options);

            var productoGuardado = await contextoLectura.Productos.FindAsync(producto.Id);

            //ASSERT

            Assert.NotNull(productoGuardado);
            Assert.Equal(101.00m, productoGuardado.Precio);


            
        }
    }
}
