using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Win32;
using MTFApp.UIHelpers;
using MTFClientServerCommon;

namespace MTFApp.SequenceEditor.Handlers
{
    public class ComponentDataHandler : NotifyPropertyBase
    {
        private readonly SequenceEditorPresenter editor;
        private Dictionary<string, DataPackageWrapper> sequenceData;
        private MTFSequenceClassInfo selectedClassInfo;
        private SequenceVariant dataFilter;
        private bool enableDataFilter = true;

        public ComponentDataHandler(SequenceEditorPresenter editor)
        {
            this.editor = editor;
        }

        #region Commands

        public ICommand RemoveStoredDataCommand
        {
            get { return new Command(RemoveStoredData); }
        }

        public ICommand ImportStoredDataCommand
        {
            get { return new Command(ImportStoredData); }
        }

        public ICommand ExportStoredDataCommand
        {
            get { return new Command(ExportStoredData); }
        }

        public ICommand DeleteAllStoredDataCommand
        {
            get { return new Command(DeleteAllStoredData); }
        }

        public ICommand CloneDataCommand
        {
            get { return new Command(CloneData); }
        }


        #endregion

        #region Properties

        public Dictionary<string, DataPackageWrapper> SequenceData
        {
            get { return sequenceData; }
        }

        public SequenceVariant DataFilter
        {
            get { return dataFilter; }
            set
            {
                dataFilter = value;
                ApplyDataFilter(value);
            }
        }

        public bool EnableDataFilter
        {
            get { return enableDataFilter; }
            set
            {
                enableDataFilter = value;
                NotifyPropertyChanged();
                if (value)
                {
                    ApplyDataFilter(dataFilter);
                }
                else
                {
                    ClearDataFilter();
                }
            }
        }

        private void CloneData(object param)
        {
            var list = param as IList<object>;
            if (list != null && list.Count == 2)
            {
                var dataWrapper = list[0] as DataWrapper;
                var dataPackageName = list[1] as string;
                if (dataWrapper != null && dataWrapper.Data != null && dataPackageName != null && selectedClassInfo != null)
                {
                    var uc = new CloneDataControl(dataWrapper.Data, dataPackageName, selectedClassInfo, editor.Sequence);
                    var pw = new PopupWindow.PopupWindow(uc, true) { Title = "Clone data" };
                    pw.ShowDialog();
                    if (uc.DialogResult!=null && uc.DialogResult.Result == MTFDialogResultEnum.Ok)
                    {
                        FillSequenceDataAsync(selectedClassInfo);
                    }
                }
            }

        }


        #endregion

        #region private methods

        private void DeleteAllStoredData(object param)
        {
            if (selectedClassInfo != null && selectedClassInfo.Data != null && Confirmation("Are you sure you want to remove all component data?"))
            {
                selectedClassInfo.Data.Clear();
                FillSequenceDataAsync(selectedClassInfo);
            }
        }

        private void ExportStoredData(object param)
        {
            SaveFileDialog sf = new SaveFileDialog
            {
                Filter = "Data|*.dat"
            };
            if (param is KeyValuePair<string, DataPackageWrapper>)
            {
                sf.Title = "Export all stored data";
                if (sf.ShowDialog() == true)
                {
                    var key = ((KeyValuePair<string, DataPackageWrapper>)param).Key;
                    if (selectedClassInfo.Data.ContainsKey(key))
                    {
                        SaveStoredData(new KeyValuePair<string, IList<ClassInfoData>>(key, selectedClassInfo.Data[key]), sf.FileName);
                    }
                }
            }
            else if (param is MTFSequenceClassInfo)
            {
                sf.Title = "Export stored data with all variants";
                if (sf.ShowDialog() == true)
                {
                    SaveStoredData(((MTFSequenceClassInfo)param).Data, sf.FileName);
                }
            }
            else if (param is DataWrapper)
            {
                sf.Title = "Export stored data for one variant";
                if (sf.ShowDialog() == true)
                {
                    SaveStoredData(((DataWrapper)param).Data.Data, sf.FileName);
                }
            }
        }

        private void SaveStoredData(object data, string fileName)
        {
            BinaryFormatter formatter = new BinaryFormatter();

            using (FileStream writer = new FileStream(fileName, FileMode.OpenOrCreate))
            {
                formatter.Serialize(writer, data);
                writer.Close();
            }
        }

