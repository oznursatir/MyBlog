using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MyBlog.Data.Migrations
{
    /// <inheritdoc />
    public partial class Add_Identity_Roles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "5c59aeb6-264d-4693-a36c-34e32ee2d9f4", null, "Admin", "ADMIN" },
                    { "8706a370-a288-4536-9412-15cbb61bd7e2", null, "Editör", "EDITOR" },
                    { "b4f504ab-6d9a-47c5-b7ea-a14867b0a834", null, "User", "USER" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "5c59aeb6-264d-4693-a36c-34e32ee2d9f4");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "8706a370-a288-4536-9412-15cbb61bd7e2");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "b4f504ab-6d9a-47c5-b7ea-a14867b0a834");
        }
    }
}
