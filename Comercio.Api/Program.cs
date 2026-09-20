using Comercio.Api.Data;
using Comercio.Api.Repository;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using FluentValidation;
using Comercio.Api.Filters;
using Comercio.Api.Service;
using Comercio.Api.Mappings;
using Comercio.Api.Validators.Producto;
using Comercio.Api.Service.Token;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<ICarritoRepository, CarritoRepository>();
builder.Services.AddScoped<ICarritoService, CarritoService>();


builder.Services.AddAutoMapper(config =>
{
    config.AddProfile<ProductoProfile>();
    config.AddProfile<UserProfile>();
    config.AddProfile<CarritoProfile>();
});

builder.Services.AddValidatorsFromAssemblyContaining<ProductoCrearDtoValidator>();
builder.Services.AddScoped(typeof(ValidationFilter<>));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Secret"]!))
        };
    });

builder.Services.AddCors(options =>
{   
    options.AddPolicy("PermitirAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});

//Hacer los test unitarios y ver si podemos hacer alguno que otro test de integracion, quizas aplicando otra base de datos o usando
//dockert

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("PermitirAngular");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();


public partial class Program { }