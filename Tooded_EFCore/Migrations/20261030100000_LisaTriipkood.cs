using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tooded_EFCore.Migrations
{
    /// <inheritdoc />
    public partial class LisaTriipkood : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Triipkood",
                table: "Toodetabel",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Toodetabel_Triipkood",
                table: "Toodetabel",
                column: "Triipkood",
                unique: true,
                filter: "[Triipkood] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Toodetabel_Triipkood",
                table: "Toodetabel");

            migrationBuilder.DropColumn(
                name: "Triipkood",
                table: "Toodetabel");
        }
    }
}
