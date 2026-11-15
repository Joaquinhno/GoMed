using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoMed.Models
{
    public class Cita
    {
        [Key]
        public int CitaID { get; set; }

        [Required]
        public DateTime FechaHora { get; set; }

        public int DuracionMinutos { get; set; } = 20;

        [MaxLength(250)]
        public string? Motivo { get; set; }

        [Required]
        public EstadoCita Estado { get; set; } = EstadoCita.Pendiente;

        public string? Notas { get; set; }


        // --- RELACIONES ---

        [Required]
        public int PacienteID { get; set; }
        public virtual Paciente Paciente { get; set; } = null!;

        [Required]
        public int MedicoID { get; set; }
        public virtual Medico Medico { get; set; } = null!;

  
    }
}