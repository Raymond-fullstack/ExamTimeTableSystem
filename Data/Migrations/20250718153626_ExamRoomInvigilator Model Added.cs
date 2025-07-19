using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExamTimeTable.Data.Migrations
{
    /// <inheritdoc />
    public partial class ExamRoomInvigilatorModelAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExamRoomInvigilators",
                columns: table => new
                {
                    ExamId = table.Column<int>(type: "int", nullable: false),
                    RoomId = table.Column<int>(type: "int", nullable: false),
                    InvigilatorId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamRoomInvigilators", x => new { x.ExamId, x.RoomId, x.InvigilatorId });
                    table.ForeignKey(
                        name: "FK_ExamRoomInvigilators_ExamRooms_ExamId_RoomId",
                        columns: x => new { x.ExamId, x.RoomId },
                        principalTable: "ExamRooms",
                        principalColumns: new[] { "ExamId", "RoomId" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExamRoomInvigilators_Invigilators_InvigilatorId",
                        column: x => x.InvigilatorId,
                        principalTable: "Invigilators",
                        principalColumn: "InvigilatorId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExamRoomInvigilators_InvigilatorId",
                table: "ExamRoomInvigilators",
                column: "InvigilatorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExamRoomInvigilators");
        }
    }
}
