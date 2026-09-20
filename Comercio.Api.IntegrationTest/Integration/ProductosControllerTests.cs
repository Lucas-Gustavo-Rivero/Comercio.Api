using Comercio.Api.Data;
using Comercio.Api.DTOs.Paginacion;
using Comercio.Api.DTOs.Producto;
using Comercio.Api.Models;
using Comercio.Api.Service.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Renci.SshNet.Sftp;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;

namespace Comercio.Api.IntegrationTest.Integration
{
    public class ProductosControllerTests: IClassFixture<ComercioApiFixture>
    {
        private readonly HttpClient _client;
        private readonly AppDbContext _context;
        private readonly ComercioApiFixture _fixture;
        private const string RutaBase = "/api/productos";


        public ProductosControllerTests(ComercioApiFixture fixture)
        {
            _fixture = fixture;

            _client = fixture.HttpClient;

            var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlServer(fixture.DbContext.Database.GetConnectionString()).Options;

            _context = new AppDbContext(options);
        }

        [Fact]
        public async Task PostProducto_ComoAdministradorConNombreVacio_DevuelveBadRequest()
        {
            //ARRANGE
            var token = await _fixture.ObtenerTokenAdminAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var dto = new ProductoCrearDto { Nombre = "" , Precio = 123, Stock = 1};

            //ACT

            var response = await _client.PostAsJsonAsync(RutaBase, dto);

            //ASSERT

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task PostProducto_ComoAdministradorDatosValidos_DevuelveCreatedConProductoDto()
        {
            //ARRANGE
            var token = await _fixture.ObtenerTokenAdminAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var nombreUnico = $"Producto-{Guid.NewGuid()}";
            var dto = new ProductoCrearDto { Nombre = nombreUnico, Precio = 123, Stock = 2 };

            //ACT

            var response = await _client.PostAsJsonAsync(RutaBase, dto);

            //ASSERT

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var productoCreado = await response.Content.ReadFromJsonAsync<ProductoDto>();
            Assert.NotNull(productoCreado);
            Assert.Equal(nombreUnico, productoCreado.Nombre);
            Assert.True(productoCreado.Id > 0);
        }

        [Fact]
        public async Task EliminarProducto_ComoAdministradorIdInvalido_DevuelveNotFound()
        {
            //ARRANGE
            var token = await _fixture.ObtenerTokenAdminAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var idInvalido = 999;

            //ACT
            var response = await _client.DeleteAsync($"{RutaBase}/{idInvalido}");

            //ASSERT
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            Assert.NotNull(problemDetails);
            Assert.Equal(StatusCodes.Status404NotFound, problemDetails.Status);
            Assert.Equal("Recurso no encontrado", problemDetails.Title);
            Assert.Contains(idInvalido.ToString(), problemDetails.Detail);
        }

        [Fact]
        public async Task EliminarProducto_ComoAdministradorIdValido_DevuelveNoContent()
        {
            //ARRANGE
            var token = await _fixture.ObtenerTokenAdminAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var nombreUnico = $"Producto-{Guid.NewGuid()}";
            var producto = new Producto { Nombre = nombreUnico, Precio = 100, Stock = 1, RowVersion = Array.Empty<byte>() };

            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();

            //ACT

            var response = await _client.DeleteAsync($"{RutaBase}/{producto.Id}");

            //ASSERT

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlServer(_context.Database.GetConnectionString()).Options;

            await using var contextoVerificacion = new AppDbContext(options);

            var productoBorrado = await contextoVerificacion.Productos.FindAsync(producto.Id);
            Assert.Null(productoBorrado);
        }

        [Fact]
        public async Task ModificarProducto_ComoAdministradorIdsNoCoinciden_DevuelveBadRequest()
        {
            //ARRANGE
            var token = await _fixture.ObtenerTokenAdminAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var dto = new ProductoModificarDto
            {
                Id = 1,
                Nombre = "test",
                Stock = 1,
                Precio = 1,
                RowVersion = new byte[] { 1 }
            };

            var idRuta = 999;

            //ACT

            var response = await _client.PutAsJsonAsync($"{RutaBase}/{idRuta}", dto);

            //ASSERT

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();

            Assert.NotNull(problemDetails);
            Assert.Equal(StatusCodes.Status400BadRequest, problemDetails.Status);
            Assert.Equal("El id de la ruta no coincide con el id del cuerpo", problemDetails.Detail);
            Assert.Equal("Solicitud invalida", problemDetails.Title);
        }

        [Fact]
        public async Task ModificarProducto_ComoAdministradorProductoNoExist_DevuelveNotFound()
        {
            //ARRANGE
            var token = await _fixture.ObtenerTokenAdminAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var idRuta = 99999;

            var dto = new ProductoModificarDto { Id = idRuta, Nombre = "test", Stock = 2, Precio = 1, RowVersion = new byte[] { 1 } };

            //ACT

            var response = await _client.PutAsJsonAsync($"{RutaBase}/{idRuta}", dto);

            //ASSERT

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();

            Assert.NotNull(problemDetails);
            Assert.Equal(StatusCodes.Status404NotFound, problemDetails.Status);
            Assert.Equal("Recurso no encontrado", problemDetails.Title);
            Assert.Contains(idRuta.ToString(), problemDetails.Detail);
        }

        [Fact]
        public async Task ModificarProducto_ComoAdministradorProductoExiste_DevuelveNoContent()
        {
            //ARRANGE
            var token = await _fixture.ObtenerTokenAdminAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var nombreUnico = $"Producto-{Guid.NewGuid()}";
            var producto = new Producto { Nombre = nombreUnico, Stock = 1, Precio = 1, RowVersion = Array.Empty<byte>() };
            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();

            var idRuta = producto.Id;
            var dto = new ProductoModificarDto { Id = idRuta, Nombre = producto.Nombre, Stock = 3, Precio = producto.Precio, RowVersion = producto.RowVersion };

            //ACT

            var response = await _client.PutAsJsonAsync($"{RutaBase}/{idRuta}", dto);

            //ASSERT

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);


            //Verificamos que se haya modificado el producto en la base de datos

            var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlServer(_context.Database.GetConnectionString()).Options;

            await using var context = new AppDbContext(options);

            var productoModificado = await context.Productos.FindAsync(idRuta);

            Assert.NotNull(productoModificado);
            Assert.Equal(productoModificado.Stock, dto.Stock);
        }

