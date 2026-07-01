using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestauranteCbba.Models
{
    [Table("Productos")]
    public class Producto
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(200)]
        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "La cantidad no puede ser negativa")]
        public int Cantidad { get; set; }  // Stock

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
        [DataType(DataType.Currency)]
        public decimal Precio { get; set; }

        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

        // Para el menú del restaurante
        public bool Activo { get; set; } = true;
        public string Categoria { get; set; } = string.Empty;

        // Para compatibilidad con el carrito
        [NotMapped]
        public int Stock
        {
            get => Cantidad;
            set => Cantidad = value;
        }
    }
}