using Microsoft.AspNetCore.Identity;
using RestauranteCbba.Models;

namespace RestauranteCbba.Data
{
    public class DbInitializer
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            // Obtener todos los usuarios que no tienen cliente asociado
            var users = userManager.Users.ToList();

            foreach (var user in users)
            {
                // Verificar si el cliente ya existe
                var clienteExistente = context.Clientes.FirstOrDefault(c => c.Correo == user.Email);
                if (clienteExistente == null && user.Email != null)
                {
                    // Crear cliente automáticamente
                    var cliente = new Cliente
                    {
                        NombreCompleto = user.Email.Split('@')[0], // Usa el nombre antes del @
                        Telefono = "Sin teléfono",
                        Correo = user.Email,
                        FechaRegistro = DateTime.Now
                    };
                    context.Clientes.Add(cliente);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}