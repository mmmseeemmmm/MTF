using System.Collections.Generic;
using System.IO;
using System.Linq;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using MTFClientServerCommon.DbReporting.UiReportEntities;
using MTFCommon;
using Color = MigraDoc.DocumentObjectModel.Color;


namespace MTFCore.ReportBuilder
{
    class PdfReportDetailBuilder : ReportDetailBuilder
    {
        private readonly Color alYellow;
        private readonly Color alGreen;
        private readonly Color alLightGreen;
        private readonly Color alRed;
        private readonly Color alLightRed;
        private readonly Color alSilver;
        private readonly Color alLightSilver;
        private Document document;
        private Section tableSection = null;
        public PdfReportDetailBuilder(SequenceReportDetail reportDetail) : base(reportDetail)
        {
            alYellow = CreateColor(AlYellowColor);
            alGreen = CreateColor(AlGreenColor);
            alLightGreen = CreateColor(AlLightGreenColor);
            alRed = CreateColor(AlRedColor);
            alLightRed = CreateColor(AlLightRedColor);
            alSilver = CreateColor(AlSilverColor);
            alLightSilver = CreateColor(AlLightSilverColor);
        }

        protected override void Init()
        {
            CreateDocument();
        }

        protected override void AppendHeader(SequenceReportDetail reportDetail)
        {
            Section section = AddSection();

            //Automotive Lighting logo
            section.AddParagraph();
            section.LastParagraph.Format.SpaceBefore = Unit.FromCentimeter(1);
            section.LastParagraph.Format.Alignment = ParagraphAlignment.Center;
            var alLogo = section.LastParagraph.AddImage(AlLogoFilePath);
            alLogo.Width = Unit.FromCentimeter(6);

            //Sequence name
            section.AddParagraph();
            section.LastParagraph.Format.SpaceBefore = Unit.FromCentimeter(2);
            section.LastParagraph.Format.Alignment = ParagraphAlignment.Center;
            section.LastParagraph.AddText("Report of sequence: ");
            section.AddParagraph();
            section.LastParagraph.Format.SpaceAfter = Unit.FromCentimeter(2);
            section.LastParagraph.Format.Alignment = ParagraphAlignment.Center;
            section.LastParagraph.Format.Font.Bold = true;
            section.LastParagraph.Format.Font.Size = Unit.FromCentimeter(1);
            section.LastParagraph.AddText(reportDetail.SequenceName);

            //Summary test detail table
            section.AddTable();
            section.LastTable.Format.Alignment = ParagraphAlignment.Left;
            section.LastTable.Rows.Alignment = RowAlignment.Center;
            section.LastTable.AddColumn().Width = Unit.FromCentimeter(1);
            section.LastTable.AddColumn().Width = Unit.FromCentimeter(5);
            section.LastTable.AddColumn().Width = Unit.FromCentimeter(1);
            section.LastTable.AddColumn().Width = Unit.FromCentimeter(5);
            AddReportSummaryRow(section.LastTable, MachineImagePath, "Machine", reportDetail.Machine, StartTimeImagePath, "Start time", reportDetail.StartTime.ToString());
            AddReportSummaryRow(section.LastTable, UserImagePath, "User", reportDetail.WinUser, StopTimeImagePath, "Stop time", reportDetail.StopTime.ToString());
            AddReportSummaryRow(section.LastTable, ErrorsImagePath, "Errors count", reportDetail.Errors.Count.ToString(), DurationImagePath, "Duration", reportDetail.Duration.ToString());
            AddReportSummaryRow(section.LastTable, VariantImagePath, "Sequence variant", reportDetail.SequenceVariant, GoldSampleLifeImagePath, "Gold sample life", reportDetail.GsRemains ?? string.Empty);

            AddReportResult(section, reportDetail.SequenceStatus);
        }

        protected override void AppendTables(IEnumerable<SequenceReportValidationTableDetail> validationTables)
        {
            foreach (var validationTable in validationTables)
            {
                AddTable(TableSection, validationTable);
            }
        }

        protected override void AppendErrorImages(IList<string> images)
        {
            AppendImages(images);
        }

        protected override void AppendGraphicalViewImages(IList<string> images)
        {
            AppendImages(images);
        }

        protected override void AppendMessages(IList<SequenceReportMessageDetail> messages)
        {
            Unit timestampColumnWidth = Unit.FromCentimeter(4);
            Unit messageColumnWidth = SectionWidth(TableSection) - timestampColumnWidth;

            TableSection.AddImage(MessageImagePath).Width = Unit.FromCentimeter(1);

            CreateTable(TableSection, 2);
            CreateTableHeader(TableSection.LastTable, new[] { "Timestamp", "Message" }, new[]{timestampColumnWidth, messageColumnWidth}, alSilver);

            foreach (var message in messages)
            {
                var r = TableSection.LastTable.AddRow();
                r.VerticalAlignment = VerticalAlignment.Center;
                r[0].Shading.Color = alLightSilver;
                r[0].AddParagraph(message.TimeStamp.ToString());
                r[1].Shading.Color = alLightSilver;
                r[1].AddParagraph(message.Message);
            }
            TableSection.AddParagraph().Format.SpaceAfter = Unit.FromMillimeter(5);
        }

