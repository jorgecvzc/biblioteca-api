using BibliotecaAPI.Data;
using BibliotecaAPI.Entities;
using BibliotecaAPI.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Azure;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel;
using BibliotecaAPI.Utilities;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using BibliotecaAPI.Servicios;
using Microsoft.AspNetCore.Mvc.Authorization;
using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.OutputCaching;
using BibliotecaAPI.Servicios.V1;
using BibliotecaAPI.Utilities.V1;

namespace BibliotecaAPI.Controllers.V1
{
    [ApiController]
    [Route("api/V1/autores")]
    [Authorize(Policy = "esadmin")]
    public class AuthorController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly IAlmacenadorArchivos almacenadorArchivos;
        private readonly ILogger<AuthorController> logger;
        private readonly IOutputCacheStore outputCacheStore;
        private readonly IServicioAutores servicioAutoresV1;
        private const string contenedor = "autores";
        private const string cache = "autores-obtener"; 

        public AuthorController(ApplicationDbContext context, IMapper mapper, 
                IAlmacenadorArchivos almacenadorArchivos, ILogger<AuthorController> logger,
                IOutputCacheStore outputCacheStore, IServicioAutores servicioAutoresV1)
        {
            this.context = context;
            this.mapper = mapper;
            this.almacenadorArchivos = almacenadorArchivos;
            this.logger = logger;
            this.outputCacheStore = outputCacheStore;
            this.servicioAutoresV1 = servicioAutoresV1;
        }

        [HttpGet(Name = "ObtenerAutoresV1")]
        [AllowAnonymous]
        [OutputCache(Tags = [cache])]
        [ServiceFilter<HATEOASAutoresAttribute>]
        public async Task<IEnumerable<AuthorDTO>> Get(
            [FromQuery] PaginacionDTO paginacionDTO)
        {
            throw new ApplicationException("Prueba test error");
            return await servicioAutoresV1.Get(paginacionDTO);
        }

