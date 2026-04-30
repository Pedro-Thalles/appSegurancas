using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace appSegurancas.Migrations
{
    /// <inheritdoc />
    public partial class AlterarStatusAprovacaoParaEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "isApproved",
                table: "Segurancas",
                type: "int",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "isApproved",
                table: "Segurancas",
                type: "bit",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
