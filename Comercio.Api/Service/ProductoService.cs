using AutoMapper;
using Comercio.Api.DTOs.Paginacion;
using Comercio.Api.DTOs.Producto;
using Comercio.Api.Models;
using Comercio.Api.Repository;
using Comercio.Api.Service.Results;
using Microsoft.EntityFrameworkCore;

namespace Comercio.Api.Service
{
    public class ProductoService:IProductoService
    {
        private readonly IProductoRepository _productoRepository;
        private readonly IMapper _mapper;
        public ProductoService(IProductoRepository productoRepository, IMapper mapper)
        {
            _productoRepository = productoRepository;
            _mapper = mapper;
        }

        public async Task<Result<ProductoDto>> CrearProductoAsync(ProductoCrearDto dto)
        {
            var existeProducto = await _productoRepository.ExisteProductoPorNombreAsync(dto.Nombre);
            if(existeProducto)
            {
                return Result<ProductoDto>.Failure("El producto ya fue creado", ErrorType.Conflict);
            }
            var producto = _mapper.Map<Producto>(dto);

            try
            {
                await _productoRepository.CrearProductoAsync(producto);
            }
            catch (DbUpdateException)
            {

                return Result<ProductoDto>.Failure("El producto ya fue creado", ErrorType.Conflict);
            }

            return Result<ProductoDto>.Success(_mapper.Map<ProductoDto>(producto));

        }

        public async Task<Result> EliminarProductoAsync(int id)
        {
            var producto = await _productoRepository.ObtenerProductoPorIdAsync(id);

            if(producto == null)
            {
                return Result.Failure($"No existe el producto con el id:{id}", ErrorType.NotFound);
            }

            await _productoRepository.EliminarProductoAsync(producto);
            return Result.Success();
        }

        public async Task<Result> ModificarProductoAsync(int id, ProductoModificarDto dto)
        {
            var producto = await _productoRepository.ObtenerProductoPorIdAsync(id);
            if(producto == null)
            {
                return Result.Failure($"No existe el producto con el id {id}", ErrorType.NotFound);
            }

            _mapper.Map(dto, producto);

            try
            {
                await _productoRepository.GuardarProductoAsync(producto, producto.RowVersion);
                return Result.Success();
            }
            catch (DbUpdateConcurrencyException)
            {
                return Result.Failure("El producto ya fue modificado por otro usuario, vuelva a cargar la pagina para ver los cambios", ErrorType.Conflict);
                
            }
        }

        public async Task<ProductoDto?> ObtenerProductoPorIdAsync(int id)
        {
            var producto = await _productoRepository.ObtenerProductoPorIdAsync(id);
            return producto == null ? null : _mapper.Map<ProductoDto>(producto);
        }

        public async Task<ResultadoPaginado<ProductoDto>> ObtenerProductosAsync(ProductoQueryParametros parametros)
        {
            var resultado = await _productoRepository.ObtenerProductosAsync(parametros);
            var productosDto = _mapper.Map<IEnumerable<ProductoDto>>(resultado.Datos);

            return new ResultadoPaginado<ProductoDto>(productosDto, resultado.NumeroPagina, resultado.TamPagina, resultado.TotalRegistros);
        }
    }
}
