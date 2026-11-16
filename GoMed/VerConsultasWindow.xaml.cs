using GoMed.Data;
using GoMed.Utils;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GoMed
{
    public partial class VerConsultasWindow : UserControl
    {
        public VerConsultasWindow()
        {
            InitializeComponent();
            CargarHistorial();
        }

        public void RefrescarDatos()
        {
            CargarHistorial(txtBusqueda?.Text ?? "");
        }

        private void CargarHistorial(string filtro = "")
        {
            using (var context = new GoMed.Data.AppContext())
            {
                int idMedico = SesionUsuario.MedicoID;

                var consultas = context.Consultas
                    .Include(c => c.Paciente)
                    .Where(c => c.MedicoID == idMedico)
                    .Where(c => string.IsNullOrEmpty(filtro) ||
                                (c.Paciente.Nombre + " " + c.Paciente.Apellido).ToLower().Contains(filtro.ToLower()))
                    .OrderByDescending(c => c.Fecha)
                    .Select(c => new
                    {
                        c.ConsultaID,
                        c.Fecha,
                        PacienteNombre = c.Paciente.Nombre + " " + c.Paciente.Apellido,
                        Motivo = c.MotivoConsulta
                    })
                    .ToList();

                dgConsultas.ItemsSource = consultas;
            }
        }

        private void TxtBusqueda_TextChanged(object sender, TextChangedEventArgs e)
        {
            CargarHistorial(txtBusqueda.Text);
        }

        private void BtnNuevaConsulta_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance?.BtnAgregarConsulta_Click(sender, e);
        }

        private void BtnVerDetalle_Click(object sender, RoutedEventArgs e)
        {
            var consultaSeleccionada = ((FrameworkElement)sender).DataContext as dynamic ?? dgConsultas.SelectedItem as dynamic;

            if (consultaSeleccionada != null)
            {
                int id = consultaSeleccionada.ConsultaID;
                MainWindow.Instance?.NavegarADetalleConsulta(id);
            }
            else
            {
                MessageBox.Show("Por favor, seleccione una consulta de la lista.");
            }
        }

        private void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            var seleccion = ((FrameworkElement)sender).DataContext as dynamic ?? dgConsultas.SelectedItem as dynamic;

            if (seleccion == null) return;

            int idConsulta = seleccion.ConsultaID;
            string paciente = seleccion.PacienteNombre;

            var resultado = MessageBox.Show($"¿Está seguro de que desea eliminar la consulta del paciente {paciente}?\nEsta acción no se puede deshacer.",
                                            "Confirmar Eliminación",
                                            MessageBoxButton.YesNo,
                                            MessageBoxImage.Warning);

            if (resultado == MessageBoxResult.Yes)
            {
                try
                {
                    using (var context = new GoMed.Data.AppContext())
                    {
                        var consulta = context.Consultas.Find(idConsulta);

                        if (consulta != null)
                        {
                            var fotosVinculadas = context.ConsultaFotos.Where(f => f.ConsultaId == idConsulta);
                            context.ConsultaFotos.RemoveRange(fotosVinculadas);

                            context.Consultas.Remove(consulta);
                            context.SaveChanges();

                            MessageBox.Show("Consulta eliminada correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                            CargarHistorial(txtBusqueda.Text);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al eliminar la consulta: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnVolver_Click(object sender, RoutedEventArgs e) => MainWindow.Instance?.Volver();
    }
}