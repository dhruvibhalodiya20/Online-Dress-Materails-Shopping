using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mypro.Migrations
{
    /// <inheritdoc />
    public partial class id : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Color",
                table: "Wishlist");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Wishlist");

            migrationBuilder.DropColumn(
                name: "ProductName",
                table: "Wishlist");

            migrationBuilder.AddColumn<int>(
                name: "ImageId",
                table: "Wishlist",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageId",
                table: "Wishlist");

            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "Wishlist",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Wishlist",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ProductName",
                table: "Wishlist",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
