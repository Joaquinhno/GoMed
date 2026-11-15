using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoMed.Models
{
    public class ConsultaFoto
    {
        [Key]
        public int ConsultaFotoID { get; set; }

        public string RutaFoto { get; set; } = null!;

        //CLAVE FORANEA//

        public int ConsultaId { get; set; }

        [ForeignKey("ConsultaId")]
        public virtual Consulta Consulta { get; set; } = null!;
    }
}
