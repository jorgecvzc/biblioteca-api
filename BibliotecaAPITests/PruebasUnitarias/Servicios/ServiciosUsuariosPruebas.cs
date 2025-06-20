using BibliotecaAPI.Entities;
using BibliotecaAPI.Servicios;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BibliotecaAPITests.PruebasUnitarias.Servicios
{
    [TestClass]
    public class ServiciosUsuariosPruebas
    {
        private UserManager<Usuario> userManager = null!;
        private IHttpContextAccessor contextAccessor = null!;
        private ServiciosUsuarios servicioUsuarios = null!;

        [TestInitialize]
        public void Setup()
        {
            userManager = Substitute.For<UserManager<Usuario>>(
                Substitute.For<IUserStore<Usuario>>(), null, null, null, null, null, null, null, null);
            contextAccessor = Substitute.For<IHttpContextAccessor>();
            servicioUsuarios = new ServiciosUsuarios(userManager, contextAccessor);
        }

        [TestMethod]
        public async Task ObtenerUsuario_RetornaNulo_CuandoNoHayClaimEmail()
        {
            // Preparación
            var httpContext = new DefaultHttpContext();
            contextAccessor.HttpContext.Returns(httpContext); // Valor devuelto por el método de la clase tipo substitute

            // Prueba
            var usuario = await servicioUsuarios.ObtenerUsuario();

            // Verificación
            Assert.IsNull(usuario);
        }

        [TestMethod]
        public async Task ObtenerUsuario_RetornaUsuario_CuandoHayClaimEmail()
        {
            // Preparación
            var email = "prueba@gnail.com";
            var usuarioEsperado = new Usuario {  Email = email };

            userManager.FindByEmailAsync(email)!.Returns(Task.FromResult(usuarioEsperado));
            // Al realizar la llamada con Task. nose ejecuta asincronía, llamándose al método directamente

            var claims = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim("email", email)
            }));

            var httpContext = new DefaultHttpContext() { User = claims };
            contextAccessor.HttpContext.Returns(httpContext); // Valor devuelto por el método de la clase tipo substitute

            // Prueba
            var usuario = await servicioUsuarios.ObtenerUsuario();

            // Verificación
            Assert.IsNotNull(usuario);
            Assert.AreEqual(expected: email, actual: usuario.Email);
        }

        [TestMethod]
        public async Task ObtenerUsuario_RetornaNulo_CuandoUsuarioNoExiste()
        {
            // Preparación
            var email = "prueba@gnail.com";
            var usuarioEsperado = new Usuario { Email = email };

            userManager.FindByEmailAsync(email)!.Returns(Task.FromResult<Usuario>(null));
            // Al realizar la llamada con Task. nose ejecuta asincronía, llamándose al método directamente

            var claims = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim("email", email)
            }));

            var httpContext = new DefaultHttpContext() { User = claims };
            contextAccessor.HttpContext.Returns(httpContext); // Valor devuelto por el método de la clase tipo substitute

            // Prueba
            var usuario = await servicioUsuarios.ObtenerUsuario();

            // Verificación
            Assert.IsNull(usuario);
        }
    }
}
