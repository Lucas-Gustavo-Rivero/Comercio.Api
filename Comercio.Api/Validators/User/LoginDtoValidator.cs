using Comercio.Api.DTOs.User;
using FluentValidation;

namespace Comercio.Api.Validators.User
{
    public class LoginDtoValidator:AbstractValidator<LoginDto>
    {
        public LoginDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .MaximumLength(256);

            RuleFor(x => x.Password)
                .NotEmpty();
        }
    }
}
