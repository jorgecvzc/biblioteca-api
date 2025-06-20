using AutoMapper;
using BibliotecaAPI.DTOs;
using BibliotecaAPI.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace BibliotecaAPI.Utilities
{
    public class AutoMapperAuthorsProfiles : Profile
    {
        private string MapearNombreYApellido(Author author) =>  $"{author.Names} {author.Apellidos}";

        public AutoMapperAuthorsProfiles()
        {
            CreateMap<Author, AuthorDTO>()
                .ForMember(dto => dto.NombreCompleto,
                           config => config.MapFrom(author => MapearNombreYApellido(author)));
            CreateMap<Author, AuthorConLibrosDTO>()
                .ForMember(dto => dto.NombreCompleto,
                           config => config.MapFrom(author => MapearNombreYApellido(author)));
            
            CreateMap<AuthorCreacionDTO, Author>();
            CreateMap<AuthorCreacionDTOConFoto, Author>()
                .ForMember(ent => ent.Foto, config => config.Ignore());

            CreateMap<Author, AuthorPatchDTO>().ReverseMap();

            CreateMap<AuthorBook, BookDTO>()
                .ForMember(dto => dto.Id, config => config.MapFrom(ent => ent.BookId))
                .ForMember(dto => dto.Title, config => config.MapFrom(ent => ent.Book!.Title));
        }
    }

    public class AutoMapperBooksProfiles : Profile
    {
        private string MapearNombreYApellido(Author author) => $"{author.Names} {author.Apellidos}";

        public AutoMapperBooksProfiles()
        {
            CreateMap<Book, BookDTO>();
           
            CreateMap<BookCreacionDTO, Book>()
                .ForMember(ent => ent.Authors, 
                           config => config.MapFrom(dto => dto.AuthorsId.Select(id => new AuthorBook { AuthorId = id })));

            CreateMap<Book, BookConAuthorsDTO>();
            CreateMap<AuthorBook, AuthorDTO>()
                .ForMember(dto => dto.Id, config => config.MapFrom(ent => ent.AuthorId))
                .ForMember(dto => dto.NombreCompleto, config => config.MapFrom(ent => MapearNombreYApellido(ent.Author!)));

            CreateMap<BookCreacionDTO, AuthorBook>()
                .ForMember(ent => ent.Book, config => config.MapFrom(dto => new Book { Title = dto.Title }));
        }
    }

    public class AutoMapperComentariosProfiles : Profile
    {
        public AutoMapperComentariosProfiles()
        {
            CreateMap<ComentarioCreacionDTO, Comentario>();
            CreateMap<ComentarioPatchDTO, Comentario>().ReverseMap();
            CreateMap<Comentario, ComentarioDTO>()
                .ForMember(dto => dto.UsuarioEmail, config => config.MapFrom(ent => ent.Usuario!.Email));
        }
    }

    public class AutoMapperUsuariosProfiles : Profile
    {
        public AutoMapperUsuariosProfiles()
        {
            CreateMap<Usuario, UsuarioDTO>();
        }
    }
}