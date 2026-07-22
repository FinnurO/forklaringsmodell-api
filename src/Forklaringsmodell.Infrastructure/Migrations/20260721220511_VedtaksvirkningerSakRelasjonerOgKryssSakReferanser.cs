using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Forklaringsmodell.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class VedtaksvirkningerSakRelasjonerOgKryssSakReferanser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TjenesteReferanse",
                table: "Saker",
                newName: "CpsvTjenesteReferanse");

            migrationBuilder.RenameColumn(
                name: "CpsvRuleReferanse",
                table: "Regler",
                newName: "CpsvRegelReferanse");

            migrationBuilder.RenameColumn(
                name: "CpsvReferanse",
                table: "Kilder",
                newName: "CccevReferanse");

            migrationBuilder.AddColumn<string>(
                name: "UtlosendeHendelse",
                table: "Saker",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "SakRelasjoner",
                columns: table => new
                {
                    RelasjonId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SakId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RelatertSakId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Type = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SakRelasjoner", x => x.RelasjonId);
                    table.ForeignKey(
                        name: "FK_SakRelasjoner_Saker_RelatertSakId",
                        column: x => x.RelatertSakId,
                        principalTable: "Saker",
                        principalColumn: "SakId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SakRelasjoner_Saker_SakId",
                        column: x => x.SakId,
                        principalTable: "Saker",
                        principalColumn: "SakId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Vedtaksvirkninger",
                columns: table => new
                {
                    VirkningId = table.Column<Guid>(type: "TEXT", nullable: false),
                    VedtakId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Type = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Beskrivelse = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Varighet = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    GyldigFra = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    GyldigTil = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    Belop = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: true),
                    LopendeVilkar = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    RapporteringsFrekvens = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vedtaksvirkninger", x => x.VirkningId);
                    table.ForeignKey(
                        name: "FK_Vedtaksvirkninger_Vedtak_VedtakId",
                        column: x => x.VedtakId,
                        principalTable: "Vedtak",
                        principalColumn: "VedtakId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VurderingReferanse",
                columns: table => new
                {
                    VurderingId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RefererteVurderingId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VurderingReferanse", x => new { x.VurderingId, x.RefererteVurderingId });
                    table.ForeignKey(
                        name: "FK_VurderingReferanse_Vurderinger_RefererteVurderingId",
                        column: x => x.RefererteVurderingId,
                        principalTable: "Vurderinger",
                        principalColumn: "VurderingId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VurderingReferanse_Vurderinger_VurderingId",
                        column: x => x.VurderingId,
                        principalTable: "Vurderinger",
                        principalColumn: "VurderingId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VedtaksvirkningFaktum",
                columns: table => new
                {
                    VirkningId = table.Column<Guid>(type: "TEXT", nullable: false),
                    FaktumId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VedtaksvirkningFaktum", x => new { x.VirkningId, x.FaktumId });
                    table.ForeignKey(
                        name: "FK_VedtaksvirkningFaktum_Faktum_FaktumId",
                        column: x => x.FaktumId,
                        principalTable: "Faktum",
                        principalColumn: "FaktumId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VedtaksvirkningFaktum_Vedtaksvirkninger_VirkningId",
                        column: x => x.VirkningId,
                        principalTable: "Vedtaksvirkninger",
                        principalColumn: "VirkningId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VedtaksvirkningVurdering",
                columns: table => new
                {
                    VirkningId = table.Column<Guid>(type: "TEXT", nullable: false),
                    VurderingId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VedtaksvirkningVurdering", x => new { x.VirkningId, x.VurderingId });
                    table.ForeignKey(
                        name: "FK_VedtaksvirkningVurdering_Vedtaksvirkninger_VirkningId",
                        column: x => x.VirkningId,
                        principalTable: "Vedtaksvirkninger",
                        principalColumn: "VirkningId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VedtaksvirkningVurdering_Vurderinger_VurderingId",
                        column: x => x.VurderingId,
                        principalTable: "Vurderinger",
                        principalColumn: "VurderingId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SakRelasjoner_RelatertSakId",
                table: "SakRelasjoner",
                column: "RelatertSakId");

            migrationBuilder.CreateIndex(
                name: "IX_SakRelasjoner_SakId",
                table: "SakRelasjoner",
                column: "SakId");

            migrationBuilder.CreateIndex(
                name: "IX_Vedtaksvirkninger_VedtakId",
                table: "Vedtaksvirkninger",
                column: "VedtakId");

            migrationBuilder.CreateIndex(
                name: "IX_VedtaksvirkningFaktum_FaktumId",
                table: "VedtaksvirkningFaktum",
                column: "FaktumId");

            migrationBuilder.CreateIndex(
                name: "IX_VedtaksvirkningVurdering_VurderingId",
                table: "VedtaksvirkningVurdering",
                column: "VurderingId");

            migrationBuilder.CreateIndex(
                name: "IX_VurderingReferanse_RefererteVurderingId",
                table: "VurderingReferanse",
                column: "RefererteVurderingId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SakRelasjoner");

            migrationBuilder.DropTable(
                name: "VedtaksvirkningFaktum");

            migrationBuilder.DropTable(
                name: "VedtaksvirkningVurdering");

            migrationBuilder.DropTable(
                name: "VurderingReferanse");

            migrationBuilder.DropTable(
                name: "Vedtaksvirkninger");

            migrationBuilder.DropColumn(
                name: "UtlosendeHendelse",
                table: "Saker");

            migrationBuilder.RenameColumn(
                name: "CpsvTjenesteReferanse",
                table: "Saker",
                newName: "TjenesteReferanse");

            migrationBuilder.RenameColumn(
                name: "CpsvRegelReferanse",
                table: "Regler",
                newName: "CpsvRuleReferanse");

            migrationBuilder.RenameColumn(
                name: "CccevReferanse",
                table: "Kilder",
                newName: "CpsvReferanse");
        }
    }
}
