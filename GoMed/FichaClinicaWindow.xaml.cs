using GoMed.Data;
using GoMed.Models;
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
    /// <summary>
    /// Lógica de interacción para FichaClinicaWindow.xaml
    /// </summary>
    public partial class FichaClinicaWindow : Window
    {
        private FichaClinica? _fichaActual; // Almacena la ficha cargada

        public FichaClinicaWindow()
        {
            InitializeComponent();
            CargarPacientes();
            HabilitarFormulario(false); // Deshabilita los campos al inicio
        }

        // --- LÓGICA DE PACIENTES ---

        /// <summary>
        /// Carga la lista de pacientes en el ComboBox (código reutilizado)
        /// </summary>
        private void CargarPacientes()
        {
            try
            {
                using (var context = new GoMed.Data.AppContext())
                {
                    // 1. Obtenemos el ID del médico que está usando la app actualmente
                    int idMedicoLogueado = SesionUsuario.MedicoID;

                    // 2. FILTRAMOS la lista antes de convertirla en ToList()
                    var misPacientes = context.Pacientes
                        .Where(p => p.MedicoID == idMedicoLogueado) // <--- ESTA ES LA LÍNEA CLAVE
                        .Select(p => new
                        {
                            p.PacienteID,
                            p.FechaNacimiento,
                            // Creamos el texto que verá el médico en el buscador
                            NombreCompleto = $"{p.Nombre} {p.Apellido} (DNI: {p.Dni})"
                        })
                        .ToList();

                    // 3. Asignamos al ComboBox
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

        /// <summary>
        /// Se activa al seleccionar un paciente. Carga su ficha clínica.
        /// </summary>
        private void CmbPaciente_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbPaciente.SelectedItem == null)
            {
                LimpiarFormulario();
                HabilitarFormulario(false);
                return;
            }

            // Obtiene el ID del paciente seleccionado
            int pacienteId = (int)cmbPaciente.SelectedValue;

            // Carga la ficha
            CargarFichaClinica(pacienteId);
            HabilitarFormulario(true);
        }

        // --- LÓGICA DE FICHA CLÍNICA ---

        /// <summary>
        /// Busca la Ficha Clínica del paciente. Si no existe, crea una nueva.
        /// </summary>
        private void CargarFichaClinica(int pacienteId)
        {
            try
            {
                using (var context = new GoMed.Data.AppContext())
                {
                    // Busca la ficha usando el PacienteID.
                    _fichaActual = context.FichasClinicas
                                     .FirstOrDefault(f => f.PacienteID == pacienteId);

                    // Si no existe, crea una nueva ficha en memoria
                    if (_fichaActual == null)
                    {
                        _fichaActual = new FichaClinica
                        {
                            PacienteID = pacienteId
                            // Los campos de texto estarán vacíos
                        };
                    }
                }

                // Rellena el formulario con los datos de la ficha
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

        /// <summary>
        /// ESTE ES EL MÉTODO QUE TE FALTA
        /// Guarda los cambios de la ficha (ya sea una nueva o una existente).
        /// </summary>
        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (_fichaActual == null)
            {
                MessageBox.Show("Por favor, seleccione un paciente primero.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 1. Actualiza el objeto _fichaActual con los datos del formulario
            _fichaActual.AntecedentesPersonales = txtAntecedentesPersonales.Text;
            _fichaActual.AntecedentesFamiliares = txtAntecedentesFamiliares.Text;
            _fichaActual.ExamenFisico = txtExamenFisico.Text;
            _fichaActual.Notas = txtNotas.Text;

            // 2. Guarda en la Base de Datos
            try
            {
                using (var context = new GoMed.Data.AppContext())
                {
                    // Entity Framework es lo suficientemente inteligente para saber
                    // si _fichaActual es nueva (Add) o existente (Update).
                    // Usamos .Update() para ser explícitos.
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

        // --- MÉTODOS AUXILIARES ---

        private void HabilitarFormulario(bool habilitado)
        {
            // Habilita o deshabilita todos los campos de texto y el botón de guardar
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

        /// <summary>
        /// ESTE ES EL OTRO MÉTODO QUE TE FALTA
        /// </summary>
        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            // Si el usuario quiere cancelar, recargamos la ficha desde la BD
            // para deshacer los cambios no guardados.
            if (_fichaActual != null)
            {
                CargarFichaClinica(_fichaActual.PacienteID);
            }
        }

        private void BtnVolverInicio_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
