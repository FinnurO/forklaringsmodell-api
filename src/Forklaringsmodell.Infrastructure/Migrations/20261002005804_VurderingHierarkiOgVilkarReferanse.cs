using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Forklaringsmodell.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class VurderingHierarkiOgVilkarReferanse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ForelderVurderingId",
                table: "Vurderinger",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "VilkarId",
                table: "Vurderinger",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vurderinger_ForelderVurderingId",
                table: "Vurderinger",
                column: "ForelderVurderingId");

            migrationBuilder.CreateIndex(
                name: "IX_Vurderinger_VilkarId",
                table: "Vurderinger",
                column: "VilkarId");

            migrationBuilder.AddForeignKey(
                name: "FK_Vurderinger_Vilkar_VilkarId",
                table: "Vurderinger",
                column: "VilkarId",
                principalTable: "Vilkar",
                principalColumn: "VilkarId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Vurderinger_Vurderinger_ForelderVurderingId",
                table: "Vurderinger",
                column: "ForelderVurderingId",
                principalTable: "Vurderinger",
                principalColumn: "VurderingId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vurderinger_Vilkar_VilkarId",
                table: "Vurderinger");

            migrationBuilder.DropForeignKey(
                name: "FK_Vurderinger_Vurderinger_ForelderVurderingId",
                table: "Vurderinger");

            migrationBuilder.DropIndex(
                name: "IX_Vurderinger_ForelderVurderingId",
                table: "Vurderinger");

            migrationBuilder.DropIndex(
                name: "IX_Vurderinger_VilkarId",
                table: "Vurderinger");

            migrationBuilder.DropColumn(
                name: "ForelderVurderingId",
                table: "Vurderinger");

            migrationBuilder.DropColumn(
                name: "VilkarId",
                table: "Vurderinger");
        }
    }
}
