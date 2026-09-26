using AlitasGo.Domain.Interfaces;
using AlitasGo.Repository;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ── Base de Datos (EF Core + SQL Server / LocalDB) ───────────────────────────
builder.Services.AddDbContext<AlitasGoDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("AlitasGoDb")));

// ── Repositorios ─────────────────────────────────────────────────────────────
builder.Services.AddScoped<IPedidoRepository, PedidoRepository>();
builder.Services.AddScoped<IInsumoRepository, InsumoRepository>();

// Add services to the container.
builder.Services.AddControllersWithViews();

// Registrar SignalR
builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Endpoint para la conexión en tiempo real de Cocina
app.MapHub<AlitasGo.Web.Hubs.CocinaHub>("/hubs/cocina");

app.Run();
