using AutoMapper;
using Comercio.Api.DTOs.Paginacion;
using Comercio.Api.DTOs.Producto;
using Comercio.Api.Models;
using Comercio.Api.Repository;
using Comercio.Api.Service;
using Comercio.Api.Service.Results;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Comercio.Api.Tests.Service
{
    public class ProductoServiceTests
    {
        private readonly Mock<IProductoRepository> _repositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly ProductoService _productoService;

        public ProductoServiceTests()
        {
            _repositoryMock = new Mock<IProductoRepository>();
            _mapperMock = new Mock<IMapper>();
            _productoService = new ProductoService(_repositoryMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task ObtenerProductoPorIdAsync_NoExisteId_RetornaNull()
        {
            //ARRANGE
            int idNoExiste = 1111;
            _repositoryMock.Setup(repo => repo.ObtenerProductoPorIdAsync(idNoExiste)).ReturnsAsync((Producto?)null);

            //ACT
            var resultado = await _productoService.ObtenerProductoPorIdAsync(idNoExiste);

            //ASSERT
            Assert.Null(resultado);
        }

        [Fact]
        public async Task ObtenerProductoPorIdAsync_ExisteId_RetornaProductoDto()
        {
            //ARRANGE
            var id = 1;
            var producto = new Producto
            {
                Id = id,
                Nombre = "test",
                Precio = 1,
            };

            var productoDto = new ProductoDto
            {
                Id = id,
                Nombre = "test",
                Precio = 1
            };

            _repositoryMock.Setup(repo => repo.ObtenerProductoPorIdAsync(id)).ReturnsAsync(producto);
            _mapperMock.Setup(m => m.Map<ProductoDto>(producto)).Returns(productoDto);

            //ACT

            var resultado = await _productoService.ObtenerProductoPorIdAsync(id);

            //ASSERT

            Assert.NotNull(resultado);
            Assert.Same(productoDto, resultado);

            _mapperMock.Verify(repo => repo.Map<ProductoDto>(producto), Times.Once);
        }

        [Fact]
        public async Task ModificarProductoAsync_ProductoNoExiste_RetornaFailureNotFound()
        {
            //ARRANGE
            var idNoExiste = 99999;

            var productoDto = new ProductoModificarDto
            {
                Id = idNoExiste
            };

            _repositoryMock.Setup(repo => repo.ObtenerProductoPorIdAsync(idNoExiste)).ReturnsAsync((Producto?)null);

            //ACT
            var resultado = await _productoService.ModificarProductoAsync(idNoExiste, productoDto);

            //ASSERT

            Assert.False(resultado.IsSuccess);
            Assert.Equal(ErrorType.NotFound, resultado.ErrorType);

            //Verificamos que nunca se llame a la sobrecarga exacta que usa modificarProductoAsync
            //En este caso verificamos que nunca se haya llamado al map
            _mapperMock.Verify(m => m.Map(It.IsAny<ProductoModificarDto>(), It.IsAny<Producto>()), Times.Never);

            /*Verificamos que no se haya guardado nada en la base de datos, en pocas palabras, estamos chequeando
            que el metodo GuardarProductoAsync nunca se haya ejecutado
             */

            _repositoryMock.Verify(repo => repo.GuardarProductoAsync(It.IsAny<Producto>(), It.IsAny<byte[]>()), Times.Never);

        }

        [Fact]
        public async Task ModificarProductoAsync_ActualizacionExitosa_RetornaSuccess()
        {
            //ARRANGE
            var idExiste = 1;
            var rowVersion = new byte[] { 1, 2, 3 };

            var productoDto = new ProductoModificarDto
            {
                Id = idExiste,
            };

            var producto = new Producto { Id = idExiste, RowVersion = rowVersion};

            _repositoryMock.Setup(repo => repo.ObtenerProductoPorIdAsync(idExiste)).ReturnsAsync(producto);
            _repositoryMock.Setup(repo => repo.GuardarProductoAsync(producto, producto.RowVersion)).Returns(Task.CompletedTask);

            //ACT

            var resultado = await _productoService.ModificarProductoAsync(idExiste, productoDto);

            //ASSERT

            Assert.True(resultado.IsSuccess);
            Assert.Equal(ErrorType.Ninguno, resultado.ErrorType);

            _mapperMock.Verify(m => m.Map(productoDto, producto), Times.Once);
            _repositoryMock.Verify(repo => repo.GuardarProductoAsync(producto, producto.RowVersion), Times.Once);
        }

        [Fact]
        public async Task ModificarProductoAsync_ConflictoConcurrencia_RetornaFailureConflict()
        {

            //ARRANGE

            var idExiste = 1;

            var productoDto = new ProductoModificarDto { Id = idExiste };

            var producto = new Producto { Id = idExiste, RowVersion = new byte[] { 1, 2, 3 } };

            _repositoryMock.Setup(repo => repo.ObtenerProductoPorIdAsync(idExiste)).ReturnsAsync(producto);
            _repositoryMock.Setup(repo => repo.GuardarProductoAsync(producto, producto.RowVersion)).ThrowsAsync(new DbUpdateConcurrencyException());

            //ACT

            var resultado = await _productoService.ModificarProductoAsync(idExiste, productoDto);

            //ASSERT

            Assert.False(resultado.IsSuccess);
            Assert.Equal(ErrorType.Conflict, resultado.ErrorType);
            _mapperMock.Verify(m => m.Map(productoDto, producto), Times.Once);
            _repositoryMock.Verify(repo => repo.GuardarProductoAsync(producto, producto.RowVersion), Times.Once);
        }

        [Fact]
        public async Task EliminarProductoAsync_ProductoNoExiste_ReturnFailureNotFound()
        {
            //ARRANGE

            var idNoExiste = 999;
            _repositoryMock.Setup(repo => repo.ObtenerProductoPorIdAsync(idNoExiste)).ReturnsAsync((Producto?)null);

            //ACT

            var resultado = await _productoService.EliminarProductoAsync(idNoExiste);

            //ASSERT

            Assert.False(resultado.IsSuccess);
            Assert.Equal(ErrorType.NotFound, resultado.ErrorType);
            _repositoryMock.Verify(repo => repo.EliminarProductoAsync(It.IsAny<Producto>()), Times.Never);

        }

        [Fact]
        public async Task EliminarProductoAsync_ProductoExiste_RetornaSuccess()
        {

            //ARRANGE

            var idExiste = 1;
            var producto = new Producto { Id = idExiste };
            _repositoryMock.Setup(repo => repo.ObtenerProductoPorIdAsync(idExiste)).ReturnsAsync(producto);

            //ACT

            var repuesta = await _productoService.EliminarProductoAsync(idExiste);

            //ASSERT

            Assert.True(repuesta.IsSuccess);
            Assert.Equal(ErrorType.Ninguno, repuesta.ErrorType);
            _repositoryMock.Verify(repo => repo.EliminarProductoAsync(producto), Times.Once);
        }

        [Fact]
        public async Task CrearProductoAsync_ExisteProducto_RetornaFailureConflict()
        {
            //ARRANGE

            var productoCreateDto = new ProductoCrearDto { Nombre = "test"};

            _repositoryMock.Setup(repo => repo.ExisteProductoPorNombreAsync(productoCreateDto.Nombre)).ReturnsAsync(true);

            //ACT

            var resultado = await _productoService.CrearProductoAsync(productoCreateDto);

            //ASSERT

            Assert.False(resultado.IsSuccess);
            Assert.Equal(ErrorType.Conflict, resultado.ErrorType);
            _mapperMock.Verify(m => m.Map<Producto>(productoCreateDto), Times.Never);
            _repositoryMock.Verify(repo => repo.CrearProductoAsync(It.IsAny<Producto>()), Times.Never);
        }

        [Fact]
        public async Task CrearProductoAsync_NoExisteProducto_RetornaSuccess()
        {
            //ARRANGE

            var productoCreateDto = new ProductoCrearDto { Nombre = "test" };
            var producto = new Producto { Id = 1, Nombre = "test" };
            var productoDto = new ProductoDto { Id = 1, Nombre = "test" };

            _repositoryMock.Setup(repo => repo.ExisteProductoPorNombreAsync(productoCreateDto.Nombre)).ReturnsAsync(false);
            _mapperMock.Setup(m => m.Map<Producto>(productoCreateDto)).Returns(producto);
            _mapperMock.Setup(m => m.Map<ProductoDto>(producto)).Returns(productoDto);

            //ACT

            var resultado = await _productoService.CrearProductoAsync(productoCreateDto);

            //ASSERT

            Assert.True(resultado.IsSuccess);
            Assert.Equal(ErrorType.Ninguno, resultado.ErrorType);
            Assert.NotNull(resultado.Data);
            Assert.Same(productoDto, resultado.Data);

            _mapperMock.Verify(m => m.Map<ProductoDto>(producto), Times.Once);
            _repositoryMock.Verify(repo => repo.CrearProductoAsync(producto), Times.Once);
            _repositoryMock.Verify(repo => repo.ExisteProductoPorNombreAsync(productoCreateDto.Nombre), Times.Once);
        }

        [Fact]
        public async Task CrearProductoAsync_dbUpdateException_RetornaFailureConflict()
        {

            //ARRANGE
            var productoCreateDto = new ProductoCrearDto { Nombre = "test" };
            var producto = new Producto { Id = 1, Nombre = "test" };

            _repositoryMock.Setup(repo => repo.ExisteProductoPorNombreAsync(productoCreateDto.Nombre)).ReturnsAsync(false);
            _mapperMock.Setup(m => m.Map<Producto>(productoCreateDto)).Returns(producto);
            _repositoryMock.Setup(repo => repo.CrearProductoAsync(producto)).ThrowsAsync(new DbUpdateException());

            //ACT

            var resultado = await _productoService.CrearProductoAsync(productoCreateDto);

            //ASSERT

            Assert.False(resultado.IsSuccess);
            Assert.Equal(ErrorType.Conflict, resultado.ErrorType);
            _mapperMock.Verify(m => m.Map<Producto>(productoCreateDto), Times.Once);
            _mapperMock.Verify(m => m.Map<ProductoDto>(It.IsAny<Producto>()), Times.Never);
            _repositoryMock.Verify(repo => repo.CrearProductoAsync(producto), Times.Once);
            _repositoryMock.Verify(repo => repo.ExisteProductoPorNombreAsync(productoCreateDto.Nombre), Times.Once);

        }

        [Fact]
        public async Task ObtenerProductosAsync_RetornaResultadoPaginado()
        {
            //ARRANGE

            var parametros = new ProductoQueryParametros
            {
                NumeroPagina = 2,
                TamPagina = 5,
                Busqueda = "test"
            };

            var productos = new List<Producto>
            {
                new Producto {Id = 1, Nombre = "test_1"},
                new Producto {Id = 2, Nombre = "test_2"}
            };

            var productosPaginado = new ResultadoPaginado<Producto>(productos, 2, 5, 2);

            var productosDto = new List<ProductoDto>
            {
                new ProductoDto {Id = 1, Nombre = "test_1"},
                new ProductoDto {Id = 2, Nombre = "test_2"}
            };

            var productosDtoPaginado = new ResultadoPaginado<ProductoDto>(productosDto, 2, 5, 2);

            _repositoryMock.Setup(repo => repo.ObtenerProductosAsync(parametros)).ReturnsAsync(productosPaginado);
            _mapperMock.Setup(m => m.Map<IEnumerable<ProductoDto>>(productosPaginado.Datos)).Returns(productosDto);

            //ACT

            var resultado = await _productoService.ObtenerProductosAsync(parametros);

            //ASSERT

            Assert.NotNull(resultado);
            Assert.Equal(productosDtoPaginado.NumeroPagina, resultado.NumeroPagina);
            Assert.Equal(productosDtoPaginado.TamPagina, resultado.TamPagina);
            Assert.Equal(productosDtoPaginado.TotalPaginas, resultado.TotalPaginas);
            Assert.Equal(productosDtoPaginado.TotalRegistros, resultado.TotalRegistros);
            Assert.Same(productosDto, resultado.Datos);
            _repositoryMock.Verify(repo => repo.ObtenerProductosAsync(parametros), Times.Once);
            _mapperMock.Verify(m => m.Map<IEnumerable<ProductoDto>>(productosPaginado.Datos), Times.Once);

        }
    }
}
