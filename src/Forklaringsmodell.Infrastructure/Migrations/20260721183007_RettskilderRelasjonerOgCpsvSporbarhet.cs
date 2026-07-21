using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Forklaringsmodell.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RettskilderRelasjonerOgCpsvSporbarhet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Regler_Rettskilder_RettskildeId",
                table: "Regler");

            migrationBuilder.DropIndex(
                name: "IX_Regler_RettskildeId",
                table: "Regler");

            migrationBuilder.DropColumn(
                name: "RettskildeId",
                table: "Regler");

            migrationBuilder.RenameColumn(
                name: "Paragraf",
                table: "Rettskilder",
                newName: "Henvisning");

            migrationBuilder.AddColumn<string>(
                name: "TjenesteReferanse",
                table: "Saker",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "VersjonDato",
                table: "Rettskilder",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "EliReferanse",
                table: "Rettskilder",
                type: "TEXT",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 500);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Rettskilder",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CpsvRuleReferanse",
                table: "Regler",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CpsvReferanse",
                table: "Kilder",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FaktumRettskilde",
                columns: table => new
                {
                    FaktumId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RettskildeId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FaktumRettskilde", x => new { x.FaktumId, x.RettskildeId });
                    table.ForeignKey(
                        name: "FK_FaktumRettskilde_Faktum_FaktumId",
                        column: x => x.FaktumId,
                        principalTable: "Faktum",
                        principalColumn: "FaktumId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FaktumRettskilde_Rettskilder_RettskildeId",
                        column: x => x.RettskildeId,
                        principalTable: "Rettskilder",
                        principalColumn: "RettskildeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KildeRettskilde",
                columns: table => new
                {
                    KildeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RettskildeId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KildeRettskilde", x => new { x.KildeId, x.RettskildeId });
                    table.ForeignKey(
                        name: "FK_KildeRettskilde_Kilder_KildeId",
                        column: x => x.KildeId,
                        principalTable: "Kilder",
                        principalColumn: "KildeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_KildeRettskilde_Rettskilder_RettskildeId",
                        column: x => x.RettskildeId,
                        principalTable: "Rettskilder",
                        principalColumn: "RettskildeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RegelRettskilde",
                columns: table => new
                {
                    RegelId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RettskildeId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegelRettskilde", x => new { x.RegelId, x.RettskildeId });
                    table.ForeignKey(
                        name: "FK_RegelRettskilde_Regler_RegelId",
                        column: x => x.RegelId,
                        principalTable: "Regler",
                        principalColumn: "RegelId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RegelRettskilde_Rettskilder_RettskildeId",
                        column: x => x.RettskildeId,
                        principalTable: "Rettskilder",
                        principalColumn: "RettskildeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VurderingRettskilde",
                columns: table => new
                {
                    VurderingId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RettskildeId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VurderingRettskilde", x => new { x.VurderingId, x.RettskildeId });
                    table.ForeignKey(
                        name: "FK_VurderingRettskilde_Rettskilder_RettskildeId",
                        column: x => x.RettskildeId,
                        principalTable: "Rettskilder",
                        principalColumn: "RettskildeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VurderingRettskilde_Vurderinger_VurderingId",
                        column: x => x.VurderingId,
                        principalTable: "Vurderinger",
                        principalColumn: "VurderingId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FaktumRettskilde_RettskildeId",
                table: "FaktumRettskilde",
                column: "RettskildeId");

            migrationBuilder.CreateIndex(
                name: "IX_KildeRettskilde_RettskildeId",
                table: "KildeRettskilde",
                column: "RettskildeId");

            migrationBuilder.CreateIndex(
                name: "IX_RegelRettskilde_RettskildeId",
                table: "RegelRettskilde",
                column: "RettskildeId");

            migrationBuilder.CreateIndex(
                name: "IX_VurderingRettskilde_RettskildeId",
                table: "VurderingRettskilde",
                column: "RettskildeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FaktumRettskilde");

            migrationBuilder.DropTable(
                name: "KildeRettskilde");

            migrationBuilder.DropTable(
                name: "RegelRettskilde");

            migrationBuilder.DropTable(
                name: "VurderingRettskilde");

            migrationBuilder.DropColumn(
                name: "TjenesteReferanse",
                table: "Saker");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Rettskilder");

            migrationBuilder.DropColumn(
                name: "CpsvRuleReferanse",
                table: "Regler");

            migrationBuilder.DropColumn(
                name: "CpsvReferanse",
                table: "Kilder");

            migrationBuilder.RenameColumn(
                name: "Henvisning",
                table: "Rettskilder",
                newName: "Paragraf");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "VersjonDato",
                table: "Rettskilder",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                oldClrType: typeof(DateTimeOffset),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EliReferanse",
                table: "Rettskilder",
                type: "TEXT",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RettskildeId",
                table: "Regler",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Regler_RettskildeId",
                table: "Regler",
                column: "RettskildeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Regler_Rettskilder_RettskildeId",
                table: "Regler",
                column: "RettskildeId",
                principalTable: "Rettskilder",
                principalColumn: "RettskildeId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
