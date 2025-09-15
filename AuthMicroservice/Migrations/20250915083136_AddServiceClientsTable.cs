using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthMicroservice.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceClientsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ServiceClients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClientId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClientSecretHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AllowedScopes = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceClients", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceClients_ClientId",
                table: "ServiceClients",
                column: "ClientId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServiceClients");
        }
    }
}
