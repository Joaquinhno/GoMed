using GoMed.Utils;
using Microsoft.EntityFrameworkCore.Query.Internal;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.IO.Compression;
using Microsoft.Win32;

namespace GoMed
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Cargamos el nombre del médico desde la sesión
            lblNombreDoctor.Text = SesionUsuario.NombreCompleto;
        }

        #region NAVEGACIÓN DE MENÚS

        /// <summary>
        /// Controla qué panel se muestra en pantalla y gestiona el botón Volver.
        /// </summary>
        private void MostrarMenu(UIElement menuAMostrar)
        {
            // Ocultamos todos los contenedores
            MenuPrincipal.Visibility = Visibility.Collapsed;
            SubMenuPacientes.Visibility = Visibility.Collapsed;
            SubMenuFicha.Visibility = Visibility.Collapsed;
            SubMenuConsulta.Visibility = Visibility.Collapsed;

            // Mostramos el seleccionado
            menuAMostrar.Visibility = Visibility.Visible;

            // Si no es el menú principal, mostramos la flecha de volver
            btnVolver.Visibility = (menuAMostrar == MenuPrincipal) ? Visibility.Collapsed : Visibility.Visible;
        }

        /// Método público para que otras ventanas puedan pedirle al MainWindow que cambie al submenú de Pacientes
        /// </summary>
        public void AbrirSubMenuPacientes()
        {
            MostrarMenu(SubMenuPacientes);
        }


        // Eventos para entrar a los Submenús
        private void BtnPacientes_Click(object sender, RoutedEventArgs e) => MostrarMenu(SubMenuPacientes);
        private void BtnFichaMenu_Click(object sender, RoutedEventArgs e) => MostrarMenu(SubMenuFicha);
        private void BtnConsultaMenu_Click(object sender, RoutedEventArgs e) => MostrarMenu(SubMenuConsulta);

        // Evento para regresar al inicio
        private void BtnVolver_Click(object sender, RoutedEventArgs e) => MostrarMenu(MenuPrincipal);

        #endregion

        #region ACCIONES DE PACIENTES

        private void BtnAgregarPaciente_Click(object sender, RoutedEventArgs e)
        {
            CargarPacienteWindow ventanaCargar = new CargarPacienteWindow();
            ventanaCargar.Owner = this;
            ventanaCargar.ShowDialog();
        }

        private void BtnVisualizarPacientes_Click(object sender, RoutedEventArgs e)
        {
            VisualizarPacientesWindow win = new VisualizarPacientesWindow();
            win.Owner = this;
            win.ShowDialog();
        }

        #endregion

        #region ACCIONES DE FICHA CLÍNICA

        private void BtnAgregarFicha_Click(object sender, RoutedEventArgs e)
        {
            // Abre la ventana para crear/editar los antecedentes del paciente
            FichaClinicaWindow ventanaFicha = new FichaClinicaWindow();
            ventanaFicha.Owner = this;
            ventanaFicha.ShowDialog();
        }

        private void BtnRevisionFicha_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Abre la ventana de búsqueda y visualización de fichas existentes
                VerFichaPacienteWindow ventanaConsulta = new VerFichaPacienteWindow();
                ventanaConsulta.Owner = this;
                ventanaConsulta.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir la consulta de fichas: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region ACCIONES DE CONSULTAS

        private void BtnAgregarConsulta_Click(object sender, RoutedEventArgs e)
        {
            CargarConsultaWindow ventana = new CargarConsultaWindow();
            ventana.Owner = this;
            ventana.ShowDialog();
        }

        private void BtnVisualizarConsultas_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Abre el historial de todas las consultas realizadas
                VerConsultasWindow ventanaHistorial = new VerConsultasWindow();
                ventanaHistorial.Owner = this;
                ventanaHistorial.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir el historial: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        private void BtnAbrirRecetas_Click(object sender, RoutedEventArgs e)
        {
            // Creamos la instancia de la ventana de recetas
            GenerarRecetaWindow recetaWindow = new GenerarRecetaWindow();

            // La mostramos de forma modal (bloquea la principal hasta cerrar)
            // O puedes usar .Show() si quieres que el médico trabaje en ambas a la vez
            recetaWindow.Show();
        }

        private void BtnBackup_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // 1. Preguntamos dónde quiere guardar el backup
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Archivo Zip (*.zip)|*.zip",
                FileName = $"GoMed_Backup_{DateTime.Now:yyyyMMdd_HHmm}.zip",
                Title = "Guardar copia de seguridad"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    // 2. Definimos las rutas de lo que queremos guardar
                    string rutaBase = AppDomain.CurrentDomain.BaseDirectory;

                    // IMPORTANTE: Si tu base de datos se llama distinto en AppContext.cs, cámbialo aquí
                    string rutaDb = System.IO.Path.Combine(rutaBase, "gomed.db");
                    string rutaFotos = System.IO.Path.Combine(rutaBase, "FotosConsultas");
                    string rutaFirmas = System.IO.Path.Combine(rutaBase, "Firmas");

                    // 3. Creamos una carpeta temporal para juntar todo
                    string carpetaTemp = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "GoMedBackupTemp");
                    if (Directory.Exists(carpetaTemp)) Directory.Delete(carpetaTemp, true);
                    Directory.CreateDirectory(carpetaTemp);

                    // 4. Copiamos la base de datos (si existe)
                    if (File.Exists(rutaDb))
                    {
                        File.Copy(rutaDb, System.IO.Path.Combine(carpetaTemp, "gomed.db"));
                    }
                    else
                    {
                        MessageBox.Show("No se encontró la base de datos. Asegúrate de que el nombre 'gomed.db' sea el correcto.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }

                    // 5. Copiamos la carpeta de fotos (si existe)
                    if (Directory.Exists(rutaFotos))
                    {
                        string tempFotos = System.IO.Path.Combine(carpetaTemp, "FotosConsultas");
                        Directory.CreateDirectory(tempFotos);
                        foreach (string file in Directory.GetFiles(rutaFotos))
                        {
                            File.Copy(file, System.IO.Path.Combine(tempFotos, System.IO.Path.GetFileName(file)));
                        }
                    }

                    // 6. Copiamos la carpeta de firmas (si existe)
                    if (Directory.Exists(rutaFirmas))
                    {
                        string tempFirmas = System.IO.Path.Combine(carpetaTemp, "Firmas");
                        Directory.CreateDirectory(tempFirmas);
                        foreach (string file in Directory.GetFiles(rutaFirmas))
                        {
                            File.Copy(file, System.IO.Path.Combine(tempFirmas, System.IO.Path.GetFileName(file)));
                        }
                    }

                    // 7. Comprimimos todo en el archivo ZIP que el doctor eligió
                    if (File.Exists(saveFileDialog.FileName)) File.Delete(saveFileDialog.FileName); // Borra si sobreescribe
                    ZipFile.CreateFromDirectory(carpetaTemp, saveFileDialog.FileName);

                    // 8. Limpiamos la basura temporal
                    Directory.Delete(carpetaTemp, true);

                    MessageBox.Show("Copia de seguridad creada exitosamente.\n\nGuarda este archivo .zip en un lugar seguro (Google Drive o un Pendrive).", "Respaldo Exitoso", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al crear la copia de seguridad: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnRestaurar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // 1. Advertencia crítica antes de hacer nada
            var advertencia = MessageBox.Show(
                "¡ATENCIÓN!\n\nRestaurar una copia de seguridad reemplazará TODOS los datos actuales (pacientes, consultas, fotos y recetas).\n\n¿Estás completamente seguro de que deseas continuar?",
                "Confirmar Restauración", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (advertencia != MessageBoxResult.Yes) return;

            // 2. Pedimos al doctor que seleccione su archivo ZIP
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Archivo Zip (*.zip)|*.zip",
                Title = "Seleccionar archivo de copia de seguridad"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string archivoZip = openFileDialog.FileName;
                    string rutaBase = AppDomain.CurrentDomain.BaseDirectory;

                    string rutaDb = System.IO.Path.Combine(rutaBase, "gomed.db");
                    string rutaFotos = System.IO.Path.Combine(rutaBase, "FotosConsultas");
                    string rutaFirmas = System.IO.Path.Combine(rutaBase, "Firmas");

                    // 3. Creamos una carpeta temporal para descomprimir el ZIP
                    string carpetaTemp = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "GoMedRestoreTemp");
                    if (Directory.Exists(carpetaTemp)) Directory.Delete(carpetaTemp, true);
                    Directory.CreateDirectory(carpetaTemp);

                    // 4. Extraemos todo el contenido del ZIP
                    ZipFile.ExtractToDirectory(archivoZip, carpetaTemp);

                    // 5. TRUCO VITAL: Liberar la base de datos para que Windows nos deje borrarla/sobrescribirla
                    Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    // 6. Reemplazamos la Base de Datos
                    string dbExtraida = System.IO.Path.Combine(carpetaTemp, "gomed.db");
                    if (File.Exists(dbExtraida))
                    {
                        File.Copy(dbExtraida, rutaDb, true); // El 'true' significa "Sobrescribir si existe"
                    }
                    else
                    {
                        MessageBox.Show("El archivo ZIP seleccionado no parece ser un respaldo válido de GoMed.", "Error de Archivo", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // 7. Reemplazamos las Fotos
                    string fotosExtraidas = System.IO.Path.Combine(carpetaTemp, "FotosConsultas");
                    if (Directory.Exists(fotosExtraidas))
                    {
                        if (Directory.Exists(rutaFotos)) Directory.Delete(rutaFotos, true); // Borramos las fotos actuales
                        Directory.CreateDirectory(rutaFotos); // Creamos la carpeta limpia
                        foreach (string file in Directory.GetFiles(fotosExtraidas))
                        {
                            File.Copy(file, System.IO.Path.Combine(rutaFotos, System.IO.Path.GetFileName(file)), true);
                        }
                    }

                    // 8. Reemplazamos las Firmas
                    string firmasExtraidas = System.IO.Path.Combine(carpetaTemp, "Firmas");
                    if (Directory.Exists(firmasExtraidas))
                    {
                        if (Directory.Exists(rutaFirmas)) Directory.Delete(rutaFirmas, true);
                        Directory.CreateDirectory(rutaFirmas);
                        foreach (string file in Directory.GetFiles(firmasExtraidas))
                        {
                            File.Copy(file, System.IO.Path.Combine(rutaFirmas, System.IO.Path.GetFileName(file)), true);
                        }
                    }

                    // 9. Borramos la basura temporal
                    Directory.Delete(carpetaTemp, true);

                    MessageBox.Show("¡Los datos han sido restaurados con éxito!\n\nLa aplicación se reiniciará automáticamente para cargar la nueva información.", "Restauración Exitosa", MessageBoxButton.OK, MessageBoxImage.Information);

                    // 10. Reiniciamos la aplicación de forma automática
                    System.Diagnostics.Process.Start(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
                    Application.Current.Shutdown();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ocurrió un error al restaurar la copia de seguridad:\n\n{ex.Message}\n\nNota: Si el error dice 'El archivo está siendo utilizado', cierra la app manualmente y extrae el ZIP sobre la carpeta de instalación.", "Error Crítico", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


    }
}