using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MockSchoolManagement.Migrations
{
    public partial class Remove_SeedData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "School",
                table: "Student",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                schema: "School",
                table: "Student",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                schema: "School",
                table: "Student",
                keyColumn: "Id",
                keyValue: 3);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "School",
                table: "Student",
                columns: new[] { "Id", "Email", "EnrollmentDate", "Major", "Name", "PhotoPath" },
                values: new object[] { 1, "anonyjie@outlook.com", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, "张三", null });

            migrationBuilder.InsertData(
                schema: "School",
                table: "Student",
                columns: new[] { "Id", "Email", "EnrollmentDate", "Major", "Name", "PhotoPath" },
                values: new object[] { 2, "evilnanako@outlook.com", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, "李四", null });

            migrationBuilder.InsertData(
                schema: "School",
                table: "Student",
                columns: new[] { "Id", "Email", "EnrollmentDate", "Major", "Name", "PhotoPath" },
                values: new object[] { 3, "hackerhzj@qq.com", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 3, "赵六", null });
        }
    }
}
