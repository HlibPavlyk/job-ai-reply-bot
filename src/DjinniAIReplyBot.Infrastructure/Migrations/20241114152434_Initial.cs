using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DjinniAIReplyBot.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "user_configurations",
                columns: table => new
                {
                    chat_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    reply_language = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false, defaultValue: "En"),
                    parsed_resume = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    additional_configuration = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_configurations", x => x.chat_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_user_configurations_chat_id",
                table: "user_configurations",
                column: "chat_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_configurations_username",
                table: "user_configurations",
                column: "username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_configurations");
        }
    }
}