        [Fact]
        public async Task ModificarProducto_ComoAdministradorRowVersionDesactualizado_DevuelveConflict()
        {
            //ARRANGE
            var token = await _fixture.ObtenerTokenAdminAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var nombreUnico = $"Producto-{Guid.NewGuid()}";
            var producto = new Producto { Nombre = nombreUnico, Stock = 3, Precio =  3 , RowVersion = Array.Empty<byte>()};
            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();

            var idRuta = producto.Id;
            var dtoUsuarioA = new ProductoModificarDto { Id = idRuta, Nombre = nombreUnico, Precio = 1000, Stock = 1, RowVersion = producto.RowVersion };
            var dtoUsuarioB = new ProductoModificarDto { Id = idRuta, Nombre = nombreUnico, Precio = 3, Stock = 100, RowVersion = producto.RowVersion };


            //ACT

            var responseUsuarioB = await _client.PutAsJsonAsync($"{RutaBase}/{idRuta}", dtoUsuarioB);
            var responseUsuarioA = await _client.PutAsJsonAsync($"{RutaBase}/{idRuta}", dtoUsuarioA);

            //ASSERT
            Assert.Equal(HttpStatusCode.NoContent, responseUsuarioB.StatusCode);

            Assert.Equal(HttpStatusCode.Conflict, responseUsuarioA.StatusCode);

            var problemDetails = await responseUsuarioA.Content.ReadFromJsonAsync<ProblemDetails>();

            Assert.NotNull(problemDetails);
            Assert.Equal(StatusCodes.Status409Conflict, problemDetails.Status);
            Assert.Equal("Conflicto", problemDetails.Title);
            Assert.Equal("El producto ya fue modificado por otro usuario, vuelva a cargar la pagina para ver los cambios", problemDetails.Detail);
        }

