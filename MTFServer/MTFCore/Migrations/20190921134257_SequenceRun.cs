using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MTFCore.Migrations
{
    public partial class SequenceRun : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SequenceRun",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Machine = table.Column<string>(maxLength: 255, nullable: true),
                    WinUser = table.Column<string>(maxLength: 255, nullable: true),
                    SequenceName = table.Column<string>(maxLength: 255, nullable: true),
                    StartTime = table.Column<DateTime>(nullable: false),
                    StopTime = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SequenceRun", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SequenceRun_Id",
                table: "SequenceRun",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_SequenceRun_SequenceName",
                table: "SequenceRun",
                column: "SequenceName");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SequenceRun");
        }
    }
}
