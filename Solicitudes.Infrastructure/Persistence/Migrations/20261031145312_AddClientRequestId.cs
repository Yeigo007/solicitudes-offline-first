using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Solicitudes.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddClientRequestId : Migration
    {
        /// <inheritdoc />
        
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "ClientRequestId",
            table: "Requests",
            type: "uuid",
            nullable: true);

        migrationBuilder.Sql(
            """
            UPDATE "Requests"
            SET "ClientRequestId" = gen_random_uuid()
            WHERE "ClientRequestId" IS NULL;
            """);

        migrationBuilder.AlterColumn<Guid>(
            name: "ClientRequestId",
            table: "Requests",
            type: "uuid",
            nullable: false,
            oldClrType: typeof(Guid),
            oldType: "uuid",
            oldNullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_Requests_ClientRequestId",
            table: "Requests",
            column: "ClientRequestId",
            unique: true);
    }


        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Requests_ClientRequestId",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "ClientRequestId",
                table: "Requests");
        }
    }
}
