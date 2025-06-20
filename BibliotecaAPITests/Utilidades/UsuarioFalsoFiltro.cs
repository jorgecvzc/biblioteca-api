using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace BibliotecaAPITests.Utilidades
{
    public class UsuarioFalsoFiltro : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Antes de la acción 

            context.HttpContext.User = new System.Security.Claims.ClaimsPrincipal(new ClaimsIdentity(
                new List<Claim>
                {
                    new Claim("email", "ejemplo@gmail.com")
                }, 
                "prueba"
                ));

            // Llamada al flujo de código

            await next();

            // Después de la acción
        }
    }
}
