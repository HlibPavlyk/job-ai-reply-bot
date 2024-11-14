using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DjinniAIReplyBot.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIsAcceptedField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_accepted",
                table: "user_configurations",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_accepted",
                table: "user_configurations");
        }
    }
}
