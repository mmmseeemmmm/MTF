using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MTFApp.SequenceExecution.ImageHandling
{
    /// <summary>
    /// Interaction logic for ImageControl.xaml
    /// </summary>
    public partial class ImageControl : UserControl
    {
        private bool sliding;
        private DateTime openningTime;

        public ImageControl()
        {
            InitializeComponent();
            Root.DataContext = this;
        }

        public bool IsPreview { get; set; }

        public ImagePresenter ImagePresenter
        {
            get => (ImagePresenter)GetValue(ImagePresenterProperty);
            set => SetValue(ImagePresenterProperty, value);
        }

        public static readonly DependencyProperty ImagePresenterProperty =
            DependencyProperty.Register("ImagePresenter", typeof(ImagePresenter), typeof(ImageControl), new FrameworkPropertyMetadata());


        public bool UseSlider
        {
            get => (bool)GetValue(UseSliderProperty);
            set => SetValue(UseSliderProperty, value);
        }

        public static readonly DependencyProperty UseSliderProperty =
            DependencyProperty.Register("UseSlider", typeof(bool), typeof(ImageControl), new PropertyMetadata(false));


        private void ImagePreviewOnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (ImagePresenter != null)
            {
                if (IsPreview)
                {
                    ImagePresenter.OpenCommand.Execute(null);
                }
                else
                {
                    ImagePresenter.CloseCommand.Execute(null);
                }
            }
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == IsVisibleProperty && (bool)e.NewValue)
            {
                openningTime = DateTime.Now;
            }

            base.OnPropertyChanged(e);
        }


        private void ImagePreviewOnTouchEnter(object sender, TouchEventArgs e)
        {
            sliding = true;
        }

        private void ImagePreviewOnTouchLeave(object sender, TouchEventArgs e)
        {
            sliding = false;
        }

        private void ImagePreviewOnTouchMove(object sender, TouchEventArgs e)
        {
            var now = DateTime.Now;
            var diff = (now - openningTime).TotalMilliseconds;
            if (diff < 200)
            {
                return;
            }
            
            if (sliding && !IsPreview)
            {
                var points = e.GetIntermediateTouchPoints(this);
                if (points.Count > 2)
                {
                    var startPoint = points[0];
                    var endPoint = points[points.Count - 1];
                    var x = endPoint.Position.X - startPoint.Position.X;
                    if (Math.Abs(x) > 4)
                    {
                        if (DataContext is SequenceExecutionPresenter)
                        {
                            ImagePresenter.ChangeImage(x < 0);
                            sliding = false;
                        }
                    }
                    else
                    {
                        ImagePresenter.CloseCommand.Execute(null);
                    }
                }
                else
                {
                    ImagePresenter.CloseCommand.Execute(null);
                }
            }
        }
    }
}
