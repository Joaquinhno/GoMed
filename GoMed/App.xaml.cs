using System.Configuration;
using System.Data;
using System.Windows;
using GoMed.Data; // Necesario para AppContext
using Microsoft.EntityFrameworkCore; // Necesario para Migrate()

namespace GoMed
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // --- ESTE ES EL CÓDIGO CLAVE ---
            // 1. Llama a la base de datos
            using (var context = new GoMed.Data.AppContext())
            {
                // 2. Comprueba si hay migraciones pendientes y las aplica
                // Esto construirá las tablas si no existen
                context.Database.Migrate();
            }
        }
    }
}