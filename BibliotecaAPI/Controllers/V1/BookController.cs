using AutoMapper;
using BibliotecaAPI.Data;
using BibliotecaAPI.DTOs;
using BibliotecaAPI.Entities;
using BibliotecaAPI.Migrations;
using BibliotecaAPI.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using System.Security.AccessControl;

namespace BibliotecaAPI.Controllers.V1
{
    [ApiController]
    [Route("api/V1/libros")]
    [Authorize(Policy = "esadmin")]
    public class BookController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly IOutputCacheStore outputCacheStore;
        private const string cache = "libros";

        public BookController(ApplicationDbContext context,IMapper mapper,
            IOutputCacheStore outputCacheStore)
        {
            this.context = context;
            this.mapper = mapper;
            this.outputCacheStore = outputCacheStore;
        }

        [HttpGet(Name = "ObtenerLibrosV1")]
        [AllowAnonymous]
        [OutputCache (Tags = [cache])]
        public async Task<IEnumerable<BookDTO>> Get([FromQuery] PaginacionDTO paginacionDTO)
        {
            var queryable = context.Books.AsQueryable();
            await HttpContext.InsertarParametrosPaginacionCabecera(queryable);
            var libros = await queryable
                .OrderBy(x => x.Title)
                .Paginar(paginacionDTO).ToListAsync();
            var librosDTO = mapper.Map<IEnumerable<BookDTO>>(libros);
            return librosDTO;
        }

        [HttpGet("{id:int}", Name = "ObtenerLibroV1")] // api/V1/books/id
        [AllowAnonymous]
        [OutputCache(Tags = [cache])]
        public async Task<ActionResult<BookConAuthorsDTO>> Get(int id)
        {
            var book = await context.Books
                .Include(x => x.Authors)
                    .ThenInclude(y => y.Author)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (book is null)
            {
                return NotFound();
            }
            else
            {
                var bookDTO = mapper.Map<BookConAuthorsDTO>(book);
                return Ok(bookDTO);
            }
        }

        [HttpPost(Name = "CrearLibroV1")]
        [ServiceFilter<FiltroValidacionLibro>]
        public async Task<ActionResult> Post(BookCreacionDTO bookCreacionDTO)
        {
            var book = mapper.Map<Book>(bookCreacionDTO);
            AsignarOrdenAutores(book);

            context.Add(book);
            await context.SaveChangesAsync();
            await outputCacheStore.EvictByTagAsync(cache, default);
            return CreatedAtRoute("ObtenerLibroV1", new { id = book.Id }, bookCreacionDTO);
        }

        private void AsignarOrdenAutores(Book book)
        {
            if (book.Authors is not null)
            {
                for (int i = 0; i < book.Authors.Count; i++)
                {
                    book.Authors[i].Order = i;
                }
            }
        }

        [HttpPut("{id:int}", Name = "ActualizarLibroV1")] // api/V1/books/id
        [ServiceFilter<FiltroValidacionLibro>]
        public async Task<ActionResult> Put(int id, BookCreacionDTO bookCreacionDTO)
        {
            var bookDB = await context.Books
                .Include(x => x.Authors)
                .FirstOrDefaultAsync(x => x.Id == id);
             
            if (bookDB is null)
            {
                return NotFound();
            }

            bookDB = mapper.Map(bookCreacionDTO, bookDB);
            AsignarOrdenAutores(bookDB);
            
            await context.SaveChangesAsync();
            await outputCacheStore.EvictByTagAsync(cache, default);
            return NoContent();
        }

        [HttpDelete("{id:int}", Name = "BorrarLibroV1")] // api/V1/books/id
        public async Task<ActionResult> Delete(int id)
        {
            var registrosBorrados = await context.Books.Where(x => x.Id == id).ExecuteDeleteAsync();

            if (registrosBorrados == 0)
            {
                return NotFound();
            }
            else
            {
                await outputCacheStore.EvictByTagAsync(cache, default);
                return NoContent();
            }
        }

    }
}
