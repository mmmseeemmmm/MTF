using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MTFCore.Migrations
{
    public partial class SequenceReport : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SequenceVariant",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(maxLength: 255, nullable: false),
                    Type = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SequenceVariant", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SequenceReport",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SequenceName = table.Column<string>(maxLength: 255, nullable: false),
                    StartTime = table.Column<DateTime>(nullable: false),
                    StopTime = table.Column<DateTime>(nullable: true),
                    SequenceStatus = table.Column<bool>(nullable: true),
                    GsRemains = table.Column<string>(maxLength: 1000, nullable: true),
                    GsWarning = table.Column<bool>(nullable: false),
                    CycleName = table.Column<string>(maxLength: 1000, nullable: true),
                    ShowHiddenRows = table.Column<bool>(nullable: false),
                    GraphicalViews = table.Column<string>(nullable: true),
                    VariantVersionId = table.Column<int>(nullable: true),
                    VariantLightDistributionId = table.Column<int>(nullable: true),
                    VariantMountingSideId = table.Column<int>(nullable: true),
                    VariantGsDutId = table.Column<int>(nullable: true),
                    SequenceRunId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SequenceReport", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SequenceReport_SequenceRun_SequenceRunId",
                        column: x => x.SequenceRunId,
                        principalTable: "SequenceRun",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SequenceReport_SequenceVariant_VariantGsDutId",
                        column: x => x.VariantGsDutId,
                        principalTable: "SequenceVariant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SequenceReport_SequenceVariant_VariantLightDistributionId",
                        column: x => x.VariantLightDistributionId,
                        principalTable: "SequenceVariant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SequenceReport_SequenceVariant_VariantMountingSideId",
                        column: x => x.VariantMountingSideId,
                        principalTable: "SequenceVariant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SequenceReport_SequenceVariant_VariantVersionId",
                        column: x => x.VariantVersionId,
                        principalTable: "SequenceVariant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Error",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TimeStamp = table.Column<DateTime>(nullable: false),
                    ActivityName = table.Column<string>(maxLength: 1000, nullable: true),
                    Message = table.Column<string>(nullable: true),
                    ErrorType = table.Column<byte>(nullable: false),
                    SequenceReportId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Error", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Error_SequenceReport_SequenceReportId",
                        column: x => x.SequenceReportId,
                        principalTable: "SequenceReport",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Message",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TimeStamp = table.Column<DateTime>(nullable: false),
                    Message = table.Column<string>(nullable: true),
                    SequenceReportId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Message", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Message_SequenceReport_SequenceReportId",
                        column: x => x.SequenceReportId,
                        principalTable: "SequenceReport",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Error_SequenceReportId",
                table: "Error",
                column: "SequenceReportId");

            migrationBuilder.CreateIndex(
                name: "IX_Message_SequenceReportId",
                table: "Message",
                column: "SequenceReportId");

            migrationBuilder.CreateIndex(
                name: "IX_SequenceReport_Id",
                table: "SequenceReport",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_SequenceReport_SequenceName",
                table: "SequenceReport",
                column: "SequenceName");

            migrationBuilder.CreateIndex(
                name: "IX_SequenceReport_SequenceRunId",
                table: "SequenceReport",
                column: "SequenceRunId");

            migrationBuilder.CreateIndex(
                name: "IX_SequenceReport_SequenceStatus",
                table: "SequenceReport",
                column: "SequenceStatus");

            migrationBuilder.CreateIndex(
                name: "IX_SequenceReport_VariantGsDutId",
                table: "SequenceReport",
                column: "VariantGsDutId");

            migrationBuilder.CreateIndex(
                name: "IX_SequenceReport_VariantLightDistributionId",
                table: "SequenceReport",
                column: "VariantLightDistributionId");

            migrationBuilder.CreateIndex(
                name: "IX_SequenceReport_VariantMountingSideId",
                table: "SequenceReport",
                column: "VariantMountingSideId");

            migrationBuilder.CreateIndex(
                name: "IX_SequenceReport_VariantVersionId",
                table: "SequenceReport",
                column: "VariantVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_SequenceVariant_Id",
                table: "SequenceVariant",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_SequenceVariant_Name",
                table: "SequenceVariant",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_SequenceVariant_Type",
                table: "SequenceVariant",
                column: "Type");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Error");

            migrationBuilder.DropTable(
                name: "Message");

            migrationBuilder.DropTable(
                name: "SequenceReport");

            migrationBuilder.DropTable(
                name: "SequenceVariant");
        }
    }
}
