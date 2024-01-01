using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using MTFClientServerCommon.Helpers;
using MTFClientServerCommon.MTFTable;
using MTFClientServerCommon.MTFValidationTable;

namespace MTFClientServerCommon.GoldSamplePersist
{
    public static class GoldSampleHelper
    {
        public static GoldSamplePersist Load(string fileName)
        {
            var fullFilePath = GetFullPath(fileName);
            if (File.Exists(fullFilePath))
            {
                var formater = new BinaryFormatter();
                try
                {
                    using (var reader = new FileStream(fullFilePath, FileMode.Open))
                    {
                        return formater.Deserialize(reader) as GoldSamplePersist;
                    }
                }
                catch (Exception)
                {
                    return null;
                }
            }
            return null;
        }


        public static void Save(string fileName, GoldSamplePersist data)
        {
            var filePath = GetFullPath(fileName);
            var formater = new BinaryFormatter();
            try
            {
                using (var writer = new FileStream(filePath, FileMode.Create))
                {
                    formater.Serialize(writer, data);
                }
            }
            catch (DirectoryNotFoundException)
            {
                var directory = Path.GetDirectoryName(filePath);
                if (directory != null)
                {
                    FileHelper.CreateDirectory(directory);
                    Save(filePath, data, formater);
                }
            }
        }

        private static void Save(string filePath, GoldSamplePersist data, BinaryFormatter formater)
        {
            using (var writer = new FileStream(filePath, FileMode.Create))
            {
                formater.Serialize(writer, data);
            }
            FileHelper.SetFileForEveryone(filePath);
        }

        public static void Rename(string oldFileName, string newSequenceName)
        {
            var fullFilePath = GetFullPath(oldFileName);
            if (File.Exists(fullFilePath))
            {
                var newName = fullFilePath.Replace(oldFileName, Path.GetFileNameWithoutExtension(newSequenceName));
                File.Move(fullFilePath, newName);
            }
        }

        public static void Remove(string fileName)
        {
            var fullFileName = GetFullPath(fileName);
            if (File.Exists(fullFileName))
            {
                File.Delete(fullFileName);
            }
        }


        private static string GetFullPath(string fileName)
        {
            return Path.Combine(Constants.BaseConstants.GoldSampleBasePath, fileName);
        }

        public static object GetGoldSampleValue(IEnumerable<MTFValidationTableCell> columns)
        {
            if (columns != null)
            {
                var gs = columns.FirstOrDefault(x => x.Type == MTFTableColumnType.GoldSample);
                if (gs != null)
                {
                    return gs.Value;
                }
            }
            return null;
        }

        public static void AssignGoldSampleValue(IEnumerable<MTFValidationTableCell> columns, object value)
        {
            if (columns != null)
            {
                var goldSampleColumn = columns.FirstOrDefault(x => x.Type == MTFTableColumnType.GoldSample);
                if (goldSampleColumn != null)
                {
                    goldSampleColumn.Value = value;
                }
            }
        }

        public static FileInfo GetGoldSampleDataFileInfo(string fileName)
        {
            return new FileInfo(GetFullPath(fileName));
        }

        public static GoldSamplePersist LoadGSData(GoldSampleSetting setting)
        {
            if (setting != null && !string.IsNullOrEmpty(setting.GoldSampleDataFile))
            {
                var gs = Load(setting.GoldSampleDataFile);
                if (gs != null)
                {
                    gs.CheckNull();
                    return gs;
                }
            }
            return new GoldSamplePersist();
        }


        public static void GetNewGsDataFileName(MTFSequence sequence)
        {
            var goldSampleSetting = sequence.GoldSampleSetting;
            if (goldSampleSetting != null)
            {
                if (string.IsNullOrEmpty(goldSampleSetting.GoldSampleDataFile))
                {
                    goldSampleSetting.GoldSampleDataFile = string.Format("{0}_{1}{2}", sequence.Name, Guid.NewGuid(), Constants.BaseConstants.GoldSampleExtension);
                }
            }
        }


