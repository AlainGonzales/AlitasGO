using AlitasGo.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AlitasGo.Repository
{
    public class AlitasGoDbContext : DbContext
    {
        public AlitasGoDbContext(DbContextOptions<AlitasGoDbContext> options) : base(options) { }

        // ── DbSets ──────────────────────────────────────────────────────────────
        public DbSet<Mesa> Mesas { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Insumo> Insumos { get; set; }
        public DbSet<RecetaProducto> RecetasProducto { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<DetallePedido> DetallesPedido { get; set; }
        public DbSet<ComprobantePago> ComprobantesPago { get; set; }
        public DbSet<MovimientoKardex> MovimientosKardex { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ── Mesa ────────────────────────────────────────────────────────────
            modelBuilder.Entity<Mesa>(entity =>
            {
                entity.HasKey(m => m.MesaId);
                entity.Property(m => m.NumeroMesa).IsRequired();
                entity.Property(m => m.Estado).IsRequired().HasMaxLength(20);
                entity.Property(m => m.Ubicacion).HasMaxLength(50);
                // Índice único: no puede haber dos mesas con el mismo número
                entity.HasIndex(m => m.NumeroMesa).IsUnique();
            });

            // ── Categoria ───────────────────────────────────────────────────────
            modelBuilder.Entity<Categoria>(entity =>
            {
                entity.HasKey(c => c.CategoriaId);
                entity.Property(c => c.Nombre).IsRequired().HasMaxLength(60);
                entity.Property(c => c.Descripcion).HasMaxLength(200);
            });

            // ── Producto ────────────────────────────────────────────────────────
            modelBuilder.Entity<Producto>(entity =>
            {
                entity.HasKey(p => p.ProductoId);
                entity.Property(p => p.Codigo).IsRequired().HasMaxLength(20);
                entity.Property(p => p.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(p => p.Descripcion).HasMaxLength(300);
                entity.Property(p => p.PrecioUnitario).HasColumnType("decimal(10,2)");
                entity.HasIndex(p => p.Codigo).IsUnique();

                // N:1 → Categoria (con restricción: no eliminar categoría con productos)
                entity.HasOne(p => p.Categoria)
                      .WithMany(c => c.Productos)
                      .HasForeignKey(p => p.CategoriaId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ── Insumo ──────────────────────────────────────────────────────────
            modelBuilder.Entity<Insumo>(entity =>
            {
                entity.HasKey(i => i.InsumoId);
                entity.Property(i => i.CodigoInsumo).IsRequired().HasMaxLength(30);
                entity.Property(i => i.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(i => i.UnidadMedida).IsRequired().HasMaxLength(20);
                entity.Property(i => i.StockActual).HasColumnType("decimal(12,4)");
                entity.Property(i => i.StockMinimo).HasColumnType("decimal(12,4)");
                entity.Property(i => i.CostoUnitario).HasColumnType("decimal(10,4)");
                entity.HasIndex(i => i.CodigoInsumo).IsUnique();
                // EsStockCritico es una propiedad calculada; se ignora en la BD
                entity.Ignore(i => i.EsStockCritico);
            });

            // ── RecetaProducto  (tabla pivote N:M Producto ↔ Insumo) ────────────
            modelBuilder.Entity<RecetaProducto>(entity =>
            {
                entity.HasKey(r => r.RecetaId);
                entity.Property(r => r.CantidadRequerida).HasColumnType("decimal(12,4)");

                // Índice único compuesto: una receta solo define un insumo una vez por producto
                entity.HasIndex(r => new { r.ProductoId, r.InsumoId }).IsUnique();

                entity.HasOne(r => r.Producto)
                      .WithMany(p => p.Recetas)
                      .HasForeignKey(r => r.ProductoId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(r => r.Insumo)
                      .WithMany(i => i.Recetas)
                      .HasForeignKey(r => r.InsumoId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ── Pedido ──────────────────────────────────────────────────────────
            modelBuilder.Entity<Pedido>(entity =>
            {
                entity.HasKey(p => p.PedidoId);
                entity.Property(p => p.CodigoPedido).IsRequired().HasMaxLength(30);
                entity.Property(p => p.UsuarioId).IsRequired().HasMaxLength(50);
                entity.Property(p => p.Estado).IsRequired().HasMaxLength(20);
                entity.Property(p => p.SubTotal).HasColumnType("decimal(10,2)");
                entity.Property(p => p.Igv).HasColumnType("decimal(10,2)");
                entity.Property(p => p.TotalPagar).HasColumnType("decimal(10,2)");
                entity.Property(p => p.Observaciones).HasMaxLength(300);
                entity.HasIndex(p => p.CodigoPedido).IsUnique();

                // N:1 → Mesa
                entity.HasOne(p => p.Mesa)
                      .WithMany(m => m.Pedidos)
                      .HasForeignKey(p => p.MesaId)
                      .OnDelete(DeleteBehavior.Restrict);

                // 1:1 → ComprobantePago (el comprobante depende del pedido)
                entity.HasOne(p => p.ComprobantePago)
                      .WithOne(c => c.Pedido)
                      .HasForeignKey<ComprobantePago>(c => c.PedidoId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ── DetallePedido ───────────────────────────────────────────────────
            modelBuilder.Entity<DetallePedido>(entity =>
            {
                entity.HasKey(d => d.DetallePedidoId);
                entity.Property(d => d.SaborSalsa).HasMaxLength(30);
                entity.Property(d => d.NotasCocina).HasMaxLength(200);
                entity.Property(d => d.PrecioUnitario).HasColumnType("decimal(10,2)");
                entity.Property(d => d.Importe).HasColumnType("decimal(10,2)");

                entity.HasOne(d => d.Pedido)
                      .WithMany(p => p.Detalles)
                      .HasForeignKey(d => d.PedidoId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Producto)
                      .WithMany(p => p.DetallesPedido)
                      .HasForeignKey(d => d.ProductoId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ── ComprobantePago ──────────────────────────────────────────────────
            modelBuilder.Entity<ComprobantePago>(entity =>
            {
                entity.HasKey(c => c.ComprobanteId);
                entity.Property(c => c.TipoComprobante).IsRequired().HasMaxLength(20);
                entity.Property(c => c.Serie).IsRequired().HasMaxLength(10);
                entity.Property(c => c.MetodoPago).IsRequired().HasMaxLength(20);
                entity.Property(c => c.MontoRecibido).HasColumnType("decimal(10,2)");
                entity.Property(c => c.Vuelto).HasColumnType("decimal(10,2)");
            });

            // ── MovimientoKardex ────────────────────────────────────────────────
            modelBuilder.Entity<MovimientoKardex>(entity =>
            {
                entity.HasKey(k => k.MovimientoId);
                entity.Property(k => k.TipoMovimiento).IsRequired().HasMaxLength(30);
                entity.Property(k => k.Cantidad).HasColumnType("decimal(12,4)");
                entity.Property(k => k.StockPrevio).HasColumnType("decimal(12,4)");
                entity.Property(k => k.StockPosterior).HasColumnType("decimal(12,4)");
                entity.Property(k => k.Motivo).HasMaxLength(200);

                entity.HasOne(k => k.Insumo)
                      .WithMany(i => i.MovimientosKardex)
                      .HasForeignKey(k => k.InsumoId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Relación opcional: el movimiento puede o no venir de un pedido
                entity.HasOne(k => k.Pedido)
                      .WithMany()
                      .HasForeignKey(k => k.PedidoId)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // ── Seed Data ────────────────────────────────────────────────────────
            SeedData(modelBuilder);
        }

        private static void SeedData(ModelBuilder modelBuilder)
        {
            // ── Mesas (10 mesas del salón) ──────────────────────────────────────
            modelBuilder.Entity<Mesa>().HasData(
                new Mesa { MesaId = 1, NumeroMesa = 1, Capacidad = 4, Ubicacion = "Salón Principal", Estado = "Disponible", FechaActualizacion = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Mesa { MesaId = 2, NumeroMesa = 2, Capacidad = 4, Ubicacion = "Salón Principal", Estado = "Disponible", FechaActualizacion = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Mesa { MesaId = 3, NumeroMesa = 3, Capacidad = 4, Ubicacion = "Salón Principal", Estado = "Disponible", FechaActualizacion = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Mesa { MesaId = 4, NumeroMesa = 4, Capacidad = 6, Ubicacion = "Salón Principal", Estado = "Disponible", FechaActualizacion = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Mesa { MesaId = 5, NumeroMesa = 5, Capacidad = 6, Ubicacion = "Salón Principal", Estado = "Disponible", FechaActualizacion = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Mesa { MesaId = 6, NumeroMesa = 6, Capacidad = 2, Ubicacion = "Bar", Estado = "Disponible", FechaActualizacion = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Mesa { MesaId = 7, NumeroMesa = 7, Capacidad = 2, Ubicacion = "Bar", Estado = "Disponible", FechaActualizacion = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Mesa { MesaId = 8, NumeroMesa = 8, Capacidad = 8, Ubicacion = "Terraza", Estado = "Disponible", FechaActualizacion = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Mesa { MesaId = 9, NumeroMesa = 9, Capacidad = 8, Ubicacion = "Terraza", Estado = "Disponible", FechaActualizacion = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Mesa { MesaId = 10, NumeroMesa = 10, Capacidad = 4, Ubicacion = "Terraza", Estado = "Disponible", FechaActualizacion = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
            );

            // ── Categorías ──────────────────────────────────────────────────────
            modelBuilder.Entity<Categoria>().HasData(
                new Categoria { CategoriaId = 1, Nombre = "Alitas", Descripcion = "Alitas de pollo en diferentes presentaciones", Activo = true },
                new Categoria { CategoriaId = 2, Nombre = "Bebidas", Descripcion = "Gaseosas, cervezas y jugos", Activo = true },
                new Categoria { CategoriaId = 3, Nombre = "Acompañamientos", Descripcion = "Papas fritas, yucas y ensaladas", Activo = true }
            );

            // ── Productos ───────────────────────────────────────────────────────
            modelBuilder.Entity<Producto>().HasData(
                new Producto { ProductoId = 1, CategoriaId = 1, Codigo = "ALT-06", Nombre = "Alitas x6", Descripcion = "6 alitas a la parrilla con salsa a elegir", PrecioUnitario = 22.00m, RequiereSabor = true, Activo = true },
                new Producto { ProductoId = 2, CategoriaId = 1, Codigo = "ALT-12", Nombre = "Alitas x12", Descripcion = "12 alitas a la parrilla con salsa a elegir", PrecioUnitario = 39.00m, RequiereSabor = true, Activo = true },
                new Producto { ProductoId = 3, CategoriaId = 1, Codigo = "ALT-20", Nombre = "Alitas x20", Descripcion = "20 alitas a la parrilla con salsa a elegir", PrecioUnitario = 60.00m, RequiereSabor = true, Activo = true },
                new Producto { ProductoId = 4, CategoriaId = 3, Codigo = "PAP-FRI", Nombre = "Papas Fritas", Descripcion = "Porción de papas fritas crujientes", PrecioUnitario = 10.00m, RequiereSabor = false, Activo = true },
                new Producto { ProductoId = 5, CategoriaId = 2, Codigo = "BEB-GAS", Nombre = "Gaseosa 500ml", Descripcion = "Gaseosa personal 500ml", PrecioUnitario = 5.00m, RequiereSabor = false, Activo = true },
                new Producto { ProductoId = 6, CategoriaId = 2, Codigo = "BEB-CER", Nombre = "Cerveza 620ml", Descripcion = "Cerveza rubia 620ml", PrecioUnitario = 9.00m, RequiereSabor = false, Activo = true }
            );

            // ── Insumos ─────────────────────────────────────────────────────────
            modelBuilder.Entity<Insumo>().HasData(
                new Insumo { InsumoId = 1, CodigoInsumo = "INS-ALITA", Nombre = "Alita de pollo", UnidadMedida = "Unidades", StockActual = 200.00m, StockMinimo = 30.00m, CostoUnitario = 1.5000m },
                new Insumo { InsumoId = 2, CodigoInsumo = "INS-SAL-BBQ", Nombre = "Salsa BBQ", UnidadMedida = "Mililitros", StockActual = 5000.00m, StockMinimo = 500.00m, CostoUnitario = 0.0200m },
                new Insumo { InsumoId = 3, CodigoInsumo = "INS-SAL-BUF", Nombre = "Salsa Buffalo", UnidadMedida = "Mililitros", StockActual = 4000.00m, StockMinimo = 500.00m, CostoUnitario = 0.0250m },
                new Insumo { InsumoId = 4, CodigoInsumo = "INS-SAL-ACE", Nombre = "Salsa Acevichada", UnidadMedida = "Mililitros", StockActual = 3000.00m, StockMinimo = 400.00m, CostoUnitario = 0.0300m },
                new Insumo { InsumoId = 5, CodigoInsumo = "INS-PAPA", Nombre = "Papa", UnidadMedida = "Gramos", StockActual = 10000.00m, StockMinimo = 2000.00m, CostoUnitario = 0.0030m }
            );

            // ── Recetas (N:M Producto ↔ Insumo) ────────────────────────────────
            // ALT-06 (6 alitas): 6 alitas + 60ml de salsa (BBQ por defecto como base)
            // ALT-12 (12 alitas): 12 alitas + 80ml de salsa
            // ALT-20 (20 alitas): 20 alitas + 120ml de salsa
            // PAP-FRI: 200g de papa
            modelBuilder.Entity<RecetaProducto>().HasData(
                // Alitas x6
                new RecetaProducto { RecetaId = 1, ProductoId = 1, InsumoId = 1, CantidadRequerida = 6.0000m },
                new RecetaProducto { RecetaId = 2, ProductoId = 1, InsumoId = 2, CantidadRequerida = 60.0000m },
                // Alitas x12
                new RecetaProducto { RecetaId = 3, ProductoId = 2, InsumoId = 1, CantidadRequerida = 12.0000m },
                new RecetaProducto { RecetaId = 4, ProductoId = 2, InsumoId = 2, CantidadRequerida = 80.0000m },
                // Alitas x20
                new RecetaProducto { RecetaId = 5, ProductoId = 3, InsumoId = 1, CantidadRequerida = 20.0000m },
                new RecetaProducto { RecetaId = 6, ProductoId = 3, InsumoId = 2, CantidadRequerida = 120.0000m },
                // Papas fritas
                new RecetaProducto { RecetaId = 7, ProductoId = 4, InsumoId = 5, CantidadRequerida = 200.0000m }
            );
        }
    }
}
