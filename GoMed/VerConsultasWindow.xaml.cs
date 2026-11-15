using GoMed.Utils;
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
using GoMed.Data;
using System.Collections.Generic;

namespace GoMed
{
    public partial class VerConsultasWindow : Window
    {
        public VerConsultasWindow()
        {
            InitializeComponent();
            CargarHistorial();
        }

        private void CargarHistorial(string filtro = "")
        {
            using (var context = new GoMed.Data.AppContext())
            {
                // 1. Obtenemos el ID del médico logueado
                int idMedico = SesionUsuario.MedicoID;

                // 2. Consultamos con filtro de médico y buscador de paciente
                var consultas = context.Consultas
                    .Include(c => c.Paciente) // Traemos los datos del paciente relacionado
                    .Where(c => c.MedicoID == idMedico) // SEGURIDAD: Solo mis consultas
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

        // Se ejecuta cada vez que escribes en el buscador
        private void TxtBusqueda_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            CargarHistorial(txtBusqueda.Text);
        }

        private void BtnVerDetalle_Click(object sender, RoutedEventArgs e)
        {
            // 1. Obtenemos el objeto seleccionado en el DataGrid
            var consultaSeleccionada = dgConsultas.SelectedItem as dynamic;

            if (consultaSeleccionada != null)
            {
                // 2. Abrimos la ventana de detalle pasando el ID de la consulta
                int id = consultaSeleccionada.ConsultaID;
                DetalleConsultaWindow detalleWin = new DetalleConsultaWindow(id);
                detalleWin.Owner = this;
                detalleWin.ShowDialog();
            }
            else
            {
                MessageBox.Show("Por favor, seleccione una consulta de la lista.");
            }
        }

        private void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            // 1. Obtener la consulta seleccionada
            var seleccion = dgConsultas.SelectedItem as dynamic;

            if (seleccion == null) return;

            int idConsulta = seleccion.ConsultaID;
            string paciente = seleccion.PacienteNombre;

            // 2. Pedir confirmación (Importante en apps médicas)
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
                        // 3. Buscar la consulta
                        var consulta = context.Consultas.Find(idConsulta);

                        if (consulta != null)
                        {
                            // 4. Borrar primero las referencias a fotos para evitar errores de clave foránea
                            var fotosVinculadas = context.ConsultaFotos.Where(f => f.ConsultaId == idConsulta);
                            context.ConsultaFotos.RemoveRange(fotosVinculadas);

                            // 5. Borrar la consulta
                            context.Consultas.Remove(consulta);

                            // 6. Guardar cambios
                            context.SaveChanges();

                            MessageBox.Show("Consulta eliminada correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                            // 7. Refrescar la tabla automáticamente usando el buscador actual
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
        private void BtnVolver_Click(object sender, RoutedEventArgs e) => this.Close();
    }
}
