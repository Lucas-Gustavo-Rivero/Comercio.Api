using Comercio.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Comercio.Api.Data
{
    public class AppDbContext:DbContext
    {
        public DbSet<Producto> Productos { get; set; }
        public DbSet<User> Users { get; set; } 
        public DbSet<Carrito> Carritos { get; set; }
        public DbSet<CarritoItem> CarritoItems { get; set; }
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Producto>().HasIndex(p => p.Nombre).IsUnique();
            modelBuilder.Entity<Producto>(entity =>
            {
                entity.Property(p => p.Nombre)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(p => p.Descripcion)
                    .HasMaxLength(500);

                entity.Property(p => p.Precio)
                    .HasPrecision(18, 2);

                entity.Property(p => p.UrlImagen)
                    .HasMaxLength(500);

                entity.Property(p => p.RowVersion)
                    .IsRowVersion();
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Email).IsUnique();

                entity.Property(u => u.NombreCompleto)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(u => u.Email)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(u => u.PasswordHash)
                    .IsRequired();
            });

            modelBuilder.Entity<Carrito>(entity =>
            {
                entity.HasIndex(c => c.UsuarioId).IsUnique();
                entity.Property(c => c.RowVersion).IsRowVersion();
                entity.HasOne(c => c.User)
                    .WithOne()
                    .HasForeignKey<Carrito>(c => c.UsuarioId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(c => c.CarritoItems)
                    .WithOne(c => c.Carrito)
                    .HasForeignKey(c => c.CarritoId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<CarritoItem>(entity =>
            {
                entity.HasIndex(ci => new { ci.CarritoId, ci.ProductoId })
                    .IsUnique();

                entity.Property(ci => ci.PrecioUnitario)
                    .HasPrecision(18, 2);

                entity.HasOne(ci => ci.Producto)
                    .WithMany()
                    .HasForeignKey(ci => ci.ProductoId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
