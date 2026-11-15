using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoMed.Models
{
    public class Medico
    {
        [Key]
        public int MedicoID { get; set; }
        public string NombreUsuario { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;

        // Datos Personales y Profesionales
        public string Nombre { get; set; } = null!;
        public string Apellido { get; set; } = null!;
        public string Especialidad { get; set; } = null!;
        public string Matricula { get; set; } = null!;
        public string? RutaFirmaImagen { get; set; }

        // Datos del Consultorio (Integrados al médico para uso individual)
        public string? NombreConsultorio { get; set; }
        public string? DireccionConsultorio { get; set; }
        public string? TelefonoConsultorio { get; set; }
        public string? EmailConsultorio { get; set; }
        public string? RutaLogo { get; set; }
        public string? CUIT { get; set; }

        // Relaciones
        public virtual ICollection<Paciente> Pacientes { get; set; } = new List<Paciente>();
        public virtual ICollection<Consulta> Consultas { get; set; } = new List<Consulta>();
        public virtual ICollection<Cita> Citas { get; set; } = new List<Cita>();
    }
}