using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pwmgr_backend.Migrations
{
    /// <inheritdoc />
    public partial class MasterSalt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Salt",
                table: "Users",
                newName: "MasterSalt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MasterSalt",
                table: "Users",
                newName: "Salt");
        }
    }
}
