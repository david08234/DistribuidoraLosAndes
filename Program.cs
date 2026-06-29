using Microsoft.EntityFrameworkCore;
using DistribuidoraLosAndes.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// ENSEÑARLE AL SISTEMA QUÉ ES EL APPLICATIONDBCONTEXT (Conectado a PostgreSQL/Supabase)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 1. Encender el motor de sesiones
builder.Services.AddSession(options => {
    options.IdleTimeout = TimeSpan.FromMinutes(30); // La sesión dura 30 minutos
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

// 2. Activar el uso de la memoria de sesión
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();