        [HttpGet("{id:int}", Name = "ObtenerAutorV1")] // api/V1/authors/id?incluirLibros=true|false &
        [AllowAnonymous]
        [EndpointSummary("Obtiene autor por id")]
        [EndpointDescription("Obtiene un autor por su Id incluyendo sus libros.")]
        [ProducesResponseType<AuthorConLibrosDTO>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [OutputCache(Tags = [cache])]
        [ServiceFilter<HATEOASAuthorAttribute>()]
        public async Task<ActionResult<AuthorConLibrosDTO>> Get([FromRoute] [Description("El id del autor")] int id)
        {
            var autor = await context.Authors
                .Include(x => x.Books)
                    .ThenInclude(y => y.Book)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (autor is null)
            {
                return NotFound();
            }
            else
            {
                var authorDTO = mapper.Map<AuthorConLibrosDTO>(autor);
         
                return authorDTO;
            }
        }

        [HttpGet("filtrar", Name = "FiltrarAutoresV1")]
        [AllowAnonymous]
        public async Task<ActionResult> Filtrar([FromQuery] AutorFiltroDTO autorFiltroDTO)
        {
            var queryable = context.Authors.AsQueryable();

            if (!string.IsNullOrEmpty(autorFiltroDTO.Nombres))
            {
                queryable = queryable.Where(x => x.Names.Contains(autorFiltroDTO.Nombres));
            }

            if (!string.IsNullOrEmpty(autorFiltroDTO.Apellidos))
            {
                queryable = queryable.Where(x => x.Apellidos.Contains(autorFiltroDTO.Apellidos));
            }

            if (autorFiltroDTO.IncluirLibros)
            {
                queryable = queryable.Include(x => x.Books).ThenInclude(x => x.Book);
            }

            if (autorFiltroDTO.ConFoto.HasValue)
            {
                if (autorFiltroDTO.ConFoto.Value)
                {
                    queryable = queryable.Where(x => x.Foto != null);
                }
                else
                {
                    queryable = queryable.Where(x => x.Foto == null);
                }
            }

            if (autorFiltroDTO.ConLibros.HasValue)
            {
                if (autorFiltroDTO.ConLibros.Value)
                {
                    queryable = queryable.Where(x => x.Books.Any());
                }
                else
                {
                    queryable = queryable.Where(x => !x.Books.Any());
                }
            }

            if (!string.IsNullOrEmpty(autorFiltroDTO.TituloLibro))
            {
                queryable = queryable.Where(x => x.Books.Any(y => y.Book!.Title.Contains(autorFiltroDTO.TituloLibro)));
            }

            if (!string.IsNullOrEmpty(autorFiltroDTO.CampoOrdenar))
            {
                var tipoOrden = autorFiltroDTO.OrdenAscendente ? "ascending" : "descending";

                try
                {
                    queryable = queryable.OrderBy($"{autorFiltroDTO.CampoOrdenar} {tipoOrden}");
                }
                catch (Exception ex)
                {
                    queryable = queryable.OrderBy(x => x.Names);
                    logger.LogError(ex.Message, ex);
                }
            }
            else
            {
                queryable = queryable.OrderBy(x => x.Names);
            }

            var authors = await queryable
                                .Paginar(autorFiltroDTO.PaginacionDTO)
                                .ToListAsync();

            if (autorFiltroDTO.IncluirLibros)
            {
                var autoresDTO = mapper.Map<IEnumerable<AuthorConLibrosDTO>>(authors);
                return Ok(autoresDTO);
            }
            else
            {
                var autoresDTO = mapper.Map<IEnumerable<AuthorDTO>>(authors);
                return Ok(autoresDTO);
            }            
        }

        [HttpPost(Name = "CrearAutorV1")]
        public async Task<ActionResult> Post([FromBody] AuthorCreacionDTO authorCreDTO)
        {
            var author = mapper.Map<Author>(authorCreDTO);
            context.Add(author);
            await context.SaveChangesAsync();
            await outputCacheStore.EvictByTagAsync(cache, default); // Limpia la caché con tag {cache} al crear un autor para que se pueda devolver en el siguiente Get.
            var authorDTO = mapper.Map<AuthorDTO>(author);
            return CreatedAtRoute("ObtenerAutorV1", new { id = author.Id }, authorDTO);
        }
        
        [HttpPost("con-foto", Name = "CrearAutorConFotoV1")]
        public async Task<ActionResult> PostConFoto([FromForm] AuthorCreacionDTOConFoto authorCreDTO)
        {
            var author = mapper.Map<Author>(authorCreDTO);

            if (authorCreDTO.Foto is not null)
            {
                var url = await almacenadorArchivos.Almacenar(contenedor, authorCreDTO.Foto!);
                author.Foto = url;
            }

            context.Add(author);
            await context.SaveChangesAsync();
            await outputCacheStore.EvictByTagAsync(cache, default);
            var authorDTO = mapper.Map<AuthorDTO>(author);
            return CreatedAtRoute("ObtenerAutorV1", new { id = author.Id }, authorDTO);
        }

        [HttpPut("{id:int}", Name = "ActualizarAutorV1")] // api/V1/authors/id
        public async Task<ActionResult> Put(int id, [FromForm] AuthorCreacionDTOConFoto authorCreDTO)
        {
            var existeAutor = await context.Authors.AnyAsync(x => x.Id == id);

            if (!existeAutor)
            {
                return NotFound();
            }

            var author = mapper.Map<Author>(authorCreDTO);
            author.Id = id;

            if (authorCreDTO.Foto is not null)
            {
                var fotoActual = await context.Authors
                                        .Where(x => x.Id == id)
                                        .Select(x => x.Foto).FirstAsync();
                var url = await almacenadorArchivos.Editar(fotoActual, contenedor, authorCreDTO.Foto);
                author.Foto = url;
            }

            context.Update(author);
            await context.SaveChangesAsync();
            await outputCacheStore.EvictByTagAsync(cache, default);
            return NoContent();  // Resultado tipo 204 (todo correcto) pero indicando que no se va a devolver nada
        }

        [HttpPatch("{id:int}", Name = "PatchAutorV1")]
        public async Task<ActionResult> Patch(int id, JsonPatchDocument<AuthorPatchDTO> patchDoc)
        {
            if (patchDoc is null)
            {
                return BadRequest();
            }

            var authorDB = await context.Authors.FirstOrDefaultAsync(x => x.Id == id);
            if (authorDB is null)
            {
                return NotFound();
            }

            var authorPatchDTO = mapper.Map<AuthorPatchDTO>(authorDB);

            patchDoc.ApplyTo(authorPatchDTO, ModelState);

            var esValido = TryValidateModel(authorPatchDTO);
            if (!esValido)
            {
                return ValidationProblem();
            }

            mapper.Map(authorPatchDTO, authorDB);

            await context.SaveChangesAsync();
            await outputCacheStore.EvictByTagAsync(cache, default);

            return NoContent();
        }

        [HttpDelete("{id:int}", Name = "BorrarAutorV1")] // api/V1/authors/id
        public async Task<ActionResult> Delete(int id)
        {
            var author = await context.Authors.FirstOrDefaultAsync(x => x.Id == id);

            if (author is null)
            {
                return NotFound();
            }

            context.Remove(author);
            await context.SaveChangesAsync();
            await outputCacheStore.EvictByTagAsync(cache, default);
            await almacenadorArchivos.Borrar(author.Foto, contenedor);
            
            return NoContent();
        }

    }
}
