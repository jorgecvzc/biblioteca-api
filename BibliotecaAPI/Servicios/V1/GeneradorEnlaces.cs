using BibliotecaAPI.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System;

namespace BibliotecaAPI.Servicios.V1
{
    public interface IGeneradorEnlaces
    {
        Task<ColeccionDeRecursosDTO<AuthorDTO>> GenerarEnlaces(List<AuthorDTO> autores);
        Task GenerarEnlaces(AuthorDTO authorDTO);
    }

    public class GeneradorEnlaces : IGeneradorEnlaces
    {
        private readonly LinkGenerator linkGenerator;
        private readonly IAuthorizationService authorizationService;
        private readonly IHttpContextAccessor httpContextAccessor;

        public GeneradorEnlaces(LinkGenerator linkGenerator,
            IAuthorizationService authorizationService,
            IHttpContextAccessor httpContextAccessor)
        {
            this.linkGenerator = linkGenerator;
            this.authorizationService = authorizationService;
            this.httpContextAccessor = httpContextAccessor;
        }

        public async Task<ColeccionDeRecursosDTO<AuthorDTO>> GenerarEnlaces(List<AuthorDTO> autores)
        {
            var resultado = new ColeccionDeRecursosDTO<AuthorDTO> { Valores = autores };

            var usuario = httpContextAccessor.HttpContext!.User;
            var esAdmin = await authorizationService.AuthorizeAsync(usuario, "esadmin");

            foreach (var dto in autores)
            {
                GenerarEnlaces(dto, esAdmin.Succeeded);
            }

            resultado.Enlaces.Add(new DatosHATEOASDTO(
                Enlace: linkGenerator.GetUriByRouteValues(httpContextAccessor.HttpContext!,
                        "ObtenerAutoresV1", new { })!,
                Descripcion: "self",
                Metodo: "GET"
                ));

            if (esAdmin.Succeeded)
            {
                resultado.Enlaces.Add(new DatosHATEOASDTO(
                    Enlace: linkGenerator.GetUriByRouteValues(httpContextAccessor.HttpContext!,
                            "CrearAutorV1", new { })!,
                    Descripcion: "autor-crear",
                    Metodo: "POST"
                    ));
                resultado.Enlaces.Add(new DatosHATEOASDTO(
                   Enlace: linkGenerator.GetUriByRouteValues(httpContextAccessor.HttpContext!,
                            "CrearAutorConFotoV1", new { })!,
                   Descripcion: "autor-crear-con-foto",
                   Metodo: "POST"
                   ));
            }

            return resultado;
        }

        public async Task GenerarEnlaces(AuthorDTO authorDTO)
        {
            var usuario = httpContextAccessor.HttpContext!.User;
            var esAdmin = await authorizationService.AuthorizeAsync(usuario, "esadmin");
            GenerarEnlaces(authorDTO, esAdmin.Succeeded);
        }

        private void GenerarEnlaces(AuthorDTO authorDTO, bool esAdmin)
        {
            authorDTO.Enlaces.Add(new DatosHATEOASDTO(
                Enlace: linkGenerator.GetUriByRouteValues(httpContextAccessor.HttpContext!,
                        "ObtenerAutorV1", new { id = authorDTO.Id })!,
                Descripcion: "self",
                Metodo: "GET"));

            if (esAdmin)
            {
                authorDTO.Enlaces.Add(new DatosHATEOASDTO(
                    Enlace: linkGenerator.GetUriByRouteValues(httpContextAccessor.HttpContext!,
                            "ActualizarAutorV1", new { id = authorDTO.Id })!,
                    Descripcion: "autor-actualizar",
                    Metodo: "PUT"));

                authorDTO.Enlaces.Add(new DatosHATEOASDTO(
                    Enlace: linkGenerator.GetUriByRouteValues(httpContextAccessor.HttpContext!,
                            "PatchAutorV1", new { id = authorDTO.Id })!,
                    Descripcion: "autor-patch",
                    Metodo: "PATCH"));

                authorDTO.Enlaces.Add(new DatosHATEOASDTO(
                    Enlace: linkGenerator.GetUriByRouteValues(httpContextAccessor.HttpContext!,
                            "BorrarAutorV1", new { id = authorDTO.Id })!,
                    Descripcion: "autor-bprrar",
                    Metodo: "DELETE"));
            }
        }
    }
}
