using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoMed.Migrations
{
    /// <inheritdoc />
    public partial class CitaOpcional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Consultas_Citas_CitaID",
                table: "Consultas");

            migrationBuilder.AlterColumn<int>(
                name: "CitaID",
                table: "Consultas",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddForeignKey(
                name: "FK_Consultas_Citas_CitaID",
                table: "Consultas",
                column: "CitaID",
                principalTable: "Citas",
                principalColumn: "CitaID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Consultas_Citas_CitaID",
                table: "Consultas");

            migrationBuilder.AlterColumn<int>(
                name: "CitaID",
                table: "Consultas",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Consultas_Citas_CitaID",
                table: "Consultas",
                column: "CitaID",
                principalTable: "Citas",
                principalColumn: "CitaID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
