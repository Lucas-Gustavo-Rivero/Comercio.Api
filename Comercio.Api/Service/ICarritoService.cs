using Comercio.Api.DTOs.Carrito;
using Comercio.Api.Service.Results;

namespace Comercio.Api.Service
{
    public interface ICarritoService
    {
        Task<Result<CarritoResponseDto>> GetCarritoAsync(int userId);
        Task<Result<CarritoResponseDto>> AgregarAlCarritoAsync(int userId, CarritoAgregarDto dto);
        Task<Result> EliminarItemDeCarritoAsync(int userId, int productoId);
        Task<Result> ModificarItemDeCarritoAsync(int userId, CarritoModificarDto dto);

        Task<Result> LimpiarCarritoAsync(int userId);
    }
}
