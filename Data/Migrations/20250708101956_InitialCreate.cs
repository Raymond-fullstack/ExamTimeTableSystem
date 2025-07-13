using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExamTimeTable.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DepartmentId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InvigilatorId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProfilePicture",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProgrammeId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StudentNumber",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "YearId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CourseUnits",
                columns: table => new
                {
                    CourseUnitId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    ExamId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseUnits", x => x.CourseUnitId);
                });

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    DepartmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.DepartmentId);
                });

            migrationBuilder.CreateTable(
                name: "Exams",
                columns: table => new
                {
                    ExamId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExamDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Duration = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    SpecialInstructions = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CourseUnitId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exams", x => x.ExamId);
                    table.ForeignKey(
                        name: "FK_Exams_CourseUnits_CourseUnitId",
                        column: x => x.CourseUnitId,
                        principalTable: "CourseUnits",
                        principalColumn: "CourseUnitId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Invigilators",
                columns: table => new
                {
                    InvigilatorId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DepartmentId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invigilators", x => x.InvigilatorId);
                    table.ForeignKey(
                        name: "FK_Invigilators_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "DepartmentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Years",
                columns: table => new
                {
                    YearId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Years", x => x.YearId);
                    table.ForeignKey(
                        name: "FK_Years_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "DepartmentId");
                });

            migrationBuilder.CreateTable(
                name: "Rooms",
                columns: table => new
                {
                    RoomId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Capacity = table.Column<int>(type: "int", nullable: false),
                    InvigilatorId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rooms", x => x.RoomId);
                    table.ForeignKey(
                        name: "FK_Rooms_Invigilators_InvigilatorId",
                        column: x => x.InvigilatorId,
                        principalTable: "Invigilators",
                        principalColumn: "InvigilatorId");
                });

            migrationBuilder.CreateTable(
                name: "Programmes",
                columns: table => new
                {
                    ProgrammeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NumberOfStudents = table.Column<int>(type: "int", nullable: true),
                    YearId = table.Column<int>(type: "int", nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Programmes", x => x.ProgrammeId);
                    table.ForeignKey(
                        name: "FK_Programmes_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "DepartmentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Programmes_Years_YearId",
                        column: x => x.YearId,
                        principalTable: "Years",
                        principalColumn: "YearId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExamRooms",
                columns: table => new
                {
                    ExamId = table.Column<int>(type: "int", nullable: false),
                    RoomId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamRooms", x => new { x.ExamId, x.RoomId });
                    table.ForeignKey(
                        name: "FK_ExamRooms_Exams_ExamId",
                        column: x => x.ExamId,
                        principalTable: "Exams",
                        principalColumn: "ExamId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExamRooms_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "RoomId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProgrammeCourseUnits",
                columns: table => new
                {
                    ProgrammeId = table.Column<int>(type: "int", nullable: false),
                    CourseUnitId = table.Column<int>(type: "int", nullable: false),
                    YearId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgrammeCourseUnits", x => new { x.ProgrammeId, x.CourseUnitId, x.YearId });
                    table.ForeignKey(
                        name: "FK_ProgrammeCourseUnits_CourseUnits_CourseUnitId",
                        column: x => x.CourseUnitId,
                        principalTable: "CourseUnits",
                        principalColumn: "CourseUnitId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProgrammeCourseUnits_Programmes_ProgrammeId",
                        column: x => x.ProgrammeId,
                        principalTable: "Programmes",
                        principalColumn: "ProgrammeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProgrammeCourseUnits_Years_YearId",
                        column: x => x.YearId,
                        principalTable: "Years",
                        principalColumn: "YearId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SubjectCombinations",
                columns: table => new
                {
                    SubjectCombinationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    NumberOfStudents = table.Column<int>(type: "int", nullable: false),
                    ProgrammeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubjectCombinations", x => x.SubjectCombinationId);
                    table.ForeignKey(
                        name: "FK_SubjectCombinations_Programmes_ProgrammeId",
                        column: x => x.ProgrammeId,
                        principalTable: "Programmes",
                        principalColumn: "ProgrammeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DepartmentSubjectCombinations",
                columns: table => new
                {
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    SubjectCombinationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepartmentSubjectCombinations", x => new { x.DepartmentId, x.SubjectCombinationId });
                    table.ForeignKey(
                        name: "FK_DepartmentSubjectCombinations_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "DepartmentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DepartmentSubjectCombinations_SubjectCombinations_SubjectCombinationId",
                        column: x => x.SubjectCombinationId,
                        principalTable: "SubjectCombinations",
                        principalColumn: "SubjectCombinationId");
                });

            migrationBuilder.CreateTable(
                name: "SubjectCombinationCourseUnits",
                columns: table => new
                {
                    SubjectCombinationId = table.Column<int>(type: "int", nullable: false),
                    CourseUnitId = table.Column<int>(type: "int", nullable: false),
                    YearId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubjectCombinationCourseUnits", x => new { x.SubjectCombinationId, x.CourseUnitId, x.YearId });
                    table.ForeignKey(
                        name: "FK_SubjectCombinationCourseUnits_CourseUnits_CourseUnitId",
                        column: x => x.CourseUnitId,
                        principalTable: "CourseUnits",
                        principalColumn: "CourseUnitId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubjectCombinationCourseUnits_SubjectCombinations_SubjectCombinationId",
                        column: x => x.SubjectCombinationId,
                        principalTable: "SubjectCombinations",
                        principalColumn: "SubjectCombinationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubjectCombinationCourseUnits_Years_YearId",
                        column: x => x.YearId,
                        principalTable: "Years",
                        principalColumn: "YearId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_DepartmentId",
                table: "AspNetUsers",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_InvigilatorId",
                table: "AspNetUsers",
                column: "InvigilatorId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_ProgrammeId",
                table: "AspNetUsers",
                column: "ProgrammeId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_YearId",
                table: "AspNetUsers",
                column: "YearId");

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentSubjectCombinations_SubjectCombinationId",
                table: "DepartmentSubjectCombinations",
                column: "SubjectCombinationId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamRooms_RoomId",
                table: "ExamRooms",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_Exams_CourseUnitId",
                table: "Exams",
                column: "CourseUnitId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invigilators_DepartmentId",
                table: "Invigilators",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgrammeCourseUnits_CourseUnitId",
                table: "ProgrammeCourseUnits",
                column: "CourseUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgrammeCourseUnits_YearId",
                table: "ProgrammeCourseUnits",
                column: "YearId");

            migrationBuilder.CreateIndex(
                name: "IX_Programmes_DepartmentId",
                table: "Programmes",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Programmes_YearId",
                table: "Programmes",
                column: "YearId");

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_InvigilatorId",
                table: "Rooms",
                column: "InvigilatorId");

            migrationBuilder.CreateIndex(
                name: "IX_SubjectCombinationCourseUnits_CourseUnitId",
                table: "SubjectCombinationCourseUnits",
                column: "CourseUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_SubjectCombinationCourseUnits_YearId",
                table: "SubjectCombinationCourseUnits",
                column: "YearId");

            migrationBuilder.CreateIndex(
                name: "IX_SubjectCombinations_ProgrammeId",
                table: "SubjectCombinations",
                column: "ProgrammeId");

            migrationBuilder.CreateIndex(
                name: "IX_Years_DepartmentId",
                table: "Years",
                column: "DepartmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Departments_DepartmentId",
                table: "AspNetUsers",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "DepartmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Invigilators_InvigilatorId",
                table: "AspNetUsers",
                column: "InvigilatorId",
                principalTable: "Invigilators",
                principalColumn: "InvigilatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Programmes_ProgrammeId",
                table: "AspNetUsers",
                column: "ProgrammeId",
                principalTable: "Programmes",
                principalColumn: "ProgrammeId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Years_YearId",
                table: "AspNetUsers",
                column: "YearId",
                principalTable: "Years",
                principalColumn: "YearId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Departments_DepartmentId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Invigilators_InvigilatorId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Programmes_ProgrammeId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Years_YearId",
                table: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "DepartmentSubjectCombinations");

            migrationBuilder.DropTable(
                name: "ExamRooms");

            migrationBuilder.DropTable(
                name: "ProgrammeCourseUnits");

            migrationBuilder.DropTable(
                name: "SubjectCombinationCourseUnits");

            migrationBuilder.DropTable(
                name: "Exams");

            migrationBuilder.DropTable(
                name: "Rooms");

            migrationBuilder.DropTable(
                name: "SubjectCombinations");

            migrationBuilder.DropTable(
                name: "CourseUnits");

            migrationBuilder.DropTable(
                name: "Invigilators");

            migrationBuilder.DropTable(
                name: "Programmes");

            migrationBuilder.DropTable(
                name: "Years");

            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_DepartmentId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_InvigilatorId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_ProgrammeId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_YearId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "InvigilatorId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ProfilePicture",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ProgrammeId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "StudentNumber",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "YearId",
                table: "AspNetUsers");
        }
    }
}
