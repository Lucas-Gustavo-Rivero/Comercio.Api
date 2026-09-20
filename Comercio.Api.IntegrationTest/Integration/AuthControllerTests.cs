using Comercio.Api.Data;
using Comercio.Api.DTOs.User;
using Comercio.Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace Comercio.Api.IntegrationTest.Integration
{
    public class AuthControllerTests:IClassFixture<ComercioApiFixture>
    {
        private readonly HttpClient _httpClient;
        private readonly AppDbContext _context;
        private const string RutaBaseLogin = "/api/auth/login";
        private const string RutaBaseRegister = "/api/auth/register";

        public AuthControllerTests(ComercioApiFixture fixture)
        {
            _httpClient = fixture.HttpClient;

            var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlServer(fixture.DbContext.Database.GetConnectionString()).Options;

            _context = new AppDbContext(options);
        }

        [Fact]
        public async Task Register_UsuarioNoExisteYDatosValidos_DevuelveOk()
        {
            //ARRANGE

            string emailUnico = $"{Guid.NewGuid()}@test.com";

            var registerDto = new RegisterDto { Email = emailUnico , NombreCompleto = "test", Password = "test123456789"};

            //ACT

            var response = await _httpClient.PostAsJsonAsync(RutaBaseRegister, registerDto);

            //ASSERT

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var authResponseDto = await response.Content.ReadFromJsonAsync<AuthResponseDto>();

            Assert.NotNull(authResponseDto);
            Assert.Equal(emailUnico, authResponseDto.Email);
            Assert.Equal("Usuario", authResponseDto.Rol);
            Assert.False(string.IsNullOrWhiteSpace(authResponseDto.Token));
        }

        [Fact]
        public async Task Register_UsuarioExisteYDatosValidos_DevuelveConflict()
        {
            //ARRANGE
            string emailUnico = $"{Guid.NewGuid()}@test.com";
            const string password = "test123456789";
            var user = new User { Email = emailUnico, NombreCompleto = "test", PasswordHash = BCrypt.Net.BCrypt.HashPassword(password) };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var registerDto = new RegisterDto { Email = user.Email, NombreCompleto = "test", Password = password };

            //ACT

            var response = await _httpClient.PostAsJsonAsync(RutaBaseRegister, registerDto);

            //ASSERT

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

            var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();

            Assert.NotNull(problemDetails);
            Assert.Equal("Conflicto", problemDetails.Title);
            Assert.Equal(StatusCodes.Status409Conflict, problemDetails.Status);
            Assert.Equal("Ya existe un usuario con ese email.", problemDetails.Detail);
        }

        [Fact]
        public async Task Register_DatosInvalidos_DevuelveBadRequest()
        {
            //ARRANGE
            var registerDto = new RegisterDto { Email = "", NombreCompleto = "test", Password = "test123456789" };

            //ACT

            var response = await _httpClient.PostAsJsonAsync(RutaBaseRegister, registerDto);

            //ASSERT

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            Assert.NotNull(problemDetails);
            Assert.True(problemDetails.Errors.ContainsKey("Email"));
        }

        [Fact]
        public async Task Login_UsuarioExisteYContraseñaValida_DevuelveOk()
        {
            //ARRANGE

            string emailUnico = $"{Guid.NewGuid()}@test.com";
            const string password = "test123456789";
            var user = new User { Email = emailUnico , NombreCompleto = "test", PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)};
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var loginDto = new LoginDto { Email = emailUnico, Password = password };

            //ACT

            var response = await _httpClient.PostAsJsonAsync(RutaBaseLogin, loginDto);

            //ASSERT

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();

            Assert.NotNull(result);
            Assert.Equal(emailUnico, result.Email);
            Assert.Equal("Usuario", result.Rol);
            Assert.False(string.IsNullOrWhiteSpace(result.Token));
        }

        [Fact]
        public async Task Login_UsuarioNoExiste_DevuelveUnauthorized()
        {
            //ARRANGE

            var loginDto = new LoginDto { Email = "test@hotmail.com", Password = "test123567890" };

            //ACT

            var response = await _httpClient.PostAsJsonAsync(RutaBaseLogin, loginDto);

            //ASSERT

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

            var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();

            Assert.NotNull(problemDetails);
            Assert.Equal(StatusCodes.Status401Unauthorized, problemDetails.Status);
            Assert.Equal("El email o la contraseña son incorrectos", problemDetails.Title);
            Assert.Equal("Verifique sus datos de acceso e intente nuevamente.", problemDetails.Detail);
        }

        [Fact]
        public async Task Login_UsuarioExisteYContraseñaInvalida_DevuelveUnauthorized()
        {
            //ARRANGE

            string emailUnico = $"{Guid.NewGuid()}@test.com";
            string passwordValida = "test123456789";
            string passwordInvalida = "testInvalida123";
            var user = new User { Email = emailUnico, NombreCompleto = "test", PasswordHash = BCrypt.Net.BCrypt.HashPassword(passwordValida) };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var loginDto = new LoginDto { Email = emailUnico, Password = passwordInvalida };

            //ACT
            var response = await _httpClient.PostAsJsonAsync(RutaBaseLogin, loginDto);

            //ASSERT

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

            var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();

            Assert.NotNull(problemDetails);
            Assert.Equal(StatusCodes.Status401Unauthorized, problemDetails.Status);
            Assert.Equal("El email o la contraseña son incorrectos", problemDetails.Title);
            Assert.Equal("Verifique sus datos de acceso e intente nuevamente.", problemDetails.Detail);

        }

        [Fact]
        public async Task Login_DatosInvalidos_DevuelveBadRequest()
        {
            //ARRANGE

            var loginDto = new LoginDto { Email = "", Password = "test123456789" };

            //ACT

            var response = await _httpClient.PostAsJsonAsync(RutaBaseLogin, loginDto);

            //ASSERT

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            Assert.NotNull(problemDetails);
            Assert.True(problemDetails.Errors.ContainsKey("Email"));
        }
    }
}





























