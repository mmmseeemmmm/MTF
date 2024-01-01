using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MTFCore.Migrations
{
    public partial class ValidationTableRow : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ValidationTableRow",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(nullable: true),
                    ActualValue = table.Column<string>(nullable: true),
                    Status = table.Column<byte>(nullable: false),
                    NumberOfRepetition = table.Column<int>(nullable: false),
                    HasImage = table.Column<bool>(nullable: false),
                    IsHidden = table.Column<bool>(nullable: false),
                    MinValue = table.Column<string>(nullable: true),
                    MinStatus = table.Column<bool>(nullable: false),
                    MaxValue = table.Column<string>(nullable: true),
                    MaxStatus = table.Column<bool>(nullable: false),
                    RequiredValue = table.Column<string>(nullable: true),
                    RequiredStatus = table.Column<bool>(nullable: false),
                    ProhibitedValue = table.Column<string>(nullable: true),
                    ProhibitedStatus = table.Column<bool>(nullable: false),
                    GsValue = table.Column<string>(nullable: true),
                    GsStatus = table.Column<bool>(nullable: false),
                    GsPercentage = table.Column<double>(nullable: false),
                    TimeStamp = table.Column<DateTime>(nullable: false),
                    ValidationTableId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ValidationTableRow", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ValidationTableRow_ValidationTable_ValidationTableId",
                        column: x => x.ValidationTableId,
                        principalTable: "ValidationTable",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ValidationTableRow_Id",
                table: "ValidationTableRow",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_ValidationTableRow_Name",
                table: "ValidationTableRow",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_ValidationTableRow_ValidationTableId",
                table: "ValidationTableRow",
                column: "ValidationTableId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ValidationTableRow");
        }
    }
}
