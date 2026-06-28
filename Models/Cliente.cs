using System;

namespace RestauranteCbba.Models
{
    public class Cliente
    {
        public int Id { get; set; }
        public string? NombreCompleto { get; set; }
        public string? Telefono { get; set; }
        public string? Correo { get; set; }
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
    }
}