using AutoMapper;
using BibliotecaAPI.Data;
using BibliotecaAPI.DTOs;
using BibliotecaAPI.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAPI.Controllers.V1
{
    [ApiController]
    [Route("api/V1/autores-coleccion")]
    [Authorize(Policy = "esadmin")]
    public class AuthorsCollectionController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;

        public AuthorsCollectionController(ApplicationDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        [HttpGet("{ids}", Name = "OtenerAutoresPorIdsV1")] // api/V1/autores-coleccion/1,2,3
        //[AllowAnonymous]
        public async Task<ActionResult<List<AuthorConLibrosDTO>>> Get(string ids)
        {
            var idsColeccion = new List<int>();

            foreach (var id in ids.Split(','))
            {
                if (int.TryParse(id, out int idInt))
                {
                    idsColeccion.Add(idInt);
                }
            }

            if (!idsColeccion.Any())
            {
                ModelState.AddModelError(nameof(ids), "");
                return ValidationProblem();
            }

            var autores = await context.Authors
                .Include(x => x.Books)
                    .ThenInclude(y => y.Book)
                .Where(x => idsColeccion.Contains(x.Id))
                .ToListAsync();

            if (autores.Count() != idsColeccion.Count())
            {
                return NotFound();
            }

            var autoresDTO = mapper.Map<List<AuthorConLibrosDTO>>(autores);
            return autoresDTO;
        } 

        [HttpPost(Name = "CrearAutoresV1")]
        public async Task<ActionResult> Post(IEnumerable<AuthorCreacionDTO> autoresCreacionDTO)
        {
            var autores = mapper.Map<IEnumerable<Author>>(autoresCreacionDTO);
            context.AddRange(autores);
            await context.SaveChangesAsync();

            var autoresDTO = mapper.Map<IEnumerable<AuthorDTO>>(autores);
            var ids = autores.Select(x => x.Id);
            var idsString = string.Join(',', ids);
            return CreatedAtRoute("OtenerAutoresPorIdsV1", new { ids = idsString }, autoresDTO);
        }
    }
}
