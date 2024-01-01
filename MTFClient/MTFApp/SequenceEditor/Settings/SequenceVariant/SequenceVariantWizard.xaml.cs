using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using MTFApp.PopupWindow;
using MTFApp.UIHelpers.ValidationRules;
using MTFClientServerCommon;

namespace MTFApp.SequenceEditor.Settings.SequenceVariant
{
    /// <summary>
    /// Interaction logic for SequenceVariantWizard.xaml
    /// </summary>
    public partial class SequenceVariantWizard : UserControl, IRaiseCloseEvent
    {
        private bool hasError = false;

        public SequenceVariantWizard(MTFSequence sequence)
        {
            InitializeComponent();
            DataContext = new SequenceVariantWizardPresenter(sequence, HandleClose);
        }

        private void ButtonRemoveNewVariantOnClick(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var value = button.CommandParameter as VariantValueSetting;
            ((SequenceVariantWizardPresenter)DataContext).RemoveNewVariant(value, button.Tag);
        }

        private void NextCommandClick(object sender, RoutedEventArgs e)
        {
            ((SequenceVariantWizardPresenter)DataContext).SwitchUpdateExternal(false);
            hasError = false;
            var b = (Button)sender;
            var parentList = b.CommandParameter as FrameworkElement;

            ValidationExtension.GetChildOfTypes(parentList, new List<Type> {typeof(TextBox)}, ValidateElement);

            if (!hasError)
            {
                ((SequenceVariantWizardPresenter)DataContext).NextCommand.Execute(null);
            }

            ((SequenceVariantWizardPresenter)DataContext).SwitchUpdateExternal(true);
        }

        private void ValidateElement(FrameworkElement e)
        {
            if (e is TextBox textBox)
            {
                ProcessElement(TextBox.TextProperty, textBox);
            }
        }

        private void ProcessElement(DependencyProperty dependencyProperty, FrameworkElement element)
        {
            // ReSharper disable PossibleNullReferenceException
            element.GetBindingExpression(dependencyProperty).UpdateSource();

            if (!hasError)
            {
                hasError = Validation.GetHasError(element);
            }
        }

        public event CloseEventHandler Close;

        private void HandleClose()
        {
            Close?.Invoke(this);
        }
    }
}