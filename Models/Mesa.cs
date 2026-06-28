namespace RestauranteCbba.Models
{
    public class Mesa
    {
        public int Id { get; set; }
        public string? NumeroMesa { get; set; }
        public int Capacidad { get; set; }
        public string? Ubicacion { get; set; }
        public string? Estado { get; set; } = "Disponible";
    }
}