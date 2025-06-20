using System.ComponentModel.DataAnnotations;

namespace BibliotecaAPI.Entities
{
    public class Book
    {
        public int Id { get; set; }
        [Required]
        [StringLength(250, ErrorMessage="El campo {0} no puede tener más de {1} caracterres")] // El campo Name no puede tener más de 100 elementos
        public required string Title { get; set; }
        public List<AuthorBook> Authors { get; set; } = [];
        public List<Comentario> Comentarios { get; set; } = [];
    }
}
