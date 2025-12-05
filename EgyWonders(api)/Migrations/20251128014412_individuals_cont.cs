using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EgyWonders.Migrations
{
    /// <inheritdoc />
    public partial class individuals_cont : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NumberOfGuests",
                table: "ListingBooking",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NumberOfGuests",
                table: "ListingBooking");
        }
    }
}
