using GoMed.Utils;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GoMed
{
    public partial class VerFichaPacienteWindow : Window
    {
        public VerFichaPacienteWindow()
        {
            InitializeComponent();
            CargarListaPacientes();
        }

        // Sobrecarga para cuando vienes desde otra ventana con un ID específico
        public VerFichaPacienteWindow(int pacienteId) : this()
        {
            cmbPaciente.SelectedValue = pacienteId;
        }

        private void CargarListaPacientes()
        {
            using (var context = new GoMed.Data.AppContext())
            {
                var pacientes = context.Pacientes
                    .Where(p => p.MedicoID == SesionUsuario.MedicoID)
                    .Select(p => new {
                        p.PacienteID,
                        NombreCompleto = $"{p.Nombre} {p.Apellido} (DNI: {p.Dni})"
                    }).ToList();

                cmbPaciente.ItemsSource = pacientes;
            }
        }

        private void CmbPaciente_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbPaciente.SelectedValue == null) return;

            int idSeleccionado = (int)cmbPaciente.SelectedValue;
            CargarInformacionFicha(idSeleccionado);
        }

        private void CargarInformacionFicha(int id)
        {
            using (var context = new GoMed.Data.AppContext())
            {
                var paciente = context.Pacientes
                    .Include(p => p.FichaClinica)
                    .FirstOrDefault(p => p.PacienteID == id);

                if (paciente != null)
                {
                    txtNombrePaciente.Text = $"{paciente.Nombre} {paciente.Apellido}";
                    txtDniPaciente.Text = $"DNI: {paciente.Dni}";

                    if (paciente.FichaClinica != null)
                    {
                        txtPers.Text = paciente.FichaClinica.AntecedentesPersonales ?? "Sin registros.";
                        txtFam.Text = paciente.FichaClinica.AntecedentesFamiliares ?? "Sin registros.";
                        txtExamen.Text = paciente.FichaClinica.ExamenFisico ?? "No realizado.";
                        txtNotas.Text = paciente.FichaClinica.Notas ?? "";
                    }
                    else
                    {
                        LimpiarTextos();
                        MessageBox.Show("El paciente seleccionado no tiene una ficha clínica creada.");
                    }
                }
            }
        }

        private void LimpiarTextos()
        {
            txtPers.Text = ""; txtFam.Text = ""; txtExamen.Text = ""; txtNotas.Text = "";
        }

        private void BtnVolver_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
