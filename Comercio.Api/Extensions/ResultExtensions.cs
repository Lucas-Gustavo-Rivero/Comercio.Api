using Comercio.Api.Service.Results;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.CompilerServices;

namespace Comercio.Api.Extensions
{
    public static class ResultExtensions
    {
        public static ActionResult ToErrorResponse(this Result result, ControllerBase controller)
        {
            var (statusCode, title) = result.ErrorType switch
            {
                ErrorType.NotFound => (StatusCodes.Status404NotFound, "Recurso no encontrado"),
                ErrorType.Conflict => (StatusCodes.Status409Conflict, "Conflicto"),
                ErrorType.Validation => (StatusCodes.Status400BadRequest, "Error de validacion"),
                ErrorType.InvalidCredentials => (StatusCodes.Status401Unauthorized, "El email o la contraseña son incorrectos"),
                _ => (StatusCodes.Status500InternalServerError, "Error interno del servidor")
            };

            return controller.Problem(detail: result.ErrorMessage, statusCode: statusCode, title: title);
        }
    }
}
