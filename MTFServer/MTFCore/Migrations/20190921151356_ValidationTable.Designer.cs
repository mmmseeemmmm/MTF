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
    [Migration("20190921151356_ValidationTable")]
    partial class ValidationTable
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
#pragma warning restore 612, 618
        }
    }
}
