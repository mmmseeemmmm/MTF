using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using Microsoft.Win32;
using MTFApp.MTFWizard;
using MTFApp.SequenceEditor.GraphicalView;
using MTFApp.UIHelpers;
using MTFClientServerCommon.Helpers;
using MTFClientServerCommon;

namespace MTFApp.ImportSequence
{
    internal static class ImportHelper
    {
        internal static string ImportSequence(MTFClient client)
        {
            // ReSharper disable once PossibleNullReferenceException
            var mainPresenter = (MainWindowPresenter)Application.Current.MainWindow.DataContext;

            OpenFileDialog dialog = new OpenFileDialog
                                    {
                                        Filter = $"{LanguageHelper.GetString("Mtf_FileDialog_ZipFile")} (*.zip)|*.zip"
                                    };
            mainPresenter.IsDarken = true;


            if (dialog.ShowDialog() == true)
            {
                mainPresenter.IsDarken = false;
                var path = dialog.FileName;
                //var path = @"D:\\users\\F24098B\\Documents\\TestExportImage.zip";

                FileStream fs = null;
                try
                {
                    using (fs = new FileStream(path, FileMode.Open))
                    {
                        var sharedData = new ImportSharedData(fs);

                        List<MTFWizardUserControl> controls = new List<MTFWizardUserControl>()
                                                              {
                                                                  new ImportPreview(sharedData),
                                                                  new ImportDestination(sharedData),
                                                                  new ImportSequenceConflicts(sharedData),
                                                                  new ImportClassInstance(sharedData),
                                                                  new ImportGraphicalViewImages(sharedData),
                                                                  new ImportSummary(sharedData)
                                                              };


                        bool msgResult;
                        do
                        {
                            msgResult = false;
                            var importDialog = new MTFWizardWindow(controls) {Owner = Application.Current.MainWindow};

                            importDialog.ShowDialog();
                            if (importDialog.Result.HasValue)
                            {
                                bool importResult;
                                using (var ms = new MemoryStream())
                                {
                                    fs.Position = 0;
                                    fs.CopyTo(ms);
                                    client.ImportSequenceSetting(sharedData.ImportSetting);
                                    ms.Position = 0;
                                    importResult = client.ImportSequence(ms);
                                }

                                if (!importResult)
                                {
                                    msgResult =
                                        MTFMessageBox.Show(LanguageHelper.GetString("Msg_Header_Error"),
                                            LanguageHelper.GetString("Msg_Body_ImportError"),
                                            MTFMessageBoxType.Question, MTFMessageBoxButtons.YesNo) == MTFMessageBoxResult.Yes;
                                }
                                else
                                {
                                    GraphicalViewResourcesHelper.Instance.Reload();

                                    var importSetting = sharedData.ImportSetting;
                                    if (importDialog.Result != null && importSetting.Sequences != null && importSetting.Sequences.Count > 0)
                                    {
                                        var sequenceToOpen = importSetting.Sequences.FirstOrDefault(x => x.EnableImport);
                                        if (sequenceToOpen != null)
                                        {
                                            return sequenceToOpen.StorePath;
                                        }
                                    }
                                }
                            }
                        }
                        while (msgResult);
                        
                    }
                }
                catch (Exception ex)
                {
                    MTFMessageBox.Show(LanguageHelper.GetString("Msg_Header_Error"), ex.Message, MTFMessageBoxType.Error, MTFMessageBoxButtons.Ok);
                }
            }

            mainPresenter.IsDarken = false;
            return null;
        }
    }
}