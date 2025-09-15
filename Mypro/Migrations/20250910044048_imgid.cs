using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mypro.Migrations
{
    /// <inheritdoc />
    public partial class imgid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "Wishlist",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "ImageId",
                table: "Wishlist",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_Wishlist_ImageId",
                table: "Wishlist",
                column: "ImageId");

            migrationBuilder.AddForeignKey(
                name: "FK_Wishlist_CategoryImages_ImageId",
                table: "Wishlist",
                column: "ImageId",
                principalTable: "CategoryImages",
                principalColumn: "ImageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Wishlist_CategoryImages_ImageId",
                table: "Wishlist");

            migrationBuilder.DropIndex(
                name: "IX_Wishlist_ImageId",
                table: "Wishlist");

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "Wishlist",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ImageId",
                table: "Wishlist",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
