using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DjinniAIReplyBot.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateResumefield : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "parsed_resume",
                table: "user_configurations",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldDefaultValue: "No content");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "parsed_resume",
                table: "user_configurations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "No content",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
