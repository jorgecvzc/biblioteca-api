using BibliotecaAPI.Data;
using BibliotecaAPI.DTOs;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAPI.Utilities
{
    public class FiltroValidacionLibro : IAsyncActionFilter
    {
        private readonly ApplicationDbContext dbContext;

        public FiltroValidacionLibro(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!context.ActionArguments.TryGetValue("bookCreacionDTO", out var value) 
                || (value is not BookCreacionDTO bookCreacionDTO))
            {
                context.ModelState.AddModelError(string.Empty, "El modelo enviado no es válido");
                context.Result = context.ModelState.ConstruirProblemDetail(); // Se hace un cortocircuito al asignar el Result y ya nos se ejecuta el resto de acciones
                return;
            }

            if ((bookCreacionDTO.AuthorsId is null) || (bookCreacionDTO.AuthorsId.Count == 0))
            {
                context.ModelState.AddModelError(nameof(bookCreacionDTO.AuthorsId), "No se puede modificar un libro sin autores.");
                context.Result = context.ModelState.ConstruirProblemDetail(); // Se hace un cortocircuito al asignar el Result y ya nos se ejecuta el resto de acciones
                return;
            }

            var autoresIdsExisten = await dbContext.Authors
                                    .Where(x => bookCreacionDTO.AuthorsId.Contains(x.Id))
                                    .Select(x => x.Id).ToListAsync();
            if (autoresIdsExisten.Count != bookCreacionDTO.AuthorsId.Count())
            {
                var autoresNoExisten = bookCreacionDTO.AuthorsId.Except(autoresIdsExisten).ToList();
                var autoresNoExistenString = string.Join(", ", autoresNoExisten);
                var mensajeDeError = $"Los siguientes autores no existen {autoresNoExistenString}";
                context.ModelState.AddModelError(nameof(bookCreacionDTO.AuthorsId), mensajeDeError);
                context.Result = context.ModelState.ConstruirProblemDetail(); // Se hace un cortocircuito al asignar el Result y ya nos se ejecuta el resto de acciones
                return;
            }

            await next();
        }
    }
}
