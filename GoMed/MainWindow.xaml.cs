using GoMed.Utils;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GoMed
{
    public partial class MainWindow : Window
    {
        private readonly Stack<(UserControl Vista, string Titulo, Button? BotonSidebar)> _historialNavegacion = new();
        private Button? _botonSidebarActual;

        public static MainWindow? Instance => Application.Current.MainWindow as MainWindow;

        public MainWindow()
        {
            InitializeComponent();
            Application.Current.MainWindow = this;

            lblNombreDoctor.Text = string.IsNullOrWhiteSpace(SesionUsuario.NombreCompleto)
                ? "Dr. Daniel Infante"
                : $"Dr. {SesionUsuario.NombreCompleto}";

            MarcarBotonSidebarActivo(btnNavInicio);
            ActualizarIndicadorTema();
        }

        #region MODO OSCURO / CLARO

        private void BtnTema_MouseDown(object sender, MouseButtonEventArgs e)
        {
            App.AlternarTema();
            ActualizarIndicadorTema();
        }

        private void ActualizarIndicadorTema()
        {
            if (App.EsModoOscuro)
            {
                lblIconoTema.Text = "☀️";
                lblTextoTema.Text = " Claro";
            }
            else
            {
                lblIconoTema.Text = "🌙";
                lblTextoTema.Text = " Oscuro";
            }
        }

        #endregion

        #region MOTOR DE NAVEGACIÓN UNIFICADO (SHELL)

        public void NavegarA(UserControl nuevaVista, string tituloSeccion, Button? botonSidebar = null, bool guardarEnHistorial = false)
        {
            if (!guardarEnHistorial && MainContent.Content is CargarConsultaWindow consultaActual)
            {
                consultaActual.DetenerServidorWeb();
            }

            if (guardarEnHistorial && MainContent.Content is UserControl vistaAnterior)
            {
                _historialNavegacion.Push((vistaAnterior, lblSeccionActual.Text, _botonSidebarActual));
            }
            else if (!guardarEnHistorial)
            {
                _historialNavegacion.Clear();
            }

            VistaInicio.Visibility = Visibility.Collapsed;
            MainContent.Content = nuevaVista;
            MainContent.Visibility = Visibility.Visible;

            lblSeccionActual.Text = tituloSeccion;
            btnVolver.Visibility = _historialNavegacion.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

            if (botonSidebar != null)
            {
                MarcarBotonSidebarActivo(botonSidebar);
            }
        }

        public void Volver()
        {
            if (MainContent.Content is CargarConsultaWindow consultaActual)
            {
                consultaActual.DetenerServidorWeb();
            }

            if (_historialNavegacion.Count > 0)
            {
                var (vistaPrevia, tituloPrevio, botonPrevio) = _historialNavegacion.Pop();

                if (vistaPrevia is VisualizarPacientesWindow listadoPacientes)
                {
                    listadoPacientes.RefrescarDatos();
                }
                else if (vistaPrevia is VerConsultasWindow listadoConsultas)
                {
                    listadoConsultas.RefrescarDatos();
                }

                MainContent.Content = vistaPrevia;
                MainContent.Visibility = Visibility.Visible;
                VistaInicio.Visibility = Visibility.Collapsed;

                lblSeccionActual.Text = tituloPrevio;
                btnVolver.Visibility = _historialNavegacion.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

                if (botonPrevio != null)
                {
                    MarcarBotonSidebarActivo(botonPrevio);
                }
            }
            else
            {
                IrAInicio();
            }
        }

        public void IrAInicio()
        {
            if (MainContent.Content is CargarConsultaWindow consultaActual)
            {
                consultaActual.DetenerServidorWeb();
            }

            _historialNavegacion.Clear();
            MainContent.Content = null;
            MainContent.Visibility = Visibility.Collapsed;
            VistaInicio.Visibility = Visibility.Visible;

            lblSeccionActual.Text = "Panel Principal";
            btnVolver.Visibility = Visibility.Collapsed;
            MarcarBotonSidebarActivo(btnNavInicio);
        }

        private void MarcarBotonSidebarActivo(Button botonActivo)
        {
            var botones = new[]
            {
                btnNavInicio,
                btnNavPacientes,
                btnNavNuevoPaciente,
                btnNavFichaEditar,
                btnNavFichaVer,
                btnNavConsultas,
                btnNavNuevaConsulta,
                btnNavRecetas
            };

            Style estiloNormal = (Style)FindResource("SidebarButtonStyle");
            Style estiloActivo = (Style)FindResource("SidebarButtonActiveStyle");

            foreach (var btn in botones)
            {
                if (btn != null)
                {
                    btn.Style = (btn == botonActivo) ? estiloActivo : estiloNormal;
                }
            }

            _botonSidebarActual = botonActivo;
        }

        public void AbrirSubMenuPacientes()
        {
            NavegarA(new VisualizarPacientesWindow(), "Pacientes — Listado General", btnNavPacientes, false);
        }

        #endregion

        #region EVENTOS DEL SIDEBAR Y DASHBOARD

        private void BtnNavInicio_Click(object sender, RoutedEventArgs e) => IrAInicio();
        private void BtnVolver_Click(object sender, RoutedEventArgs e) => Volver();

        private void BtnPacientes_Click(object sender, RoutedEventArgs e)
        {
            NavegarA(new VisualizarPacientesWindow(), "Pacientes — Listado General", btnNavPacientes, false);
        }

        private void BtnFichaMenu_Click(object sender, RoutedEventArgs e)
        {
            NavegarA(new FichaClinicaWindow(), "Historia Clínica — Editar Ficha", btnNavFichaEditar, false);
        }

        private void BtnConsultaMenu_Click(object sender, RoutedEventArgs e)
        {
            NavegarA(new VerConsultasWindow(), "Consultas — Historial de Atenciones", btnNavConsultas, false);
        }

        public void BtnAgregarPaciente_Click(object sender, RoutedEventArgs e)
        {
            NavegarA(new CargarPacienteWindow(), "Pacientes — Nuevo Paciente", btnNavNuevoPaciente, false);
        }

        public void BtnVisualizarPacientes_Click(object sender, RoutedEventArgs e)
        {
            NavegarA(new VisualizarPacientesWindow(), "Pacientes — Listado General", btnNavPacientes, false);
        }

        public void NavegarAEditarPaciente(int pacienteId)
        {
            NavegarA(new CargarPacienteWindow(pacienteId), "Pacientes — Editar Paciente", btnNavPacientes, guardarEnHistorial: true);
        }

        public void NavegarAVerFichaPaciente(int pacienteId)
        {
            NavegarA(new VerFichaPacienteWindow(pacienteId), "Historia Clínica — Revisión de Ficha", btnNavFichaVer, guardarEnHistorial: true);
        }

        public void BtnAgregarFicha_Click(object sender, RoutedEventArgs e)
        {
            NavegarA(new FichaClinicaWindow(), "Historia Clínica — Editar Ficha", btnNavFichaEditar, false);
        }

        public void BtnRevisionFicha_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                NavegarA(new VerFichaPacienteWindow(), "Historia Clínica — Revisión de Ficha", btnNavFichaVer, false);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir la consulta de fichas: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void BtnAgregarConsulta_Click(object sender, RoutedEventArgs e)
        {
            NavegarA(new CargarConsultaWindow(), "Consultas — Nueva Consulta", btnNavNuevaConsulta, false);
        }

        public void BtnVisualizarConsultas_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                NavegarA(new VerConsultasWindow(), "Consultas — Historial de Atenciones", btnNavConsultas, false);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir el historial: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void NavegarADetalleConsulta(int consultaId)
        {
            NavegarA(new DetalleConsultaWindow(consultaId), "Consultas — Detalle de Atención", btnNavConsultas, guardarEnHistorial: true);
        }

        private void BtnAbrirRecetas_Click(object sender, RoutedEventArgs e)
        {
            NavegarA(new GenerarRecetaWindow(), "Prescripciones — Generar Receta Médica", btnNavRecetas, false);
        }

        #endregion

        #region RESPALDO Y RESTAURACIÓN DE BASE DE DATOS

        private void BtnBackup_MouseDown(object sender, MouseButtonEventArgs e)
        {
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
                    string rutaBase = AppDomain.CurrentDomain.BaseDirectory;
                    string rutaDb = Path.Combine(rutaBase, "gomed.db");
                    string rutaFotos = Path.Combine(rutaBase, "FotosConsultas");
                    string rutaFirmas = Path.Combine(rutaBase, "Firmas");

                    string carpetaTemp = Path.Combine(Path.GetTempPath(), "GoMedBackupTemp");
                    if (Directory.Exists(carpetaTemp)) Directory.Delete(carpetaTemp, true);
                    Directory.CreateDirectory(carpetaTemp);

                    if (File.Exists(rutaDb))
                    {
                        File.Copy(rutaDb, Path.Combine(carpetaTemp, "gomed.db"));
                    }
                    else
                    {
                        MessageBox.Show("No se encontró la base de datos. Asegúrate de que el nombre 'gomed.db' sea el correcto.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }

                    if (Directory.Exists(rutaFotos))
                    {
                        string tempFotos = Path.Combine(carpetaTemp, "FotosConsultas");
                        Directory.CreateDirectory(tempFotos);
                        foreach (string file in Directory.GetFiles(rutaFotos))
                        {
                            File.Copy(file, Path.Combine(tempFotos, Path.GetFileName(file)));
                        }
                    }

                    if (Directory.Exists(rutaFirmas))
                    {
                        string tempFirmas = Path.Combine(carpetaTemp, "Firmas");
                        Directory.CreateDirectory(tempFirmas);
                        foreach (string file in Directory.GetFiles(rutaFirmas))
                        {
                            File.Copy(file, Path.Combine(tempFirmas, Path.GetFileName(file)));
                        }
                    }

                    if (File.Exists(saveFileDialog.FileName)) File.Delete(saveFileDialog.FileName);
                    ZipFile.CreateFromDirectory(carpetaTemp, saveFileDialog.FileName);

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
            var advertencia = MessageBox.Show(
                "¡ATENCIÓN!\n\nRestaurar una copia de seguridad reemplazará TODOS los datos actuales (pacientes, consultas, fotos y recetas).\n\n¿Estás completamente seguro de que deseas continuar?",
                "Confirmar Restauración", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (advertencia != MessageBoxResult.Yes) return;

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

                    string rutaDb = Path.Combine(rutaBase, "gomed.db");
                    string rutaFotos = Path.Combine(rutaBase, "FotosConsultas");
                    string rutaFirmas = Path.Combine(rutaBase, "Firmas");

                    string carpetaTemp = Path.Combine(Path.GetTempPath(), "GoMedRestoreTemp");
                    if (Directory.Exists(carpetaTemp)) Directory.Delete(carpetaTemp, true);
                    Directory.CreateDirectory(carpetaTemp);

                    ZipFile.ExtractToDirectory(archivoZip, carpetaTemp);

                    Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    string dbExtraida = Path.Combine(carpetaTemp, "gomed.db");
                    if (File.Exists(dbExtraida))
                    {
                        File.Copy(dbExtraida, rutaDb, true);
                    }
                    else
                    {
                        MessageBox.Show("El archivo ZIP seleccionado no parece ser un respaldo válido de GoMed.", "Error de Archivo", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    string fotosExtraidas = Path.Combine(carpetaTemp, "FotosConsultas");
                    if (Directory.Exists(fotosExtraidas))
                    {
                        if (Directory.Exists(rutaFotos)) Directory.Delete(rutaFotos, true);
                        Directory.CreateDirectory(rutaFotos);
                        foreach (string file in Directory.GetFiles(fotosExtraidas))
                        {
                            File.Copy(file, Path.Combine(rutaFotos, Path.GetFileName(file)), true);
                        }
                    }

                    string firmasExtraidas = Path.Combine(carpetaTemp, "Firmas");
                    if (Directory.Exists(firmasExtraidas))
                    {
                        if (Directory.Exists(rutaFirmas)) Directory.Delete(rutaFirmas, true);
                        Directory.CreateDirectory(rutaFirmas);
                        foreach (string file in Directory.GetFiles(firmasExtraidas))
                        {
                            File.Copy(file, Path.Combine(rutaFirmas, Path.GetFileName(file)), true);
                        }
                    }

                    Directory.Delete(carpetaTemp, true);

                    MessageBox.Show("¡Los datos han sido restaurados con éxito!\n\nLa aplicación se reiniciará automáticamente para cargar la nueva información.", "Restauración Exitosa", MessageBoxButton.OK, MessageBoxImage.Information);

                    var exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;
                    if (!string.IsNullOrEmpty(exePath))
                    {
                        System.Diagnostics.Process.Start(exePath);
                    }
                    Application.Current.Shutdown();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ocurrió un error al restaurar la copia de seguridad:\n\n{ex.Message}\n\nNota: Si el error dice 'El archivo está siendo utilizado', cierra la app manualmente y extrae el ZIP sobre la carpeta de instalación.", "Error Crítico", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        #endregion
    }
}