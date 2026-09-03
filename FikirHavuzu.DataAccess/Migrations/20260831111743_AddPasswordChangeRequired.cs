using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FikirHavuzu.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddPasswordChangeRequired : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPasswordChangeRequired",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "IsPasswordChangeRequired", "RegistrationNumber" },
                values: new object[] { false, "adm001" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPasswordChangeRequired",
                table: "Users");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "RegistrationNumber",
                value: "ADM001");
        }
    }
}
