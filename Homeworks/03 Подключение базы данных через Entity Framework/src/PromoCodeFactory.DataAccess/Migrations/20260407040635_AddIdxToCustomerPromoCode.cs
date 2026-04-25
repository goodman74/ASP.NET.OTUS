using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PromoCodeFactory.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddIdxToCustomerPromoCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CustomerPromoCodes_CustomerId",
                table: "CustomerPromoCodes");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerPromoCodes_CustomerId_PromoCodeId",
                table: "CustomerPromoCodes",
                columns: new[] { "CustomerId", "PromoCodeId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CustomerPromoCodes_CustomerId_PromoCodeId",
                table: "CustomerPromoCodes");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerPromoCodes_CustomerId",
                table: "CustomerPromoCodes",
                column: "CustomerId");
        }
    }
}
