using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EgyWonders.Migrations
{
    /// <inheritdoc />
    public partial class FixPaymentMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Drop the old, conflicting Foreign Keys
            migrationBuilder.DropForeignKey(
                name: "FK__Payments__BookID__01142BA1",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK__Payments__Bookin__02084FDA",
                table: "Payments");

            // 2. DROP the duplicate/bad column 'BookingID' entirely
            // (Do not rename it, just get rid of it)
            migrationBuilder.DropColumn(
                name: "BookingID",
                table: "Payments");

            // 3. COMMENTED OUT to prevent "Index not found" error
            // We will fix the 'int?' to 'int' conversion manually in SQL if needed
            /*
            migrationBuilder.AlterColumn<int>(
                name: "BookID",
                table: "Payments",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
            */

            // 4. Re-add the correct Foreign Key to 'BookID'
            // We give it a clean name "FK_Payments_ListingBooking"
            migrationBuilder.AddForeignKey(
                name: "FK_Payments_ListingBooking",
                table: "Payments",
                column: "BookID",
                principalTable: "ListingBooking",
                principalColumn: "BookID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Logic to undo changes if necessary
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_ListingBooking",
                table: "Payments");

            migrationBuilder.AddColumn<int>(
                name: "BookingID",
                table: "Payments",
                type: "int",
                nullable: true);
        }
    }
}