        protected override void AppendErrors(IList<SequenceReportErrorDetail> errors)
        {
            Unit timestampColumnWidth = Unit.FromCentimeter(4);
            Unit typeColumnWidth = Unit.FromCentimeter(3.5);
            Unit activityNameColumnWidth = Unit.FromCentimeter(4);
            Unit messageColumnWidth = SectionWidth(TableSection) - timestampColumnWidth - typeColumnWidth - activityNameColumnWidth;

            TableSection.AddImage(ErrorsImagePath).Width = Unit.FromCentimeter(1);

            CreateTable(TableSection, 4);
            CreateTableHeader(TableSection.LastTable, new[] { "Timestamp", "Type", "Activity name", "Message" }, new [] {timestampColumnWidth, typeColumnWidth, activityNameColumnWidth, messageColumnWidth}, alSilver);

            foreach (var error in errors)
            {
                var r = TableSection.LastTable.AddRow();
                r.VerticalAlignment = VerticalAlignment.Center;
                r[0].Shading.Color = alLightSilver;
                r[0].AddParagraph(error.TimeStamp.ToString());
                r[1].Shading.Color = alLightSilver;
                r[1].AddParagraph(error.ErrorType.ToString());
                r[2].Shading.Color = alLightSilver;
                r[2].AddParagraph(error.ActivityName);
                r[3].Shading.Color = alLightSilver;
                r[3].AddParagraph(error.Message);
            }
            TableSection.AddParagraph().Format.SpaceAfter = Unit.FromMillimeter(5);
        }

        protected override Stream End()
        {
            PdfDocumentRenderer renderer = new PdfDocumentRenderer(true);
            renderer.Document = document;
            renderer.RenderDocument();
            Stream stream = new MemoryStream();
            renderer.PdfDocument.Save(stream, false);

            return stream;
        }

        private Section TableSection => tableSection ?? (tableSection = AddSectionWithFooter());

        private void AddTable(Section section, SequenceReportValidationTableDetail validationTable)
        {
            Unit criteriaColumnWidth = Unit.FromCentimeter(2.5);
            Unit nameColumnWidth = Unit.FromCentimeter(3);
            Unit actualValueColumnWidth = SectionWidth(section) - nameColumnWidth - (validationTable.Columns.Count * criteriaColumnWidth);

            section.AddParagraph(validationTable.Name);
            CreateTable(section, validationTable.Columns.Count + 2);

            var headerText = new[] {"Name", "Actual value"}.Concat(validationTable.Columns.Select(c => c.Split('_').Last())).ToArray();
            var headerWidth = new[] {nameColumnWidth, actualValueColumnWidth}.Concat(validationTable.Columns.Select(c => criteriaColumnWidth)).ToArray();
            CreateTableHeader(section.LastTable, headerText, headerWidth, validationTable.TableStatus == MTFValidationTableStatus.Nok ? alLightRed : alLightGreen);
            
            foreach (var row in validationTable.Rows)
            {
                var r = section.LastTable.AddRow();
                r.VerticalAlignment = VerticalAlignment.Center;
                r[0].AddParagraph(row.Name);
                r[0].Shading.Color = row.Status == MTFValidationTableStatus.Nok ? alLightRed : alLightGreen;
                if (row.HasImage == true)
                {
                    r[1].AddImage(GetImagePath(row.ActualValue)).Width = actualValueColumnWidth;
                }
                else
                {
                    r[1].AddParagraph(row.ActualValue);
                }
                
                r[1].Shading.Color = alLightSilver;

                for (int i = 0; i < row.Columns.Count; i++)
                {
                    r[i + 2].AddParagraph(row.Columns[i].Value ?? string.Empty);
                    r[i + 2].Shading.Color = alLightSilver;
                }
            }

            section.AddParagraph().Format.SpaceAfter = Unit.FromMillimeter(5);
        }

        private void CreateTable(Section section, int columns)
        {
            section.AddTable();
            section.LastTable.Borders.Width = Unit.FromMillimeter(0.2);
            section.LastTable.Borders.Color = Colors.White;
            section.LastTable.Format.Alignment = ParagraphAlignment.Center;
            section.LastTable.Format.SpaceBefore = Unit.FromMillimeter(1);
            section.LastTable.Format.SpaceAfter = Unit.FromMillimeter(1);
            CreateTableColumns(section.LastTable, columns);
        }

        private void CreateTableHeader(Table table, string[] headerText, Unit[] headerWidth, Color color)
        {
            var r = table.AddRow();
            r.Shading.Color = color;
            for (int i = 0; i < headerText.Length; i++)
            {
                r[i].AddParagraph(headerText[i]).Format.Font.Bold = true;
                table.Columns[i].Width = headerWidth[i];
            }
        }

        private void CreateTableColumns(Table table, int columns)
        {
            for (int i = 0; i < columns; i++)
            {
                table.AddColumn();
            }
        }

