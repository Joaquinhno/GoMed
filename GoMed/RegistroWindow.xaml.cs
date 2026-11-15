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
using GoMed.Data;
using GoMed.Models;

namespace GoMed
{
    public partial class RegistroWindow : Window
    {
        public RegistroWindow()
        {
            InitializeComponent();
            CargarEspecialidades(); // 1. Carga la lista al iniciar
        }

        /// <summary>
        /// Carga la lista de especialidades desde la clase global.
        /// </summary>
        private void CargarEspecialidades()
        {
            // Asigna la lista estática al ComboBox
            cmbEspecialidad.ItemsSource = ListasEstaticas.EspecialidadesMedicas;
        }

        /// <summary>
        /// Valida los datos y registra al nuevo usuario.
        /// </summary>
        private void BtnRegistrar_Click(object sender, RoutedEventArgs e)
        {
            // --- 1. OBTENER DATOS ---
            string nombre = txtNombre.Text;
            string apellido = txtApellido.Text;
            string email = txtEmail.Text;
            string password = txtPassword.Password;
            string confirmarPassword = txtConfirmarPassword.Password;

            // Aquí leemos el ComboBox. Si no se seleccionó nada, 'especialidad' será null
            string especialidad = (string)cmbEspecialidad.SelectedItem;

            // --- 2. VALIDACIÓN CRÍTICA ---
            // Esta validación DEBE detener la ejecución si falta algo.
            if (string.IsNullOrWhiteSpace(nombre) ||
                string.IsNullOrWhiteSpace(apellido) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(confirmarPassword) ||
                cmbEspecialidad.SelectedItem == null) // <-- ¡ESTA ES LA LÍNEA CLAVE!
            {
                MessageBox.Show("Todos los campos son obligatorios.", "Error de Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return; // <-- Esto evita que el código llegue a SaveChanges()
            }

            if (password != confirmarPassword)
            {
                MessageBox.Show("Las contraseñas no coinciden.", "Error de Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // --- 3. GUARDADO EN BASE DE DATOS ---
            try
            {
                using (var context = new GoMed.Data.AppContext())
                {
                    // Validar email duplicado
                    bool emailExiste = context.Medicos.Any(u => u.NombreUsuario.ToLower() == email.ToLower());
                    if (emailExiste)
                    {
                        MessageBox.Show("El email ingresado ya está registrado. Por favor, intente con otro.", "Error de Registro", MessageBoxButton.OK, MessageBoxImage.Stop);
                        return;
                    }

                    // Hashear contraseña
                    string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

                    // Crear y guardar Medico
                    var nuevoMedico = new Medico
                    {
                        Nombre = nombre,
                        Apellido = apellido,
                        NombreUsuario = email,
                        PasswordHash = passwordHash, // <-- Cambiado de 'Password' a 'PasswordHash'
                        Especialidad = especialidad, // 'especialidad' aquí ya no puede ser null
                        Matricula = "Pendiente"
                    };

                    context.Medicos.Add(nuevoMedico);
                    context.SaveChanges(); // <-- Aquí es donde ocurría tu error

                    MessageBox.Show("¡Usuario registrado exitosamente!", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                // Tu 'catch' mejorado para ver el error real
                Exception innerEx = ex;
                while (innerEx.InnerException != null) innerEx = innerEx.InnerException;
                string errorMessage = innerEx.Message;

                MessageBox.Show($"Ocurrió un error inesperado al registrar:\n\n{errorMessage}", "Error de Base de Datos", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
