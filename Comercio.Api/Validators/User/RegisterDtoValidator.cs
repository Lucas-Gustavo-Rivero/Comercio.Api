using Comercio.Api.DTOs.User;
using FluentValidation;

namespace Comercio.Api.Validators.User
{
    public class RegisterDtoValidator:AbstractValidator<RegisterDto>
    {
        public RegisterDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .MaximumLength(256)
                .EmailAddress();

            RuleFor(x => x.NombreCompleto)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(8)
                .MaximumLength(64);
        }
    }
}
