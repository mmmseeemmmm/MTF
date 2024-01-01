using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MTFCore.Migrations
{
    public partial class ValidationTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ValidationTable",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(maxLength: 255, nullable: false),
                    ValidationMode = table.Column<byte>(nullable: false),
                    TableStatus = table.Column<byte>(nullable: false),
                    ValidationTime = table.Column<DateTime>(nullable: false),
                    SequenceReportId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ValidationTable", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ValidationTable_SequenceReport_SequenceReportId",
                        column: x => x.SequenceReportId,
                        principalTable: "SequenceReport",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ValidationTable_Id",
                table: "ValidationTable",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_ValidationTable_Name",
                table: "ValidationTable",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_ValidationTable_SequenceReportId",
                table: "ValidationTable",
                column: "SequenceReportId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ValidationTable");
        }
    }
}
