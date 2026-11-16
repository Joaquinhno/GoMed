using GoMed.Data;
using GoMed.Models;
using GoMed.Utils;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GoMed
{
    public partial class VisualizarPacientesWindow : UserControl
    {
        public VisualizarPacientesWindow()
        {
            InitializeComponent();
            CargarPacientes();
        }

        public void RefrescarDatos()
        {
            CargarPacientes(txtBusqueda?.Text ?? "");
        }

        private void CargarPacientes(string filtro = "")
        {
            try
            {
                using (var context = new GoMed.Data.AppContext())
                {
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

        private void BtnNuevoPaciente_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance?.BtnAgregarPaciente_Click(sender, e);
        }

        private void BtnVerFicha_Click(object sender, RoutedEventArgs e)
        {
            var seleccionado = dgPacientes.SelectedItem as Paciente;
            if (seleccionado != null)
            {
                MainWindow.Instance?.NavegarAVerFichaPaciente(seleccionado.PacienteID);
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
                MainWindow.Instance?.NavegarAEditarPaciente(seleccionado.PacienteID);
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
                            var pacienteDb = context.Pacientes
                                .Include(p => p.FichaClinica)
                                .Include(p => p.Consultas)
                                    .ThenInclude(c => c.Fotos)
                                .FirstOrDefault(p => p.PacienteID == seleccionado.PacienteID);

                            if (pacienteDb != null)
                            {
                                foreach (var consulta in pacienteDb.Consultas)
                                {
                                    if (consulta.Fotos != null && consulta.Fotos.Any())
                                    {
                                        context.ConsultaFotos.RemoveRange(consulta.Fotos);
                                    }
                                }

                                if (pacienteDb.Consultas != null && pacienteDb.Consultas.Any())
                                {
                                    context.Consultas.RemoveRange(pacienteDb.Consultas);
                                }

                                if (pacienteDb.FichaClinica != null)
                                {
                                    context.Remove(pacienteDb.FichaClinica);
                                }

                                context.Pacientes.Remove(pacienteDb);
                                context.SaveChanges();

                                MessageBox.Show("Paciente y todo su historial eliminados correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                                CargarPacientes(txtBusqueda.Text);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
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

        private void BtnCerrar_Click(object sender, RoutedEventArgs e) => MainWindow.Instance?.Volver();
    }
}