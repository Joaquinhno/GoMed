using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GoMed.Models; // Asegúrate de tener este using

namespace GoMed.Data
{
    public class AppContext : DbContext
    {
        public AppContext(DbContextOptions<AppContext> options) : base(options) { }
        public AppContext() { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("Data Source=GoMed.db");
            }
        }
        
        // Tablas Principales
        public DbSet<Medico> Medicos { get; set; }
        public DbSet<Paciente> Pacientes { get; set; }
        public DbSet<Consulta> Consultas { get; set; }
        public DbSet<Cita> Citas { get; set; }
        public DbSet<FichaClinica> FichasClinicas { get; set; }

        public DbSet<ConsultaFoto>ConsultaFotos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. RELACIÓN MEDICO - PACIENTES (1:N)
            modelBuilder.Entity<Paciente>()
                .HasOne(p => p.Medico)
                .WithMany(m => m.Pacientes)
                .HasForeignKey(p => p.MedicoID)
                .OnDelete(DeleteBehavior.Cascade);

            // 2. RELACIÓN PACIENTE - CONSULTA (1:N)
            modelBuilder.Entity<Consulta>()
                .HasOne(c => c.Paciente)
                .WithMany(p => p.Consultas)
                .HasForeignKey(c => c.PacienteID)
                .OnDelete(DeleteBehavior.Restrict); // Evita borrar consultas si se borra paciente por error

            // 3. RELACIÓN MEDICO - CONSULTA (1:N)
            modelBuilder.Entity<Consulta>()
                .HasOne(c => c.Medico)
                .WithMany(m => m.Consultas)
                .HasForeignKey(c => c.MedicoID)
                .OnDelete(DeleteBehavior.Cascade);

            // 4. RELACIÓN MEDICO - CITAS (AGENDA) (1:N)
            modelBuilder.Entity<Cita>()
                .HasOne(ci => ci.Medico)
                .WithMany(m => m.Citas)
                .HasForeignKey(ci => ci.MedicoID)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}