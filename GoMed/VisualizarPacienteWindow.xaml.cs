using GoMed.Models;
using GoMed.Utils;
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
using Microsoft.EntityFrameworkCore;


namespace GoMed
{
    public partial class VisualizarPacientesWindow : Window
    {
        public VisualizarPacientesWindow()
        {
            InitializeComponent();
            CargarPacientes();
        }

        private void CargarPacientes(string filtro = "")
        {
            try
            {
                using (var context = new GoMed.Data.AppContext())
                {
                    // Filtramos por médico logueado y por el texto de búsqueda
                    var query = context.Pacientes
                        .Where(p => p.MedicoID == SesionUsuario.MedicoID);

                    if (!string.IsNullOrWhiteSpace(filtro))
                    {
                        query = query.Where(p => p.Dni.Contains(filtro) ||
                                                 p.Nombre.ToLower().Contains(filtro.ToLower()) ||
                                                 p.Apellido.ToLower().Contains(filtro.ToLower()));
                    }

                    dgPacientes.ItemsSource = query.ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar pacientes: {ex.Message}");
            }
        }

        private void TxtBusqueda_TextChanged(object sender, TextChangedEventArgs e)
        {
            CargarPacientes(txtBusqueda.Text);
        }

        private void BtnVerFicha_Click(object sender, RoutedEventArgs e)
        {
            var seleccionado = dgPacientes.SelectedItem as Paciente;
            if (seleccionado != null)
            {
                // Abrimos la ventana de revisión de ficha que ya creamos
                VerFichaPacienteWindow win = new VerFichaPacienteWindow(seleccionado.PacienteID);
                win.Owner = this;
                win.ShowDialog();
            }
            else
            {
                MessageBox.Show("Por favor, seleccione un paciente de la lista.");
            }
        }

        private void BtnEditar_Click(object sender, RoutedEventArgs e)
        {
            var seleccionado = dgPacientes.SelectedItem as Paciente;
            if (seleccionado != null)
            {
                // Abrimos la ventana de Cargar Paciente, pero le pasamos el ID para que sepa que es una EDICIÓN
                CargarPacienteWindow editWin = new CargarPacienteWindow(seleccionado.PacienteID);
                editWin.Owner = this;
                editWin.ShowDialog();

                // Cuando se cierre la ventana de edición, recargamos la lista para ver los cambios
                CargarPacientes(txtBusqueda.Text);
            }
            else
            {
                MessageBox.Show("Por favor, seleccione un paciente de la lista.");
            }
        }


        private void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            var seleccionado = dgPacientes.SelectedItem as Paciente;
            if (seleccionado != null)
            {
                var resultado = MessageBox.Show($"¿Está seguro de que desea eliminar al paciente {seleccionado.Nombre} {seleccionado.Apellido}?\n\nEsta acción borrará también su ficha clínica, consultas y fotos adjuntas de forma permanente.",
                                                "Confirmar Eliminación", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (resultado == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var context = new GoMed.Data.AppContext())
                        {
                            // 1. Buscamos al paciente e INCLUIMOS todas sus dependencias (hijos)
                            var pacienteDb = context.Pacientes
                                .Include(p => p.FichaClinica)
                                .Include(p => p.Consultas)
                                    .ThenInclude(c => c.Fotos) // Incluimos las fotos dentro de las consultas
                                .FirstOrDefault(p => p.PacienteID == seleccionado.PacienteID);

                            if (pacienteDb != null)
                            {
                                // 2. Borramos las fotos de todas sus consultas
                                foreach (var consulta in pacienteDb.Consultas)
                                {
                                    if (consulta.Fotos != null && consulta.Fotos.Any())
                                    {
                                        context.ConsultaFotos.RemoveRange(consulta.Fotos);
                                    }
                                }

                                // 3. Borramos las consultas
                                if (pacienteDb.Consultas != null && pacienteDb.Consultas.Any())
                                {
                                    context.Consultas.RemoveRange(pacienteDb.Consultas);
                                }

                                // 4. Borramos la ficha clínica
                                if (pacienteDb.FichaClinica != null)
                                {
                                    // Nota: Asegúrate de usar el nombre correcto de tu DbSet (FichasClinicas o el que hayas puesto en AppContext)
                                    // Si en tu AppContext la tabla se llama de otra forma, ajusta esta línea.
                                    context.Remove(pacienteDb.FichaClinica);
                                }

                                // 5. Finalmente, borramos al paciente
                                context.Pacientes.Remove(pacienteDb);

                                // Guardamos todos los cambios en cascada
                                context.SaveChanges();

                                MessageBox.Show("Paciente y todo su historial eliminados correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                                CargarPacientes(txtBusqueda.Text); // Recargamos la tabla
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // Extraemos el error real para no ver el mensaje genérico
                        Exception innerEx = ex;
                        while (innerEx.InnerException != null)
                        {
                            innerEx = innerEx.InnerException;
                        }
                        MessageBox.Show($"Error al eliminar:\n\n{innerEx.Message}", "Error de Base de Datos", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Por favor, seleccione un paciente de la lista para eliminar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }





        private void BtnCerrar_Click(object sender, RoutedEventArgs e) => this.Close();
    }
}
