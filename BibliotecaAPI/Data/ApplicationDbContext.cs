using BibliotecaAPI.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAPI.Data
{
    public class ApplicationDbContext : IdentityDbContext<Usuario>
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Comentario>().HasQueryFilter(b => !b.EstaBorrado);

            // modelBuilder.Entity<Author>().Property(x => x.Name).HasMaxLength(120); 
            // Ejemplo de restricciones de definición para un atributo hacia la BBDD
        }

        public DbSet<Author> Authors { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Comentario> Comentarios { get; set; }
        public DbSet<AuthorBook> AuthorsBooks { get; set; }
        public DbSet<Error> Errors { get; set; }
    }
}
