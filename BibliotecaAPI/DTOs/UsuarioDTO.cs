using System.ComponentModel.DataAnnotations;

namespace BibliotecaAPI.DTOs
{
    public class UsuarioDTO
    {
        public required string Email { get; set; }
        public DateTime FechaNacimiento { get; set; }
    }
    public class CredencialesUsuarioDTO
    {
        [Required]
        [EmailAddress]
        public required String Email { get; set; }
        [Required]
        public string? Password { get; set; }
    }

    public class EditarClaimDTO
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }
    }

    public class  ActualizarUsuarioDTO
    {
        public DateTime FechaNacimiento { get; set; }
    }
}
