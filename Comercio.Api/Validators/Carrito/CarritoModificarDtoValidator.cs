using Comercio.Api.DTOs.Carrito;
using FluentValidation;

namespace Comercio.Api.Validators.Carrito
{
    public class CarritoModificarDtoValidator:AbstractValidator<CarritoModificarDto>
    {
        public CarritoModificarDtoValidator()
        {
            RuleFor(x => x.ProductoId)
                .GreaterThan(0);

            RuleFor(x => x.Cantidad)
                .GreaterThan(0)
                .LessThanOrEqualTo(100);
        }
    }
}
