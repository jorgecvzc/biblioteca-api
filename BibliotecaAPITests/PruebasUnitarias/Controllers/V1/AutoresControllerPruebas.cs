using BibliotecaAPI.Controllers.V1;
using BibliotecaAPI.DTOs;
using BibliotecaAPI.Entities;
using BibliotecaAPI.Servicios;
using BibliotecaAPI.Servicios.V1;
using BibliotecaAPITests.Utilidades;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BibliotecaAPITests.PruebasUnitarias.Controllers.V1
{
    [TestClass]
    public class AutoresControllerPruebas : BasePruebas
    {
        IAlmacenadorArchivos almacenadorArchivos = null;
        ILogger<AuthorController> logger = null;
        IOutputCacheStore outpuCacheStore = null;
        IServicioAutores servicioAutores = null!;
        private string nombreBD = Guid.NewGuid().ToString();
        private AuthorController controller = null;
        private const string contenedor = "autores";
        private const string cache = "autores-obtener";

        [TestInitialize]
        public void Setup()
        {            
            var context = ConstruirContext(nombreBD);
            var mapper = ConfigurarAutoMapper();
            almacenadorArchivos = Substitute.For< IAlmacenadorArchivos>();
            logger = Substitute.For<ILogger<AuthorController>>();
            IOutputCacheStore outpuCacheStoreClase = new OutputCacheStoreFalso(); // Ejemplo de asignación con clase sustituta
            outpuCacheStore = Substitute.For<IOutputCacheStore>();
            servicioAutores = Substitute.For<IServicioAutores>();

            controller = new AuthorController(context, mapper, almacenadorArchivos,
                logger, outpuCacheStore, servicioAutores);
        }

        [TestMethod]
        public async Task Get_Retorna404_CuandoAutorConIdNoExiste()
        {
            // Prueba
            var respuesta = await controller.Get(1);

            // Verificación
            var resultado = respuesta.Result as StatusCodeResult;

            Assert.AreEqual(expected: 404, actual: resultado!.StatusCode);
        }

        [TestMethod]
        public async Task Get_RetornaAutor_CuandoAutorConIdExiste()
        {
            // Preparación

            var context = ConstruirContext(nombreBD);
            context.Authors.Add(new Author { Names = "NJorge", Apellidos = "Gavilán" });
            context.Authors.Add(new Author { Names = "Ana", Apellidos = "Favilan" });
            await context.SaveChangesAsync();

            // Prueba
            var respuesta = await controller.Get(1);

            // Verificación
            var resultado = respuesta.Value;
            Assert.AreEqual(expected: 1, actual: resultado!.Id);
        }

        [TestMethod]
        public async Task Get_RetornaAutorConLibros_CuandoAutorTieneLibros()
        {
            // Preparación
            var context = ConstruirContext(nombreBD);

            var libro1 = new Book { Title = "Libre 1" };
            var libro2 = new Book { Title = "Libro 2" };

            var autor = new Author
            { 
                Names = "NJorge", 
                Apellidos = "Gavilán", 
                Books = new List<AuthorBook>
                {
                    new AuthorBook { Book = libro1 },
                    new AuthorBook { Book = libro2 }
                }
            };

            context.Add(autor);

            await context.SaveChangesAsync();

            // Prueba
            var respuesta = await controller.Get(1);

            // Verificación
            var resultado = respuesta.Value;
            Assert.AreEqual(expected: 1, actual: resultado!.Id);
            Assert.AreEqual(expected: 2, actual: resultado.Books.Count);
        }

        [TestMethod]
        public async Task Get_DebeLlamarGetServicioAutores()
        {
            // Preparación
            var paginacionDTO = new PaginacionDTO(2, 2);

            // Prueba
            await controller.Get(paginacionDTO);

            // Verificación
            await servicioAutores.Received(1).Get(paginacionDTO); // Se especifica que se ha tenido que llamar 1 vez y que se la ha pasado paginacionDTO
        }

        [TestMethod]
        public async Task Post_DebeCrearAutor_CuandoEnviamosAutor()
        {
            // Preparación
            var nuevoAuthor = new AuthorCreacionDTO { Names = "nombres", Apellidos = "apellidods" };

            // Prueba
            var respuesta = await controller.Post(nuevoAuthor);

            // Verificación
            var resultado = respuesta as CreatedAtRouteResult;
            Assert.IsNotNull(resultado);

            // Se comprueba que se ha generado el objeto en la tabla en BBDD de prueba
            var contexto2 = ConstruirContext(nombreBD);
            var cantidad = await contexto2.Authors.CountAsync();
            Assert.AreEqual(expected: 1, actual: cantidad);
        }

        [TestMethod]
        public async Task Put_Retorna404_CuandoAutorNoExiste()
        {
            // Prueba
            var respuesta = await controller.Put(1, authorCreDTO: null);

            // Verificación
            var resultado = respuesta as StatusCodeResult;
            Assert.AreEqual(expected: 404, actual: resultado!.StatusCode);
        }

        [TestMethod]
        public async Task Put_ActualizaAutor_CuandoEnviamosAutorSinFoto()
        {
            // Preparacióon
            var context = ConstruirContext(nombreBD);

            context.Authors.Add(new Author
            {
                Names = "Felipe",
                Apellidos = "Gabilan",
               Identificacion = "ID"
            });

            await context.SaveChangesAsync();

            var autorCreacionDTO = new AuthorCreacionDTOConFoto
            {
                Names = "felipe2",
                Apellidos = "Gavilan",
                Identificacion = "Id2"
            };

            // Prueba
            var respuesta = await controller.Put(1, authorCreDTO: autorCreacionDTO);

            // Verificación
            var resultado = respuesta as StatusCodeResult;
            Assert.AreEqual(expected: 204, actual: resultado!.StatusCode);

            var context3 = ConstruirContext(nombreBD);
            var autorActualizado = await context3.Authors.SingleAsync();
            Assert.AreEqual(expected: "felipe2", actual: autorActualizado.Names);
            Assert.AreEqual(expected: "Gavilan", actual: autorActualizado.Apellidos);
            Assert.AreEqual(expected: "Id2", actual: autorActualizado.Identificacion);

            await outpuCacheStore.Received(1).EvictByTagAsync(cache, default);
            // Como no se ha mandado foto se comprobará que no se llama al método que guarda las imágenes
            await almacenadorArchivos.DidNotReceiveWithAnyArgs().Editar(default, default!, default!); // Indica que editar no se llama
        }

        [TestMethod]
        public async Task Put_ActualizaAutor_CuandoEnviamosAutorConFoto()
        {
            // Preparacióon
            var context = ConstruirContext(nombreBD);

            var urlFotoAnterior = "URL-1";
            var urlFotoNueva = "URL-2";
            almacenadorArchivos.Editar(default, default!, default!).ReturnsForAnyArgs(urlFotoNueva);
            // Indica que cuando se llame a Editar dentro del Controlador devolverá el valor URL-2

            context.Authors.Add(new Author
            {
                Names = "Felipe",
                Apellidos = "Gabilan",
                Identificacion = "ID",
                Foto = urlFotoAnterior
            });

            await context.SaveChangesAsync();

            var formFile = Substitute.For<IFormFile>();
            var autorCreacionDTO = new AuthorCreacionDTOConFoto
            {
                Names = "felipe2",
                Apellidos = "Gavilan",
                Identificacion = "Id2",
                Foto = formFile
            };

            // Prueba
            var respuesta = await controller.Put(1, authorCreDTO: autorCreacionDTO);

            // Verificación
            var resultado = respuesta as StatusCodeResult;
            Assert.AreEqual(expected: 204, actual: resultado!.StatusCode);

            var context3 = ConstruirContext(nombreBD);
            var autorActualizado = await context3.Authors.SingleAsync();
            Assert.AreEqual(expected: "felipe2", actual: autorActualizado.Names);
            Assert.AreEqual(expected: "Gavilan", actual: autorActualizado.Apellidos);
            Assert.AreEqual(expected: "Id2", actual: autorActualizado.Identificacion);
            Assert.AreEqual(expected: urlFotoNueva, actual: autorActualizado.Foto);

            await outpuCacheStore.Received(1).EvictByTagAsync(cache, default);
            // Como se ha mandado foto se comprobará que se llama al método que la actualiza
            await almacenadorArchivos.Received(1).Editar(urlFotoAnterior, contenedor, formFile);
        }

        [TestMethod]
        public async Task Patch_Retorna400_CuandoPatchDocEsNulo()
        {
            // Prueba
            var respuesta = await controller.Patch(1, patchDoc: null!);

            // Verificación
            var resultado = respuesta as StatusCodeResult;
            Assert.AreEqual(expected: 400, actual: resultado!.StatusCode);
        }

        [TestMethod]
        public async Task Patch_Retorna404_CuandoAutorNoExiste()
        {
            // Preparación
            var patchDoc = new JsonPatchDocument<AuthorPatchDTO>();

            // Prueba
            var respuesta = await controller.Patch(1, patchDoc);

            // Verificación
            var resultado = respuesta as StatusCodeResult;
            Assert.AreEqual(expected: 404, actual: resultado!.StatusCode);
        }

        [TestMethod]
        public async Task Patch_RetornaValidationProblem_CuandoHayErrorDeValidacion()
        {
            // Preparación
            var context = ConstruirContext(nombreBD);
            context.Authors.Add(new Author { Names = "Jorge", Apellidos = "Villalba", Identificacion = "142" });
            await context.SaveChangesAsync();

            var objectValidator = Substitute.For<IObjectModelValidator>();
            controller.ObjectValidator = objectValidator;

            var mensajeDeError = "mensaje de error";
            controller.ModelState.AddModelError("", mensajeDeError);

            var patchDoc = new JsonPatchDocument<AuthorPatchDTO>();

            // Prueba
            var respuesta = await controller.Patch(1, patchDoc);

            // Verificación
            var resultado = respuesta as ObjectResult;
            var problemDetails = resultado!.Value as ValidationProblemDetails;
            Assert.IsNotNull(problemDetails);
            Assert.AreEqual(expected: 1, actual: problemDetails.Errors.Keys.Count);
            Assert.AreEqual(expected: mensajeDeError, actual: problemDetails.Errors.Values.First().First());
        }

        [TestMethod]
        public async Task Patch_ActualizaUnCampo_CuandoSeEnviaUnaOperacion()
        {
            // Preparación
            var context = ConstruirContext(nombreBD);
            context.Authors.Add(new Author { Names = "Jorge", Apellidos = "Villalba", Identificacion = "142", Foto = "URL-1" });
            await context.SaveChangesAsync();

            var objectValidator = Substitute.For<IObjectModelValidator>();
            controller.ObjectValidator = objectValidator;

            var patchDoc = new JsonPatchDocument<AuthorPatchDTO>();
            patchDoc.Operations.Add(new Operation<AuthorPatchDTO>("replace", "/names", null, "Jorge2"));

            // Prueba
            var respuesta = await controller.Patch(1, patchDoc);

            // Verificación
            var resultado = respuesta as StatusCodeResult;
            Assert.AreEqual(expected: 204, resultado!.StatusCode);

            await outpuCacheStore.Received(1).EvictByTagAsync(cache, default); 

            var context2 = ConstruirContext(nombreBD);
            var authorBD = await context2.Authors.SingleAsync();
            
            Assert.AreEqual(expected: "Jorge2", actual: authorBD.Names);
            Assert.AreEqual(expected: "Villalba", actual: authorBD.Apellidos);
            Assert.AreEqual(expected: "142", actual: authorBD.Identificacion);
            Assert.AreEqual(expected: "URL-1", actual: authorBD.Foto);
        }

        [TestMethod]
        public async Task Delete_Retorna404_CuandoAutorNoExiste()
        {
            // Prueba
            var respuesta = await controller.Delete(1);

            // Validación
            var resultado = respuesta as StatusCodeResult;
            Assert.AreEqual(404, resultado!.StatusCode);
        }

        [TestMethod]
        public async Task Delete_BorraAutor_CuandoAutorExiste()
        {
            // Preparación
            var urlFoto = "URL-1";

            var context = ConstruirContext(nombreBD);

            context.Authors.Add(new Author { Names ="Autor1", Apellidos = "Autor1", Foto = urlFoto });
            context.Authors.Add(new Author { Names = "Autor2", Apellidos = "Autor2"});

            await context.SaveChangesAsync();

            // Prueba
            var respuesta = await controller.Delete(1);

            // Validación
            var resultado = respuesta as StatusCodeResult;
            Assert.AreEqual(expected: 204, actual: resultado!.StatusCode);

            var context2 = ConstruirContext(nombreBD);
            var cantidadAutores = await context2.Authors.CountAsync();
            Assert.AreEqual(expected: 1, actual: cantidadAutores);

            var autorsExiste = await context2.Authors.AnyAsync(x => x.Names == "Autor2");
            Assert.IsTrue(autorsExiste);

            await outpuCacheStore.Received(1).EvictByTagAsync(cache, default);
            await almacenadorArchivos.Received(1).Borrar(urlFoto, contenedor);
        }
    }
}
