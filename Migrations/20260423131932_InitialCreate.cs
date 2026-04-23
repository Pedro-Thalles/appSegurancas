using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace appSegurancas.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Segurancas",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    sobreNome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    cpf = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: false),
                    passwordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    matricula = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    isApproved = table.Column<bool>(type: "bit", nullable: false),
                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Segurancas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Contracheques",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    segurancaId = table.Column<int>(type: "int", nullable: false),
                    MesUpload = table.Column<int>(type: "int", nullable: false),
                    AnoUpload = table.Column<int>(type: "int", nullable: false),
                    filePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contracheques", x => x.id);
                    table.ForeignKey(
                        name: "FK_Contracheques_Segurancas_segurancaId",
                        column: x => x.segurancaId,
                        principalTable: "Segurancas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Contracheques_segurancaId",
                table: "Contracheques",
                column: "segurancaId");

            migrationBuilder.CreateIndex(
                name: "IX_Segurancas_cpf",
                table: "Segurancas",
                column: "cpf",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Segurancas_matricula",
                table: "Segurancas",
                column: "matricula",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Contracheques");

            migrationBuilder.DropTable(
                name: "Segurancas");
        }
    }
}
