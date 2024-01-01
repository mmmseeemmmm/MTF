using System.Windows;
using System.Windows.Controls;

namespace MTFApp.SequenceEditor.DetailDataTemplates
{
    public abstract class ActivityDetailBase : UserControl
    {
        public EditorDisplayMode DisplayMode
        {
            get { return (EditorDisplayMode)GetValue(DisplayModeProperty); }
            set { SetValue(DisplayModeProperty, value); }
        }

        public static readonly DependencyProperty DisplayModeProperty =
            DependencyProperty.Register("DisplayMode", typeof(EditorDisplayMode), typeof(ActivityDetailBase), new FrameworkPropertyMetadata());

    }
}
