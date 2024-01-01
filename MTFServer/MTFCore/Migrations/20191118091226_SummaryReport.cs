using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MTFCore.Migrations
{
    public partial class SummaryReport : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SummaryReportSetting",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(nullable: false),
                    Title = table.Column<string>(nullable: true),
                    Index = table.Column<int>(nullable: false),
                    IsPinned = table.Column<bool>(nullable: false),
                    CanModifyFilterInView = table.Column<bool>(nullable: false),
                    SequenceName = table.Column<string>(nullable: true),
                    CycleName = table.Column<string>(nullable: true),
                    StartTimeFrom = table.Column<DateTime>(nullable: true),
                    StartTimeTo = table.Column<DateTime>(nullable: true),
                    Last24Hours = table.Column<bool>(nullable: false),
                    LastWeek = table.Column<bool>(nullable: false),
                    ReportStatus = table.Column<bool>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SummaryReportSetting", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SummaryReportSetting_Id",
                table: "SummaryReportSetting",
                column: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SummaryReportSetting");
        }
    }
}
