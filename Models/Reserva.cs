using System;
using System.ComponentModel.DataAnnotations;

namespace RestauranteCbba.Models
{
    public class Reserva
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }

        [Display(Name = "Mesa")]
        [Required(ErrorMessage = "Debes seleccionar una mesa")]
        public int MesaId { get; set; }

        [Display(Name = "Fecha")]
        [Required(ErrorMessage = "Debes seleccionar una fecha")]
        [DataType(DataType.Date)]
        public DateTime FechaReserva { get; set; }

        [Display(Name = "Hora")]
        [Required(ErrorMessage = "Debes seleccionar una hora")]
        [DataType(DataType.Time)]
        public TimeSpan HoraReserva { get; set; }

        [Display(Name = "Número de Personas")]
        [Required(ErrorMessage = "Debes indicar el número de personas")]
        [Range(1, 50, ErrorMessage = "El número de personas debe ser entre 1 y 50")]
        public int NumeroPersonas { get; set; }

        public string? Estado { get; set; } = "Pendiente";

        [Display(Name = "Observaciones")]
        public string? Observacion { get; set; }

        // ===== CAMBIO IMPORTANTE: DateTime.UtcNow =====
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        // Relaciones (Navigation Properties)
        public virtual Cliente? Cliente { get; set; }
        public virtual Mesa? Mesa { get; set; }
    }
}