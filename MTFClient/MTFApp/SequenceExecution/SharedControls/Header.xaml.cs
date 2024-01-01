using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using MTFApp.SequenceExecution.TableHandling;

namespace MTFApp.SequenceExecution.SharedControls
{
    /// <summary>
    /// Interaction logic for Header.xaml
    /// </summary>
    public partial class Header : UserControl
    {
        private readonly DispatcherTimer timer = new DispatcherTimer();
        private object senderControl = null;
        public Header()
        {
            InitializeComponent();
            timer.Interval = new TimeSpan(0, 0, 0, 0, 400);
            timer.Tick += timer_Tick;
        }

        void timer_Tick(object sender, EventArgs e)
        {
            timer.Stop();
            OpenFirstNokImage(senderControl as FrameworkElement);
        }

        private void TableHeaderOnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            senderControl = sender;
            timer.Start();
            if (e.ClickCount == 2)
            {
                timer.Stop();
                OpenTableView(sender as FrameworkElement);
                e.Handled = true;
            }
            
        }

        private void OpenTableView(FrameworkElement fe)
        {
            var presenter = DataContext as SequenceExecutionPresenter;
            if (presenter != null)
            {
                presenter.OpenAllNokImages(fe.DataContext as ExecutionValidTable);
            }
        }

        private void OpenFirstNokImage(FrameworkElement fe)
        {
            var table = fe.DataContext as ExecutionValidTable;
            if (table != null)
            {
                var presenter = DataContext as SequenceExecutionPresenter;
                if (presenter != null)
                {
                    var img = table.GetFirstNokImage();
                    if (img != null)
                    {
                        var data = img.ImageData;
                        presenter.ImageHandler.ShowTableImages(new List<byte[]> { data });
                    }
                }

            }
        }
    }
}
