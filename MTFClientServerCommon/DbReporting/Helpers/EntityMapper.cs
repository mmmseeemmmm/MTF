using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using MTFClientServerCommon.Constants;
using MTFClientServerCommon.DbEntities.DbReportEntities;
using MTFClientServerCommon.DbEntities.DbReportViewer;
using MTFClientServerCommon.DbReporting.UiReportEntities;
using MTFClientServerCommon.DbReporting.UiReportEntities.SummaryReport;

namespace MTFClientServerCommon.DbReporting.Helpers
{
    public static class EntityMapper
    {
        public static IEnumerable<SequenceReportPreview> GetReportPreviews(IQueryable<SequenceReport> source)
        {
            return source.Select(x => new SequenceReportPreview
                                      {
                                          Id = x.Id,
                                          CycleName = x.CycleName,
                                          SequenceStatus = x.SequenceStatus,
                                          StartTime = x.StartTime,
                                          StopTime = x.StopTime,
                                          VariantVersion = x.VariantVersion != null ? x.VariantVersion.Name : null,
                                          VariantMountingSide = x.VariantMountingSide != null ? x.VariantMountingSide.Name : null,
                                          VariantLightDistribution = x.VariantLightDistribution != null ? x.VariantLightDistribution.Name : null,
                                          VariantGsDut = x.VariantGsDut != null ? x.VariantGsDut.Name : null,
                                      });
        }

        public static void FillSequenceReportDetail(SequenceReportDetail detail, SequenceReport report)
        {
            detail.Id = report.Id;
            detail.SequenceName = report.SequenceName;
            detail.CycleName = report.CycleName;
            detail.StartTime = report.StartTime;
            detail.StopTime = report.StopTime;
            detail.VariantVersion = report.VariantVersion?.Name;
            detail.VariantLightDistribution = report.VariantLightDistribution?.Name;
            detail.VariantMountingSide = report.VariantMountingSide?.Name;
            detail.VariantGsDut = report.VariantGsDut?.Name;
            detail.ShowHiddenRows = report.ShowHiddenRows;
            detail.GraphicalViews = report.GraphicalViews;
            detail.GsRemains = report.GsRemains;
            detail.GsWarning = report.GsWarning;
            detail.SequenceStatus = report.SequenceStatus;
            detail.Errors = report.Errors.Select(x => new SequenceReportErrorDetail
                                                      {
                                                          ActivityName = x.ActivityName,
                                                          ErrorType = EnumTransformHelper.TransformErrorType(x.ErrorType),
                                                          Message = x.Message,
                                                          TimeStamp = x.TimeStamp
                                                      }).ToList();
            detail.Messages = report.Messages.Select(x => new SequenceReportMessageDetail
                                                          {
                                                              Message = x.Message,
                                                              TimeStamp = x.TimeStamp
                                                          })
                .ToList();
            detail.ValidationTables = GenerateValidationTables(report.ValidationTables, report.ShowHiddenRows);
        }

        private static List<SequenceReportValidationTableDetail> GenerateValidationTables(List<ReportValidationTable> validationTables,
            bool showHiddenRows)
        {
            var output = new List<SequenceReportValidationTableDetail>(validationTables.Count);

            foreach (var validationTable in validationTables)
            {
                var columns = GetColumns(validationTable.Rows);
                var reportValidationTable = new SequenceReportValidationTableDetail
                                            {
                                                Name = validationTable.Name,
                                                TableStatus = EnumTransformHelper.TransformTableStatus(validationTable.TableStatus),
                                                ValidationMode = EnumTransformHelper.TransformValidationMode(validationTable.ValidationMode),
                                                ValidationTime = validationTable.ValidationTime,
                                                Columns = columns,
                                                Rows = GetRows(validationTable.Rows, columns, showHiddenRows),
                                            };

                output.Add(reportValidationTable);
            }

            return output;
        }

        private static List<SequenceReportValidationTableRowDetail> GetRows(List<ReportValidationTableRow> rows, List<string> columns,
            bool showHiddenRows)
        {
            return new List<SequenceReportValidationTableRowDetail>(
                rows.Where(row => !row.IsHidden || showHiddenRows).Select(x => new SequenceReportValidationTableRowDetail
                                                                               {
                                                                                   ActualValue = x.ActualValue,
                                                                                   HasImage = x.HasImage,
                                                                                   IsHidden = x.IsHidden,
                                                                                   Name = x.Name,
                                                                                   Status = EnumTransformHelper.TransformTableStatus(
                                                                                       x.Status),
                                                                                   NumberOfRepetition = x.NumberOfRepetition,
                                                                                   Columns = GenerateColumnValues(x, columns),
                                                                               }));
        }

