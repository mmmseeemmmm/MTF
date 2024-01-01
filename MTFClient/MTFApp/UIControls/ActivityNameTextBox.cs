using System.Windows;
using System.Windows.Controls;
using MTFClientServerCommon;

namespace MTFApp.UIControls
{
    public class ActivityNameTextBox : TextBox
    {
        private string tmpText = string.Empty;
        private string newText = string.Empty;

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == TextProperty)
            {
                newText = (string)e.NewValue;
                if ((string)e.NewValue == string.Empty)
                {
                    tmpText = (string)e.OldValue;
                }
            }
            if (e.Property == DataContextProperty)
            {
                if (e.NewValue == null)
                {
                    if (!string.IsNullOrEmpty(tmpText))
                    {
                        var activity = e.OldValue as MTFSequenceActivity;
                        if (activity != null)
                        {
                            activity.ActivityName = tmpText;
                        }
                    }
                    else if (!string.IsNullOrEmpty(newText))
                    {
                        var activity = e.OldValue as MTFSequenceActivity;
                        if (activity != null)
                        {
                            activity.ActivityName = newText;
                        }
                    }
                }
            }
            base.OnPropertyChanged(e);
        }
    }
}
