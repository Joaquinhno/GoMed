using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoMed.Migrations
{
    /// <inheritdoc />
    public partial class PlanTratamiento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PlanTratamiento",
                table: "Consultas",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlanTratamiento",
                table: "Consultas");
        }
    }
}
