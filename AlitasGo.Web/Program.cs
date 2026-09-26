using AlitasGo.Domain.Handlers;
using AlitasGo.Domain.Interfaces;
using AlitasGo.Domain.Services;
using AlitasGo.Repository;
using AlitasGo.Repository.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Registrar SignalR
builder.Services.AddSignalR();

// Persistencia (EF Core / SQL Server)
builder.Services.AddDbContext<AlitasGoDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IPedidoRepository, PedidoRepository>();
builder.Services.AddScoped<IProductoRepository, ProductoRepository>();

// Agregar aquí - Nivel 2
builder.Services.AddScoped<IReceptionistHandler, ReceptionistHandler>();
builder.Services.AddScoped<IValidadorPedido, ValidadorPedido>();
builder.Services.AddScoped<IPedidoService, PedidoService>();

var app = builder.Build();

// Aplica migraciones pendientes y siembra datos mínimos (Mesas, Categorías, Productos)
// para que siempre haya datos válidos contra los cuales registrar pedidos.
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AlitasGoDbContext>();
    await AlitasGo.Repository.DbSeeder.SeedAsync(dbContext);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Pedidos/Index");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Pedidos}/{action=Index}/{id?}")
    .WithStaticAssets();

// Endpoint para la conexión en tiempo real de Cocina
app.MapHub<AlitasGo.Web.Hubs.CocinaHub>("/hubs/cocina");

app.Run();
