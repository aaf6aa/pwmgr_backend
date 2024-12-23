using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pwmgr_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EncryptedBodyKey",
                table: "Notes",
                newName: "EncryptedNoteKey");

            migrationBuilder.RenameColumn(
                name: "EncryptedBody",
                table: "Notes",
                newName: "EncryptedNote");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EncryptedNoteKey",
                table: "Notes",
                newName: "EncryptedBodyKey");

            migrationBuilder.RenameColumn(
                name: "EncryptedNote",
                table: "Notes",
                newName: "EncryptedBody");
        }
    }
}