        [Fact]
        public async Task GetProducto_ProductoNoExiste_DevuelveNotFound()
        {
            //ARRANGE

            var idRuta = 9999;

            //ACT

            var response = await _client.GetAsync($"{RutaBase}/{idRuta}");

            //ASSERT

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();

            Assert.NotNull(problemDetails);
            Assert.Equal(StatusCodes.Status404NotFound, problemDetails.Status);
            Assert.Equal("Recurso no encontrado", problemDetails.Title);
            Assert.Contains(idRuta.ToString(), problemDetails.Detail);
        }

        [Fact]
        public async Task GetProducto_ProductoExiste_DevuelveOk()
        {
            //ARRANGE

            var nombreUnico = $"Producto-{Guid.NewGuid()}";
            var producto = new Producto { Nombre =  nombreUnico, Precio = 1, Stock = 1, RowVersion = Array.Empty<byte>()};
            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();

            var idRuta = producto.Id;

            //ACT

            var response = await _client.GetAsync($"{RutaBase}/{idRuta}");

            //ASSERT

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var productoDto = await response.Content.ReadFromJsonAsync<ProductoDto>();

            Assert.NotNull(productoDto);
            Assert.Equal(productoDto.Id, producto.Id);
            Assert.Equal(productoDto.Nombre, producto.Nombre);
            Assert.Equal(productoDto.Precio, producto.Precio);
            Assert.Equal(productoDto.Stock, producto.Stock);
        }

        [Fact]
        public async Task GetProductos_ConBusqueda_DevuelveSoloLosQueCoinciden()
        {
            //ARRANGE

            var prefijoUnico = $"Buscable-{Guid.NewGuid()}";

            var productoQueCoincide = new Producto { Nombre =  prefijoUnico, Precio = 1, Stock = 1, RowVersion = Array.Empty<byte>()};
            var productoQueNoCoincide = new Producto { Nombre = $"Otro-{Guid.NewGuid()}", Precio = 1, Stock = 2, RowVersion = Array.Empty<byte>()};

            _context.Productos.AddRange(productoQueCoincide, productoQueNoCoincide);
            await _context.SaveChangesAsync();

            //ACT

            var response = await _client.GetAsync($"{RutaBase}?Busqueda={prefijoUnico}");

            //ASSERT

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var resultado = await response.Content.ReadFromJsonAsync<ResultadoPaginado<ProductoDto>>();

            Assert.NotNull(resultado);
            Assert.Single(resultado.Datos);
            Assert.Equal(productoQueCoincide.Nombre, resultado.Datos.First().Nombre);
        }

        [Fact]
        public async Task GetProductos_OrdenadoPorPrecioYDescendenteFalso_DevuelveEnOrdenAscendente()
        {

            //ARRANGE
            var prefijoUnico = $"OrdenPrecio-{Guid.NewGuid()}";

            var productos = new List<Producto>
            {
                new Producto{Nombre = $"{prefijoUnico}-1", Precio = 100, Stock = 3, RowVersion = Array.Empty<byte>()},
                new Producto{Nombre = $"{prefijoUnico}-2", Precio = 10, Stock = 3, RowVersion = Array.Empty<byte>()},
                new Producto{Nombre = $"{prefijoUnico}-3", Precio = 300, Stock = 3, RowVersion = Array.Empty<byte>()},
                new Producto{Nombre = $"{prefijoUnico}-4", Precio = 1, Stock = 3, RowVersion = Array.Empty<byte>()}
            };

            _context.AddRange(productos);
            await _context.SaveChangesAsync();

            //ACT

            var response = await _client.GetAsync($"{RutaBase}?Busqueda={prefijoUnico}&OrdenarPor=Precio&Descendente=false");

            //ASSERT

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var resultado = await response.Content.ReadFromJsonAsync<ResultadoPaginado<ProductoDto>>();

            Assert.NotNull(resultado);

            var listaEsperada = productos.OrderBy(p => p.Precio).Select(p => p.Nombre).ToList();
            var listaDevuelta = resultado.Datos.Select(p => p.Nombre).ToList();

            Assert.Equal(listaEsperada, listaDevuelta);
            
        }


