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
using GoMed.Properties;
using GoMed.Utils;

namespace GoMed
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            CargarEmailGuardado();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            // --- 1. OBTENER DATOS ---
            string email = txtEmail.Text;
            string password = txtPassword.Password;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Por favor, ingrese email y contraseña.", "Campos Vacíos", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var context = new GoMed.Data.AppContext())
                {
                    // --- 2. BUSCAR AL USUARIO POR EMAIL ---
                    // Cambiar 'Email' por 'NombreUsuario' según la definición de la clase Usuario
                    Medico medico = context.Medicos.FirstOrDefault(u => u.NombreUsuario.ToLower() == email.ToLower());

                    if (medico == null)
                    {
                        // Email no encontrado
                        MessageBox.Show("Email o contraseña incorrectos.", "Error de Acceso", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // --- 3. VERIFICAR LA CONTRASEÑA ---
                    // Cambiar 'usuario.Password' por 'usuario.PasswordHash' según la definición de la clase Usuario
                    bool esPasswordValido = BCrypt.Net.BCrypt.Verify(password, medico.PasswordHash);

                    if (esPasswordValido)
                    {
                        SesionUsuario.MedicoID = medico.MedicoID;
                        SesionUsuario.NombreCompleto = $"{medico.Nombre} {medico.Apellido}";
                        // ¡ACCESO CONCEDIDO!
                        MessageBox.Show($"¡Bienvenido Dr. {medico.Apellido}!", "Acceso Exitoso", MessageBoxButton.OK, MessageBoxImage.Information);
                        // Abrimos la ventana principal de la aplicación
                        MainWindow mainWindow = new MainWindow();
                        mainWindow.Show();
                
                        this.Close();
                    }
                    else
                    {
                        // Contraseña incorrecta
                        MessageBox.Show("Email o contraseña incorrectos.", "Error de Acceso", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    if (chkRecordarEmail.IsChecked == true)
                    {
                        Settings.Default.EmailGuardado = txtEmail.Text;
                    }
                    else
                    {
                        Settings.Default.EmailGuardado = "";
                    }

                    // ¡ESTA LÍNEA ES LA MÁS IMPORTANTE!
                    Settings.Default.Save();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error inesperado: {ex.Message}", "Error de Base de Datos", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnMostrarRegistro_Click(object sender, RoutedEventArgs e)
        {
            // Abrimos la ventana de registro que ya creamos
            RegistroWindow registroWin = new RegistroWindow();

            // Usamos ShowDialog() para que el usuario no pueda
            // interactuar con el Login hasta que cierre el Registro.
            registroWin.ShowDialog();
        }

        // Método para cargar el email guardado en las configuraciones//
        private void CargarEmailGuardado()
        {
            if (!string.IsNullOrEmpty(Settings.Default.EmailGuardado))
            {
                txtEmail.Text = Settings.Default.EmailGuardado;
                chkRecordarEmail.IsChecked = true;
            }
        }
        
            
       

    }
}
