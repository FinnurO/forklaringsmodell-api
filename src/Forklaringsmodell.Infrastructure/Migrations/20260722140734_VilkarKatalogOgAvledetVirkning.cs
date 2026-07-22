using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Forklaringsmodell.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class VilkarKatalogOgAvledetVirkning : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AvledetFraVirkningId",
                table: "Vedtaksvirkninger",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Fastsettelsesmate",
                table: "Vedtaksvirkninger",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "VilkarId",
                table: "Vedtaksvirkninger",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Vilkar",
                columns: table => new
                {
                    VilkarId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Navn = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    Type = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Fastsettelsesmate = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    StandardTekst = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    RegelId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vilkar", x => x.VilkarId);
                    table.ForeignKey(
                        name: "FK_Vilkar_Regler_RegelId",
                        column: x => x.RegelId,
                        principalTable: "Regler",
                        principalColumn: "RegelId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VilkarRettskilde",
                columns: table => new
                {
                    VilkarId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RettskildeId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VilkarRettskilde", x => new { x.VilkarId, x.RettskildeId });
                    table.ForeignKey(
                        name: "FK_VilkarRettskilde_Rettskilder_RettskildeId",
                        column: x => x.RettskildeId,
                        principalTable: "Rettskilder",
                        principalColumn: "RettskildeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VilkarRettskilde_Vilkar_VilkarId",
                        column: x => x.VilkarId,
                        principalTable: "Vilkar",
                        principalColumn: "VilkarId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Vedtaksvirkninger_AvledetFraVirkningId",
                table: "Vedtaksvirkninger",
                column: "AvledetFraVirkningId");

            migrationBuilder.CreateIndex(
                name: "IX_Vedtaksvirkninger_VilkarId",
                table: "Vedtaksvirkninger",
                column: "VilkarId");

            migrationBuilder.CreateIndex(
                name: "IX_Vilkar_RegelId",
                table: "Vilkar",
                column: "RegelId");

            migrationBuilder.CreateIndex(
                name: "IX_VilkarRettskilde_RettskildeId",
                table: "VilkarRettskilde",
                column: "RettskildeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Vedtaksvirkninger_Vedtaksvirkninger_AvledetFraVirkningId",
                table: "Vedtaksvirkninger",
                column: "AvledetFraVirkningId",
                principalTable: "Vedtaksvirkninger",
                principalColumn: "VirkningId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Vedtaksvirkninger_Vilkar_VilkarId",
                table: "Vedtaksvirkninger",
                column: "VilkarId",
                principalTable: "Vilkar",
                principalColumn: "VilkarId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vedtaksvirkninger_Vedtaksvirkninger_AvledetFraVirkningId",
                table: "Vedtaksvirkninger");

            migrationBuilder.DropForeignKey(
                name: "FK_Vedtaksvirkninger_Vilkar_VilkarId",
                table: "Vedtaksvirkninger");

            migrationBuilder.DropTable(
                name: "VilkarRettskilde");

            migrationBuilder.DropTable(
                name: "Vilkar");

            migrationBuilder.DropIndex(
                name: "IX_Vedtaksvirkninger_AvledetFraVirkningId",
                table: "Vedtaksvirkninger");

            migrationBuilder.DropIndex(
                name: "IX_Vedtaksvirkninger_VilkarId",
                table: "Vedtaksvirkninger");

            migrationBuilder.DropColumn(
                name: "AvledetFraVirkningId",
                table: "Vedtaksvirkninger");

            migrationBuilder.DropColumn(
                name: "Fastsettelsesmate",
                table: "Vedtaksvirkninger");

            migrationBuilder.DropColumn(
                name: "VilkarId",
                table: "Vedtaksvirkninger");
        }
    }
}
