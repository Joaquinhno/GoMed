using GoMed.Utils;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GoMed
{
    public partial class VerFichaPacienteWindow : UserControl
    {
        public VerFichaPacienteWindow()
        {
            InitializeComponent();
            CargarListaPacientes();
        }

        // Sobrecarga para cuando vienes desde otra vista con un ID específico
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
                    .Select(p => new
                    {
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
                        txtPers.Text = string.IsNullOrWhiteSpace(paciente.FichaClinica.AntecedentesPersonales) ? "Sin registros." : paciente.FichaClinica.AntecedentesPersonales;
                        txtFam.Text = string.IsNullOrWhiteSpace(paciente.FichaClinica.AntecedentesFamiliares) ? "Sin registros." : paciente.FichaClinica.AntecedentesFamiliares;
                        txtExamen.Text = string.IsNullOrWhiteSpace(paciente.FichaClinica.ExamenFisico) ? "No realizado." : paciente.FichaClinica.ExamenFisico;
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
            txtPers.Text = "";
            txtFam.Text = "";
            txtExamen.Text = "";
            txtNotas.Text = "";
        }

        private void BtnVolver_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance?.Volver();
        }
    }
}