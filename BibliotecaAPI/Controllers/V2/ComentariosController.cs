using AutoMapper;
using BibliotecaAPI.Data;
using BibliotecaAPI.DTOs;
using BibliotecaAPI.Entities;
using BibliotecaAPI.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;

namespace BibliotecaAPI.Controllers.V2
{
    [ApiController]
    [Route("api/V2/libros/{bookId:int}/comentarios")]
    [Authorize]
    public class ComentariosController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly IServiciosUsuarios serviciosUsuarios;
        private readonly IOutputCacheStore outputCacheStore;
        private const string cache = "comentarios";

        public ComentariosController(ApplicationDbContext context, IMapper mapper, IServiciosUsuarios serviciosUsuarios,
            IOutputCacheStore outputCacheStore)
        {
            this.context = context;
            this.mapper = mapper;
            this.serviciosUsuarios = serviciosUsuarios;
            this.outputCacheStore = outputCacheStore;
        }

        [HttpGet]
        [AllowAnonymous]
        [OutputCache(Tags = [cache])]
        public async Task<ActionResult<List <ComentarioDTO>>> Get(int bookId)
        {
            var existeBook = await context.Books.AnyAsync(x => x.Id == bookId);
            if (!existeBook)
            {
                return NotFound();
            }

            var comentarios = await context.Comentarios
                .Include(x => x.Usuario)
                .Where(x => x.BookId == bookId)
                .OrderByDescending(x => x.FechaPublicacion)
                .ToListAsync();

            return mapper.Map<List<ComentarioDTO>>(comentarios);
        }

        [HttpGet("{id}", Name = "ObtenerComentarioV2")]
        [AllowAnonymous]
        [OutputCache(Tags = [cache])]
        public async Task<ActionResult<ComentarioDTO>> Get(Guid id)
        {
            var comentario = await context.Comentarios
                .Include (x => x.Usuario)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (comentario is null)
            {
                return NotFound();
            }

            return mapper.Map<ComentarioDTO>(comentario);
        }

        [HttpPost]
        public async Task<ActionResult> Post(int bookId, ComentarioCreacionDTO comentarioCreacionDTO)
        {
            var existeBook = await context.Books.AnyAsync(x => x.Id == bookId);
            if (!existeBook)
            {
                return NotFound();
            }

            var usuario = await serviciosUsuarios.ObtenerUsuario();
            if (usuario is null)
            {
                return NotFound();
            }

            var comentario = mapper.Map<Comentario> (comentarioCreacionDTO);
            comentario.BookId = bookId;
            comentario.FechaPublicacion = DateTime.UtcNow;
            comentario.UsuarioId = usuario.Id;
            context.Add(comentario);
            await context.SaveChangesAsync();
            await outputCacheStore.EvictByTagAsync(cache, default);

            var comentarioDTO = mapper.Map<ComentarioDTO> (comentario);

            return CreatedAtRoute("ObtenerComentarioV2", new {id = comentario.Id, bookId }, comentarioDTO);
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult> Patch(Guid id, int bookId, JsonPatchDocument<ComentarioPatchDTO> patchDoc)
        {
            if (patchDoc is null)
            {
                return BadRequest();
            }
            
            var existeBook = await context.Books.AnyAsync(x => x.Id == bookId);
            if (!existeBook)
            {
                return NotFound();
            }

            var usuario = await serviciosUsuarios.ObtenerUsuario();
            if (usuario is null)
            {
                return NotFound();
            }

            var comentarioDB = await context.Comentarios.FirstOrDefaultAsync(x => x.Id == id);
            if (comentarioDB is null)
            {
                return NotFound();
            }

            if (comentarioDB.UsuarioId != usuario.Id)
            {
                return Forbid();
            }

            var comentarioPatchDTO = mapper.Map<ComentarioPatchDTO>(comentarioDB);

            patchDoc.ApplyTo(comentarioPatchDTO, ModelState);

            var esValido = TryValidateModel(comentarioPatchDTO);
            if (!esValido)
            {
                return ValidationProblem();
            }

            mapper.Map(comentarioPatchDTO, comentarioDB);

            await context.SaveChangesAsync();
            await outputCacheStore.EvictByTagAsync(cache, default);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id, int bookId)
        {
            var existeBook = await context.Books.AnyAsync(x => x.Id == bookId);
            if (!existeBook)
            {
                return NotFound();
            }

            var usuario = await serviciosUsuarios.ObtenerUsuario();
            if (usuario is null)
            {
                return NotFound();
            }
            var comentarioDB = await context.Comentarios.FirstOrDefaultAsync(x => x.Id == id);

            if (comentarioDB is null)
            {
                return NotFound();
            }

            if (comentarioDB.UsuarioId != usuario.Id)
            {
                return Forbid();
            }

            comentarioDB.EstaBorrado = true;
            context.Update(comentarioDB);
            await context.SaveChangesAsync();
            await outputCacheStore.EvictByTagAsync(cache, default);

            return NoContent();
        }

    }
}
