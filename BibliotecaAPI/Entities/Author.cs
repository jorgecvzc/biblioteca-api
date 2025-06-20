using BibliotecaAPI.Validaciones;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace BibliotecaAPI.Entities
{
    public class Author
    {
        public int Id { get; set; }
        // Sección de validaciones predeterminadas para ASP.NET-Core
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
        [Unicode(false)]
        public string? Foto { get; set; }
        public List<AuthorBook> Books { get; set; } = [];
    }
}
