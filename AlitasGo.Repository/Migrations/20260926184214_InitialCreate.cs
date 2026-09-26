using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AlitasGo.Repository.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categorias",
                columns: table => new
                {
                    CategoriaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categorias", x => x.CategoriaId);
                });

            migrationBuilder.CreateTable(
                name: "Insumos",
                columns: table => new
                {
                    InsumoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodigoInsumo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UnidadMedida = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    StockActual = table.Column<decimal>(type: "decimal(12,4)", nullable: false),
                    StockMinimo = table.Column<decimal>(type: "decimal(12,4)", nullable: false),
                    CostoUnitario = table.Column<decimal>(type: "decimal(10,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Insumos", x => x.InsumoId);
                });

            migrationBuilder.CreateTable(
                name: "Mesas",
                columns: table => new
                {
                    MesaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NumeroMesa = table.Column<int>(type: "int", nullable: false),
                    Capacidad = table.Column<int>(type: "int", nullable: false),
                    Ubicacion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mesas", x => x.MesaId);
                });

            migrationBuilder.CreateTable(
                name: "Productos",
                columns: table => new
                {
                    ProductoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoriaId = table.Column<int>(type: "int", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    RequiereSabor = table.Column<bool>(type: "bit", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Productos", x => x.ProductoId);
                    table.ForeignKey(
                        name: "FK_Productos_Categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categorias",
                        principalColumn: "CategoriaId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Pedidos",
                columns: table => new
                {
                    PedidoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodigoPedido = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    MesaId = table.Column<int>(type: "int", nullable: false),
                    UsuarioId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FechaHoraRegistro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SubTotal = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Igv = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    TotalPagar = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pedidos", x => x.PedidoId);
                    table.ForeignKey(
                        name: "FK_Pedidos_Mesas_MesaId",
                        column: x => x.MesaId,
                        principalTable: "Mesas",
                        principalColumn: "MesaId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RecetasProducto",
                columns: table => new
                {
                    RecetaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductoId = table.Column<int>(type: "int", nullable: false),
                    InsumoId = table.Column<int>(type: "int", nullable: false),
                    CantidadRequerida = table.Column<decimal>(type: "decimal(12,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecetasProducto", x => x.RecetaId);
                    table.ForeignKey(
                        name: "FK_RecetasProducto_Insumos_InsumoId",
                        column: x => x.InsumoId,
                        principalTable: "Insumos",
                        principalColumn: "InsumoId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecetasProducto_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "ProductoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ComprobantesPago",
                columns: table => new
                {
                    ComprobanteId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PedidoId = table.Column<int>(type: "int", nullable: false),
                    TipoComprobante = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Serie = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    NumeroCorrelativo = table.Column<int>(type: "int", nullable: false),
                    MetodoPago = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MontoRecibido = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Vuelto = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    FechaEmision = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComprobantesPago", x => x.ComprobanteId);
                    table.ForeignKey(
                        name: "FK_ComprobantesPago_Pedidos_PedidoId",
                        column: x => x.PedidoId,
                        principalTable: "Pedidos",
                        principalColumn: "PedidoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DetallesPedido",
                columns: table => new
                {
                    DetallePedidoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PedidoId = table.Column<int>(type: "int", nullable: false),
                    ProductoId = table.Column<int>(type: "int", nullable: false),
                    SaborSalsa = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Importe = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    NotasCocina = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesPedido", x => x.DetallePedidoId);
                    table.ForeignKey(
                        name: "FK_DetallesPedido_Pedidos_PedidoId",
                        column: x => x.PedidoId,
                        principalTable: "Pedidos",
                        principalColumn: "PedidoId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DetallesPedido_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "ProductoId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MovimientosKardex",
                columns: table => new
                {
                    MovimientoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InsumoId = table.Column<int>(type: "int", nullable: false),
                    PedidoId = table.Column<int>(type: "int", nullable: true),
                    FechaHora = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TipoMovimiento = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Cantidad = table.Column<decimal>(type: "decimal(12,4)", nullable: false),
                    StockPrevio = table.Column<decimal>(type: "decimal(12,4)", nullable: false),
                    StockPosterior = table.Column<decimal>(type: "decimal(12,4)", nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimientosKardex", x => x.MovimientoId);
                    table.ForeignKey(
                        name: "FK_MovimientosKardex_Insumos_InsumoId",
                        column: x => x.InsumoId,
                        principalTable: "Insumos",
                        principalColumn: "InsumoId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovimientosKardex_Pedidos_PedidoId",
                        column: x => x.PedidoId,
                        principalTable: "Pedidos",
                        principalColumn: "PedidoId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.InsertData(
                table: "Categorias",
                columns: new[] { "CategoriaId", "Activo", "Descripcion", "Nombre" },
                values: new object[,]
                {
                    { 1, true, "Alitas de pollo en diferentes presentaciones", "Alitas" },
                    { 2, true, "Gaseosas, cervezas y jugos", "Bebidas" },
                    { 3, true, "Papas fritas, yucas y ensaladas", "Acompañamientos" }
                });

            migrationBuilder.InsertData(
                table: "Insumos",
                columns: new[] { "InsumoId", "CodigoInsumo", "CostoUnitario", "Nombre", "StockActual", "StockMinimo", "UnidadMedida" },
                values: new object[,]
                {
                    { 1, "INS-ALITA", 1.5000m, "Alita de pollo", 200.00m, 30.00m, "Unidades" },
                    { 2, "INS-SAL-BBQ", 0.0200m, "Salsa BBQ", 5000.00m, 500.00m, "Mililitros" },
                    { 3, "INS-SAL-BUF", 0.0250m, "Salsa Buffalo", 4000.00m, 500.00m, "Mililitros" },
                    { 4, "INS-SAL-ACE", 0.0300m, "Salsa Acevichada", 3000.00m, 400.00m, "Mililitros" },
                    { 5, "INS-PAPA", 0.0030m, "Papa", 10000.00m, 2000.00m, "Gramos" }
                });

            migrationBuilder.InsertData(
                table: "Mesas",
                columns: new[] { "MesaId", "Capacidad", "Estado", "FechaActualizacion", "NumeroMesa", "Ubicacion" },
                values: new object[,]
                {
                    { 1, 4, "Disponible", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "Salón Principal" },
                    { 2, 4, "Disponible", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "Salón Principal" },
                    { 3, 4, "Disponible", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "Salón Principal" },
                    { 4, 6, "Disponible", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, "Salón Principal" },
                    { 5, 6, "Disponible", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 5, "Salón Principal" },
                    { 6, 2, "Disponible", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 6, "Bar" },
                    { 7, 2, "Disponible", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 7, "Bar" },
                    { 8, 8, "Disponible", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 8, "Terraza" },
                    { 9, 8, "Disponible", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 9, "Terraza" },
                    { 10, 4, "Disponible", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 10, "Terraza" }
                });

            migrationBuilder.InsertData(
                table: "Productos",
                columns: new[] { "ProductoId", "Activo", "CategoriaId", "Codigo", "Descripcion", "Nombre", "PrecioUnitario", "RequiereSabor" },
                values: new object[,]
                {
                    { 1, true, 1, "ALT-06", "6 alitas a la parrilla con salsa a elegir", "Alitas x6", 22.00m, true },
                    { 2, true, 1, "ALT-12", "12 alitas a la parrilla con salsa a elegir", "Alitas x12", 39.00m, true },
                    { 3, true, 1, "ALT-20", "20 alitas a la parrilla con salsa a elegir", "Alitas x20", 60.00m, true },
                    { 4, true, 3, "PAP-FRI", "Porción de papas fritas crujientes", "Papas Fritas", 10.00m, false },
                    { 5, true, 2, "BEB-GAS", "Gaseosa personal 500ml", "Gaseosa 500ml", 5.00m, false },
                    { 6, true, 2, "BEB-CER", "Cerveza rubia 620ml", "Cerveza 620ml", 9.00m, false }
                });

            migrationBuilder.InsertData(
                table: "RecetasProducto",
                columns: new[] { "RecetaId", "CantidadRequerida", "InsumoId", "ProductoId" },
                values: new object[,]
                {
                    { 1, 6.0000m, 1, 1 },
                    { 2, 60.0000m, 2, 1 },
                    { 3, 12.0000m, 1, 2 },
                    { 4, 80.0000m, 2, 2 },
                    { 5, 20.0000m, 1, 3 },
                    { 6, 120.0000m, 2, 3 },
                    { 7, 200.0000m, 5, 4 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ComprobantesPago_PedidoId",
                table: "ComprobantesPago",
                column: "PedidoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DetallesPedido_PedidoId",
                table: "DetallesPedido",
                column: "PedidoId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesPedido_ProductoId",
                table: "DetallesPedido",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_Insumos_CodigoInsumo",
                table: "Insumos",
                column: "CodigoInsumo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Mesas_NumeroMesa",
                table: "Mesas",
                column: "NumeroMesa",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosKardex_InsumoId",
                table: "MovimientosKardex",
                column: "InsumoId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosKardex_PedidoId",
                table: "MovimientosKardex",
                column: "PedidoId");

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_CodigoPedido",
                table: "Pedidos",
                column: "CodigoPedido",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_MesaId",
                table: "Pedidos",
                column: "MesaId");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_CategoriaId",
                table: "Productos",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_Codigo",
                table: "Productos",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecetasProducto_InsumoId",
                table: "RecetasProducto",
                column: "InsumoId");

            migrationBuilder.CreateIndex(
                name: "IX_RecetasProducto_ProductoId_InsumoId",
                table: "RecetasProducto",
                columns: new[] { "ProductoId", "InsumoId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ComprobantesPago");

            migrationBuilder.DropTable(
                name: "DetallesPedido");

            migrationBuilder.DropTable(
                name: "MovimientosKardex");

            migrationBuilder.DropTable(
                name: "RecetasProducto");

            migrationBuilder.DropTable(
                name: "Pedidos");

            migrationBuilder.DropTable(
                name: "Insumos");

            migrationBuilder.DropTable(
                name: "Productos");

            migrationBuilder.DropTable(
                name: "Mesas");

            migrationBuilder.DropTable(
                name: "Categorias");
        }
    }
}
