using GoMed.Data;
using GoMed.Models;
using GoMed.Utils;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GoMed
{
    public partial class CargarPacienteWindow : UserControl
    {
        // Variable para saber si estamos editando (si es mayor a 0, estamos editando)
        private int _pacienteIdAEditar = 0;

        // 1. CONSTRUCTOR NORMAL (Para crear un paciente nuevo)
        public CargarPacienteWindow()
        {
            InitializeComponent();
            CargarLugaresDeNacimiento();
        }

        // 2. CONSTRUCTOR DE EDICIÓN (Se llama desde el botón "Editar")
        public CargarPacienteWindow(int pacienteId) : this()
        {
            _pacienteIdAEditar = pacienteId;
            lblTituloVista.Text = "Editar Información del Paciente";
            btnGuardar.Content = "💾 Actualizar Paciente";

            CargarDatosDelPaciente();
        }

        private void CargarDatosDelPaciente()
        {
            using (var context = new GoMed.Data.AppContext())
            {
                var p = context.Pacientes.Find(_pacienteIdAEditar);
                if (p != null)
                {
                    txtNombre.Text = p.Nombre;
                    txtApellido.Text = p.Apellido;
                    txtDni.Text = p.Dni;
                    dpFechaNacimiento.SelectedDate = p.FechaNacimiento;
                    cmbLugarNacimiento.Text = p.LugarNacimiento;
                    txtDireccion.Text = p.Direccion;
                    txtTelefono.Text = p.Telefono;
                    txtEmail.Text = p.Email;
                    txtOcupacion.Text = p.Ocupacion;

                    if (p.Genero == "Masculino") rbMasculino.IsChecked = true;
                    else if (p.Genero == "Femenino") rbFemenino.IsChecked = true;
                    else if (p.Genero == "Otro") rbOtro.IsChecked = true;

                    if (p.EstadoCivil == "Soltero/a") rbSoltero.IsChecked = true;
                    else if (p.EstadoCivil == "Casado/a") rbCasado.IsChecked = true;
                    else if (p.EstadoCivil == "Divorciado/a") rbDivorciado.IsChecked = true;
                    else if (p.EstadoCivil == "Viudo/a") rbViudo.IsChecked = true;
                }
            }
        }

        private void CargarLugaresDeNacimiento()
        {
            try
            {
                using (var context = new GoMed.Data.AppContext())
                {
                    var lugares = context.Pacientes.Select(p => p.LugarNacimiento).Distinct().ToList();
                    cmbLugarNacimiento.ItemsSource = lugares;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar lugares: {ex.Message}");
            }
        }

        private void DpFechaNacimiento_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dpFechaNacimiento.SelectedDate.HasValue)
            {
                var hoy = DateTime.Today;
                int edad = hoy.Year - dpFechaNacimiento.SelectedDate.Value.Year;
                if (dpFechaNacimiento.SelectedDate.Value.Date > hoy.AddYears(-edad)) edad--;
                lblEdad.Text = $"Edad: {edad} años";
            }
            else
            {
                lblEdad.Text = "-- años";
            }
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text) || string.IsNullOrWhiteSpace(txtApellido.Text) || string.IsNullOrWhiteSpace(txtDni.Text))
            {
                MessageBox.Show("Por favor, complete al menos los campos de Nombre, Apellido y DNI.", "Datos Incompletos", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var context = new GoMed.Data.AppContext())
                {
                    if (_pacienteIdAEditar == 0)
                    {
                        var nuevoPaciente = new Paciente
                        {
                            Nombre = txtNombre.Text,
                            Apellido = txtApellido.Text,
                            Dni = txtDni.Text,
                            FechaNacimiento = dpFechaNacimiento.SelectedDate ?? DateTime.Now,
                            LugarNacimiento = cmbLugarNacimiento.Text,
                            Direccion = txtDireccion.Text,
                            Telefono = txtTelefono.Text,
                            Email = txtEmail.Text,
                            Genero = ObtenerGeneroSeleccionado(),
                            EstadoCivil = ObtenerEstadoCivilSeleccionado(),
                            Ocupacion = txtOcupacion.Text,
                            MedicoID = SesionUsuario.MedicoID,
                            FichaClinica = new FichaClinica { AntecedentesPersonales = "", AntecedentesFamiliares = "", Notas = "", ExamenFisico = "" }
                        };
                        context.Pacientes.Add(nuevoPaciente);
                        MessageBox.Show("Paciente guardado exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        var pacienteExistente = context.Pacientes.Find(_pacienteIdAEditar);
                        if (pacienteExistente != null)
                        {
                            pacienteExistente.Nombre = txtNombre.Text;
                            pacienteExistente.Apellido = txtApellido.Text;
                            pacienteExistente.Dni = txtDni.Text;
                            pacienteExistente.FechaNacimiento = dpFechaNacimiento.SelectedDate ?? DateTime.Now;
                            pacienteExistente.LugarNacimiento = cmbLugarNacimiento.Text;
                            pacienteExistente.Direccion = txtDireccion.Text;
                            pacienteExistente.Telefono = txtTelefono.Text;
                            pacienteExistente.Email = txtEmail.Text;
                            pacienteExistente.Genero = ObtenerGeneroSeleccionado();
                            pacienteExistente.EstadoCivil = ObtenerEstadoCivilSeleccionado();
                            pacienteExistente.Ocupacion = txtOcupacion.Text;

                            context.Pacientes.Update(pacienteExistente);
                            MessageBox.Show("Datos del paciente actualizados exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }

                    context.SaveChanges();
                }

                // Si venimos de editar desde el listado, volvemos al listado y lo refrescamos; si fue alta directa, vamos al listado de pacientes
                if (_pacienteIdAEditar > 0)
                {
                    MainWindow.Instance?.Volver();
                }
                else
                {
                    MainWindow.Instance?.BtnVisualizarPacientes_Click(this, new RoutedEventArgs());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error: {ex.Message}", "Error de Base de Datos", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string ObtenerGeneroSeleccionado()
        {
            if (rbMasculino.IsChecked == true) return "Masculino";
            if (rbFemenino.IsChecked == true) return "Femenino";
            if (rbOtro.IsChecked == true) return "Otro";
            return "No especificado";
        }

        private string ObtenerEstadoCivilSeleccionado()
        {
            if (rbSoltero.IsChecked == true) return "Soltero/a";
            if (rbCasado.IsChecked == true) return "Casado/a";
            if (rbDivorciado.IsChecked == true) return "Divorciado/a";
            if (rbViudo.IsChecked == true) return "Viudo/a";
            return "No especificado";
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e) => MainWindow.Instance?.Volver();
        private void BtnVolver_Click(object sender, RoutedEventArgs e) => MainWindow.Instance?.Volver();
    }
}