        private static List<ReportValidationTableColumnDetail> GenerateColumnValues(ReportValidationTableRow row, List<string> columns)
        {
            var output = new List<ReportValidationTableColumnDetail>();

            if (columns.Contains(ValidationTableConstants.ColumnMinKey))
            {
                output.Add(CreateColumn(row.MinValue, row.MinStatus, true));
            }

            if (columns.Contains(ValidationTableConstants.ColumnMaxKey))
            {
                output.Add(CreateColumn(row.MaxValue, row.MaxStatus, true));
            }

            if (columns.Contains(ValidationTableConstants.ColumnRequiredKey))
            {
                output.Add(CreateColumn(row.RequiredValue, row.RequiredStatus, false));
            }

            if (columns.Contains(ValidationTableConstants.ColumnProhibitedKey))
            {
                output.Add(CreateColumn(row.ProhibitedValue, row.ProhibitedStatus, false));
            }

            if (columns.Contains(ValidationTableConstants.ColumnGsKey))
            {
                output.Add(CreateColumn(row.GsValue, row.GsStatus, true));
            }

            return output;
        }

        private static ReportValidationTableColumnDetail CreateColumn(string value, bool status, bool canRound)
        {
            return new ReportValidationTableColumnDetail
                   {
                       Value = value,
                       Status = status,
                       CanRound = canRound,
                   };
        }


        private static List<string> GetColumns(List<ReportValidationTableRow> rows)
        {
            var columns = new List<string>();

            if (rows != null)
            {
                if (rows.Any(x => x.MinValue != null))
                {
                    columns.Add(ValidationTableConstants.ColumnMinKey);
                }

                if (rows.Any(x => x.MaxValue != null))
                {
                    columns.Add(ValidationTableConstants.ColumnMaxKey);
                }

                if (rows.Any(x => x.RequiredValue != null))
                {
                    columns.Add(ValidationTableConstants.ColumnRequiredKey);
                }

                if (rows.Any(x => x.ProhibitedValue != null))
                {
                    columns.Add(ValidationTableConstants.ColumnProhibitedKey);
                }

                if (rows.Any(x => x.GsValue != null))
                {
                    columns.Add(ValidationTableConstants.ColumnGsKey);
                }
            }

            return columns;
        }

        public static TTarget MapEntity<TTarget, TSource>(TSource entity)
            where TTarget : class, new()
            where TSource : class
        {
            var item = new TTarget();

            MapEntity(item, entity);
            
            return item;
        }

        private static void MapListCollection(IList newList, IEnumerable originalList)
        {
            if (originalList != null && newList != null)
            {
                foreach (var item in originalList)
                {
                    if (newList.GetType().GenericTypeArguments.Length == 1)
                    {
                        var newItem = Activator.CreateInstance(newList.GetType().GenericTypeArguments[0]);
                        MapEntity(newItem, item);
                        newList.Add(newItem);
                    }
                }
            }
        }

        private static bool IsListType(Type type)
        {
            return type.GetInterface(nameof(IList)) != null;
        }

        private static void MapEntity<TTarget, TSource>(TTarget targetEntity, TSource sourceEntity, bool ignoreCollection = false)
        {
            var entityProperties = sourceEntity.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => x.CanRead && x.CanWrite).ToDictionary(key => key.Name, value => value);


