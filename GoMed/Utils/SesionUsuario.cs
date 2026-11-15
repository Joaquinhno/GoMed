using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoMed.Utils
{
    public static class SesionUsuario
    {
        // Aquí guardaremos el ID del médico que inició sesión
        public static int MedicoID { get; set; }
        public static string NombreCompleto { get; set; }
    }
}
