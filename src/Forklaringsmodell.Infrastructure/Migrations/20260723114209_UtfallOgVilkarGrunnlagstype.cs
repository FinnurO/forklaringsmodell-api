using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Forklaringsmodell.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UtfallOgVilkarGrunnlagstype : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Utfall",
                table: "Vurderinger",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CpsvTjenesteReferanse",
                table: "Vilkar",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Grunnlagstype",
                table: "Vilkar",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Kode",
                table: "Vilkar",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Kodeverk",
                table: "Vilkar",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegeldefinisjonReferanse",
                table: "Regler",
                type: "TEXT",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Utfall",
                table: "Vurderinger");

            migrationBuilder.DropColumn(
                name: "CpsvTjenesteReferanse",
                table: "Vilkar");

            migrationBuilder.DropColumn(
                name: "Grunnlagstype",
                table: "Vilkar");

            migrationBuilder.DropColumn(
                name: "Kode",
                table: "Vilkar");

            migrationBuilder.DropColumn(
                name: "Kodeverk",
                table: "Vilkar");

            migrationBuilder.DropColumn(
                name: "RegeldefinisjonReferanse",
                table: "Regler");
        }
    }
}
