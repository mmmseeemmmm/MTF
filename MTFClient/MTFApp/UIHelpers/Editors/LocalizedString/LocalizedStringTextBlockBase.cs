using System.Windows;
using System.Windows.Controls;

namespace MTFApp.UIHelpers.Editors.LocalizedString
{
    public abstract class LocalizedStringTextBlockBase : LocalizedStringBase
    {
        public LocalizedStringTextBlockBase()
        {
            TextWrapping = TextWrapping.NoWrap;
        }
        public Style TextBlockStyle
        {
            get { return (Style)GetValue(TextBlockStyleProperty); }
            set { SetValue(TextBlockStyleProperty, value); }
        }

        public static readonly DependencyProperty TextBlockStyleProperty =
            DependencyProperty.Register("TextBlockStyle", typeof(Style), typeof(LocalizedStringTextBlockBase),
            new PropertyMetadata(Application.Current.TryFindResource(typeof(TextBlock))));

        public TextWrapping TextWrapping { get; set; }
    }
}
