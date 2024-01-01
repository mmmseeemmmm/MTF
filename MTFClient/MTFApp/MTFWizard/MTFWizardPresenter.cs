using MTFApp.UIHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace MTFApp.MTFWizard
{
    class MTFWizardPresenter : PresenterBase
    {
        private MTFWizardUserControl control;
        private readonly List<MTFWizardUserControl> controls;
        private int selectedControlIndex = 0;
        private bool? dialogResult = null;

        public MTFWizardPresenter(List<MTFWizardUserControl> controls)
        {
            this.controls = controls;
            if (controls.Count > 0)
            {
                this.Control = controls[selectedControlIndex];
            }
        }


        public bool? DialogResult
        {
            get { return dialogResult; }
            set { dialogResult = value; }
        }


        public MTFWizardUserControl Control
        {
            get { return control; }
            set
            {
                control = value;
                NotifyPropertyChanged();
            }
        }

        public ICommand CancelCommand
        {
            get { return new Command(Close); }
        }

        public ICommand FinishCommand
        {
            get
            {
                return new Command(() =>
                                   {
                                       this.DialogResult = this.control.Finish();
                                       Close();
                                   });
            }
        }

        public ICommand NextCommand
        {
            get
            {
                return new Command(() =>
                                   {
                                       if (selectedControlIndex + 1 < controls.Count)
                                       {
                                           SwitchControl(++selectedControlIndex, true);
                                       }
                                   });
            }
        }

        private void SwitchControl(int index, bool next)
        {
            var nextControl = controls[index];
            if (nextControl.Skip)
            {
                if (next)
                {
                    NextCommand.Execute(null);
                }
                else
                {
                    BackCommand.Execute(null);
                }

                return;
            }
            Control.Hide();
            Control = nextControl;
            Control.Show();
        }


        public ICommand BackCommand
        {
            get
            {
                return new Command(() =>
                                   {
                                       if (selectedControlIndex - 1 >= 0)
                                       {
                                           SwitchControl(--selectedControlIndex, false);
                                       }
                                   });
            }
        }

        private void Close()
        {
            foreach (var item in controls)
            {
                if (item is IDisposable)
                {
                    (item as IDisposable).Dispose();
                }
            }
            App.Current.Windows.OfType<MTFWizardWindow>().FirstOrDefault().Close();
        }
    }
}