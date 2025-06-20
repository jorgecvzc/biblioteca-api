using BibliotecaAPI.Entities;
using BibliotecaAPI.Validaciones;
using System.ComponentModel.DataAnnotations;

namespace BibliotecaAPI.DTOs
{
    public class AuthorDTO : RecursoDTO
    {
        public int Id { get; set; }
        // Sección de validaciones predeterminadas para ASP.NET-Core
        public required string NombreCompleto { get; set; }
        public string? Foto { get; set; }
    }

    public class AuthorConLibrosDTO : AuthorDTO
    {
        public List<BookDTO> Books { get; set; } = [];
    }

    public class AuthorCreacionDTO
    {
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [StringLength(120, ErrorMessage = "El campo {0} no puede tener más de {1} caracterres")] // El campo Name no puede tener más de 100 elementos
        [PrimeraLetraMayuscula]
        public required string Names { get; set; }
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [StringLength(120, ErrorMessage = "El campo {0} no puede tener más de {1} caracterres")] // El campo Name no puede tener más de 100 elementos
        [PrimeraLetraMayuscula]
        public required string Apellidos { get; set; }
        [StringLength(20, ErrorMessage = "El campo {0} no puede tener más de {1} caracterres")] // El campo Name no puede tener más de 100 elementos
        public string? Identificacion { get; set; }
        public List<BookCreacionDTO> Books { get; set; } = [];
    }

    public class AuthorCreacionDTOConFoto : AuthorCreacionDTO
    {
        public IFormFile? Foto { get; set; }
    }
}
