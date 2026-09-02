using Comercio.Api.DTOs.Paginacion;
using Comercio.Api.DTOs.Producto;
using Comercio.Api.Extensions;
using Comercio.Api.Filters;
using Comercio.Api.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Comercio.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductosController : ControllerBase
    {
        private readonly IProductoService _productoService;

        public ProductosController(IProductoService productoService)
        {
            _productoService = productoService;
        }

        [HttpGet]
        [ServiceFilter(typeof(ValidationFilter<ProductoQueryParametros>))]
        public async Task<ActionResult<ResultadoPaginado<ProductoDto>>> GetProductos([FromQuery] ProductoQueryParametros parametros)
        {
            var productos = await _productoService.ObtenerProductosAsync(parametros);

            return Ok(productos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductoDto?>> GetProducto(int id)
        {
            var producto = await _productoService.ObtenerProductoPorIdAsync(id);
            
            if(producto == null)
            {
                return Problem(detail: $"No existe el producto con el id:{id}", statusCode: StatusCodes.Status404NotFound, title: "Recurso no encontrado");
            }

            return Ok(producto);
        }

        [HttpPost]
        [ServiceFilter(typeof(ValidationFilter<ProductoCrearDto>))]
        public async Task<ActionResult<ProductoDto>> CrearProducto(ProductoCrearDto dto)
        {
            var resultado = await _productoService.CrearProductoAsync(dto);

            if(resultado.IsSuccess)
            {
                return CreatedAtAction(nameof(GetProducto), new { id = resultado.Data!.Id }, resultado.Data);
            }

            return resultado.ToErrorResponse(this);
            
        }

        [HttpPut("{id}")]
        [ServiceFilter(typeof(ValidationFilter<ProductoModificarDto>))]
        public async Task<IActionResult> ModificarProducto(int id, ProductoModificarDto dto)
        {
            if (id != dto.Id)
            {
                return Problem(detail: "El id de la ruta no coincide con el id del cuerpo", statusCode: StatusCodes.Status400BadRequest, title: "Solicitud invalida");
            }

            var resultado = await _productoService.ModificarProductoAsync(id, dto);

            if(resultado.IsSuccess)
            {
                return NoContent();
            }

            return resultado.ToErrorResponse(this);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarProducto(int id)
        {
            var resultado = await _productoService.EliminarProductoAsync(id);
            if (resultado.IsSuccess)
            {
                return NoContent();
            }

            return resultado.ToErrorResponse(this);
        }
    }
}
