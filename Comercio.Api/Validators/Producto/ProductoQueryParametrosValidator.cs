using Comercio.Api.DTOs.Producto;
using FluentValidation;

namespace Comercio.Api.Validators.Producto
{
    public class ProductoQueryParametrosValidator:AbstractValidator<ProductoQueryParametros>
    {
        public ProductoQueryParametrosValidator()
        {
            RuleFor(x => x.Busqueda)
                .MaximumLength(50);

            RuleFor(x => x.PrecioMax)
                .GreaterThanOrEqualTo(0)
                .When(x => x.PrecioMax.HasValue);

            RuleFor(x => x.PrecioMin)
                .GreaterThanOrEqualTo(0)
                .When(x => x.PrecioMin.HasValue);

            RuleFor(x => x)
                .Must(x => !x.PrecioMin.HasValue || !x.PrecioMax.HasValue || x.PrecioMin  <= x.PrecioMax)
                .WithMessage("El precio minimo no puede ser mayor al precio maximo")
                .WithName("PrecioMin");
        }
    }
}
