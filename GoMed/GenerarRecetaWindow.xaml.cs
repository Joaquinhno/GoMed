using GoMed.Models; // <-- Añade esta línea si la clase Paciente está en GoMed.Models
using GoMed.Utils;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Previewer; // Opcional para previsualizar
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    // Clase interna para manejar los medicamentos en la lista
    public class MedicamentoReceta
    {
        public string Nombre { get; set; } = "";
        public string Dosis { get; set; } = "";
        public string Instrucciones { get; set; } = "";
    }

    public partial class GenerarRecetaWindow : Window
    {
        // Esta lista está conectada al DataGrid
        public ObservableCollection<MedicamentoReceta> ListaMedicamentos { get; set; }

        public GenerarRecetaWindow()
        {
            InitializeComponent();
            ListaMedicamentos = new ObservableCollection<MedicamentoReceta>();

            // Conectamos la lista a la tabla (DataGrid)
            dgRecetaTemporal.ItemsSource = ListaMedicamentos;

            CargarListaPacientes(); // <-- Cambia 'CargarPacientes' por 'CargarListaPacientes'

            QuestPDF.Settings.License = LicenseType.Community;
        }

        private void CargarListaPacientes()
        {
            using (var context = new GoMed.Data.AppContext())
            {
                // Cargamos los objetos Paciente reales, filtrados por el médico actual
                var pacientes = context.Pacientes
                    .Where(p => p.MedicoID == SesionUsuario.MedicoID)
                    .ToList();

                cmbPaciente.ItemsSource = pacientes;
            }
        }

        // --- AÑADIR MEDICAMENTO A LA LISTA ---
        private void BtnAgregarLista_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMedicamento.Text) || string.IsNullOrWhiteSpace(txtDosis.Text))
            {
                MessageBox.Show("Por favor, complete al menos el nombre y la dosis.", "GoMed", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Creamos el objeto con los datos de los TextBox
            var nuevoMed = new MedicamentoReceta
            {
                Nombre = txtMedicamento.Text,
                Dosis = txtDosis.Text,
                Instrucciones = txtInstrucciones.Text
            };

            // Se añade a la lista y aparece mágicamente en la tabla
            ListaMedicamentos.Add(nuevoMed);

            // Limpiamos los campos para el siguiente medicamento
            txtMedicamento.Clear();
            txtDosis.Clear();
            txtInstrucciones.Clear();
            txtMedicamento.Focus();
        }

        // --- QUITAR MEDICAMENTO ---
        private void BtnQuitarMedicamento_Click(object sender, RoutedEventArgs e)
        {
            var seleccionado = (MedicamentoReceta)((FrameworkElement)sender).DataContext;
            if (seleccionado != null)
            {
                ListaMedicamentos.Remove(seleccionado);
            }
        }

        private void BtnVolverInicio_Click(object sender, RoutedEventArgs e) => this.Close();

        private void BtnGenerarPDF_Click(object sender, RoutedEventArgs e)
        {
            // 1. Validaciones iniciales
            var paciente = cmbPaciente.SelectedItem as Paciente;
            if (paciente == null || ListaMedicamentos.Count == 0)
            {
                MessageBox.Show("Asegúrese de seleccionar un paciente y añadir al menos un medicamento.",
                                "Datos Incompletos", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 2. Cuadro de diálogo para elegir dónde guardar (Mejor UX)
            var sfd = new Microsoft.Win32.SaveFileDialog
            {
                FileName = $"Receta_{paciente.Apellido}_{DateTime.Now:yyyyMMdd}",
                DefaultExt = ".pdf",
                Filter = "Documento PDF (.pdf)|*.pdf"
            };

            if (sfd.ShowDialog() != true) return;

            try
            {
                string rutaArchivo = sfd.FileName;

                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        // Configuración de página A5 (Estándar médico)
                        page.Size(PageSizes.A5);
                        page.Margin(1, Unit.Centimetre);
                        page.PageColor(QuestPDF.Helpers.Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Verdana"));

                        // --- ENCABEZADO PROFESIONAL ---
                        page.Header().Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("⚕ GoMed").FontSize(22).SemiBold().FontColor("#007BFF");
                                col.Item().Text("Dr. Daniel Infante").FontSize(12).SemiBold();
                                col.Item().Text("Matrícula: 123456").FontSize(9).FontColor(QuestPDF.Helpers.Colors.Grey.Medium);
                            });

                            row.RelativeItem().AlignRight().Column(col =>
                            {
                                col.Item().Text(DateTime.Now.ToString("dd/MM/yyyy")).SemiBold();
                                col.Item().Text("Consultorio Central").FontSize(9);
                            });
                        });

                        // --- CUERPO DE LA RECETA ---
                        page.Content().PaddingVertical(10).Column(col =>
                        {
                            // Bloque de datos del paciente con fondo sutil
                            col.Item().Background(QuestPDF.Helpers.Colors.Grey.Lighten4).Padding(8).Row(row =>
                            {
                                row.RelativeItem().Text(t => {
                                    t.Span("Paciente: ").SemiBold();
                                    t.Span($"{paciente.Nombre} {paciente.Apellido}");
                                });
                                row.RelativeItem().AlignRight().Text(t => {
                                    t.Span("DNI: ").SemiBold();
                                    t.Span(paciente.Dni);
                                });
                            });

                            col.Item().PaddingTop(15).Text("PRESCRIPCIÓN MÉDICA").FontSize(12).SemiBold().Underline();

                            // Tabla de Medicamentos estilizada
                            col.Item().PaddingTop(5).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3); // Medicamento e instrucciones
                                    columns.RelativeColumn(1); // Dosis
                                });

                                foreach (var med in ListaMedicamentos)
                                {
                                    table.Cell().BorderBottom(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten3).PaddingVertical(5).Column(c => {
                                        c.Item().Text(med.Nombre).Bold();
                                        if (!string.IsNullOrEmpty(med.Instrucciones))
                                            c.Item().Text(med.Instrucciones).FontSize(8).Italic().FontColor(QuestPDF.Helpers.Colors.Grey.Medium);
                                    });
                                    table.Cell().BorderBottom(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten3).PaddingVertical(5).AlignRight().Text(med.Dosis);
                                }
                            });
                        });

                        // --- PIE DE PÁGINA ---
                        page.Footer().Column(col =>
                        {
                            col.Item().PaddingTop(30).Row(row => {
                                row.RelativeItem();
                                row.RelativeItem().BorderTop(1).PaddingTop(5).Text("Firma y Sello").AlignCenter().FontSize(9);
                                row.RelativeItem();
                            });
                            col.Item().PaddingTop(10).AlignCenter().Text("Documento digital generado por GoMed").FontSize(7).FontColor(QuestPDF.Helpers.Colors.Grey.Medium);
                        });
                    });
                }).GeneratePdf(rutaArchivo);

                MessageBox.Show("La receta se ha generado correctamente.", "GoMed", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar el PDF: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GenerarDocumentoPDF(string ruta, Paciente paciente)
        {



            Document.Create(container =>
            {
                container.Page(page =>
                {
                    // Configuramos el tamaño A5 (típico de recetarios médicos)
                    page.Size(PageSizes.A5);
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(QuestPDF.Helpers.Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Verdana"));

                    // --- 1. ENCABEZADO (Datos del Médico/Clínica) ---
                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("⚕ GoMed").FontSize(22).SemiBold().FontColor("#007BFF");
                            col.Item().Text("Dr. Daniel Infante").FontSize(12).SemiBold();
                            col.Item().Text("Especialista en Medicina Interna").FontSize(9).FontColor(QuestPDF.Helpers.Colors.Grey.Medium);
                            col.Item().Text("Matrícula: 123456").FontSize(9);
                        });

                        row.RelativeItem().AlignRight().Column(col =>
                        {
                            col.Item().Text(DateTime.Now.ToString("dd/MM/yyyy")).FontSize(10);
                            col.Item().Text("Consultorio Médico GoMed").FontSize(9);
                            col.Item().Text("Calle Principal 123 - Ciudad").FontSize(9);
                        });
                    });

                    // --- 2. CONTENIDO (Paciente e Indicaciones) ---
                    page.Content().PaddingVertical(15).Column(col =>
                    {
                        // Información del Paciente
                        col.Item().Background("#F8F9FA").Padding(10).Row(row =>
                        {
                            row.RelativeItem().Text(t =>
                            {
                                t.Span("Paciente: ").SemiBold();
                                t.Span($"{paciente.Nombre} {paciente.Apellido}");
                            });
                            row.RelativeItem().AlignRight().Text(t =>
                            {
                                t.Span("DNI: ").SemiBold();
                                t.Span(paciente.Dni);
                            });
                        });

                        col.Item().PaddingTop(15).PaddingBottom(5).Text("PRESCRIPCIÓN MÉDICA").FontSize(12).SemiBold().Underline();

                        // Tabla de Medicamentos
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3); // Medicamento e instrucciones
                                columns.RelativeColumn(1); // Dosis/Cantidad
                            });

                            // Encabezado de la tabla
                            table.Header(header =>
                            {
                                header.Cell().BorderBottom(1).Padding(5).Text("Medicamento / Indicación").SemiBold();
                                header.Cell().BorderBottom(1).Padding(5).Text("Dosis").SemiBold();
                            });

                            // Filas de medicamentos de tu lista temporal
                            foreach (var med in ListaMedicamentos)
                            {
                                table.Cell().BorderBottom(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten3).Padding(5).Column(c =>
                                {
                                    c.Item().Text(med.Nombre).Bold();
                                    if (!string.IsNullOrEmpty(med.Instrucciones))
                                        c.Item().Text(med.Instrucciones).FontSize(8).Italic().FontColor(QuestPDF.Helpers.Colors.Grey.Medium);
                                });

                                table.Cell().BorderBottom(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten3).Padding(5).Text(med.Dosis).AlignCenter();
                            }
                        });
                    });

                    // --- 3. PIE DE PÁGINA (Firma y Software) ---
                    page.Footer().Column(col =>
                    {
                        col.Item().PaddingTop(30).Row(row =>
                        {
                            row.RelativeItem(); // Espacio vacío

                            row.ConstantItem(120).Column(c =>
                            {
                                // Obtenemos los datos del médico desde la sesión
                                string rutaFirma = ObtenerRutaFirmaMedico(SesionUsuario.MedicoID);

                                // Si el médico tiene una firma cargada, la mostramos
                                if (!string.IsNullOrEmpty(rutaFirma) && System.IO.File.Exists(rutaFirma))
                                {
                                    c.Item().Height(60).Image(rutaFirma).FitHeight();
                                }

                                c.Item().BorderTop(1).PaddingTop(2).AlignCenter().Text(t =>
                                {
                                    t.Span("Firma y Sello Digital").FontSize(8).SemiBold();
                                });
                            });

                            row.RelativeItem(); // Espacio vacío
                        });

                        col.Item().PaddingTop(10).AlignCenter().Text(t =>
                        {
                            t.Span("La validez de esta receta puede ser verificada en el sistema GoMed.").FontSize(7).Italic();
                        });

                        col.Item().PaddingTop(15).AlignCenter().Text(t =>
                        {
                            t.Span("Documento generado electrónicamente por ").FontSize(7).FontColor(QuestPDF.Helpers.Colors.Grey.Medium);
                            t.Span("GoMed").FontSize(7).SemiBold().FontColor("#007BFF");
                        });
                    });
                });
            })
            .GeneratePdf(ruta);
        }

        private void BtnPrevisualizar_Click(object sender, RoutedEventArgs e)
        {
            // 1. Validamos que haya datos antes de intentar crear nada
            var paciente = cmbPaciente.SelectedItem as Paciente;
            if (paciente == null || ListaMedicamentos.Count == 0)
            {
                MessageBox.Show("Debe seleccionar un paciente y agregar al menos un medicamento para ver la vista previa.",
                                "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 2. Creamos una ruta en la carpeta temporal de Windows
            // Esto evita que se guarden archivos basura en la carpeta de tu programa.
            string nombreTemporal = $"Previsualizacion_{paciente.Apellido}.pdf";
            string rutaTemporal = System.IO.Path.Combine(System.IO.Path.GetTempPath(), nombreTemporal);

            try
            {
                // 3. Llamamos al método de diseño que ya creamos anteriormente
                GenerarDocumentoPDF(rutaTemporal, paciente);

                // 4. Abrimos el archivo automáticamente
                // Usamos ProcessStartInfo para asegurar compatibilidad con Windows 10 y 11
                var proceso = new System.Diagnostics.ProcessStartInfo(rutaTemporal)
                {
                    UseShellExecute = true
                };
                System.Diagnostics.Process.Start(proceso);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo abrir la previsualización: {ex.Message}",
                                "Error de Sistema", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Añade este método dentro de la clase GenerarRecetaWindow o como método estático en una clase utilitaria accesible
        private string ObtenerRutaFirmaMedico(int usuarioId)
        {
        return System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Firmas", $"firma_{usuarioId}.png");
        }
    }
}