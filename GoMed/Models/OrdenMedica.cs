using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoMed.Models
{
    public class OrdenMedica
    {
        public int OrdenMedicaID { get; set; }
        public string? TipoOrden { get; set; }
        public string? Descripcion { get; set; }

        //clave foranea //
        public int ConsultaID { get; set; }
        public virtual Consulta Consulta { get; set; } = null!;
    }
}
