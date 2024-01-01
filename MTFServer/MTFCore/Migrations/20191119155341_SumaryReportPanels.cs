using Microsoft.EntityFrameworkCore.Migrations;

namespace MTFCore.Migrations
{
    public partial class SumaryReportPanels : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SummaryReportLineChartPanel",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Index = table.Column<int>(nullable: false),
                    Title = table.Column<string>(nullable: true),
                    LegendPosition = table.Column<int>(nullable: false),
                    SummaryReportId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SummaryReportLineChartPanel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SummaryReportLineChartPanel_SummaryReportSetting_SummaryReportId",
                        column: x => x.SummaryReportId,
                        principalTable: "SummaryReportSetting",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SummaryReportOverviewPanel",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Index = table.Column<int>(nullable: false),
                    Title = table.Column<string>(nullable: true),
                    TimeQuantumInMinutes = table.Column<int>(nullable: false),
                    SummaryReportId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SummaryReportOverviewPanel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SummaryReportOverviewPanel_SummaryReportSetting_SummaryReportId",
                        column: x => x.SummaryReportId,
                        principalTable: "SummaryReportSetting",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SummaryReportTextPanel",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Index = table.Column<int>(nullable: false),
                    Text = table.Column<string>(nullable: true),
                    FontSize = table.Column<int>(nullable: false),
                    TextAlignment = table.Column<int>(nullable: false),
                    SummaryReportId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SummaryReportTextPanel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SummaryReportTextPanel_SummaryReportSetting_SummaryReportId",
                        column: x => x.SummaryReportId,
                        principalTable: "SummaryReportSetting",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SummaryReportLineChartPanel_Id",
                table: "SummaryReportLineChartPanel",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_SummaryReportLineChartPanel_SummaryReportId",
                table: "SummaryReportLineChartPanel",
                column: "SummaryReportId");

            migrationBuilder.CreateIndex(
                name: "IX_SummaryReportOverviewPanel_Id",
                table: "SummaryReportOverviewPanel",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_SummaryReportOverviewPanel_SummaryReportId",
                table: "SummaryReportOverviewPanel",
                column: "SummaryReportId");

            migrationBuilder.CreateIndex(
                name: "IX_SummaryReportTextPanel_Id",
                table: "SummaryReportTextPanel",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_SummaryReportTextPanel_SummaryReportId",
                table: "SummaryReportTextPanel",
                column: "SummaryReportId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SummaryReportLineChartPanel");

            migrationBuilder.DropTable(
                name: "SummaryReportOverviewPanel");

            migrationBuilder.DropTable(
                name: "SummaryReportTextPanel");
        }
    }
}
