using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoMed.Models
{
    public enum EstadoCita
    {
        Pendiente,    // La cita ha sido creada pero no confirmada
        Confirmada,  // La cita ha sido confirmada por el paciente o el médico
        Atendida,    // La cita ha sido atendida por el médico
        Cancelada,   // La cita ha sido cancelada por el paciente o el médico
        Ausente      // El paciente no se presentó a la cita
    }
}
