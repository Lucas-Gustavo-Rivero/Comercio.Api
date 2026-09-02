using System;
using System.Collections.Generic;
using System.Text;
using Comercio.Api.Data;
using Microsoft.EntityFrameworkCore;
using Testcontainers;
using Testcontainers.MsSql;


namespace Comercio.Api.IntegrationTest.Integration
{
    public class ProductoApiFixture : IAsyncLifetime
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
    }
}