        private void AddReportResult(Section section, bool? status)
        {
            section.AddParagraph().Format.SpaceBefore = Unit.FromCentimeter(2);
            section.AddTable();
            section.LastTable.Format.Alignment = ParagraphAlignment.Center;
            section.LastTable.Rows.Alignment = RowAlignment.Center;
            section.LastTable.AddColumn(Unit.FromCentimeter(6));

            var r = section.LastTable.AddRow();
            if (!status.HasValue)
            {
                r.Shading.Color = alSilver;
            }
            else if (status.Value)
            {
                r.Shading.Color = alGreen;
            }
            else
            {
                r.Shading.Color = alRed;
            }
            var p = r[0].AddParagraph("Final result");
            p.Format.SpaceBefore = Unit.FromMillimeter(2);
            p.Format.SpaceAfter = Unit.FromMillimeter(2);

            r = section.LastTable.AddRow();
            p = r[0].AddParagraph();
            if (!status.HasValue)
            {
                r.Shading.Color = alLightSilver;
            }
            else if (status.Value)
            {
                r.Shading.Color = alLightGreen;
                p.AddText("Pass");
            }
            else
            {
                r.Shading.Color = alLightRed;
                p.AddText("Fail");
            }
            p.Format.SpaceBefore = Unit.FromMillimeter(5);
            p.Format.SpaceAfter = Unit.FromMillimeter(5);
            p.Format.Font.Bold = true;
            p.Format.Font.Size = Unit.FromPoint(22);
            p.Format.Font.Color = Colors.White;
        }

        private static void AddReportSummaryRow(Table table, string icon1, string row1, string row2, string icon2, string row3, string row4)
        {
            var row = table.AddRow();
            row[0].AddImage(icon1).Width = Unit.FromCentimeter(0.7);
            row[0].VerticalAlignment = VerticalAlignment.Center;
            var p =row[1].AddParagraph(row1);
            p.Format.SpaceBefore = Unit.FromMillimeter(2);
            p.Format.Font.Italic = true;
            p.Format.Font.Size = Unit.FromPoint(8);
            p = row[1].AddParagraph(row2);
            p.Format.SpaceBefore = Unit.Zero;
            p.Format.SpaceAfter = Unit.FromMillimeter(2);
            p.Format.Font.Bold = true;
            p.Format.Font.Size = Unit.FromPoint(12);

            row[2].AddImage(icon2).Width = Unit.FromCentimeter(0.7);
            row[2].VerticalAlignment = VerticalAlignment.Center;
            p = row[3].AddParagraph(row3);
            p.Format.SpaceBefore = Unit.FromMillimeter(2);
            p.Format.Font.Italic = true;
            p.Format.Font.Size = Unit.FromPoint(8);
            p = row[3].AddParagraph(row4);
            p.Format.SpaceBefore = Unit.Zero;
            p.Format.SpaceAfter = Unit.FromMillimeter(2);
            p.Format.Font.Bold = true;
            p.Format.Font.Size = Unit.FromPoint(12);
        }

        private void AppendImages(IList<string> images)
        {
            int maxImagesPerLine = 3;
            int imagesPerLine = images.Count < maxImagesPerLine ? images.Count : maxImagesPerLine;
            Section section = AddSectionWithFooter();
            Unit imgWidth = SectionWidth(section) / imagesPerLine;

            section.AddTable();
            for (int i = 0; i < imagesPerLine; i++)
            {
                section.LastTable.AddColumn(imgWidth);
            }

            int columnNum = 0;
            Row row = null;
            foreach (var img in images)
            {
                if (columnNum == 0)
                {
                    row = section.LastTable.AddRow();
                }
                row[columnNum].AddImage(img).Width = imgWidth;

                columnNum = columnNum == imagesPerLine - 1 ? 0 : columnNum + 1;
            }
        }

        private void CreateDocument()
        {
            document = new Document();
            document.Info.Author = "Automotive Lighting";
        }

        private Section AddSection()
        {
            var section = document.AddSection();
            section.PageSetup = document.DefaultPageSetup.Clone();

            return section;
        }

        private Section AddSectionWithFooter()
        {
            var section = AddSection();
            Unit imgWidth = Unit.FromCentimeter(2);
            Unit textWidth = SectionWidth(section) - imgWidth;

            var p = section.Footers.Primary.AddParagraph();
            p.Format.Borders.Top.Color = alYellow;
            p.Format.Borders.Top.Width = Unit.FromPoint(1);

            var t = section.Footers.Primary.AddTable();
            
            t.AddColumn(imgWidth);
            t.AddColumn(textWidth);
            var r = t.AddRow();
            r.VerticalAlignment = VerticalAlignment.Center;

            var img = r[0].AddImage(AlLogoFilePath);
            img.Width = imgWidth;

            p = r[1].AddParagraph();
            p.Format.Alignment = ParagraphAlignment.Center;
            p.AddPageField();
            p.AddText(" / ");
            p.AddNumPagesField();

            return section;
        }

        private static Unit SectionWidth(Section section) => section.PageSetup.PageWidth - section.PageSetup.LeftMargin - section.PageSetup.RightMargin;
        private static Color CreateColor(System.Drawing.Color color) => Color.FromArgb(color.A, color.R, color.G, color.B);
    }
}
