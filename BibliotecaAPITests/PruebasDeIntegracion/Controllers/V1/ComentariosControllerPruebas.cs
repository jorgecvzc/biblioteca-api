using BibliotecaAPI.Entities;
using BibliotecaAPITests.Utilidades;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core.Tokenizer;
using System.Net;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;

namespace BibliotecaAPITests.PruebasDeIntegracion.Controllers.V1
{
    [TestClass]
    public class ComentariosControllerPruebas : BasePruebas
    {
        private readonly string url = "/api/v1/libros/1/comentarios";
        private string nombreBD = Guid.NewGuid().ToString();

        private async Task CrearDataDePrueba()
        {
            var context = ConstruirContext(nombreBD);
            var autor = new Author { Names = "Jorge", Apellidos = "Cortés" };
            context.Add(autor);
            await context.SaveChangesAsync();

            var libro = new Book { Title = "título" };
            libro.Authors.Add(new AuthorBook { Author = autor });
            context.Add(libro);
            await context.SaveChangesAsync();
        }

        [TestMethod]
        public async Task Delete_Devuelve204_CuandoUsuarioBorraSuComentario()
        {
            // Preparación

            await CrearDataDePrueba();

            var factory = ConstruirWebApplicationFactory(nombreBD, ignorarSeguridad: false);

            var token = await CrearUsuario(nombreBD, factory);

            var context = ConstruirContext(nombreBD);
            var usuario = await context.Users.FirstAsync();

            var comentario = new Comentario
            {
                Cuerpo = "Contenido",
                UsuarioId = usuario!.Id,
                BookId = 1
            };

            context.Add(comentario);
            await context.SaveChangesAsync();

            var cliente = factory.CreateClient();
            cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Prueba

            var respuesta = await cliente.DeleteAsync($"{url}/{comentario.Id}");

            // Verificación

            Assert.AreEqual(expected: HttpStatusCode.NoContent, actual: respuesta.StatusCode);
        }

        [TestMethod]
        public async Task Delete_Devuelve403_CuandoUsuarioIntentaBorrarComentarioDeOtro()
        {
            // Preparación

            await CrearDataDePrueba();

            var factory = ConstruirWebApplicationFactory(nombreBD, ignorarSeguridad: false);

            var emailCreadorComentario = "creador@gg.com";
            var token = await CrearUsuario(nombreBD, factory, [], emailCreadorComentario);

            var context = ConstruirContext(nombreBD);
            var usuarioCreadorComentario = await context.Users.FirstAsync();

            var comentario = new Comentario
            {
                Cuerpo = "Contenido",
                UsuarioId = usuarioCreadorComentario!.Id,
                BookId = 1
            };

            context.Add(comentario);
            await context.SaveChangesAsync();

            var tokenUsuarioDistinto = await CrearUsuario(nombreBD, factory, [], "otrousu@hh.com");

            var cliente = factory.CreateClient();
            cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenUsuarioDistinto);

            // Prueba

            var respuesta = await cliente.DeleteAsync($"{url}/{comentario.Id}");

            // Verificación

            Assert.AreEqual(expected: HttpStatusCode.Forbidden, actual: respuesta.StatusCode);
        }
    }
}
