using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using ALControls;
using MTFClientServerCommon;
using MTFClientServerCommon.Constants;
using MTFClientServerCommon.Helpers;
using MTFClientServerCommon.SequenceLocalization;
using TextBlock = System.Windows.Controls.TextBlock;

namespace MTFApp.UIHelpers.Editors.LocalizedString
{
    /// <summary>
    /// Interaction logic for LocalizedActivityPathTextBlock.xaml
    /// </summary>
    public partial class LocalizedActivityPathTextBlock : UserControl, ILocTextKeyExplicit, INotifyPropertyChanged
    {
        private string value;

        private NameDictionary ActualDictionary
        {
            get { return SequenceLocalizationHelper.ActualDictionary; }
        }

        public LocalizedActivityPathTextBlock()
        {
            InitializeComponent();
            Root.DataContext = this;
        }

        public string Value
        {
            get { return value; }
            set
            {
                this.value = value;
                NotifyPropertyChanged();
            }
        }

        public string ListSeparator { get; set; }



        public List<ActivityIdentifier> ActivityList
        {
            get { return (List<ActivityIdentifier>)GetValue(ActivityListProperty); }
            set { SetValue(ActivityListProperty, value); }
        }

        public static readonly DependencyProperty ActivityListProperty =
            DependencyProperty.Register("ActivityList", typeof(List<ActivityIdentifier>), typeof(LocalizedActivityPathTextBlock), new FrameworkPropertyMetadata(ActivityLichChanged));

        private static void ActivityLichChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var source = (LocalizedActivityPathTextBlock)d;
            source.JoinValues(e.NewValue as List<ActivityIdentifier>);
        }

        private void JoinValues(List<ActivityIdentifier> activityList)
        {
            var separator = ListSeparator ?? ActivityNameConstants.ActivityPathSeparator;
            Value = SequenceLocalizationHelper.TranslateActivityPath(activityList, separator);
        }


        public MTFSequenceActivity Activity
        {
            get { return (MTFSequenceActivity)GetValue(ActivityProperty); }
            set { SetValue(ActivityProperty, value); }
        }

        public static readonly DependencyProperty ActivityProperty =
            DependencyProperty.Register("Activity", typeof(MTFSequenceActivity), typeof(LocalizedActivityPathTextBlock),
            new FrameworkPropertyMetadata(ActivityChanged));

        private static void ActivityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var source = (LocalizedActivityPathTextBlock)d;
            source.GenerateActivityPath(e.NewValue as MTFSequenceActivity);
        }

        private void GenerateActivityPath(MTFSequenceActivity activity)
        {
            if (activity != null)
            {
                StringBuilder sb = new StringBuilder();
                GeneratePath(activity, sb);
                Value = sb.ToString();
            }
        }

        private string GeneratePath(MTFSequenceActivity activity, StringBuilder sb)
        {
            if (activity.Parent is MTFSequenceActivity)
            {
                sb.Append(GeneratePath(activity.Parent as MTFSequenceActivity, sb));
                sb.Append("\\");
            }
            else
            {
                var mtfCase = activity.Parent as MTFCase;
                if (mtfCase != null)
                {
                    sb.Append(GeneratePath(mtfCase.Parent as MTFSequenceActivity, sb));
                    sb.Append(mtfCase.Name);
                    sb.Append("\\");
                    return mtfCase.Name;
                }
            }
            return MTFSequenceActivityHelper.CombineTranslatedActivityName(ActualDictionary.GetValue(activity.ActivityName), activity.UniqueIndexer);
        }

        public void SetLocTextExplicit()
        {
            if (ActivityList == null)
            {
                GenerateActivityPath(Activity);
            }
            else
            {
                JoinValues(ActivityList);
            }
        }

        public Style TextBlockStyle
        {
            get { return (Style)GetValue(TextBlockStyleProperty); }
            set { SetValue(TextBlockStyleProperty, value); }
        }

        public static readonly DependencyProperty TextBlockStyleProperty =
            DependencyProperty.Register("TextBlockStyle", typeof(Style), typeof(LocalizedActivityPathTextBlock),
            new PropertyMetadata(Application.Current.TryFindResource(typeof(TextBlock))));

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (!string.IsNullOrEmpty(propertyName))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); 
            }
        }
    }
}
