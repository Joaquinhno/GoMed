using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoMed.Models
{
    public class Paciente
    {
        public int PacienteID { get; set; }
        public string? Nombre { get; set; }
        public string? Apellido { get; set; }

        public string Dni { get; set; } = null!;
        public DateTime FechaNacimiento { get; set; }
        public string? Genero { get; set; }
        public string? Direccion { get; set; }
        public string? Telefono { get; set; }
        public string? Email { get; set; }
        public string? LugarNacimiento { get; set; }

        public string? EstadoCivil { get; set; }
        public string? Ocupacion { get; set; }

        public int UsuarioID { get; set; }

        public string NombreCompleto => $"{Nombre} {Apellido} (DNI: {Dni})";

        public virtual FichaClinica FichaClinica { get; set; } = null!;

        //relacion con el medico//
        public int MedicoID { get; set; }
        public virtual Medico Medico { get; set; } = null!;
        public virtual ICollection<Consulta> Consultas { get; set; } = new List<Consulta>();



    }
}
