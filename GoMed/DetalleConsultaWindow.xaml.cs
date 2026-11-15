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
    public partial class DetalleConsultaWindow : Window
    {
        private int _consultaId;

        public DetalleConsultaWindow(int consultaId)
        {
            InitializeComponent();
            _consultaId = consultaId;
            CargarDatos();
        }

        private void CargarDatos()
        {
            using (var context = new GoMed.Data.AppContext())
            {
                // 1. Buscamos la consulta incluyendo al paciente
                var consulta = context.Consultas
                    .Include(c => c.Paciente)
                    .FirstOrDefault(c => c.ConsultaID == _consultaId);

                if (consulta != null)
                {
                    // Info de cabecera
                    lblInfoPaciente.Text = $"Paciente: {consulta.Paciente.Nombre} {consulta.Paciente.Apellido} | Fecha: {consulta.Fecha:dd/MM/yyyy}";

                    // ASIGNACIÓN A LOS DOS CUADROS NUEVOS
                    txtMotivoDetalle.Text = string.IsNullOrWhiteSpace(consulta.MotivoConsulta)
                                            ? "No se registró motivo."
                                            : consulta.MotivoConsulta;

                    txtTratamientoDetalle.Text = string.IsNullOrWhiteSpace(consulta.Diagnostico)
                                                ? "No se registró diagnóstico o tratamiento."
                                                : consulta.Diagnostico;

                    // Cargar las fotos (buscando en la tabla ConsultaFotos)
                    var fotosRutas = context.ConsultaFotos
                        .Where(f => f.ConsultaId == _consultaId)
                        .Select(f => f.RutaFoto)
                        .ToList();

                    icFotos.ItemsSource = fotosRutas;
                }
            }
        }

        // Abrir imagen en grande al hacer clic
        private void Image_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var img = sender as System.Windows.Controls.Image;
            if (img != null)
            {
                // Un truco rápido para ver la foto en grande es usar un visor externo
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(img.Source.ToString()) { UseShellExecute = true });
            }
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e) => this.Close();
    }
}
