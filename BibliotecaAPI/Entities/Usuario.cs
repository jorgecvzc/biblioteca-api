using Microsoft.AspNetCore.Identity;

namespace BibliotecaAPI.Entities
{
    public class Usuario : IdentityUser // Nueva clase usuario. Se debe aplicar en ApplicationDbContext
    {
        public DateTime FechaNacimiento { get; set; }
    }
}
