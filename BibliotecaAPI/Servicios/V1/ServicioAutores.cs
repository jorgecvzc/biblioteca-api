using AutoMapper;
using BibliotecaAPI.Data;
using BibliotecaAPI.DTOs;
using BibliotecaAPI.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAPI.Servicios.V1
{
    public interface IServicioAutores
    {
        Task<IEnumerable<AuthorDTO>> Get(PaginacionDTO paginacionDTO);
    }

    public class ServicioAutores : IServicioAutores
    {
        private readonly ApplicationDbContext context;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IMapper mapper;

        public ServicioAutores(ApplicationDbContext context,
            IHttpContextAccessor httpContextAccessor, IMapper mapper)
        {
            this.context = context;
            this.httpContextAccessor = httpContextAccessor;
            this.mapper = mapper;
        }

        public async Task<IEnumerable<AuthorDTO>> Get(PaginacionDTO paginacionDTO)
        {
            var queryable = context.Authors.AsQueryable();
            await httpContextAccessor.HttpContext!.InsertarParametrosPaginacionCabecera(queryable);
            var autores = await queryable.OrderBy(x => x.Names).Paginar(paginacionDTO).ToListAsync();
            var autoresDTO = mapper.Map<IEnumerable<AuthorDTO>>(autores);
            return autoresDTO;
        }

    }
}
