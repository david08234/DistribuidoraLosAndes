using Microsoft.EntityFrameworkCore;
using DistribuidoraLosAndes.Models;

namespace DistribuidoraLosAndes.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Rol> Roles { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<PedidoDetalle> PedidoDetalles { get; set; }

        // ESTE ES EL CÓDIGO NUEVO QUE OBLIGA AL SISTEMA A USAR MINÚSCULAS EXACTAS
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Rol>().ToTable("roles");
            modelBuilder.Entity<Usuario>().ToTable("usuarios");
            modelBuilder.Entity<Categoria>().ToTable("categorias");
            modelBuilder.Entity<Producto>().ToTable("productos");
            modelBuilder.Entity<Pedido>().ToTable("pedidos");
            modelBuilder.Entity<PedidoDetalle>().ToTable("pedidodetalles");
        }
    }
}