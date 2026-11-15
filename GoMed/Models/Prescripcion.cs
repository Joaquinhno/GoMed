using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoMed.Models
{
    public class Prescripcion
    {
        public int PrescripcionId { get; set; }
        public string? Medicamento { get; set; }
        public string? Dosis { get; set; }
        public string? Instrucciones { get; set; }


        //clave foranea para relacion con consulta//
        public int ConsultaID { get; set; }
        public virtual Consulta Consulta { get; set; } = null!;
    }
}
