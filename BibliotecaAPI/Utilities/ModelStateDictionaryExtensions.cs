﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BibliotecaAPI.Utilities
{
    public static class ModelStateDictionaryExtensions
    {
        public static BadRequestObjectResult ConstruirProblemDetail(this ModelStateDictionary modelState)
        {
            var problemDetails = new ValidationProblemDetails(modelState)
            {
                Title = "One or more validation errors ocurred.",
                Status = StatusCodes.Status400BadRequest
            };
            return new BadRequestObjectResult(problemDetails);
        }
    }
}