        [Fact]
        public async Task GetProductos_ConPrecioMax_DevuelveSoloLosQueCoinciden()
        {
            //ARRANGE

            var prefijoUnico = $"PrecioMax-{Guid.NewGuid()}";

            var productoCoincide1 = new Producto { Nombre = $"{prefijoUnico}-1", Precio = 1, Stock = 1, RowVersion = Array.Empty<byte>()};
            var productoCoincide2 = new Producto { Nombre = $"{prefijoUnico}-2", Precio = 10, Stock = 1, RowVersion = Array.Empty<byte>() };
            var productoNoCoincide = new Producto { Nombre = $"{prefijoUnico}-3", Precio = 30, Stock = 1, RowVersion = Array.Empty<byte>() };

            _context.AddRange(productoCoincide1, productoCoincide2, productoNoCoincide);
            await _context.SaveChangesAsync();

            //ACT

            var response = await _client.GetAsync($"{RutaBase}?Busqueda={prefijoUnico}&PrecioMax=10");

            //ASSERT

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var resultado = await response.Content.ReadFromJsonAsync<ResultadoPaginado<ProductoDto>>();

            Assert.NotNull(resultado);
            Assert.Equal(2, resultado.Datos.Count());

            var nombresDevueltos = resultado.Datos.Select(p => p.Nombre).ToHashSet();
            var nombresEsperados = new[] { productoCoincide1.Nombre, productoCoincide2.Nombre }.ToHashSet();

            Assert.Equal(nombresEsperados, nombresDevueltos);

            Assert.DoesNotContain(productoNoCoincide.Nombre, nombresDevueltos);

            Assert.All(resultado.Datos, p => Assert.True(p.Precio <= 10));
        }

        [Fact]
        public async Task GetProductos_ConPrecioMin_DevuelveSoloLosqueCoinciden()
        {
            //ARRANGE

            var prejifoUnico = $"PrecioMin-{Guid.NewGuid()}";

            var productoCoincide1 = new Producto { Nombre = $"{prejifoUnico}-1", Precio = 20, Stock = 1, RowVersion = Array.Empty<byte>() };
            var productoCoincide2 = new Producto { Nombre = $"{prejifoUnico}-2", Precio = 10, Stock = 1, RowVersion = Array.Empty<byte>() };
            var productoNoCoincide = new Producto { Nombre = $"{prejifoUnico}-3", Precio = 5, Stock = 1, RowVersion = Array.Empty<byte>() };

            _context.Productos.AddRange(productoCoincide1, productoCoincide2, productoNoCoincide);
            await _context.SaveChangesAsync();

            //ACT

            var response = await _client.GetAsync($"{RutaBase}?Busqueda={prejifoUnico}&PrecioMin=10");

            //ASSERT

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var resultado = await response.Content.ReadFromJsonAsync<ResultadoPaginado<ProductoDto>>();

            Assert.NotNull(resultado);
            Assert.Equal(2, resultado.Datos.Count());

            var nombresDevueltos = resultado.Datos.Select(p => p.Nombre).ToHashSet();
            var nombresEsperados = new[] { productoCoincide1.Nombre, productoCoincide2.Nombre }.ToHashSet();

            Assert.Equal(nombresEsperados, nombresDevueltos);

            Assert.DoesNotContain(productoNoCoincide.Nombre, nombresDevueltos);

            Assert.All(resultado.Datos, p => Assert.True(p.Precio >= 10));
        }

