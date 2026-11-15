using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoMed.Models
{
    public class Consulta
    {
        public int ConsultaID { get; set; }

        public int? CitaID { get; set; }
        public DateTime Fecha { get; set; }
        public string? MotivoConsulta { get; set; }

        public string? RutaFoto { get; set; }

        public string? Diagnostico { get; set; }

        public string? PlanTratamiento { get; set; }

        // Relación: Una consulta puede tener muchas fotos
        public virtual ICollection<ConsultaFoto> Fotos { get; set; }

        public Consulta()
        {
            // Inicializa la colección para evitar errores
            Fotos = new HashSet<ConsultaFoto>();
        }

        //CLAVES FORANEAS//

        public int PacienteID { get; set; }
        public virtual Paciente Paciente { get; set; } = null!;

        public virtual Cita? Cita { get; set; }

        public int MedicoID { get; set; } // Cambiado de UsuarioId a MedicoID
        public virtual Medico Medico { get; set; } = null!; // Cambiado de Usuario a Medico

        public virtual ICollection<Prescripcion> Prescripciones { get; set; } = new List<Prescripcion>();
        public virtual ICollection<OrdenMedica> OrdenesMedicas { get; set; } = new List<OrdenMedica>();
    }
}
