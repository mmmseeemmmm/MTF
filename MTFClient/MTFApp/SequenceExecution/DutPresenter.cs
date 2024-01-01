using System;
using System.Collections.ObjectModel;
using MTFApp.SequenceExecution.GraphicalViewHandling;
using MTFApp.SequenceExecution.TableHandling;
using MTFApp.UIHelpers;
using MTFClientServerCommon;
using MTFClientServerCommon.Constants;
using MTFClientServerCommon.Helpers;

namespace MTFApp.SequenceExecution
{
    class DutPresenter : PresenterBase
    {
        private DeviceUnderTestInfo deviceUnderTest;

        public DeviceUnderTestInfo DeviceUnderTest
        {
            get => deviceUnderTest;
            set
            {
                deviceUnderTest = value;
                NotifyPropertyChanged();
            }
        }

        public ObservableCollection<ExecutionValidTable> ValidationTables { get; set; }

        private string line1;

        public string Line1
        {
            get => line1;
            set
            {
                switch (value)
                {
                    case BaseConstants.ExecutionStatusOk:
                        Status = true;
                        break;
                    case BaseConstants.ExecutionStatusNok:
                        Status = false;
                        break;
                    default:
                        Status = null;
                        break;
                }

                line1 = LanguageHelper.GetString(value);

                NotifyPropertyChanged();
            }
        }

        private string line2;

        public string Line2
        {
            get => line2;
            set
            {
                line2 = value;
                NotifyPropertyChanged();
            }
        }

        private string line3;

        public string Line3
        {
            get => line3;
            set
            {
                line3 = value;
                NotifyPropertyChanged();
            }
        }

        private StatusLinesFontSize linesFontSize;

        public StatusLinesFontSize LinesFontSize
        {
            get => linesFontSize;
            set
            {
                linesFontSize = value;
                NotifyPropertyChanged();
            }
        }

        private bool? status;
        public bool? Status
        {
            get => status;
            set
            {
                status = value;
                NotifyPropertyChanged();
            }
        }

        public void ClearStatus()
        {
            line1 = string.Empty;
            line2 = string.Empty;
            line3 = string.Empty;
            status = null;
            NotifyPropertyChanged("Line1");
            NotifyPropertyChanged("Line2");
            NotifyPropertyChanged("Line3");
            NotifyPropertyChanged("Status");
        }

        private SequenceVariant sequenceVariant;
        public SequenceVariant SequenceVariant
        {
            get => sequenceVariant;
            set
            {
                sequenceVariant = value; 
                NotifyPropertyChanged();
            }
        }

        private ExecutionGraphicalViewWrapper graphicalView;
        public ExecutionGraphicalViewWrapper GraphicalView
        {
            get => graphicalView;
            set
            {
                graphicalView = value;
                NotifyPropertyChanged();
            }
        }
    }
}
