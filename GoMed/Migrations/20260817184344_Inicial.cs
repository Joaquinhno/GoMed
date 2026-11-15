using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoMed.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Medicos",
                columns: table => new
                {
                    MedicoID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NombreUsuario = table.Column<string>(type: "TEXT", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", nullable: false),
                    Apellido = table.Column<string>(type: "TEXT", nullable: false),
                    Especialidad = table.Column<string>(type: "TEXT", nullable: false),
                    Matricula = table.Column<string>(type: "TEXT", nullable: false),
                    RutaFirmaImagen = table.Column<string>(type: "TEXT", nullable: true),
                    NombreConsultorio = table.Column<string>(type: "TEXT", nullable: true),
                    DireccionConsultorio = table.Column<string>(type: "TEXT", nullable: true),
                    TelefonoConsultorio = table.Column<string>(type: "TEXT", nullable: true),
                    EmailConsultorio = table.Column<string>(type: "TEXT", nullable: true),
                    RutaLogo = table.Column<string>(type: "TEXT", nullable: true),
                    CUIT = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Medicos", x => x.MedicoID);
                });

            migrationBuilder.CreateTable(
                name: "Pacientes",
                columns: table => new
                {
                    PacienteID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", nullable: true),
                    Apellido = table.Column<string>(type: "TEXT", nullable: true),
                    Dni = table.Column<string>(type: "TEXT", nullable: false),
                    FechaNacimiento = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Genero = table.Column<string>(type: "TEXT", nullable: true),
                    Direccion = table.Column<string>(type: "TEXT", nullable: true),
                    Telefono = table.Column<string>(type: "TEXT", nullable: true),
                    Email = table.Column<string>(type: "TEXT", nullable: true),
                    LugarNacimiento = table.Column<string>(type: "TEXT", nullable: true),
                    EstadoCivil = table.Column<string>(type: "TEXT", nullable: true),
                    Ocupacion = table.Column<string>(type: "TEXT", nullable: true),
                    UsuarioID = table.Column<int>(type: "INTEGER", nullable: false),
                    MedicoID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pacientes", x => x.PacienteID);
                    table.ForeignKey(
                        name: "FK_Pacientes_Medicos_MedicoID",
                        column: x => x.MedicoID,
                        principalTable: "Medicos",
                        principalColumn: "MedicoID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Citas",
                columns: table => new
                {
                    CitaID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FechaHora = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DuracionMinutos = table.Column<int>(type: "INTEGER", nullable: false),
                    Motivo = table.Column<string>(type: "TEXT", maxLength: 250, nullable: true),
                    Estado = table.Column<int>(type: "INTEGER", nullable: false),
                    Notas = table.Column<string>(type: "TEXT", nullable: true),
                    PacienteID = table.Column<int>(type: "INTEGER", nullable: false),
                    MedicoID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Citas", x => x.CitaID);
                    table.ForeignKey(
                        name: "FK_Citas_Medicos_MedicoID",
                        column: x => x.MedicoID,
                        principalTable: "Medicos",
                        principalColumn: "MedicoID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Citas_Pacientes_PacienteID",
                        column: x => x.PacienteID,
                        principalTable: "Pacientes",
                        principalColumn: "PacienteID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FichasClinicas",
                columns: table => new
                {
                    FichaClinicaID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AntecedentesPersonales = table.Column<string>(type: "TEXT", nullable: true),
                    AntecedentesFamiliares = table.Column<string>(type: "TEXT", nullable: true),
                    Notas = table.Column<string>(type: "TEXT", nullable: true),
                    ExamenFisico = table.Column<string>(type: "TEXT", nullable: true),
                    PacienteID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FichasClinicas", x => x.FichaClinicaID);
                    table.ForeignKey(
                        name: "FK_FichasClinicas_Pacientes_PacienteID",
                        column: x => x.PacienteID,
                        principalTable: "Pacientes",
                        principalColumn: "PacienteID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Consultas",
                columns: table => new
                {
                    ConsultaID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CitaID = table.Column<int>(type: "INTEGER", nullable: false),
                    Fecha = table.Column<DateTime>(type: "TEXT", nullable: false),
                    MotivoConsulta = table.Column<string>(type: "TEXT", nullable: true),
                    RutaFoto = table.Column<string>(type: "TEXT", nullable: true),
                    Diagnostico = table.Column<string>(type: "TEXT", nullable: true),
                    PacienteID = table.Column<int>(type: "INTEGER", nullable: false),
                    MedicoID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Consultas", x => x.ConsultaID);
                    table.ForeignKey(
                        name: "FK_Consultas_Citas_CitaID",
                        column: x => x.CitaID,
                        principalTable: "Citas",
                        principalColumn: "CitaID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Consultas_Medicos_MedicoID",
                        column: x => x.MedicoID,
                        principalTable: "Medicos",
                        principalColumn: "MedicoID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Consultas_Pacientes_PacienteID",
                        column: x => x.PacienteID,
                        principalTable: "Pacientes",
                        principalColumn: "PacienteID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ConsultaFotos",
                columns: table => new
                {
                    ConsultaFotoID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RutaFoto = table.Column<string>(type: "TEXT", nullable: false),
                    ConsultaId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsultaFotos", x => x.ConsultaFotoID);
                    table.ForeignKey(
                        name: "FK_ConsultaFotos_Consultas_ConsultaId",
                        column: x => x.ConsultaId,
                        principalTable: "Consultas",
                        principalColumn: "ConsultaID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrdenMedica",
                columns: table => new
                {
                    OrdenMedicaID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TipoOrden = table.Column<string>(type: "TEXT", nullable: true),
                    Descripcion = table.Column<string>(type: "TEXT", nullable: true),
                    ConsultaID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdenMedica", x => x.OrdenMedicaID);
                    table.ForeignKey(
                        name: "FK_OrdenMedica_Consultas_ConsultaID",
                        column: x => x.ConsultaID,
                        principalTable: "Consultas",
                        principalColumn: "ConsultaID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Prescripcion",
                columns: table => new
                {
                    PrescripcionId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Medicamento = table.Column<string>(type: "TEXT", nullable: true),
                    Dosis = table.Column<string>(type: "TEXT", nullable: true),
                    Instrucciones = table.Column<string>(type: "TEXT", nullable: true),
                    ConsultaID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prescripcion", x => x.PrescripcionId);
                    table.ForeignKey(
                        name: "FK_Prescripcion_Consultas_ConsultaID",
                        column: x => x.ConsultaID,
                        principalTable: "Consultas",
                        principalColumn: "ConsultaID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Citas_MedicoID",
                table: "Citas",
                column: "MedicoID");

            migrationBuilder.CreateIndex(
                name: "IX_Citas_PacienteID",
                table: "Citas",
                column: "PacienteID");

            migrationBuilder.CreateIndex(
                name: "IX_ConsultaFotos_ConsultaId",
                table: "ConsultaFotos",
                column: "ConsultaId");

            migrationBuilder.CreateIndex(
                name: "IX_Consultas_CitaID",
                table: "Consultas",
                column: "CitaID");

            migrationBuilder.CreateIndex(
                name: "IX_Consultas_MedicoID",
                table: "Consultas",
                column: "MedicoID");

            migrationBuilder.CreateIndex(
                name: "IX_Consultas_PacienteID",
                table: "Consultas",
                column: "PacienteID");

            migrationBuilder.CreateIndex(
                name: "IX_FichasClinicas_PacienteID",
                table: "FichasClinicas",
                column: "PacienteID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrdenMedica_ConsultaID",
                table: "OrdenMedica",
                column: "ConsultaID");

            migrationBuilder.CreateIndex(
                name: "IX_Pacientes_MedicoID",
                table: "Pacientes",
                column: "MedicoID");

            migrationBuilder.CreateIndex(
                name: "IX_Prescripcion_ConsultaID",
                table: "Prescripcion",
                column: "ConsultaID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConsultaFotos");

            migrationBuilder.DropTable(
                name: "FichasClinicas");

            migrationBuilder.DropTable(
                name: "OrdenMedica");

            migrationBuilder.DropTable(
                name: "Prescripcion");

            migrationBuilder.DropTable(
                name: "Consultas");

            migrationBuilder.DropTable(
                name: "Citas");

            migrationBuilder.DropTable(
                name: "Pacientes");

            migrationBuilder.DropTable(
                name: "Medicos");
        }
    }
}