        private void ImportStoredData(object param)
        {
            OpenFileDialog of = new OpenFileDialog
            {
                Filter = "Data|*.dat"
            };
            if (of.ShowDialog() != true)
            {
                return;
            }
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream reader = new FileStream(of.FileName, FileMode.Open);
            try
            {
                var data = formatter.Deserialize(reader);

                if (data is KeyValuePair<string, IList<ClassInfoData>>)
                {
                    if (!(param is MTFSequenceClassInfo))
                    {
                        MTFMessageBox.Show("Import Error", "Loaded data can't be imported to only one value.", MTFMessageBoxType.Error, MTFMessageBoxButtons.Ok);
                    }
                    else
                    {
                        var storedData = (KeyValuePair<string, IList<ClassInfoData>>)data;
                        ((MTFSequenceClassInfo)param).Data[storedData.Key] = storedData.Value;
                    }
                }
                else if (data is Dictionary<string, IList<ClassInfoData>>)
                {
                    if (!(param is MTFSequenceClassInfo))
                    {
                        MTFMessageBox.Show("Import Error", "Loaded data can't be imported to only one value.", MTFMessageBoxType.Error, MTFMessageBoxButtons.Ok);
                    }
                    else
                    {
                        ((MTFSequenceClassInfo)param).Data = (Dictionary<string, IList<ClassInfoData>>)data;
                    }
                }
                else if (data is byte[])
                {
                    if (!(param is ClassInfoData))
                    {
                        MTFMessageBox.Show("Import Error", "Single data can't be imported in this way.", MTFMessageBoxType.Error, MTFMessageBoxButtons.Ok);
                    }
                    else
                    {
                        ((ClassInfoData)param).Data = (byte[])data;
                        ((ClassInfoData)param).LastModifiedTime = DateTime.Now;
                    }
                }
            }
            catch (Exception e)
            {
                MTFMessageBox.Show("Import Error", "Data import failed: " + e, MTFMessageBoxType.Error, MTFMessageBoxButtons.Ok);
            }
            finally
            {
                reader.Close();
                reader.Dispose();
            }

            FillSequenceDataAsync(selectedClassInfo);
        }

        private void RemoveStoredData(object param)
        {
            bool reload = false;
            if (param is KeyValuePair<string, DataPackageWrapper>)
            {
                KeyValuePair<string, DataPackageWrapper> classInfoDataPair = (KeyValuePair<string, DataPackageWrapper>)param;
                if (Confirmation(string.Format("Are you sure you want to remove package {0}?", classInfoDataPair.Key)))
                {
                    reload = true;
                    selectedClassInfo.Data.Remove(classInfoDataPair.Key); 
                }
            }
            else
            {
                var dataWrapper = param as DataWrapper;
                if (dataWrapper != null)
                {
                    foreach (var dataList in selectedClassInfo.Data.Values)
                    {
                        if (dataList.Any(d => d == dataWrapper.Data) && Confirmation(string.Format("Are you sure you want to remove data for variant {0}?", dataWrapper.Data.SequenceVariant)))
                        {
                            reload = true;
                            dataList.Remove(dataWrapper.Data);
                        }
                    }
                }
            }

            if (reload)
            {
                FillSequenceDataAsync(selectedClassInfo); 
            }
        }

        private void ApplyDataFilter(SequenceVariant filterVariant)
        {
            if (sequenceData == null || filterVariant == null)
            {
                return;
            }
            SetAllSequenceDataVisibility(sequenceData, filterVariant.Match);
        }

        private void ClearDataFilter()
        {
            SetAllSequenceDataVisibility(sequenceData, x => true);
        }

        private void SetAllSequenceDataVisibility(Dictionary<string, DataPackageWrapper> data, Func<SequenceVariant, bool> func)
        {
            foreach (var value in data.Values)
            {
                foreach (var dataWrapper in value.DataPackage)
                {
                    if (dataWrapper.Data.SequenceVariant != null)
                    {
                        dataWrapper.IsVisible = func(dataWrapper.Data.SequenceVariant);
                    }
                }
            }
        }

        private List<DataWrapper> CreateDataWrapper(IList<ClassInfoData> list)
        {
            if (list == null)
            {
                return null;
            }
            var output = new List<DataWrapper>();
            foreach (var classInfoData in list)
            {
                output.Add(new DataWrapper { Data = classInfoData });
            }

            return output.OrderBy(d => d.Data.SequenceVariant).ToList();
        }

        private bool Confirmation(string msg)
        {
            var messageInfo = new MessageInfo
            {
                Text = msg,
                Type = SequenceMessageType.Question,
                Buttons = MessageButtons.OkCancel
            };
            var msgBox = new MessageBoxControl.MessageBoxControl(messageInfo);
            PopupWindow.PopupWindow pw = new PopupWindow.PopupWindow(msgBox)
            {
                CanClose = false,
                Title = "Remove data"
            };
            pw.ShowDialog();
            return msgBox.DialogResult.Result == MTFDialogResultEnum.Ok;
        }

        #endregion

        #region public methods

        public async void FillSequenceDataAsync(MTFSequenceClassInfo currentClassInfo)
        {
            Dictionary<string, DataPackageWrapper> tmpDict = null;
            await Task.Run(() =>
            {
                if (currentClassInfo.Data != null)
                {
                    tmpDict = new Dictionary<string, DataPackageWrapper>();

                    foreach (var dataDict in currentClassInfo.Data)
                    {
                        tmpDict[dataDict.Key] = new DataPackageWrapper { DataPackage = CreateDataWrapper(dataDict.Value) };
                    }
                }
                else
                {
                    tmpDict = null;
                }
            });
            sequenceData = tmpDict;
            NotifyPropertyChanged("SequenceData");
            ApplyDataFilter(dataFilter);
        }

        public void SetSelectedSequenceClassInfo(MTFSequenceClassInfo classInfo)
        {
            selectedClassInfo = classInfo;
        }

        #endregion
    }
}
