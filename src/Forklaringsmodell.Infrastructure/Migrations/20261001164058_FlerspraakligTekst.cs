using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Forklaringsmodell.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FlerspraakligTekst : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StandardTekst",
                table: "Vilkar");

            migrationBuilder.DropColumn(
                name: "Beskrivelse",
                table: "Vedtaksvirkninger");

            migrationBuilder.DropColumn(
                name: "LopendeVilkar",
                table: "Vedtaksvirkninger");

            migrationBuilder.RenameColumn(
                name: "Hovedhensyn",
                table: "Vurderinger",
                newName: "HovedhensynTekstId");

            migrationBuilder.RenameColumn(
                name: "ForkastedeUtfall",
                table: "Vurderinger",
                newName: "ForkastedeUtfallTekstId");

            migrationBuilder.AddColumn<Guid>(
                name: "StandardTekstId",
                table: "Vilkar",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BeskrivelseTekstId",
                table: "Vedtaksvirkninger",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "LopendeVilkarTekstId",
                table: "Vedtaksvirkninger",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FlerspraakligeTekster",
                columns: table => new
                {
                    FlerspraakligTekstId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlerspraakligeTekster", x => x.FlerspraakligTekstId);
                });

            migrationBuilder.CreateTable(
                name: "TekstVarianter",
                columns: table => new
                {
                    FlerspraakligTekstId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SpraakKode = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    Verdi = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TekstVarianter", x => new { x.FlerspraakligTekstId, x.SpraakKode });
                    table.ForeignKey(
                        name: "FK_TekstVarianter_FlerspraakligeTekster_FlerspraakligTekstId",
                        column: x => x.FlerspraakligTekstId,
                        principalTable: "FlerspraakligeTekster",
                        principalColumn: "FlerspraakligTekstId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Vurderinger_ForkastedeUtfallTekstId",
                table: "Vurderinger",
                column: "ForkastedeUtfallTekstId");

            migrationBuilder.CreateIndex(
                name: "IX_Vurderinger_HovedhensynTekstId",
                table: "Vurderinger",
                column: "HovedhensynTekstId");

            migrationBuilder.CreateIndex(
                name: "IX_Vilkar_StandardTekstId",
                table: "Vilkar",
                column: "StandardTekstId");

            migrationBuilder.CreateIndex(
                name: "IX_Vedtaksvirkninger_BeskrivelseTekstId",
                table: "Vedtaksvirkninger",
                column: "BeskrivelseTekstId");

            migrationBuilder.CreateIndex(
                name: "IX_Vedtaksvirkninger_LopendeVilkarTekstId",
                table: "Vedtaksvirkninger",
                column: "LopendeVilkarTekstId");

            migrationBuilder.AddForeignKey(
                name: "FK_Vedtaksvirkninger_FlerspraakligeTekster_BeskrivelseTekstId",
                table: "Vedtaksvirkninger",
                column: "BeskrivelseTekstId",
                principalTable: "FlerspraakligeTekster",
                principalColumn: "FlerspraakligTekstId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Vedtaksvirkninger_FlerspraakligeTekster_LopendeVilkarTekstId",
                table: "Vedtaksvirkninger",
                column: "LopendeVilkarTekstId",
                principalTable: "FlerspraakligeTekster",
                principalColumn: "FlerspraakligTekstId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Vilkar_FlerspraakligeTekster_StandardTekstId",
                table: "Vilkar",
                column: "StandardTekstId",
                principalTable: "FlerspraakligeTekster",
                principalColumn: "FlerspraakligTekstId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Vurderinger_FlerspraakligeTekster_ForkastedeUtfallTekstId",
                table: "Vurderinger",
                column: "ForkastedeUtfallTekstId",
                principalTable: "FlerspraakligeTekster",
                principalColumn: "FlerspraakligTekstId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Vurderinger_FlerspraakligeTekster_HovedhensynTekstId",
                table: "Vurderinger",
                column: "HovedhensynTekstId",
                principalTable: "FlerspraakligeTekster",
                principalColumn: "FlerspraakligTekstId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vedtaksvirkninger_FlerspraakligeTekster_BeskrivelseTekstId",
                table: "Vedtaksvirkninger");

            migrationBuilder.DropForeignKey(
                name: "FK_Vedtaksvirkninger_FlerspraakligeTekster_LopendeVilkarTekstId",
                table: "Vedtaksvirkninger");

            migrationBuilder.DropForeignKey(
                name: "FK_Vilkar_FlerspraakligeTekster_StandardTekstId",
                table: "Vilkar");

            migrationBuilder.DropForeignKey(
                name: "FK_Vurderinger_FlerspraakligeTekster_ForkastedeUtfallTekstId",
                table: "Vurderinger");

            migrationBuilder.DropForeignKey(
                name: "FK_Vurderinger_FlerspraakligeTekster_HovedhensynTekstId",
                table: "Vurderinger");

            migrationBuilder.DropTable(
                name: "TekstVarianter");

            migrationBuilder.DropTable(
                name: "FlerspraakligeTekster");

            migrationBuilder.DropIndex(
                name: "IX_Vurderinger_ForkastedeUtfallTekstId",
                table: "Vurderinger");

            migrationBuilder.DropIndex(
                name: "IX_Vurderinger_HovedhensynTekstId",
                table: "Vurderinger");

            migrationBuilder.DropIndex(
                name: "IX_Vilkar_StandardTekstId",
                table: "Vilkar");

            migrationBuilder.DropIndex(
                name: "IX_Vedtaksvirkninger_BeskrivelseTekstId",
                table: "Vedtaksvirkninger");

            migrationBuilder.DropIndex(
                name: "IX_Vedtaksvirkninger_LopendeVilkarTekstId",
                table: "Vedtaksvirkninger");

            migrationBuilder.DropColumn(
                name: "StandardTekstId",
                table: "Vilkar");

            migrationBuilder.DropColumn(
                name: "BeskrivelseTekstId",
                table: "Vedtaksvirkninger");

            migrationBuilder.DropColumn(
                name: "LopendeVilkarTekstId",
                table: "Vedtaksvirkninger");

            migrationBuilder.RenameColumn(
                name: "HovedhensynTekstId",
                table: "Vurderinger",
                newName: "Hovedhensyn");

            migrationBuilder.RenameColumn(
                name: "ForkastedeUtfallTekstId",
                table: "Vurderinger",
                newName: "ForkastedeUtfall");

            migrationBuilder.AddColumn<string>(
                name: "StandardTekst",
                table: "Vilkar",
                type: "TEXT",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Beskrivelse",
                table: "Vedtaksvirkninger",
                type: "TEXT",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LopendeVilkar",
                table: "Vedtaksvirkninger",
                type: "TEXT",
                maxLength: 500,
                nullable: true);
        }
    }
}
