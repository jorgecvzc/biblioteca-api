using BibliotecaAPI.DTOs;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace BibliotecaAPI.Entities
{
    public class Comentario
    {
        public Guid Id { get; set; }
        [Required]
        public required string Cuerpo { get; set; }
        public DateTime FechaPublicacion { get; set; }
        public int BookId { get; set; }
        public Book? Book { get; set; }
        public required string UsuarioId { get; set; }
        public bool EstaBorrado { get; set; }
        public Usuario? Usuario { get; set; }
    }
}
