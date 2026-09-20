using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;
using Comercio.Api.Data;
using Comercio.Api.DTOs.User;
using Comercio.Api.Models;
using Microsoft.EntityFrameworkCore;
using Testcontainers;
using Testcontainers.MsSql;


namespace Comercio.Api.IntegrationTest.Integration
{
    public class ComercioApiFixture : IAsyncLifetime
    {
        private readonly MsSqlContainer _msSqlContainer = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();
       
        private ComercioApiFactory _factory = null!;
        public AppDbContext DbContext { get; private set; } = null!;
        public HttpClient HttpClient { get; private set; } = null!;

        public async Task DisposeAsync()
        {
            await DbContext.DisposeAsync();
            HttpClient.Dispose();
            await _factory.DisposeAsync();
            await _msSqlContainer.DisposeAsync();
        }

        public async Task InitializeAsync()
        {
            await _msSqlContainer.StartAsync();
            var connectionString = _msSqlContainer.GetConnectionString();

            var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlServer(connectionString).Options;

            DbContext = new AppDbContext(options);
            await DbContext.Database.MigrateAsync();

            _factory = new ComercioApiFactory(connectionString);
            HttpClient = _factory.CreateClient();
        }

        public async Task<string> ObtenerTokenAdminAsync()
        {
            return await CrearUsuarioYObtenerTokenAsync(Roles.Administrador);
        }

        public async Task<string> ObtenerTokenUsuarioAsync()
        {
            return await CrearUsuarioYObtenerTokenAsync(Roles.Usuario);
        }

        private async Task<string> CrearUsuarioYObtenerTokenAsync(Roles rol)
        {
            string email = $"{rol}-{Guid.NewGuid()}@test.com";
            const string password = "test123456789";

            var usuario = new User
            {
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                NombreCompleto = $"test de prueba - {rol}",
                Rol = rol
            };

            DbContext.Users.Add(usuario);
            await DbContext.SaveChangesAsync();

            var loginDto = new LoginDto { Email = email, Password = password };
            var response = await HttpClient.PostAsJsonAsync("/api/auth/login", loginDto);

            var resultado = await response.Content.ReadFromJsonAsync<AuthResponseDto>();

            return resultado!.Token;
        }
    }
}