        [Fact]
        public async Task GetProductos_PrecioMinMayorQuePrecioMax_DevuelveBadRequest()
        {
            //ACT
            var response = await _client.GetAsync($"{RutaBase}?PrecioMin=500&PrecioMax=100");

            //ASSERT

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetProductos_Paginacion_DevuelveCantidadYTotalesCorrectos()
        {
            //ARRANGE

            var prefijoUnico = $"Paginacion-{Guid.NewGuid()}";

            var productos = new List<Producto>
            {
                new Producto {Nombre = $"{prefijoUnico}-1", Precio = 1, Stock = 1, RowVersion = Array.Empty<byte>()},
                new Producto {Nombre = $"{prefijoUnico}-2", Precio = 2, Stock = 2, RowVersion = Array.Empty<byte>()},
                new Producto {Nombre = $"{prefijoUnico}-3", Precio = 3, Stock = 3, RowVersion = Array.Empty<byte>()},
                new Producto {Nombre = $"{prefijoUnico}-4", Precio = 4, Stock = 4, RowVersion = Array.Empty<byte>()},
            };

            _context.Productos.AddRange(productos);
            await _context.SaveChangesAsync();

            //ACT - pagina 1

            var responsePagina1 = await _client.GetAsync($"{RutaBase}?Busqueda={prefijoUnico}&NumeroPagina=1&TamPagina=2");

            //ASSERT - pagina 1

            Assert.Equal(HttpStatusCode.OK, responsePagina1.StatusCode);
            var resultadoPagina1 = await responsePagina1.Content.ReadFromJsonAsync<ResultadoPaginado<ProductoDto>>();

            Assert.NotNull(resultadoPagina1);
            Assert.Equal(2, resultadoPagina1.TamPagina);
            Assert.Equal(1, resultadoPagina1.NumeroPagina);
            Assert.Equal(4, resultadoPagina1.TotalRegistros);
            Assert.Equal(2, resultadoPagina1.TotalPaginas);
            Assert.Equal(2, resultadoPagina1.Datos.Count());

            //ACT - pagina 2

            var responsePagina2 = await _client.GetAsync($"{RutaBase}?Busqueda{prefijoUnico}&NumeroPagina=2&TamPagina=2");

            //ASSERT - pagina 2

            Assert.Equal(HttpStatusCode.OK, responsePagina2.StatusCode);

            var resultadoPagina2 = await responsePagina2.Content.ReadFromJsonAsync<ResultadoPaginado<ProductoDto>>();

            Assert.NotNull(resultadoPagina2);
            Assert.Equal(2, resultadoPagina2.NumeroPagina);
            Assert.Equal(2, resultadoPagina2.Datos.Count());

            var nombresPagina1 = resultadoPagina1.Datos.Select(p => p.Nombre).ToHashSet();
            var nombresPagina2 = resultadoPagina2.Datos.Select(p => p.Nombre).ToHashSet();

            Assert.Empty(nombresPagina1.Intersect(nombresPagina2));
        }

        [Fact]
        public async Task EliminarProducto_SinToken_DevuelveUnauthorized()
        {
            //ACT
            var response = await _client.DeleteAsync($"{RutaBase}/1");

            //ASSERT
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task EliminarProducto_ComoUsuario_DevuelveForbidden()
        {
            //ARRANGE
            var token = await _fixture.ObtenerTokenUsuarioAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            //ACT

            var response = await _client.DeleteAsync($"{RutaBase}/1");

            //ASSERT

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task ModificarProducto_SinToken_DevuelveUnauthorized()
        {
            //ACT
            var response = await _client.PutAsJsonAsync($"{RutaBase}/1", new ProductoModificarDto { Id = 1, Nombre = "test", Precio = 123, Stock = 123 });

            //ASSERT

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task ModificarProducto_ComoUsuario_DevuelveForbidden()
        {
            //ARRANGE
            var token = await _fixture.ObtenerTokenUsuarioAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            //ACT

            var response = await _client.PutAsJsonAsync($"{RutaBase}/1", new ProductoModificarDto { Id = 1, Nombre = "test", Precio = 123, Stock = 123 });

            //ASSERT

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task PostProducto_SinToken_DevuelveUnauthorized()
        {
            //ACT
            var response = await _client.PostAsJsonAsync($"{RutaBase}", new ProductoCrearDto { Nombre = "test", Precio = 123, Stock = 123 });

            //ASSERT

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task PostProducto_ComoUsuario_DevuelveForbidden()
        {
            //ARRANGE
            var token = await _fixture.ObtenerTokenUsuarioAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            //ACT
            var response = await _client.PostAsJsonAsync($"{RutaBase}", new ProductoCrearDto { Nombre = "test", Precio = 123, Stock = 123 });

            //ASSERT

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

    }
}
