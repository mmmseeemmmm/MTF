using System.Collections.Generic;
using System.Linq;
using MTFClientServerCommon.Helpers;

namespace MTFApp.ExportSequence
{
    /// <summary>
    /// Interaction logic for ExportSequences.xaml
    /// </summary>
    public partial class ExportSequences : ExportSequenceBase
    {
        private readonly List<SequenceExportWrapper> sequencesToExport;
        private bool allowExport = true;

        public ExportSequences(ExportSharedData sharedData)
            : base(sharedData)
        {
            InitializeComponent();
            DataContext = this;

            sequencesToExport = FillSequences().ToList();
        }

        public override string Title
        {
            get { return LanguageHelper.GetString("Mtf_Export_SequencesTitle"); }
        }

        public override string Description
        {
            get { return LanguageHelper.GetString("Mtf_Export_SequencesDesc"); }
        }

        public override bool Skip
        {
            get { return sequencesToExport.Count == 0; }
        }

        public bool AllowExport
        {
            get { return allowExport; }
            set
            {
                allowExport = value;
                SetExportState(value);
            }
        }


        public List<SequenceExportWrapper> SequencesToExport
        {
            get { return sequencesToExport; }
        }

        private IEnumerable<SequenceExportWrapper> FillSequences()
        {
            foreach (var exportSetting in SharedData.SequencesToExport)
            {
                var wrapper = new SequenceExportWrapper(exportSetting) {IsMain = exportSetting.Guid == SharedData.MainSequence.Id};
                if (wrapper.IsMain)
                {
                    wrapper.EnableExport = true;
                    wrapper.PropertyChanged += WrapperPropertyChanged;
                }
                yield return wrapper;
            }
        }

        private void WrapperPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Export")
            {
                var item = (SequenceExportWrapper)sender;
                if (item.IsMain)
                {
                    if (item.Export)
                    {
                        SequencesToExport.ForEach(x =>
                                                  {
                                                      if (!x.IsMain)
                                                      {
                                                          x.Export = true;
                                                          x.EnableExport = false;
                                                      }
                                                  });
                    }
                    else
                    {
                        SequencesToExport.ForEach(x =>
                                                  {
                                                      if (!x.IsMain)
                                                      {
                                                          x.EnableExport = true;
                                                      }
                                                  });
                    }
                }
            }
        }

        private void SetExportState(bool value)
        {
            foreach (var item in SequencesToExport)
            {
                item.Export = value;
            }
        }
    }
}