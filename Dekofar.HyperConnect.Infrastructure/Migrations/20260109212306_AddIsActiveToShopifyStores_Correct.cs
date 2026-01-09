using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dekofar.HyperConnect.Infrastructure.Migrations
{
    public partial class AddIsActiveToShopifyStores_Correct : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "ShopifyStores",
                type: "boolean",
                nullable: false,
                defaultValue: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "ShopifyStores");
        }
    }
}
