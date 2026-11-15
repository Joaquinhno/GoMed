using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoMed.Models
{
    public class FichaClinica
    {
        public int FichaClinicaID { get; set; }

        public string? AntecedentesPersonales { get; set; }

        public string? AntecedentesFamiliares { get; set; }
        public string? Notas { get; set; }

        public string? ExamenFisico { get; set; }

        //clave foranea para relacion 1 a 1  con paciente//
        public int PacienteID { get; set; }
        public virtual Paciente Paciente { get; set; } = null!;
    }
}
