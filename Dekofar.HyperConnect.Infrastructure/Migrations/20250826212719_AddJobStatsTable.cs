using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dekofar.HyperConnect.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddJobStatsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "JobStats",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PaidMarked = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CancelTagged = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobStats", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobStats");
        }
    }
}
