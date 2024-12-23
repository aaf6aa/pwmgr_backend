using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pwmgr_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddNotesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Notes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    EncryptedMetadata = table.Column<string>(type: "text", nullable: false),
                    EncryptedBody = table.Column<string>(type: "text", nullable: false),
                    EncryptedBodyKey = table.Column<string>(type: "text", nullable: false),
                    HkdfSalt = table.Column<string>(type: "text", nullable: false),
                    TitleHash = table.Column<string>(type: "text", nullable: false),
                    Hmac = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notes_UserId_TitleHash",
                table: "Notes",
                columns: new[] { "UserId", "TitleHash" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notes");
        }
    }
}
