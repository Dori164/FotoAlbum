using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FotoAlbum.Migrations
{
    /// <inheritdoc />
    public partial class InitialDbCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Album",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Cim = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    Leiras = table.Column<string>(type: "TEXT", nullable: false),
                    Letrehozva = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Album", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Kep",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AlbumId = table.Column<int>(type: "INTEGER", nullable: false),
                    Cim = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    KepUrl = table.Column<string>(type: "TEXT", nullable: false),
                    Leiras = table.Column<string>(type: "TEXT", nullable: false),
                    FeltoltesDatuma = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kep", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Kep_Album_AlbumId",
                        column: x => x.AlbumId,
                        principalTable: "Album",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Kep_AlbumId",
                table: "Kep",
                column: "AlbumId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Kep");

            migrationBuilder.DropTable(
                name: "Album");
        }
    }
}
