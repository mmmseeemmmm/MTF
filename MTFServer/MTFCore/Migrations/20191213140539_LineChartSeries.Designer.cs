﻿// <auto-generated />
using System;
using MTFCore.DbReporting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MTFCore.Migrations
{
    [DbContext(typeof(DbReportingContext))]
    [Migration("20191213140539_LineChartSeries")]
    partial class LineChartSeries
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.6-servicing-10079");

            modelBuilder.Entity("MTFClientServerCommon.DbEntities.DbReportEntities.ReportError", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ActivityName")
                        .HasMaxLength(1000);

                    b.Property<byte>("ErrorType");

                    b.Property<string>("Message");

                    b.Property<int>("SequenceReportId");

                    b.Property<DateTime>("TimeStamp");

                    b.HasKey("Id");

                    b.HasIndex("SequenceReportId");

                    b.ToTable("Error");
                });

            modelBuilder.Entity("MTFClientServerCommon.DbEntities.DbReportEntities.ReportMessage", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Message");

                    b.Property<int>("SequenceReportId");

                    b.Property<DateTime>("TimeStamp");

                    b.HasKey("Id");

                    b.HasIndex("SequenceReportId");

                    b.ToTable("Message");
                });

            modelBuilder.Entity("MTFClientServerCommon.DbEntities.DbReportEntities.ReportRoundingRules", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("Digits");

                    b.Property<int>("Max");

                    b.Property<int>("Min");

                    b.Property<int>("SequenceRunId");

                    b.HasKey("Id");

                    b.HasIndex("SequenceRunId");

                    b.ToTable("RoundingRule");
                });

            modelBuilder.Entity("MTFClientServerCommon.DbEntities.DbReportEntities.ReportSequenceRun", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Machine")
                        .HasMaxLength(255);

                    b.Property<string>("SequenceName")
                        .HasMaxLength(255);

                    b.Property<DateTime>("StartTime");

                    b.Property<DateTime?>("StopTime");

                    b.Property<string>("WinUser")
                        .HasMaxLength(255);

                    b.HasKey("Id");

                    b.HasIndex("Id");

                    b.HasIndex("SequenceName");

                    b.ToTable("SequenceRun");
                });

            modelBuilder.Entity("MTFClientServerCommon.DbEntities.DbReportEntities.ReportSequenceVariant", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(255);

                    b.Property<int>("Type");

                    b.HasKey("Id");

                    b.HasIndex("Id");

                    b.HasIndex("Name");

                    b.HasIndex("Type");

                    b.ToTable("SequenceVariant");
                });

            modelBuilder.Entity("MTFClientServerCommon.DbEntities.DbReportEntities.ReportValidationTable", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(255);

                    b.Property<int>("SequenceReportId");

                    b.Property<byte>("TableStatus");

                    b.Property<byte>("ValidationMode");

                    b.Property<DateTime>("ValidationTime");

                    b.HasKey("Id");

                    b.HasIndex("Id");

                    b.HasIndex("Name");

                    b.HasIndex("SequenceReportId");

                    b.ToTable("ValidationTable");
                });

            modelBuilder.Entity("MTFClientServerCommon.DbEntities.DbReportEntities.ReportValidationTableRow", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ActualValue");

                    b.Property<double>("GsPercentage");

                    b.Property<bool>("GsStatus");

                    b.Property<string>("GsValue");

                    b.Property<bool>("HasImage");

                    b.Property<bool>("IsHidden");

                    b.Property<bool>("MaxStatus");

                    b.Property<string>("MaxValue");

                    b.Property<bool>("MinStatus");

                    b.Property<string>("MinValue");

                    b.Property<string>("Name");

                    b.Property<int>("NumberOfRepetition");

                    b.Property<bool>("ProhibitedStatus");

                    b.Property<string>("ProhibitedValue");

                    b.Property<bool>("RequiredStatus");

                    b.Property<string>("RequiredValue");

                    b.Property<byte>("Status");

                    b.Property<DateTime>("TimeStamp");

                    b.Property<int>("ValidationTableId");

                    b.HasKey("Id");

                    b.HasIndex("Id");

                    b.HasIndex("Name");

                    b.HasIndex("ValidationTableId");

                    b.ToTable("ValidationTableRow");
                });

            modelBuilder.Entity("MTFClientServerCommon.DbEntities.DbReportEntities.SequenceReport", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("CycleName")
                        .HasMaxLength(1000);

                    b.Property<string>("GraphicalViews");

                    b.Property<string>("GsRemains")
                        .HasMaxLength(1000);

                    b.Property<bool>("GsWarning");

                    b.Property<string>("SequenceName")
                        .IsRequired()
                        .HasMaxLength(255);

                    b.Property<int>("SequenceRunId");

                    b.Property<bool?>("SequenceStatus");

                    b.Property<bool>("ShowHiddenRows");

                    b.Property<DateTime>("StartTime");

                    b.Property<DateTime?>("StopTime");

                    b.Property<int?>("VariantGsDutId");

                    b.Property<int?>("VariantLightDistributionId");

                    b.Property<int?>("VariantMountingSideId");

                    b.Property<int?>("VariantVersionId");

                    b.HasKey("Id");

                    b.HasIndex("Id");

                    b.HasIndex("SequenceName");

                    b.HasIndex("SequenceRunId");

                    b.HasIndex("SequenceStatus");

                    b.HasIndex("VariantGsDutId");

                    b.HasIndex("VariantLightDistributionId");

                    b.HasIndex("VariantMountingSideId");

                    b.HasIndex("VariantVersionId");

                    b.ToTable("SequenceReport");
                });

            modelBuilder.Entity("MTFClientServerCommon.DbEntities.DbReportViewer.DbLineChartPanel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("Index");

                    b.Property<int>("LegendPosition");

                    b.Property<int>("SummaryReportId");

                    b.Property<string>("Title");

                    b.HasKey("Id");

                    b.HasIndex("Id");

                    b.HasIndex("SummaryReportId");

                    b.ToTable("SummaryReportLineChartPanel");
                });

            modelBuilder.Entity("MTFClientServerCommon.DbEntities.DbReportViewer.DbLineChartSeries", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("Color");

                    b.Property<string>("ColumnName")
                        .HasMaxLength(255);

                    b.Property<int>("LineChartPanelId");

                    b.Property<string>("RowName")
                        .HasMaxLength(255);

                    b.Property<string>("TableName")
                        .HasMaxLength(255);

                    b.Property<string>("Title")
                        .HasMaxLength(255);

                    b.HasKey("Id");

                    b.HasIndex("Id");

                    b.HasIndex("LineChartPanelId");

                    b.ToTable("SummaryReportLineChartSeries");
                });

            modelBuilder.Entity("MTFClientServerCommon.DbEntities.DbReportViewer.DbReportsOverviewPanel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("Index");

                    b.Property<int>("SummaryReportId");

                    b.Property<int>("TimeQuantumInMinutes");

                    b.Property<string>("Title");

                    b.HasKey("Id");

                    b.HasIndex("Id");

                    b.HasIndex("SummaryReportId");

                    b.ToTable("SummaryReportOverviewPanel");
                });

            modelBuilder.Entity("MTFClientServerCommon.DbEntities.DbReportViewer.DbTextPanel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("FontSize");

                    b.Property<int>("Index");

                    b.Property<int>("SummaryReportId");

                    b.Property<string>("Text");

                    b.Property<int>("TextAlignment");

                    b.HasKey("Id");

                    b.HasIndex("Id");

                    b.HasIndex("SummaryReportId");

                    b.ToTable("SummaryReportTextPanel");
                });

            modelBuilder.Entity("MTFClientServerCommon.DbEntities.DbReportViewer.SummaryReport", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("CanModifyFilterInView");

                    b.Property<string>("CycleName");

                    b.Property<int>("Index");

                    b.Property<bool>("IsPinned");

                    b.Property<bool>("Last24Hours");

                    b.Property<bool>("LastWeek");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<bool?>("ReportStatus");

                    b.Property<string>("SequenceName");

                    b.Property<DateTime?>("StartTimeFrom");

                    b.Property<DateTime?>("StartTimeTo");

                    b.Property<string>("Title");

                    b.HasKey("Id");

                    b.HasIndex("Id");

                    b.ToTable("SummaryReportSetting");
                });

            modelBuilder.Entity("MTFClientServerCommon.DbEntities.DbReportEntities.ReportError", b =>
                {
                    b.HasOne("MTFClientServerCommon.DbEntities.DbReportEntities.SequenceReport", "SequenceReport")
                        .WithMany("Errors")
                        .HasForeignKey("SequenceReportId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("MTFClientServerCommon.DbEntities.DbReportEntities.ReportMessage", b =>
                {
                    b.HasOne("MTFClientServerCommon.DbEntities.DbReportEntities.SequenceReport", "SequenceReport")
                        .WithMany("Messages")
                        .HasForeignKey("SequenceReportId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("MTFClientServerCommon.DbEntities.DbReportEntities.ReportRoundingRules", b =>
                {
                    b.HasOne("MTFClientServerCommon.DbEntities.DbReportEntities.ReportSequenceRun", "SequenceRun")
                        .WithMany("RoundingRules")
                        .HasForeignKey("SequenceRunId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("MTFClientServerCommon.DbEntities.DbReportEntities.ReportValidationTable", b =>
                {
                    b.HasOne("MTFClientServerCommon.DbEntities.DbReportEntities.SequenceReport", "SequenceReport")
                        .WithMany("ValidationTables")
                        .HasForeignKey("SequenceReportId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("MTFClientServerCommon.DbEntities.DbReportEntities.ReportValidationTableRow", b =>
                {
                    b.HasOne("MTFClientServerCommon.DbEntities.DbReportEntities.ReportValidationTable", "ValidationTable")
                        .WithMany("Rows")
                        .HasForeignKey("ValidationTableId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("MTFClientServerCommon.DbEntities.DbReportEntities.SequenceReport", b =>
                {
                    b.HasOne("MTFClientServerCommon.DbEntities.DbReportEntities.ReportSequenceRun", "SequenceRun")
                        .WithMany("Reports")
                        .HasForeignKey("SequenceRunId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("MTFClientServerCommon.DbEntities.DbReportEntities.ReportSequenceVariant", "VariantGsDut")
                        .WithMany()
                        .HasForeignKey("VariantGsDutId");

                    b.HasOne("MTFClientServerCommon.DbEntities.DbReportEntities.ReportSequenceVariant", "VariantLightDistribution")
                        .WithMany()
                        .HasForeignKey("VariantLightDistributionId");

                    b.HasOne("MTFClientServerCommon.DbEntities.DbReportEntities.ReportSequenceVariant", "VariantMountingSide")
                        .WithMany()
                        .HasForeignKey("VariantMountingSideId");

                    b.HasOne("MTFClientServerCommon.DbEntities.DbReportEntities.ReportSequenceVariant", "VariantVersion")
                        .WithMany()
                        .HasForeignKey("VariantVersionId");
                });

            modelBuilder.Entity("MTFClientServerCommon.DbEntities.DbReportViewer.DbLineChartPanel", b =>
                {
                    b.HasOne("MTFClientServerCommon.DbEntities.DbReportViewer.SummaryReport", "SummaryReport")
                        .WithMany("LineChartPanels")
                        .HasForeignKey("SummaryReportId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("MTFClientServerCommon.DbEntities.DbReportViewer.DbLineChartSeries", b =>
                {
                    b.HasOne("MTFClientServerCommon.DbEntities.DbReportViewer.DbLineChartPanel", "LineChartPanel")
                        .WithMany("Series")
                        .HasForeignKey("LineChartPanelId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("MTFClientServerCommon.DbEntities.DbReportViewer.DbReportsOverviewPanel", b =>
                {
                    b.HasOne("MTFClientServerCommon.DbEntities.DbReportViewer.SummaryReport", "SummaryReport")
                        .WithMany("OverviewPanels")
                        .HasForeignKey("SummaryReportId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("MTFClientServerCommon.DbEntities.DbReportViewer.DbTextPanel", b =>
                {
                    b.HasOne("MTFClientServerCommon.DbEntities.DbReportViewer.SummaryReport", "SummaryReport")
                        .WithMany("TextPanels")
                        .HasForeignKey("SummaryReportId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}