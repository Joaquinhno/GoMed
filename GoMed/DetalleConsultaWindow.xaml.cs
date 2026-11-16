using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GoMed
{
    public partial class DetalleConsultaWindow : UserControl
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
                var consulta = context.Consultas
                    .Include(c => c.Paciente)
                    .FirstOrDefault(c => c.ConsultaID == _consultaId);

                if (consulta != null)
                {
                    lblInfoPaciente.Text = $"Paciente: {consulta.Paciente.Nombre} {consulta.Paciente.Apellido}   |   Fecha: {consulta.Fecha:dd/MM/yyyy}";

                    txtMotivoDetalle.Text = string.IsNullOrWhiteSpace(consulta.MotivoConsulta)
                                            ? "No se registró motivo."
                                            : consulta.MotivoConsulta;

                    string diag = string.IsNullOrWhiteSpace(consulta.Diagnostico) ? "Sin diagnóstico especificado." : consulta.Diagnostico;
                    string plan = string.IsNullOrWhiteSpace(consulta.PlanTratamiento) ? "" : $"\n\nPlan / Indicaciones:\n{consulta.PlanTratamiento}";

                    txtTratamientoDetalle.Text = diag + plan;

                    var fotosRutas = context.ConsultaFotos
                        .Where(f => f.ConsultaId == _consultaId)
                        .Select(f => f.RutaFoto)
                        .ToList();

                    icFotos.ItemsSource = fotosRutas;
                }
            }
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var img = sender as Image;
            if (img != null && img.Source != null)
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(img.Source.ToString()) { UseShellExecute = true });
            }
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e) => MainWindow.Instance?.Volver();
    }
}