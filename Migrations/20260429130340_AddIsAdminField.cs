using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace appSegurancas.Migrations
{
    /// <inheritdoc />
    public partial class AddIsAdminField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MesUpload",
                table: "Contracheques",
                newName: "mesUpload");

            migrationBuilder.RenameColumn(
                name: "AnoUpload",
                table: "Contracheques",
                newName: "anoUpload");

            migrationBuilder.AddColumn<bool>(
                name: "IsAdmin",
                table: "Segurancas",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAdmin",
                table: "Segurancas");

            migrationBuilder.RenameColumn(
                name: "mesUpload",
                table: "Contracheques",
                newName: "MesUpload");

            migrationBuilder.RenameColumn(
                name: "anoUpload",
                table: "Contracheques",
                newName: "AnoUpload");
        }
    }
}
