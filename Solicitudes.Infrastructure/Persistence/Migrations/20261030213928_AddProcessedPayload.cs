using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Solicitudes.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProcessedPayload : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProcessedPayload",
                table: "Requests",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProcessedPayload",
                table: "Requests");
        }
    }
}
