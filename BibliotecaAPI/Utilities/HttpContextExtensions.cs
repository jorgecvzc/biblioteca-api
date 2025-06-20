using Microsoft.AspNetCore.Server.IISIntegration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

namespace BibliotecaAPI.Utilities
{
    public static class HttpContextExtensions
    {
        public async static Task
            InsertarParametrosPaginacionCabecera<T>(this HttpContext httpContext,
            IQueryable<T> queryable)
        {
            if (httpContext is null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            double cantidad = await queryable.CountAsync();
            httpContext.Response.Headers.Append("cantidad-total-registros",  cantidad.ToString());
        }
    }
}
