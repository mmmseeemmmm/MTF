using Microsoft.EntityFrameworkCore.Migrations;

namespace MTFCore.Migrations
{
    public partial class LineChartSeries : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SummaryReportLineChartSeries",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TableName = table.Column<string>(maxLength: 255, nullable: true),
                    RowName = table.Column<string>(maxLength: 255, nullable: true),
                    ColumnName = table.Column<string>(maxLength: 255, nullable: true),
                    Title = table.Column<string>(maxLength: 255, nullable: true),
                    Color = table.Column<int>(nullable: true),
                    LineChartPanelId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SummaryReportLineChartSeries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SummaryReportLineChartSeries_SummaryReportLineChartPanel_LineChartPanelId",
                        column: x => x.LineChartPanelId,
                        principalTable: "SummaryReportLineChartPanel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SummaryReportLineChartSeries_Id",
                table: "SummaryReportLineChartSeries",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_SummaryReportLineChartSeries_LineChartPanelId",
                table: "SummaryReportLineChartSeries",
                column: "LineChartPanelId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SummaryReportLineChartSeries");
        }
    }
}
