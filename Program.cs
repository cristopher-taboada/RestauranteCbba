using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RestauranteCbba.Data;

var builder = WebApplication.CreateBuilder(args);

// Configurar la conexión a la base de datos (Neon/PostgreSQL)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configurar Identity (Login/Registro) con contraseñas simples
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;

    // ==== CONFIGURACIÓN DE CONTRASEÑA SIMPLE ====
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 3; // Mínimo 3 caracteres
})
.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Proteger todas las páginas de Identity
app.MapRazorPages().RequireAuthorization();

// Mapear rutas de los controladores
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();