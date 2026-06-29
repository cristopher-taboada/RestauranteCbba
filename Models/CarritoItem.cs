namespace RestauranteCbba.Models
{
    public class CarritoItem
    {
        public int Id { get; set; }
        public string? Nombre { get; set; }
        public decimal Precio { get; set; }
        public int Cantidad { get; set; } = 1;
        public decimal Subtotal => Precio * Cantidad;
    }
}