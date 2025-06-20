﻿using BibliotecaAPI.DTOs;
using BibliotecaAPI.Servicios.V1;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BibliotecaAPI.Utilities.V1
{
    public class HATEOASAutoresAttribute : HATEOASFilterAttriburte
    {
        private readonly IGeneradorEnlaces generadorEnlaces;

        public HATEOASAutoresAttribute(IGeneradorEnlaces generadorEnlaces)
        {
            this.generadorEnlaces = generadorEnlaces;
        }

        public override async Task OnResultExecutionAsync(
            ResultExecutingContext context, ResultExecutionDelegate next)
        {
            var incluirHATEOAS = DebeIncluirHATEOAS(context);

            if (!incluirHATEOAS)
            {
                await next();
                return;
            }

            var result = context.Result as ObjectResult;
            var modelo = result!.Value as List<AuthorDTO> ??
                    throw new ArgumentNullException("Se esperaba un listado de AuthorDTO");
            context.Result = new OkObjectResult(await generadorEnlaces.GenerarEnlaces(modelo));
            await next();
        }
    }
}
