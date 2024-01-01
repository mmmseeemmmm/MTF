using System;
using System.Windows;
using MTFClientServerCommon;
using MTFClientServerCommon.Constants;
using MTFClientServerCommon.Helpers;

namespace MTFApp.UIHelpers.Editors.LocalizedString
{
    /// <summary>
    /// Interaction logic for LocalizedCallActivity.xaml
    /// </summary>
    public partial class LocalizedCallActivity : LocalizedStringTextBlockBase
    {
        private string displayValue;

        public LocalizedCallActivity()
        {
            InitializeComponent();
            Root.DataContext = this;
        }

        public MTFExecuteActivity CallActivity
        {
            get { return (MTFExecuteActivity)GetValue(CallActivityProperty); }
            set { SetValue(CallActivityProperty, value); }
        }

        public static readonly DependencyProperty CallActivityProperty =
            DependencyProperty.Register("CallActivity", typeof(MTFExecuteActivity), typeof(LocalizedCallActivity),
                new FrameworkPropertyMetadata(CallActivityChanged));

        private static void CallActivityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var editor = (LocalizedCallActivity)d;
            if (e.NewValue != null)
            {
                editor.RefreshValue();
            }
        }

        public bool InvalidateName
        {
            get { return (bool)GetValue(InvalidateNameProperty); }
            set { SetValue(InvalidateNameProperty, value); }
        }

        public static readonly DependencyProperty InvalidateNameProperty =
            DependencyProperty.Register("InvalidateName", typeof(bool), typeof(LocalizedCallActivity),
            new FrameworkPropertyMetadata() { CoerceValueCallback = Invalidate1 });

        private static object Invalidate1(DependencyObject d, object basevalue)
        {
            var editor = (LocalizedCallActivity)d;
            editor.RefreshValue();
            return basevalue;
        }

        protected override void OnIdentifierChanged(object newValue)
        {
            RefreshValue();
        }

        protected override void OnUniqueIndexerChanged(int newValue)
        {
            if (newValue > 0)
            {
                RefreshValue();
            }
        }

        public string DisplayValue
        {
            get { return displayValue; }
            set
            {
                displayValue = value;
                NotifyPropertyChanged();
            }
        }

        public override void SetLocTextExplicit()
        {
            RefreshValue();
        }

        private void RefreshValue()
        {
            var baseName = ActualDictionary.GetValue(GetBaseKey());
            var displayName = string.Format("{0} <{1}>", baseName, GetCalledActivity(CallActivity));
            DisplayValue = UniqueIndexer > 0 ? AdjustName(displayName, UniqueIndexer) : displayName;
        }

        private string GetBaseKey()
        {
            if (CallActivity!=null && CallActivity.Type == ExecuteActyvityTypes.Dynamic)
            {
                return ActivityNameConstants.DynamicCallActivity;
            }
            return ActivityNameConstants.CallActivity;
        }

        private string GetCalledActivity(MTFExecuteActivity callActivity)
        {
            if (callActivity != null)
            {
                switch (callActivity.Type)
                {
                    case ExecuteActyvityTypes.Local:
                        if (callActivity.ActivityToCall != null)
                        {
                            return AdjustName(ActualDictionary.GetValue(callActivity.ActivityToCall.ActivityName), callActivity.ActivityToCall.UniqueIndexer);
                        }
                        break;
                    case ExecuteActyvityTypes.External:
                        if (callActivity.ExternalCall!=null)
                        {
                            if (string.IsNullOrEmpty(callActivity.ExternalCall.OriginalCallActivityName))
                            {
                                if (!string.IsNullOrEmpty(callActivity.ExternalCall.ExternalSequenceToCall))
                                {
                                    return callActivity.ExternalCall.ExternalSequenceToCall; 
                                }
                            }
                            else
                            {
                                return AdjustName(ActualDictionary.GetValue(callActivity.ExternalCall.OriginalCallActivityName), callActivity.ExternalCall.CallActivityIndexer);
                            }
                        }
                        break;
                    case ExecuteActyvityTypes.Dynamic:
                        return LanguageHelper.GetString(callActivity.DynamicActivityType.Description());
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            return LanguageHelper.GetString("Activity_CallActivity_CallNull");
        }
    }
}