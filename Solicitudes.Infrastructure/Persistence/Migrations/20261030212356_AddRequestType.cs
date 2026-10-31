using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Solicitudes.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRequestType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Requests",
                type: "text",
                nullable: false,
                defaultValue: "TEXT");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "Requests");
        }
    }
}
