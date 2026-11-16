using GoMed.Data;
using GoMed.Models;
using GoMed.Utils;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GoMed
{
    public partial class FichaClinicaWindow : UserControl
    {
        private FichaClinica? _fichaActual;

        public FichaClinicaWindow()
        {
            InitializeComponent();
            CargarPacientes();
            HabilitarFormulario(false);
        }

        private void CargarPacientes()
        {
            try
            {
                using (var context = new GoMed.Data.AppContext())
                {
                    int idMedicoLogueado = SesionUsuario.MedicoID;

                    var misPacientes = context.Pacientes
                        .Where(p => p.MedicoID == idMedicoLogueado)
                        .Select(p => new
                        {
                            p.PacienteID,
                            p.FechaNacimiento,
                            NombreCompleto = $"{p.Nombre} {p.Apellido} (DNI: {p.Dni})"
                        })
                        .ToList();

                    cmbPaciente.ItemsSource = misPacientes;
                    cmbPaciente.DisplayMemberPath = "NombreCompleto";
                    cmbPaciente.SelectedValuePath = "PacienteID";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al filtrar pacientes: " + ex.Message);
            }
        }

        private void CmbPaciente_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbPaciente.SelectedItem == null || cmbPaciente.SelectedValue == null)
            {
                LimpiarFormulario();
                HabilitarFormulario(false);
                return;
            }

            int pacienteId = (int)cmbPaciente.SelectedValue;
            CargarFichaClinica(pacienteId);
            HabilitarFormulario(true);
        }

        private void CargarFichaClinica(int pacienteId)
        {
            try
            {
                using (var context = new GoMed.Data.AppContext())
                {
                    _fichaActual = context.FichasClinicas
                                          .FirstOrDefault(f => f.PacienteID == pacienteId);

                    if (_fichaActual == null)
                    {
                        _fichaActual = new FichaClinica
                        {
                            PacienteID = pacienteId
                        };
                    }
                }

                txtAntecedentesPersonales.Text = _fichaActual.AntecedentesPersonales;
                txtAntecedentesFamiliares.Text = _fichaActual.AntecedentesFamiliares;
                txtExamenFisico.Text = _fichaActual.ExamenFisico;
                txtNotas.Text = _fichaActual.Notas;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar la ficha clínica: {ex.Message}", "Error de Base de Datos", MessageBoxButton.OK, MessageBoxImage.Error);
                HabilitarFormulario(false);
            }
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (_fichaActual == null)
            {
                MessageBox.Show("Por favor, seleccione un paciente primero.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _fichaActual.AntecedentesPersonales = txtAntecedentesPersonales.Text;
            _fichaActual.AntecedentesFamiliares = txtAntecedentesFamiliares.Text;
            _fichaActual.ExamenFisico = txtExamenFisico.Text;
            _fichaActual.Notas = txtNotas.Text;

            try
            {
                using (var context = new GoMed.Data.AppContext())
                {
                    context.Update(_fichaActual);
                    context.SaveChanges();
                }

                MessageBox.Show("Ficha Clínica guardada exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar la ficha clínica: {ex.Message}", "Error de Base de Datos", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void HabilitarFormulario(bool habilitado)
        {
            txtAntecedentesPersonales.IsEnabled = habilitado;
            txtAntecedentesFamiliares.IsEnabled = habilitado;
            txtExamenFisico.IsEnabled = habilitado;
            txtNotas.IsEnabled = habilitado;
            btnGuardar.IsEnabled = habilitado;
        }

        private void LimpiarFormulario()
        {
            txtAntecedentesPersonales.Text = "";
            txtAntecedentesFamiliares.Text = "";
            txtExamenFisico.Text = "";
            txtNotas.Text = "";
            _fichaActual = null;
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            if (_fichaActual != null)
            {
                CargarFichaClinica(_fichaActual.PacienteID);
            }
        }

        private void BtnVolverInicio_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance?.Volver();
        }
    }
}