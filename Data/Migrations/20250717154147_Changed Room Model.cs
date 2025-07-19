using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExamTimeTable.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangedRoomModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Block",
                table: "Rooms",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Block",
                table: "Rooms");
        }
    }
}
