
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestauranteCbba.Models
{
    [Table("Pedidos")]
    public class Pedido
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UsuarioId { get; set; } = string.Empty;

        [Required]
        public string Direccion { get; set; } = string.Empty;

        [Required]
        public string MetodoPago { get; set; } = string.Empty;

        [Required]
        public decimal Total { get; set; }

        public DateTime FechaPedido { get; set; } = DateTime.UtcNow;

        public string? Estado { get; set; } = "Pendiente";
    }
}