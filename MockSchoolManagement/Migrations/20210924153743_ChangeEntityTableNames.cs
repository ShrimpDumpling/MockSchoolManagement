using Microsoft.EntityFrameworkCore.Migrations;

namespace MockSchoolManagement.Migrations
{
    public partial class ChangeEntityTableNames : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentCourses_Courses_CourseID",
                table: "StudentCourses");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentCourses_Students_StudentID",
                table: "StudentCourses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Students",
                table: "Students");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StudentCourses",
                table: "StudentCourses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Courses",
                table: "Courses");

            migrationBuilder.EnsureSchema(
                name: "School");

            migrationBuilder.RenameTable(
                name: "Students",
                newName: "Student",
                newSchema: "School");

            migrationBuilder.RenameTable(
                name: "StudentCourses",
                newName: "StudentCourse",
                newSchema: "School");

            migrationBuilder.RenameTable(
                name: "Courses",
                newName: "Course",
                newSchema: "School");

            migrationBuilder.RenameIndex(
                name: "IX_StudentCourses_StudentID",
                schema: "School",
                table: "StudentCourse",
                newName: "IX_StudentCourse_StudentID");

            migrationBuilder.RenameIndex(
                name: "IX_StudentCourses_CourseID",
                schema: "School",
                table: "StudentCourse",
                newName: "IX_StudentCourse_CourseID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Student",
                schema: "School",
                table: "Student",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StudentCourse",
                schema: "School",
                table: "StudentCourse",
                column: "StudentsCourseId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Course",
                schema: "School",
                table: "Course",
                column: "CourseID");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentCourse_Course_CourseID",
                schema: "School",
                table: "StudentCourse",
                column: "CourseID",
                principalSchema: "School",
                principalTable: "Course",
                principalColumn: "CourseID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentCourse_Student_StudentID",
                schema: "School",
                table: "StudentCourse",
                column: "StudentID",
                principalSchema: "School",
                principalTable: "Student",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentCourse_Course_CourseID",
                schema: "School",
                table: "StudentCourse");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentCourse_Student_StudentID",
                schema: "School",
                table: "StudentCourse");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StudentCourse",
                schema: "School",
                table: "StudentCourse");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Student",
                schema: "School",
                table: "Student");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Course",
                schema: "School",
                table: "Course");

            migrationBuilder.RenameTable(
                name: "StudentCourse",
                schema: "School",
                newName: "StudentCourses");

            migrationBuilder.RenameTable(
                name: "Student",
                schema: "School",
                newName: "Students");

            migrationBuilder.RenameTable(
                name: "Course",
                schema: "School",
                newName: "Courses");

            migrationBuilder.RenameIndex(
                name: "IX_StudentCourse_StudentID",
                table: "StudentCourses",
                newName: "IX_StudentCourses_StudentID");

            migrationBuilder.RenameIndex(
                name: "IX_StudentCourse_CourseID",
                table: "StudentCourses",
                newName: "IX_StudentCourses_CourseID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StudentCourses",
                table: "StudentCourses",
                column: "StudentsCourseId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Students",
                table: "Students",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Courses",
                table: "Courses",
                column: "CourseID");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentCourses_Courses_CourseID",
                table: "StudentCourses",
                column: "CourseID",
                principalTable: "Courses",
                principalColumn: "CourseID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentCourses_Students_StudentID",
                table: "StudentCourses",
                column: "StudentID",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
