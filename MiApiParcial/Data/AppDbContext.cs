using Microsoft.EntityFrameworkCore;
using MiApiParcial.Models;

namespace MiApiParcial.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Producto> Productos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración de Producto
            modelBuilder.Entity<Producto>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Precio).HasPrecision(18, 2);
            });

            // Datos de ejemplo (seed data)
            modelBuilder.Entity<Producto>().HasData(
                new Producto { Id = 1, Nombre = "Laptop Gaming", Descripcion = "Laptop para gaming", Precio = 1500.00m, Stock = 10 },
                new Producto { Id = 2, Nombre = "Mouse Inalámbrico", Descripcion = "Mouse ergonómico", Precio = 25.00m, Stock = 50 },
                new Producto { Id = 3, Nombre = "Teclado Mecánico", Descripcion = "Teclado RGB", Precio = 75.00m, Stock = 30 }
            );
        }
    }
}