using Comercio.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Comercio.Api.Data
{
    public class AppDbContext:DbContext
    {
        public DbSet<Producto> Productos { get; set; }
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
        }
    }
}