        public static GoldSampleInfo CheckGoldSample(MTFSequence sequence, GoldSamplePersist data,
            SequenceVariant currentVariant, DateTime sequenceStart, bool checkAfterStart, bool requireAfterInactivity)
        {
            var now = DateTime.Now;
            var setting = sequence.GoldSampleSetting;
            
            bool isGoldSample = currentVariant.MatchGoldSample(setting.GoldSampleSelector, sequence.VariantGroups);
            currentVariant = currentVariant.Clone() as SequenceVariant;

            var gsInfo = new GoldSampleInfo
            {
                CurrentVariant = currentVariant,
                SequenceVariantInfo = new SequenceVariantInfo
                {
                    SequenceVariant = currentVariant,
                    AllowMoreGS = setting.MoreGoldSamples,
                    IsGoldSample = isGoldSample,
                }
            };

            if (isGoldSample)
            {
                data.LastGoldSample = currentVariant;
                data.OtherVariantCounter = 0;
                var goldSampleVariantInfo = data.GoldSampleList.FirstOrDefault(x => currentVariant.Match(x.SequenceVariant));
                if (goldSampleVariantInfo == null)
                {
                    data.AddGoldSampleVariant(gsInfo.SequenceVariantInfo, setting.MoreGoldSamples);
                    gsInfo.SequenceVariantInfo.Reset(setting, now);
                }
                else
                {
                    goldSampleVariantInfo.Reset(setting, now);
                    AssignValues(gsInfo.SequenceVariantInfo, goldSampleVariantInfo);
                    if (!setting.MoreGoldSamples)
                    {
                        data.AddGoldSampleVariant(gsInfo.SequenceVariantInfo, setting.MoreGoldSamples);
                    }
                }
            }
            else
            {
                var existingGoldSample = currentVariant.GetBestGoldSample(data.GoldSampleList.Select(x => x.SequenceVariant), setting.GoldSampleSelector);
                if (existingGoldSample != null)
                {
                    var existingVariantInfo = data.GoldSampleList.FirstOrDefault(x => Equals(x.SequenceVariant, existingGoldSample));
                    if (existingVariantInfo != null)
                    {
                        gsInfo.SequenceVariantInfo = existingVariantInfo;
                        gsInfo.SequenceVariantInfo.AllowMoreGS = setting.MoreGoldSamples;
                        gsInfo.SequenceVariantInfo.GoldSampleExpired = false;
                        gsInfo.SequenceVariantInfo.MissingGoldSample = false;
                        gsInfo.SequenceVariantInfo.IsGoldSample = false;
                    }
                }

                var inactivity = SetInactivity(now, data.LastUsedVariantTime, sequenceStart);
                if (requireAfterInactivity || (setting.AllowGoldSampleAfterInactivity && inactivity.TotalMinutes > setting.GoldSampleAfterInactivityInMinutes))
                {
                    gsInfo.ReguireAfterInactivity = true;
                    SetErrorMessage(gsInfo, "Gold sample is required because of inactivity.");
                    return gsInfo;
                }
                gsInfo.ReguireAfterInactivity = false;


                if (checkAfterStart)
                {
                    var afterStart = now - sequenceStart;
                    if (setting.AllowGoldSampleAfterStart && afterStart.TotalMinutes > setting.GoldSampleAfterStartInMinutes)
                    {
                        SetErrorMessage(gsInfo, "Gold sample is required after start sequence.");
                        return gsInfo;
                    }
                }


                gsInfo.VariantIsChanged = data.LastGoldSample == null ||
                    !currentVariant.Match(data.LastGoldSample, setting.GoldSampleSelector.VariantGroups.Select(x=>x.Name));
                if (gsInfo.VariantIsChanged)
                {
                    data.OtherVariantCounter++;
                    if (setting.GoldSampleAfterVariantChanged && data.OtherVariantCounter > setting.VariantChangedAfterSamplesCount)
                    {
                        SetErrorMessage(gsInfo, "Gold sample is required because of variant has been changed.");
                        gsInfo.SequenceVariantInfo.MissingGoldSample = existingGoldSample == null;
                        return gsInfo;
                    }
                }

                if (existingGoldSample != null)
                {
                    if (gsInfo.SequenceVariantInfo != null)
                    {
                        gsInfo.SequenceVariantInfo.Increase(setting, data.LastUsedVariantTime, now);
                        if (!gsInfo.SequenceVariantInfo.IsValid(setting))
                        {
                            if (setting.GoldSampleValidationMode == GoldSampleValidationMode.Shift &&
                                Math.Abs(gsInfo.SequenceVariantInfo.NonGoldSampleRemainsMinutes) < setting.GoldSampleRequiredAfterShiftStartInMinutes)
                            {
                                gsInfo.SequenceVariantInfo.GoldSampleExpired = true;
                            }
                            else
                            {
                                SetErrorMessage(gsInfo, "Gold sample is expired.");
                            }
                        }
                    }
                    else
                    {
                        //it shouldn't happen
                        gsInfo.SequenceVariantInfo = new SequenceVariantInfo
                                                     {
                                                         SequenceVariant = currentVariant,
                                                         AllowMoreGS = setting.MoreGoldSamples,
                                                     };
                    }

                }
                else
                {
                    gsInfo.SequenceVariantInfo.MissingGoldSample = true;
                    if (gsInfo.VariantIsChanged)
                    {
                        gsInfo.SequenceVariantInfo.GoldSampleExpired = true;
                    }
                    else
                    {
                        SetErrorMessage(gsInfo, "Gold sample is required.");
                    }
                }
            }

            return gsInfo;
        }

        private static void AssignValues(SequenceVariantInfo sequenceVariantInfo, SequenceVariantInfo goldSampleVariantInfo)
        {
            sequenceVariantInfo.GoldSampleExpired = goldSampleVariantInfo.GoldSampleExpired;
            sequenceVariantInfo.MissingGoldSample = goldSampleVariantInfo.MissingGoldSample;
            sequenceVariantInfo.GoldSampleDate = goldSampleVariantInfo.GoldSampleDate;
            sequenceVariantInfo.NonGoldSampleCount = goldSampleVariantInfo.NonGoldSampleCount;
            sequenceVariantInfo.NonGoldSampleRemainsMinutes = goldSampleVariantInfo.NonGoldSampleRemainsMinutes;
            sequenceVariantInfo.NonGoldSampleShiftRemainsMinutes = goldSampleVariantInfo.NonGoldSampleShiftRemainsMinutes;
        }


        private static void SetErrorMessage(GoldSampleInfo gsInfo, string msg)
        {
            gsInfo.SequenceVariantInfo.GoldSampleExpired = true;
            gsInfo.RaiseMessage = true;
            gsInfo.Message = msg;
        }

        private static TimeSpan SetInactivity(DateTime now, DateTime? lastUsedTime, DateTime sequenceStart)
        {
            return lastUsedTime.HasValue ? now - lastUsedTime.Value : now - sequenceStart;
        }
    }


    public class GoldSampleInfo
    {
        public SequenceVariantInfo SequenceVariantInfo { get; set; }

        public SequenceVariant CurrentVariant { get; set; }

        public bool RaiseMessage { get; set; }

        public bool VariantIsChanged { get; set; }

        public string Message { get; set; }

        public bool ReguireAfterInactivity { get; set; }

    }
}
