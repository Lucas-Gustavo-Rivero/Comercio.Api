using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Comercio.Api.Migrations
{
    /// <inheritdoc />
    public partial class TablaCarrito : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CarritoId",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Carritos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Carritos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Carritos_Users_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CarritoItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CarritoId = table.Column<int>(type: "int", nullable: false),
                    ProductoId = table.Column<int>(type: "int", nullable: false),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ProductoId1 = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarritoItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CarritoItems_Carritos_CarritoId",
                        column: x => x.CarritoId,
                        principalTable: "Carritos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CarritoItems_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CarritoItems_Productos_ProductoId1",
                        column: x => x.ProductoId1,
                        principalTable: "Productos",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_CarritoId",
                table: "Users",
                column: "CarritoId");

            migrationBuilder.CreateIndex(
                name: "IX_CarritoItems_CarritoId_ProductoId",
                table: "CarritoItems",
                columns: new[] { "CarritoId", "ProductoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CarritoItems_ProductoId",
                table: "CarritoItems",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_CarritoItems_ProductoId1",
                table: "CarritoItems",
                column: "ProductoId1");

            migrationBuilder.CreateIndex(
                name: "IX_Carritos_UsuarioId",
                table: "Carritos",
                column: "UsuarioId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Carritos_CarritoId",
                table: "Users",
                column: "CarritoId",
                principalTable: "Carritos",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Carritos_CarritoId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "CarritoItems");

            migrationBuilder.DropTable(
                name: "Carritos");

            migrationBuilder.DropIndex(
                name: "IX_Users_CarritoId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CarritoId",
                table: "Users");
        }
    }
}
