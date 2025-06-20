using System.Runtime.CompilerServices;

namespace BibliotecaAPI.Servicios
{
    public interface IAlmacenadorArchivos
    {
        Task Borrar(string? ruta, string contenedor); // Para poder borrar archivos
        Task<string> Almacenar(string contenedor, IFormFile archivo); // para guardar un archivo. Devolverá la ruta
        async Task<string> Editar(string?ruta, string contenedor, IFormFile archivo) // Borra un archivo y guarda uno nuevo con el mismo nombre
        {
            await Borrar(ruta, contenedor);
            return await Almacenar(contenedor, archivo);
        }

    }
}
