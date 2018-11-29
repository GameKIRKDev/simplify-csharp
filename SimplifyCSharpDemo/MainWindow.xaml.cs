using SimplifyCSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SimplifyCSharpDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private UInt32 _numInputPoints;
        public UInt32 NumInputPoints
        {
            get => _numInputPoints;
            set
            {
                _numInputPoints = value;
                NotifyPropertyChanged(nameof(NumInputPoints));
            }
        }

        private UInt32 _numOutputPoints = 0;
        public UInt32 NumOutputPoints
        {
            get => _numOutputPoints;
            set
            {
                _numOutputPoints = value;
                NotifyPropertyChanged(nameof(NumOutputPoints));
            }
        }

        private Double _algorithmRuntime = -1;
        public Double AlgorithmRuntime
        {
            get => _algorithmRuntime;
            set
            {
                _algorithmRuntime = value;
                NotifyPropertyChanged(nameof(AlgorithmRuntime));
            } 
        }
        public Boolean HighQualityEnabled { get; set; }
        public String ToleranceText { get; set; } = "0.5";

        public Polyline CurrentPolyline { get; } = new Polyline();
        public Polyline SimplifiedPolyline { get; } = new Polyline();

        public MainWindow()
        {
            CurrentPolyline.Stroke = new SolidColorBrush(Colors.Red);
            CurrentPolyline.StrokeThickness = 2.0;
            SimplifiedPolyline.Stroke = new SolidColorBrush(Colors.Blue);
            SimplifiedPolyline.StrokeThickness = 2.0;

            InitializeComponent();

            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = this;
            Canvas_UserInput.Children.Add(CurrentPolyline);
            Canvas_SimplifiedOutput.Children.Add(SimplifiedPolyline);
        }

        private void Canvas_UserInput_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point currentPoint = e.GetPosition(Canvas_UserInput);
                CurrentPolyline.Points.Add(currentPoint);

                NumInputPoints += 1;
            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            CurrentPolyline.Points.Clear();
            SimplifiedPolyline.Points.Clear();

            NumInputPoints = 0;
            NumOutputPoints = 0;
            AlgorithmRuntime = -1;
        }

        private void Simplify_Click(object sender, RoutedEventArgs e)
        {
            if (!Double.TryParse(ToleranceText, out var tolerence))
            {
                MessageBox.Show("Invalid tolerance value provided");
                return;
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var simplifiedPoints = SimplificationHelpers.Simplify<Point>(
                    CurrentPolyline.Points,
                    (p1, p2) => p1 == p2,
                    (p) => p.X,
                    (p) => p.Y,
                    tolerence,
                    HighQualityEnabled
                    );
            stopwatch.Stop();
            AlgorithmRuntime = Math.Round(stopwatch.Elapsed.TotalMilliseconds, 3);
            SimplifiedPolyline.Points.Clear();
            foreach (var point in simplifiedPoints)
            {
                SimplifiedPolyline.Points.Add(point);
            }

            NumOutputPoints = (UInt32)SimplifiedPolyline.Points.Count;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(String name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Double.TryParse((sender as TextBox).Text, out var res);
        }
    }
}
