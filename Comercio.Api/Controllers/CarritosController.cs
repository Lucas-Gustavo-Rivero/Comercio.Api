using Comercio.Api.DTOs.Carrito;
using Comercio.Api.Extensions;
using Comercio.Api.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Comercio.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CarritosController : ControllerBase
    {
        private readonly ICarritoService _carritoService;

        public CarritosController(ICarritoService carritoService)
        {
            _carritoService = carritoService;
        }

        [HttpGet]
        public async Task<ActionResult<CarritoResponseDto>> VerCarrito()
        {
            var userId = User.GetUserId();
            var resultado = await _carritoService.GetCarritoAsync(userId);

            if(resultado.IsSuccess)
            {
                return Ok(resultado.Data);
            }

            return resultado.ToErrorResponse(this);
        }

        [HttpPost("items")]
        public async Task<ActionResult<CarritoResponseDto>> AgregarProductoCarrito(CarritoAgregarDto dto)
        {
            var userId = User.GetUserId();
            var resultado = await _carritoService.AgregarAlCarritoAsync(userId, dto);

            if(resultado.IsSuccess)
            {
                return CreatedAtAction(nameof(VerCarrito), new { }, resultado.Data);
            }

            return resultado.ToErrorResponse(this);
        }

        [HttpPut("items/{productoId}")]
        public async Task<IActionResult> ModificarProductoCarrito(int productoId, CarritoModificarDto dto)
        {
            if(productoId != dto.ProductoId)
            {
                return Problem(detail: "El id de la ruta no coincide con el id del cuerp", statusCode: StatusCodes.Status400BadRequest, title: "Solicitud invalida");
            }

            var userId = User.GetUserId();
            var resultado = await _carritoService.ModificarItemDeCarritoAsync(userId, dto);

            if(resultado.IsSuccess)
            {
                return NoContent();
            }

            return resultado.ToErrorResponse(this);
        }

        [HttpDelete("items/{productoId}")]
        public async Task<IActionResult> EliminarProductoCarrito(int productoId)
        {
            var userId = User.GetUserId();
            var resultado = await _carritoService.EliminarItemDeCarritoAsync(userId, productoId);

            if(resultado.IsSuccess)
            {
                return NoContent();
            }

            return resultado.ToErrorResponse(this);
        }

        [HttpDelete]
        public async Task<IActionResult> LimpiarCarrito()
        {
            var userId = User.GetUserId();
            var resultado = await _carritoService.LimpiarCarritoAsync(userId);

            if(resultado.IsSuccess)
            {
                return NoContent();
            }

            return resultado.ToErrorResponse(this);
        }
    }
}
