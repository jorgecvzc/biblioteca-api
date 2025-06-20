using BibliotecaAPI.Entities;
using System.ComponentModel.DataAnnotations;

namespace BibliotecaAPI.DTOs
{
    public class BookDTO
    {
        public int Id { get; set; }
        public required string Title { get; set; }
    }

    public class BookConAuthorsDTO : BookDTO
    {
        public List<AuthorDTO> Authors { get; set; } = [];
    }

    public class BookCreacionDTO
    {
        [Required]
        [StringLength(250, ErrorMessage = "El campo {0} no puede tener más de {1} caracterres")] // El campo Name no puede tener más de 100 elementos
        public required string Title { get; set; }
        public List<int> AuthorsId { get; set; } = new List<int>();
    }
}
