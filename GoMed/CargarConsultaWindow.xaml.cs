using GoMed.Data;
using GoMed.Models;
using GoMed.Utils;
using Microsoft.Win32;
using QRCoder;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;


namespace GoMed
{
    public partial class CargarConsultaWindow : Window
    {
        //variable para el listener HTTP que recibirá las fotos desde el celular//
        private HttpListener _httpListener;
        private bool _isListening;
        private int _fotosRecibidasTemp = 0;

        // CORRECCIÓN: Inicializamos la lista para evitar la advertencia CS8618
        private System.Collections.IList listaPacientes = new List<object>();

        private List<string> _rutasFotosSeleccionadas = new List<string>();
        private int _medicoId;

        // CORRECCIÓN: Constructor sin parámetros para que funcione con MainWindow
        public CargarConsultaWindow()
        {
            InitializeComponent();

            // Tomamos el ID del médico directamente de la sesión global
            _medicoId = SesionUsuario.MedicoID;

            CargarPacientes();
            cmbDiagnostico.ItemsSource = ListasEstaticas.Diagnosticos;
        }

        private void CargarPacientes()
        {
            try
            {
                using (var context = new GoMed.Data.AppContext())
                {
                    // Filtramos pacientes que pertenezcan al médico actual
                    listaPacientes = context.Pacientes
                        .Where(p => p.MedicoID == _medicoId)
                        .Select(p => new
                        {
                            p.PacienteID,
                            p.FechaNacimiento,
                            DisplayText = $"{p.Nombre} {p.Apellido} (DNI: {p.Dni})"
                        })
                        .ToList();

                    cmbPaciente.ItemsSource = listaPacientes;
                    cmbPaciente.DisplayMemberPath = "DisplayText";
                    cmbPaciente.SelectedValuePath = "PacienteID";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar pacientes: {ex.Message}", "Error de Base de Datos", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (cmbPaciente.SelectedItem == null)
            {
                MessageBox.Show("Por favor, seleccione un paciente.", "Datos Incompletos", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(txtMotivo.Text))
            {
                MessageBox.Show("Por favor, ingrese un motivo de consulta.", "Datos Incompletos", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var nuevaConsulta = new Consulta
            {
                PacienteID = (int)cmbPaciente.SelectedValue,
                MedicoID = _medicoId,
                Fecha = DateTime.Now,
                MotivoConsulta = txtMotivo.Text,

                // Extraemos el texto del ComboBox (ya sea elegido o escrito)
                Diagnostico = cmbDiagnostico.Text,

                // Guardamos el nuevo campo
                PlanTratamiento = txtPlan.Text,

                RutaFoto = null
            };

            try
            {
                using (var context = new GoMed.Data.AppContext())
                {
                    context.Consultas.Add(nuevaConsulta);
                    context.SaveChanges();

                    if (_rutasFotosSeleccionadas.Any())
                    {
                        foreach (string rutaFoto in _rutasFotosSeleccionadas)
                        {
                            var nuevaFoto = new ConsultaFoto
                            {
                                ConsultaId = nuevaConsulta.ConsultaID,
                                RutaFoto = rutaFoto
                            };
                            context.ConsultaFotos.Add(nuevaFoto);
                        }
                        context.SaveChanges();
                    }
                }

                MessageBox.Show("Consulta y fotos guardadas exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                Exception innerEx = ex;
                while (innerEx.InnerException != null)
                {
                    innerEx = innerEx.InnerException;
                }

                MessageBox.Show($"Error al guardar:\n\n{innerEx.Message}", "Error de Base de Datos", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnAdjuntarFoto_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "Archivos de imagen|*.jpg;*.jpeg;*.png;*.gif";

            if (openFileDialog.ShowDialog() == true)
            {
                foreach (string origenRuta in openFileDialog.FileNames)
                {
                    try
                    {
                        string nombreArchivo = System.IO.Path.GetFileName(origenRuta);
                        string carpetaDestino = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FotosConsultas");

                        if (!Directory.Exists(carpetaDestino)) Directory.CreateDirectory(carpetaDestino);

                        string nombreArchivoUnico = $"{Guid.NewGuid().ToString()}_{nombreArchivo}";
                        string destinoRuta = System.IO.Path.Combine(carpetaDestino, nombreArchivoUnico);

                        File.Copy(origenRuta, destinoRuta, true);
                        _rutasFotosSeleccionadas.Add(destinoRuta);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error con la foto {origenRuta}: {ex.Message}");
                    }
                }
                ActualizarListBoxFotos();
            }
        }

        private void ActualizarListBoxFotos()
        {
            if (lstFotosAdjuntadas != null)
            {
                lstFotosAdjuntadas.ItemsSource = null;
                lstFotosAdjuntadas.ItemsSource = _rutasFotosSeleccionadas;
            }
        }

        private void BtnQuitarFoto_Click(object sender, RoutedEventArgs e)
        {
            if (lstFotosAdjuntadas != null && lstFotosAdjuntadas.SelectedItem != null)
            {
                _rutasFotosSeleccionadas.Remove((string)lstFotosAdjuntadas.SelectedItem);
                ActualizarListBoxFotos();
            }
        }

        private void CmbPaciente_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lblEdad == null || cmbPaciente.SelectedItem == null)
            {
                if (lblEdad != null) lblEdad.Text = "-- años";
                return;
            }

            DateTime fechaNacimiento = ((dynamic)cmbPaciente.SelectedItem).FechaNacimiento;
            var hoy = DateTime.Today;
            int edad = hoy.Year - fechaNacimiento.Year;
            if (fechaNacimiento.Date > hoy.AddYears(-edad)) edad--;
            lblEdad.Text = $"{edad} años";
        }
 
        private void BtnCancelar_Click(object sender, RoutedEventArgs e) => this.Close();
        private void BtnVolverInicio_Click(object sender, RoutedEventArgs e) => this.Close();

        private void BtnPacientes_Click(object sender, RoutedEventArgs e)
        {
            // 1. Verificamos si la ventana que abrió esta consulta fue el MainWindow
            if (this.Owner is MainWindow ventanaPrincipal)
            {
                // 2. Le ordenamos al MainWindow que cambie su vista al SubMenú de Pacientes
                ventanaPrincipal.AbrirSubMenuPacientes();
            }

            // 3. Cerramos la ventana de Cargar Consulta
            this.Close();
        }

        #region CONEXIÓN CON CELULAR (SERVIDOR LOCAL)
        private async void BtnSubirDesdeTelefono_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 1. Obtenemos la IP de la computadora
                string ipLocal = ObtenerIpLocal();
                string url = $"http://{ipLocal}:8080/";

                // 2. Generamos el Código QR
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
                PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
                byte[] qrBytes = qrCode.GetGraphic(20);

                BitmapImage qrImage = new BitmapImage();
                using (MemoryStream stream = new MemoryStream(qrBytes))
                {
                    qrImage.BeginInit();
                    qrImage.CacheOption = BitmapCacheOption.OnLoad;
                    qrImage.StreamSource = stream;
                    qrImage.EndInit();
                }
                imgQR.Source = qrImage;

                // 3. Mostramos la pantalla y arrancamos el servidor
                _fotosRecibidasTemp = 0;
                lblFotosRecibidas.Text = "Esperando fotos...";
                OverlayQR.Visibility = Visibility.Visible;

                await IniciarServidorWeb(url);
            }
            catch (HttpListenerException)
            {
                MessageBox.Show("Windows bloqueó el puerto. Por favor, cierra la app y ábrela haciendo clic derecho -> 'Ejecutar como Administrador'.", "Permisos Requeridos", MessageBoxButton.OK, MessageBoxImage.Warning);
                OverlayQR.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al generar QR: " + ex.Message);
                OverlayQR.Visibility = Visibility.Collapsed;
            }
        }

        private string ObtenerIpLocal()
        {
            try
            {
                // Este truco abre una conexión de prueba hacia internet (Google) 
                // para descubrir cuál es la tarjeta de red real que usa tu Wi-Fi.
                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                {
                    socket.Connect("8.8.8.8", 65530);
                    IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                    return endPoint.Address.ToString();
                }
            }
            catch
            {
                return "127.0.0.1";
            }

        }

        private async Task IniciarServidorWeb(string url)
        {
            if (_httpListener != null && _httpListener.IsListening) return;

            _httpListener = new HttpListener();
            _httpListener.Prefixes.Add(url);
            _httpListener.Start();
            _isListening = true;

            while (_isListening)
            {
                try
                {
                    var context = await _httpListener.GetContextAsync();
                    var request = context.Request;
                    var response = context.Response;

                    if (request.HttpMethod == "GET")
                    {
                        // Esta es la página web que verá el doctor en su celular (Diseñada para móviles)
                        string html = @"
                        <!DOCTYPE html>
                        <html lang='es'>
                        <head>
                            <meta charset='UTF-8'>
                            <meta name='viewport' content='width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no'>
                            <title>GoMed - Cámara</title>
                            <style>
                                :root { --primary: #007BFF; --bg: #f4f7f6; --text: #2C3E50; }
                                body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; background-color: var(--bg); margin: 0; padding: 0; color: var(--text); }
                                
                                /* Encabezado */
                                .header { background: #fff; padding: 20px; text-align: center; box-shadow: 0 2px 15px rgba(0,0,0,0.05); font-size: 22px; font-weight: 800; color: var(--primary); }
                                
                                /* Contenedor Principal */
                                .container { padding: 25px 20px; text-align: center; }
                                .instructions { font-size: 15px; color: #666; margin-bottom: 25px; line-height: 1.5; font-weight: 500; }
                                
                                /* Botón de Cámara Premium */
                                .btn-camera { 
                                    background: linear-gradient(135deg, #007BFF 0%, #0056b3 100%); 
                                    color: white; border: none; border-radius: 50px; 
                                    padding: 16px 20px; font-size: 18px; font-weight: bold; 
                                    width: 100%; box-sizing: border-box; 
                                    display: flex; align-items: center; justify-content: center; gap: 12px;
                                    box-shadow: 0 10px 20px rgba(0,123,255,0.3); transition: transform 0.2s;
                                }
                                .btn-camera:active { transform: scale(0.95); }
                                input[type=file] { display: none; }
                                
                                /* Galería de miniaturas */
                                .gallery { display: grid; grid-template-columns: 1fr 1fr; gap: 15px; margin-top: 30px; }
                                .card { background: #fff; border-radius: 15px; overflow: hidden; box-shadow: 0 5px 15px rgba(0,0,0,0.08); position: relative; animation: fadeIn 0.4s ease-out; }
                                .card img { width: 100%; height: 140px; object-fit: cover; display: block; }
                                
                                /* Etiquetas de estado */
                                .status-badge { position: absolute; top: 10px; right: 10px; background: rgba(0,0,0,0.6); color: white; padding: 5px 10px; border-radius: 20px; font-size: 12px; font-weight: bold; backdrop-filter: blur(4px); }
                                .status-badge.success { background: rgba(40, 167, 69, 0.9); }
                                .status-badge.error { background: rgba(220, 53, 69, 0.9); }
                                .status-badge.loading { background: rgba(255, 193, 7, 0.9); color: #000; }
                                
                                @keyframes fadeIn { from { opacity: 0; transform: translateY(10px); } to { opacity: 1; transform: translateY(0); } }
                            </style>
                        </head>
                        <body>
                            <div class='header'>⚕ GoMed Móvil</div>
                            
                            <div class='container'>
                                <p class='instructions'>Toca el botón seleccionar las fotos. Se enviarán al instante a tu computadora.</p>
                                
                                <label class='btn-camera'>
                                    <svg width='26' height='26' viewBox='0 0 24 24' fill='none' stroke='currentColor' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'>
                                        <path d='M23 19a2 2 0 0 1-2 2H3a2 2 0 0 1-2-2V8a2 2 0 0 1 2-2h4l2-3h6l2 3h4a2 2 0 0 1 2 2z'></path>
                                        <circle cx='12' cy='13' r='4'></circle>
                                    </svg>
                                    Subir Fotografías
                                    <input type='file' id='fileInput' multiple accept='image/*' onchange='handleFiles(this.files)'>
                                </label>

                                <div class='gallery' id='gallery'></div>
                            </div>

                            <script>
                                async function handleFiles(files) {
                                    const gallery = document.getElementById('gallery');
                                    
                                    // Procesamos cada foto seleccionada o tomada
                                    for(let i = 0; i < files.length; i++) {
                                        let file = files[i];
                                        let cardId = 'card_' + Math.random().toString(36).substr(2, 9);
                                        
                                        // 1. Crear miniatura local para que el doc vea qué foto sacó
                                        let url = URL.createObjectURL(file);
                                        
                                        // 2. Crear la 'Tarjeta' visual
                                        let cardHTML = `
                                            <div class='card' id='${cardId}'>
                                                <div class='status-badge loading' id='badge_${cardId}'>⏳ Subiendo...</div>
                                                <img src='${url}'>
                                            </div>`;
                                        
                                        // Agregar al principio de la galería
                                        gallery.insertAdjacentHTML('afterbegin', cardHTML);
                                        
                                        // 3. Subir la foto
                                        await uploadFile(file, cardId);
                                    }
                                }

                                async function uploadFile(file, cardId) {
                                    let reader = new FileReader();
                                    reader.onload = async function(e) {
                                        let base64 = e.target.result.split(',')[1]; 
                                        let badge = document.getElementById('badge_' + cardId);
                                        
                                        try {
                                            let response = await fetch('/upload', { method: 'POST', body: base64 });
                                            if(response.ok) {
                                                badge.className = 'status-badge success';
                                                badge.innerHTML = '✅ Lista';
                                            } else {
                                                badge.className = 'status-badge error';
                                                badge.innerHTML = '❌ Error';
                                            }
                                        } catch(err) {
                                            badge.className = 'status-badge error';
                                            badge.innerHTML = '❌ Falló red';
                                        }
                                    };
                                    reader.readAsDataURL(file);
                                }
                            </script>
                        </body>
                        </html>";

                        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(html);
                        response.ContentLength64 = buffer.Length;
                        await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                        response.Close();
                    }
                    else if (request.HttpMethod == "POST" && request.Url.AbsolutePath == "/upload")
                    {
                        // 4. El celular envió una foto, la guardamos
                        using (StreamReader reader = new StreamReader(request.InputStream))
                        {
                            string base64Data = await reader.ReadToEndAsync();
                            byte[] imageBytes = Convert.FromBase64String(base64Data);

                            string carpetaDestino = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FotosConsultas");
                            if (!Directory.Exists(carpetaDestino)) Directory.CreateDirectory(carpetaDestino);

                            string rutaCompleta = System.IO.Path.Combine(carpetaDestino, $"{Guid.NewGuid()}.jpg");
                            await File.WriteAllBytesAsync(rutaCompleta, imageBytes);

                            // 5. Actualizamos la Interfaz Gráfica (En el hilo principal)
                            Dispatcher.Invoke(() =>
                            {
                                _rutasFotosSeleccionadas.Add(rutaCompleta);
                                ActualizarListBoxFotos();
                                _fotosRecibidasTemp++;
                                lblFotosRecibidas.Text = $"¡Se han recibido {_fotosRecibidasTemp} foto(s)!";
                            });
                        }
                        response.StatusCode = 200;
                        response.Close();
                    }
                }
                catch (Exception)
                {
                    // Si se cierra el listener, ignoramos el error.
                }
            }
        }

        private void BtnCerrarQR_Click(object sender, RoutedEventArgs e)
        {
            OverlayQR.Visibility = Visibility.Collapsed;
            _isListening = false;
            if (_httpListener != null)
            {
                _httpListener.Stop();
                _httpListener.Close();
                _httpListener = null;
            }
        }

        #endregion

    }
}