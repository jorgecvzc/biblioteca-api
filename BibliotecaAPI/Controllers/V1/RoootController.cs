using BibliotecaAPI.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaAPI.Controllers.V1
{
    [ApiController]
    [Route("api/v1")]
    [Authorize]
    public class RoootController : ControllerBase
    {
        private readonly IAuthorizationService authorizationService;

        public RoootController(IAuthorizationService authorizationService)
        {
            this.authorizationService = authorizationService;
        }

        [HttpGet(Name = "ObtenerRootV1")]
        [AllowAnonymous]
        public async Task<IEnumerable<DatosHATEOASDTO>> Get()
        {
            var datosHATEOAS = new List<DatosHATEOASDTO>();

            var esAdmin = await authorizationService.AuthorizeAsync(User, "esadmin");
                
            // Acciones que cualquiera pueda realizar
            datosHATEOAS.Add(new DatosHATEOASDTO(Enlace: Url.Link("ObtenerRootV1", new { })!,
                    Descripcion: "self", Metodo: "GET"));
            datosHATEOAS.Add(new DatosHATEOASDTO(Enlace: Url.Link("ObtenerAutoresV1", new { })!,
                    Descripcion: "autores-obtener", Metodo: "GET"));

            datosHATEOAS.Add(new DatosHATEOASDTO(Enlace: Url.Link("ObtenerLibrosV1", new { })!,
                    Descripcion: "libros-obtener", Metodo: "GET"));
            datosHATEOAS.Add(new DatosHATEOASDTO(Enlace: Url.Link("ObtenerLibroV1", new { })!,
                    Descripcion: "libro-obtener", Metodo: "GET"));

            datosHATEOAS.Add(new DatosHATEOASDTO(Enlace: Url.Link("ObtenerComentariosV1", new { })!,
                    Descripcion: "comentarios-obtener", Metodo: "GET"));
            datosHATEOAS.Add(new DatosHATEOASDTO(Enlace: Url.Link("ObtenerComentarioV1", new { })!,
                    Descripcion: "comentario-obtener", Metodo: "GET"));

            datosHATEOAS.Add(new DatosHATEOASDTO(Enlace: Url.Link("RegistrarUsuarioV1", new { })!,
                    Descripcion: "usuario-crear", Metodo: "POST"));
            datosHATEOAS.Add(new DatosHATEOASDTO(Enlace: Url.Link("LoguearUsuarioV1", new { })!,
                    Descripcion: "usuario-loguear", Metodo: "POST"));

            // Acciones que sólo usuarios logueados pueden realizar

            if (User.Identity!.IsAuthenticated)
            {
                datosHATEOAS.Add(new DatosHATEOASDTO(Enlace: Url.Link("ActualizarUsuarioV1", new { })!,
                    Descripcion: "usuario-actualizar", Metodo: "PUT"));
                datosHATEOAS.Add(new DatosHATEOASDTO(Enlace: Url.Link("RenovarTokenV1", new { })!,
                    Descripcion: "token-renovar", Metodo: "GET"));
            }

            // Acciones que sólo usuarios admin puedan realizar

            if (esAdmin.Succeeded) 
            { 
                datosHATEOAS.Add(new DatosHATEOASDTO(Enlace: Url.Link("ObtenerAutorV1", new { })!,
                        Descripcion: "autor-obtener", Metodo: "GET"));
                datosHATEOAS.Add(new DatosHATEOASDTO(Enlace: Url.Link("FiltrarAutoresV1", new { })!,
                        Descripcion: "autores-filtrar", Metodo: "GET"));
                datosHATEOAS.Add(new DatosHATEOASDTO(Enlace: Url.Link("CrearAutorV1", new { })!,
                        Descripcion: "autor-crear", Metodo: "POST"));
                datosHATEOAS.Add(new DatosHATEOASDTO(Enlace: Url.Link("", new { })!,
                        Descripcion: "autor-confoto-crear", Metodo: "POST"));
                datosHATEOAS.Add(new DatosHATEOASDTO(Enlace: Url.Link("ActualizarAutorV1", new { })!,
                        Descripcion: "autor-actualizar", Metodo: "PUT"));
                datosHATEOAS.Add(new DatosHATEOASDTO(Enlace: Url.Link("", new { })!,
                        Descripcion: "autor-patch", Metodo: "PATCH"));
                datosHATEOAS.Add(new DatosHATEOASDTO(Enlace: Url.Link("BorrarAutorV1", new { })!,
                        Descripcion: "autor-borrar", Metodo: "DELETE"));

                datosHATEOAS.Add(new DatosHATEOASDTO(Enlace: Url.Link("OtenerAutoresPorIdsV1", new { })!,
                        Descripcion: "autores-obtener-varios", Metodo: "GET"));
                datosHATEOAS.Add(new DatosHATEOASDTO(Enlace: Url.Link("CrearAutorV1", new { })!,
                        Descripcion: "autores-crear", Metodo: "POST"));


                datosHATEOAS.Add(new DatosHATEOASDTO(Enlace: Url.Link("CrearLibroV1", new { })!,
                        Descripcion: "libro-crear", Metodo: "POST"));
                datosHATEOAS.Add(new DatosHATEOASDTO(Enlace: Url.Link("ActualizarLibroV1", new { })!,
                        Descripcion: "libro-actualizar", Metodo: "PUT"));
                datosHATEOAS.Add(new DatosHATEOASDTO(Enlace: Url.Link("BorrarLibroV1", new { })!,
                        Descripcion: "libro-borrar", Metodo: "DELETE"));

                datosHATEOAS.Add(new DatosHATEOASDTO(Enlace: Url.Link("CrearComentarioV1", new { })!,
                        Descripcion: "comentario-crear", Metodo: "POST"));
                datosHATEOAS.Add(new DatosHATEOASDTO(Enlace: Url.Link("PacthComentarioV1", new { })!,
                        Descripcion: "comentario-patch", Metodo: "PATCH"));
                datosHATEOAS.Add(new DatosHATEOASDTO(Enlace: Url.Link("BorrarComentarioV1", new { })!,
                        Descripcion: "comentario-borrar", Metodo: "DELETE"));

                datosHATEOAS.Add(new DatosHATEOASDTO(Enlace: Url.Link("ObtenerUsuariosV1", new { })!,
                        Descripcion: "usuarios-obtener", Metodo: "GET"));
            
                datosHATEOAS.Add(new DatosHATEOASDTO(Enlace: Url.Link("DarPermisosAdminUsuarioV1", new { })!,
                        Descripcion: "usuario-dar-permisos-admin", Metodo: "POST"));
                datosHATEOAS.Add(new DatosHATEOASDTO(Enlace: Url.Link("QuitarPermisosAdminUsuarioV1", new { })!,
                        Descripcion: "usuario-quitar-permisos-admin", Metodo: "POST")          );
            }
            return datosHATEOAS;
        }
    }
}
