using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Comercio.Api.Filters
{
    public class ValidationFilter<T> : IAsyncActionFilter where T : class
    {
        private readonly IValidator<T> _validator;

        public ValidationFilter(IValidator<T> validator)
        {
            _validator = validator;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var argumento = context.ActionArguments.Values.OfType<T>().FirstOrDefault();

            if(argumento != null)
            {
                var resultado = await _validator.ValidateAsync(argumento);

                if(!resultado.IsValid)
                {
                    context.Result = new BadRequestObjectResult(
                        new ValidationProblemDetails(resultado.ToDictionary()));

                    return;
                }
                
            }

            await next();
        }
    }
}
