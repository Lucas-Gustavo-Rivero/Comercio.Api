using Comercio.Api.DTOs.Producto;
using FluentValidation;

namespace Comercio.Api.Validators.Producto
{
    public class ProductoModificarDtoValidator:AbstractValidator<ProductoModificarDto>
    {
        public ProductoModificarDtoValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0);

            RuleFor(x => x.RowVersion)
                .NotEmpty();

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
