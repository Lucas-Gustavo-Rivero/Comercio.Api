using Comercio.Api.DTOs.Carrito;
using FluentValidation;

namespace Comercio.Api.Validators.Carrito
{
    public class CarritoAgregarDtoValidator:AbstractValidator<CarritoAgregarDto>
    {
        public CarritoAgregarDtoValidator()
        {
            RuleFor(x => x.ProductoId)
                .GreaterThan(0);

            RuleFor(x => x.Cantidad)
                .GreaterThan(0)
                .LessThanOrEqualTo(100); 
        }
    }
}
