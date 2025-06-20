using BibliotecaAPI.DTOs;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System.Runtime.CompilerServices;

namespace BibliotecaAPI.Utilities
{
    public static class IQueryableExtensions
    {
        public static IQueryable<T> Paginar<T>(this IQueryable<T> queryable,
            PaginacionDTO paginacionDTO)
        {
            return queryable
                .Skip((paginacionDTO.Pagina - 1) * paginacionDTO.RecordsPorPagina)
                .Take(paginacionDTO.RecordsPorPagina);
        }
    }
}
