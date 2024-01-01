using MTFApp.OpenSaveSequencesDialog;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using MTFClientServerCommon.Constants;
using MTFClientServerCommon.Helpers;

namespace MTFApp.ImportSequence
{
    /// <summary>
    /// Interaction logic for ImportDestination.xaml
    /// </summary>
    public partial class ImportDestination : ImportSequenceBase
    {
        private readonly UserControl control;
        private readonly OpenSaveSequencesDialogPresenter dialogPresenter;

        public ImportDestination(ImportSharedData sharedData)
            : base(sharedData)
        {
            InitializeComponent();
            DataContext = this;
            control = new OpenSaveSequencesDialogControl(DialogTypeEnum.InnerDialog, BaseConstants.SequenceBasePath,
                new List<string> {BaseConstants.SequenceExtension}, true, false);

            dialogPresenter = (OpenSaveSequencesDialogPresenter)control.DataContext;
            dialogPresenter.PathChanged += dialogPresenterPathChanged;
            dialogPresenterPathChanged();
        }

        void dialogPresenterPathChanged()
        {
            SharedData.SequenceDestinationPath = dialogPresenter.LastFullName;
        }

        public UserControl Control
        {
            get { return control; }
        }

        public override string Title
        {
            get { return LanguageHelper.GetString("Mtf_Import_DestTitle"); }
        }

        public override string Description
        {
            get { return LanguageHelper.GetString("Mtf_Import_DestDesc"); }
        }

        public override Button UserButton1
        {
            get
            {
                var button = new Button {Content = LanguageHelper.GetString("Buttons_NewFolder")};
                button.Click += UserButton1_Click;
                return button;
            }
        }

        private void UserButton1_Click(object sender, RoutedEventArgs e)
        {
            string fullName = Path.Combine(SharedData.SequenceDestinationPath, LanguageHelper.GetString("Buttons_NewFolder"));
            dialogPresenter.CreateDirectory.Execute(fullName);
        }

        public override void Dispose()
        {
            dialogPresenter.PathChanged -= dialogPresenterPathChanged;
        }
    }
}