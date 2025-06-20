using BibliotecaAPI.Entities;
using BibliotecaAPI.Validaciones;
using System.ComponentModel.DataAnnotations;

namespace BibliotecaAPI.DTOs
{
    public class AuthorPatchDTO
    {
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [StringLength(120, ErrorMessage = "El campo {0} no puede tener más de {1} caracterres")] // El campo Name no puede tener más de 120 elementos
        [PrimeraLetraMayuscula]
        public required string Names { get; set; }
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [StringLength(120, ErrorMessage = "El campo {0} no puede tener más de {1} caracterres")] // El campo Name no puede tener más de 120 elementos
        [PrimeraLetraMayuscula]
        public required string Apellidos { get; set; }
        [StringLength(20, ErrorMessage = "El campo {0} no puede tener más de {1} caracterres")] // El campo Name no puede tener más de 20 elementos
        public string? Identificacion { get; set; }
        public List<Book> Books { get; set; } = new List<Book>();
    }
}
