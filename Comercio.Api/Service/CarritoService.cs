using Comercio.Api.DTOs.Carrito;
using Comercio.Api.Repository;
using Comercio.Api.Service.Results;
using Comercio.Api.Models;
using AutoMapper;

namespace Comercio.Api.Service
{
    public class CarritoService:ICarritoService
    {
        private readonly ICarritoRepository _carritoRepository;
        private readonly IProductoRepository _productoRepository;
        private readonly IMapper _mapper;

        public CarritoService(ICarritoRepository carritoRepository, IProductoRepository productoRepository, IMapper mapper)
        {
            _carritoRepository = carritoRepository;
            _productoRepository = productoRepository;
            _mapper = mapper;
        }

        public async Task<Result<CarritoResponseDto>> AgregarAlCarritoAsync(int userId, CarritoAgregarDto dto)
        {
            var producto = await _productoRepository.ObtenerProductoPorIdAsync(dto.ProductoId);

            if(producto == null)
            {
                return Result<CarritoResponseDto>.Failure("No existe ese producto.", ErrorType.NotFound);
            }

            var carrito = await _carritoRepository.GetCarritoAsync(userId);
            var itemExiste = carrito?.CarritoItems.FirstOrDefault(ci => ci.ProductoId == dto.ProductoId);

            var cantidadActual = itemExiste?.Cantidad ?? 0;
            var cantidadTotal = cantidadActual + dto.Cantidad;

            if(cantidadTotal > producto.Stock)
            {
                return Result<CarritoResponseDto>.Failure("Stock insuficiente.", ErrorType.Conflict);
            }

            if (carrito == null)
            {
                carrito = new Carrito { UsuarioId = userId };
                carrito.CarritoItems.Add(new CarritoItem { Cantidad = dto.Cantidad, PrecioUnitario = producto.Precio, ProductoId = dto.ProductoId });
                _carritoRepository.CrearCarrito(carrito);
            }
            else
            {
                if(itemExiste != null)
                {
                    itemExiste.Cantidad += dto.Cantidad;
                }
                else
                {
                    carrito.CarritoItems.Add(new CarritoItem { Cantidad = dto.Cantidad, PrecioUnitario = producto.Precio, ProductoId = dto.ProductoId});
                }
            }

            await _carritoRepository.GuardarCambiosAsync();


            return Result<CarritoResponseDto>.Success(_mapper.Map<CarritoResponseDto>(carrito));
            
        }

        public async Task<Result> EliminarItemDeCarritoAsync(int userId, int productoId)
        {
            var carrito = await _carritoRepository.GetCarritoAsync(userId);

            if(carrito == null)
            {
                return Result.Failure("No existe un carrito.", ErrorType.NotFound);
            }

            var itemExiste = carrito.CarritoItems.FirstOrDefault(ci => ci.ProductoId == productoId);

            if(itemExiste == null)
            {
                return Result.Failure("No existe el producto en el carrito.", ErrorType.NotFound);
            }

            carrito.CarritoItems.Remove(itemExiste);
           
            await _carritoRepository.GuardarCambiosAsync();

            return Result.Success();
        }

        public async Task<Result<CarritoResponseDto>> GetCarritoAsync(int userId)
        {
            var carrito = await _carritoRepository.GetCarritoAsync(userId);

            if(carrito == null)
            {
                return Result<CarritoResponseDto>.Failure("No existe el carrito", ErrorType.NotFound);
            }

            
            return Result<CarritoResponseDto>.Success(_mapper.Map<CarritoResponseDto>(carrito));
        }

        public async Task<Result> ModificarItemDeCarritoAsync(int userId, CarritoModificarDto dto)
        {
            var carrito = await _carritoRepository.GetCarritoAsync(userId);

            if(carrito == null)
            {
                return Result.Failure("No existe el carrito", ErrorType.NotFound);
            }

            var itemExiste = carrito.CarritoItems.FirstOrDefault(ci => ci.ProductoId == dto.ProductoId);

            if(itemExiste == null)
            {
                return Result.Failure("No existe el producto en el carrito", ErrorType.NotFound);
            }

            var producto = await _productoRepository.ObtenerProductoPorIdAsync(dto.ProductoId);

            if(producto == null)
            {
                return Result.Failure("No existe el producto", ErrorType.NotFound);
            }

            if(dto.Cantidad > producto.Stock)
            {
                return Result.Failure("No hay stock suficiente para esa cantidad.", ErrorType.Conflict);
            }

            itemExiste.Cantidad = dto.Cantidad;

            await _carritoRepository.GuardarCambiosAsync();

            return Result.Success();
        }

        public async Task<Result> LimpiarCarritoAsync(int userId)
        {

            var carrito = await _carritoRepository.GetCarritoAsync(userId);

            if(carrito == null)
            {
                return Result.Failure("No existe un carrito asociado", ErrorType.NotFound);
            }

            _carritoRepository.EliminarCarrito(carrito);
            await _carritoRepository.GuardarCambiosAsync();
            return Result.Success();
        }
    }
}
