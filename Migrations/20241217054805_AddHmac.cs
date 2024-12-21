using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pwmgr_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddHmac : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Hmac",
                table: "PasswordEntries",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Hmac",
                table: "PasswordEntries");
        }
    }
}
