using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExamTimeTable.Data.Migrations
{
    /// <inheritdoc />
    public partial class Room_Invigilator_Update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rooms_Invigilators_InvigilatorId",
                table: "Rooms");

            migrationBuilder.DropIndex(
                name: "IX_Rooms_InvigilatorId",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "InvigilatorId",
                table: "Rooms");

            migrationBuilder.AddColumn<int>(
                name: "RoomId",
                table: "Invigilators",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ExamDate",
                table: "Exams",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.CreateIndex(
                name: "IX_Invigilators_RoomId",
                table: "Invigilators",
                column: "RoomId");

            migrationBuilder.AddForeignKey(
                name: "FK_Invigilators_Rooms_RoomId",
                table: "Invigilators",
                column: "RoomId",
                principalTable: "Rooms",
                principalColumn: "RoomId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invigilators_Rooms_RoomId",
                table: "Invigilators");

            migrationBuilder.DropIndex(
                name: "IX_Invigilators_RoomId",
                table: "Invigilators");

            migrationBuilder.DropColumn(
                name: "RoomId",
                table: "Invigilators");

            migrationBuilder.AddColumn<int>(
                name: "InvigilatorId",
                table: "Rooms",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "ExamDate",
                table: "Exams",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_InvigilatorId",
                table: "Rooms",
                column: "InvigilatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Rooms_Invigilators_InvigilatorId",
                table: "Rooms",
                column: "InvigilatorId",
                principalTable: "Invigilators",
                principalColumn: "InvigilatorId");
        }
    }
}
