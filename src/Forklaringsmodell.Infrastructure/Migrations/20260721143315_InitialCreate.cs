using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Forklaringsmodell.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Kilder",
                columns: table => new
                {
                    KildeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Navn = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    Type = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Autoritativ = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kilder", x => x.KildeId);
                });

            migrationBuilder.CreateTable(
                name: "Rettskilder",
                columns: table => new
                {
                    RettskildeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Paragraf = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    VersjonDato = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    EliReferanse = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rettskilder", x => x.RettskildeId);
                });

            migrationBuilder.CreateTable(
                name: "Saker",
                columns: table => new
                {
                    SakId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Tittel = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Opprettet = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    SistEndret = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Saker", x => x.SakId);
                });

            migrationBuilder.CreateTable(
                name: "Regler",
                columns: table => new
                {
                    RegelId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RettskildeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Teknologi = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Type = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Regler", x => x.RegelId);
                    table.ForeignKey(
                        name: "FK_Regler_Rettskilder_RettskildeId",
                        column: x => x.RettskildeId,
                        principalTable: "Rettskilder",
                        principalColumn: "RettskildeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Faktum",
                columns: table => new
                {
                    FaktumId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SakId = table.Column<Guid>(type: "TEXT", nullable: false),
                    KildeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Type = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Struktur = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Verdi = table.Column<string>(type: "TEXT", nullable: false),
                    AvledetFraFaktumId = table.Column<Guid>(type: "TEXT", nullable: true),
                    InnhentetTidspunkt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Faktum", x => x.FaktumId);
                    table.ForeignKey(
                        name: "FK_Faktum_Faktum_AvledetFraFaktumId",
                        column: x => x.AvledetFraFaktumId,
                        principalTable: "Faktum",
                        principalColumn: "FaktumId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Faktum_Kilder_KildeId",
                        column: x => x.KildeId,
                        principalTable: "Kilder",
                        principalColumn: "KildeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Faktum_Saker_SakId",
                        column: x => x.SakId,
                        principalTable: "Saker",
                        principalColumn: "SakId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Partsmedvirkninger",
                columns: table => new
                {
                    MedvirkningId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SakId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Type = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Tidspunkt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    Innhold = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Partsmedvirkninger", x => x.MedvirkningId);
                    table.ForeignKey(
                        name: "FK_Partsmedvirkninger_Saker_SakId",
                        column: x => x.SakId,
                        principalTable: "Saker",
                        principalColumn: "SakId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Vedtak",
                columns: table => new
                {
                    VedtakId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SakId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Tidspunkt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    Utfall = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    AutomatiseringsGrad = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vedtak", x => x.VedtakId);
                    table.ForeignKey(
                        name: "FK_Vedtak_Saker_SakId",
                        column: x => x.SakId,
                        principalTable: "Saker",
                        principalColumn: "SakId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Vurderinger",
                columns: table => new
                {
                    VurderingId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SakId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RegelId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Type = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Beregningsspor = table.Column<string>(type: "TEXT", nullable: true),
                    Konfidens = table.Column<decimal>(type: "TEXT", precision: 3, scale: 2, nullable: true),
                    Eskalert = table.Column<bool>(type: "INTEGER", nullable: false),
                    Hovedhensyn = table.Column<string>(type: "TEXT", nullable: true),
                    ForkastedeUtfall = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vurderinger", x => x.VurderingId);
                    table.ForeignKey(
                        name: "FK_Vurderinger_Regler_RegelId",
                        column: x => x.RegelId,
                        principalTable: "Regler",
                        principalColumn: "RegelId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Vurderinger_Saker_SakId",
                        column: x => x.SakId,
                        principalTable: "Saker",
                        principalColumn: "SakId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Forklaringslogger",
                columns: table => new
                {
                    LoggId = table.Column<Guid>(type: "TEXT", nullable: false),
                    VedtakId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Forklaringslogger", x => x.LoggId);
                    table.ForeignKey(
                        name: "FK_Forklaringslogger_Vedtak_VedtakId",
                        column: x => x.VedtakId,
                        principalTable: "Vedtak",
                        principalColumn: "VedtakId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VurderingFaktum",
                columns: table => new
                {
                    VurderingId = table.Column<Guid>(type: "TEXT", nullable: false),
                    FaktumId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VurderingFaktum", x => new { x.VurderingId, x.FaktumId });
                    table.ForeignKey(
                        name: "FK_VurderingFaktum_Faktum_FaktumId",
                        column: x => x.FaktumId,
                        principalTable: "Faktum",
                        principalColumn: "FaktumId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VurderingFaktum_Vurderinger_VurderingId",
                        column: x => x.VurderingId,
                        principalTable: "Vurderinger",
                        principalColumn: "VurderingId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ForklaringsloggOppforinger",
                columns: table => new
                {
                    OppforingId = table.Column<Guid>(type: "TEXT", nullable: false),
                    LoggId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Type = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ReferanseId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForklaringsloggOppforinger", x => x.OppforingId);
                    table.ForeignKey(
                        name: "FK_ForklaringsloggOppforinger_Forklaringslogger_LoggId",
                        column: x => x.LoggId,
                        principalTable: "Forklaringslogger",
                        principalColumn: "LoggId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Faktum_AvledetFraFaktumId",
                table: "Faktum",
                column: "AvledetFraFaktumId");

            migrationBuilder.CreateIndex(
                name: "IX_Faktum_KildeId",
                table: "Faktum",
                column: "KildeId");

            migrationBuilder.CreateIndex(
                name: "IX_Faktum_SakId",
                table: "Faktum",
                column: "SakId");

            migrationBuilder.CreateIndex(
                name: "IX_Forklaringslogger_VedtakId",
                table: "Forklaringslogger",
                column: "VedtakId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ForklaringsloggOppforinger_LoggId",
                table: "ForklaringsloggOppforinger",
                column: "LoggId");

            migrationBuilder.CreateIndex(
                name: "IX_ForklaringsloggOppforinger_Type_ReferanseId",
                table: "ForklaringsloggOppforinger",
                columns: new[] { "Type", "ReferanseId" });

            migrationBuilder.CreateIndex(
                name: "IX_Partsmedvirkninger_SakId",
                table: "Partsmedvirkninger",
                column: "SakId");

            migrationBuilder.CreateIndex(
                name: "IX_Regler_RettskildeId",
                table: "Regler",
                column: "RettskildeId");

            migrationBuilder.CreateIndex(
                name: "IX_Vedtak_SakId",
                table: "Vedtak",
                column: "SakId");

            migrationBuilder.CreateIndex(
                name: "IX_Vurderinger_RegelId",
                table: "Vurderinger",
                column: "RegelId");

            migrationBuilder.CreateIndex(
                name: "IX_Vurderinger_SakId",
                table: "Vurderinger",
                column: "SakId");

            migrationBuilder.CreateIndex(
                name: "IX_VurderingFaktum_FaktumId",
                table: "VurderingFaktum",
                column: "FaktumId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ForklaringsloggOppforinger");

            migrationBuilder.DropTable(
                name: "Partsmedvirkninger");

            migrationBuilder.DropTable(
                name: "VurderingFaktum");

            migrationBuilder.DropTable(
                name: "Forklaringslogger");

            migrationBuilder.DropTable(
                name: "Faktum");

            migrationBuilder.DropTable(
                name: "Vurderinger");

            migrationBuilder.DropTable(
                name: "Vedtak");

            migrationBuilder.DropTable(
                name: "Kilder");

            migrationBuilder.DropTable(
                name: "Regler");

            migrationBuilder.DropTable(
                name: "Saker");

            migrationBuilder.DropTable(
                name: "Rettskilder");
        }
    }
}
