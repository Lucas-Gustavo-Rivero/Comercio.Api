using Comercio.Api.DTOs.Producto;
using FluentValidation;

namespace Comercio.Api.Validators.Producto
{
    public class ProductoCrearDtoValidator: AbstractValidator<ProductoCrearDto>
    {
        public ProductoCrearDtoValidator()
        {
            RuleFor(x => x.Nombre)
                .NotEmpty()
                .MaximumLength(50);

            RuleFor(x => x.Descripcion)
                .MaximumLength(500);

            RuleFor(x => x.UrlImagen)
                .MaximumLength(500);

            RuleFor(x => x.Stock)
                .GreaterThanOrEqualTo(0);

            RuleFor(x => x.Precio)
                .GreaterThan(0)
                .LessThanOrEqualTo(1000000);
        }
    }
}