            foreach (var property in targetEntity.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => x.CanRead && x.CanWrite))
            {
                if (entityProperties.ContainsKey(property.Name))
                {
                    if (property.PropertyType.IsGenericType)
                    {
                        if (property.PropertyType.IsValueType)
                        {
                            var valueType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                            var originalValue = entityProperties[property.Name].GetValue(sourceEntity);

                            if (originalValue != null)
                            {
                                originalValue = valueType.IsEnum ? Enum.ToObject(valueType, originalValue) : Convert.ChangeType(originalValue, valueType, CultureInfo.InvariantCulture);
                            }
                            property.SetValue(targetEntity, originalValue);
                        }
                        else if (!ignoreCollection)
                        {
                            var originalProperty = entityProperties[property.Name];

                            if (IsListType(property.PropertyType) && IsListType(originalProperty.PropertyType))
                            {
                                var instance = Activator.CreateInstance(property.PropertyType) as IList;
                                MapListCollection(instance, originalProperty.GetValue(sourceEntity) as IList);

                                property.SetValue(targetEntity, instance);
                            } 
                        }
                    }
                    else
                    {
                        property.SetValue(targetEntity, entityProperties[property.Name].GetValue(sourceEntity));
                    }
                }
            }
        }

        public static List<SummaryReportSettings> TransformSummaries(IEnumerable<SummaryReport> summaries)
        {
            return summaries.Select(x => new SummaryReportSettings
                                         {
                                             Id = x.Id,
                                             Name = x.Name,
                                             CanModifyFilterInView = x.CanModifyFilterInView,
                                             Index = x.Index,
                                             IsPinned = x.IsPinned,
                                             Title = x.Title,
                                             Filter = new ReportFilter
                                                      {
                                                          SequenceName = x.SequenceName,
                                                          CycleName = x.CycleName,
                                                          Last24Hours = x.Last24Hours,
                                                          LastWeek = x.LastWeek,
                                                          ReportStatus = x.ReportStatus,
                                                          StartTimeFrom = x.StartTimeFrom,
                                                          StartTimeTo = x.StartTimeTo,
                                                      }
                                         }).OrderBy(x=>x.Index).ToList();
        }

        public static void TransformSummary(SummaryReport dbSummary, SummaryReportSettings summary)
        {
            AssignSummaryProperties(dbSummary, summary);

            for (int i = 0; i < summary.Panels.Count; i++)
            {
                var panelBase = summary.Panels[i];

                switch (panelBase)
                {
                    case LineChartPanel panel:
                        AssignPanel(dbSummary.LineChartPanels, panel, i, dbSummary, true);
                        AssignSeries(dbSummary.LineChartPanels, panel);
                        break;
                    case ReportsOverviewPanel panel:
                        AssignPanel(dbSummary.OverviewPanels, panel, i, dbSummary, true);
                        break;
                    case TextPanel panel:
                        AssignPanel(dbSummary.TextPanels, panel, i, dbSummary, true);
                        break;
                }
            }
        }

        private static void AssignSeries(List<DbLineChartPanel> panelCollection, LineChartPanel panel)
        {
            if (panel.Series!=null)
            {
                var dbItem = panelCollection.FirstOrDefault(x => x.Id == panel.Id);

                if (dbItem != null)
                {
                    for (var i = 0; i < panel.Series.Count; i++)
                    {
                        var seriesItem = panel.Series[i];
                        var dbSeries = dbItem.Series.FirstOrDefault(x => x.Id == seriesItem.Id);
                        if (dbSeries != null)
                        {
                            if (!seriesItem.IsDeleted)
                            {
                                MapEntity(dbSeries, seriesItem);
                                dbSeries.Index = i;
                            }
                            else
                            {
                                dbItem.Series.Remove(dbSeries);
                            }
                        }
                        else
                        {
                            if (!seriesItem.IsDeleted)
                            {
                                var newEntity = MapEntity<DbLineChartSeries, LineChartSeriesSettings>(seriesItem);
                                newEntity.Index = i;
                                dbItem.Series.Add(newEntity);
                            }
                        }
                    }
                } 
            }
        }

        private static void AssignPanel<TBdPanel, TUiPanel>(List<TBdPanel> panelCollection, TUiPanel panel, int i, SummaryReport dbSummary, bool ignoreCollection)
            where TBdPanel : class, IDbReportSummaryPanel, new()
            where TUiPanel : PanelBase
        {
            var item = panelCollection.FirstOrDefault(x => x.Id == panel.Id);
            if (item != null)
            {
                if (!panel.IsDeleted)
                {
                    MapEntity(item, panel, ignoreCollection);
                    item.Index = i;
                }
                else
                {
                    panelCollection.Remove(item);
                }
            }
            else
            {
                if (!panel.IsDeleted)
                {
                    var entity = MapEntity<TBdPanel, TUiPanel>(panel);
                    entity.Id = 0;
                    entity.Index = i;
                    entity.SummaryReportId = dbSummary.Id;
                    entity.SummaryReport = dbSummary;
                    panelCollection.Add(entity);
                }
            }
        }


        private static void AssignSummaryProperties(SummaryReport dbSummary, SummaryReportSettings uiSummary)
        {
            dbSummary.Name = uiSummary.Name;
            dbSummary.CanModifyFilterInView = uiSummary.CanModifyFilterInView;
            dbSummary.CycleName = uiSummary.Filter.CycleName;
            dbSummary.Index = uiSummary.Index;
            dbSummary.IsPinned = uiSummary.IsPinned;
            dbSummary.Last24Hours = uiSummary.Filter.Last24Hours;
            dbSummary.LastWeek = uiSummary.Filter.LastWeek;
            dbSummary.ReportStatus = uiSummary.Filter.ReportStatus;
            dbSummary.SequenceName = uiSummary.Filter.SequenceName;
            dbSummary.StartTimeFrom = uiSummary.Filter.StartTimeFrom;
            dbSummary.StartTimeTo = uiSummary.Filter.StartTimeTo;
            dbSummary.Title = uiSummary.Title;
        }
    }
}