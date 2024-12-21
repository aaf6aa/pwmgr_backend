using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pwmgr_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceUsernameHash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PasswordEntries_UserId",
                table: "PasswordEntries");

            migrationBuilder.AddColumn<string>(
                name: "ServiceUsernameHash",
                table: "PasswordEntries",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordEntries_UserId_ServiceUsernameHash",
                table: "PasswordEntries",
                columns: new[] { "UserId", "ServiceUsernameHash" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PasswordEntries_UserId_ServiceUsernameHash",
                table: "PasswordEntries");

            migrationBuilder.DropColumn(
                name: "ServiceUsernameHash",
                table: "PasswordEntries");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordEntries_UserId",
                table: "PasswordEntries",
                column: "UserId");
        }
    }
}
