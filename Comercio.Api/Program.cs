using Comercio.Api.Data;
using Comercio.Api.Repository;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using FluentValidation;
using Comercio.Api.Filters;
using Comercio.Api.Service;
using Comercio.Api.Mappings;
using Comercio.Api.Validators.Producto;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddScoped<IProductoRepository, ProductoRepository>();
builder.Services.AddScoped<IProductoService, ProductoService>();

builder.Services.AddAutoMapper(config =>
{
    config.AddProfile<ProductoProfile>();
});

builder.Services.AddValidatorsFromAssemblyContaining<ProductoCrearDtoValidator>();
builder.Services.AddScoped(typeof(ValidationFilter<>));

//Hacer los test unitarios y ver si podemos hacer alguno que otro test de integracion, quizas aplicando otra base de datos o usando
//dockert

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();


public partial class Program { }