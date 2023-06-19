using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace TravellingSalesmanProblem
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public float CityDiameter { get; set; } = 10;
        public double FrameDelay { get; set; } = 16.67;

        public List<Vector2> Cities { get; set; } = new();

        private Solver? tspSolver;
        private readonly System.Timers.Timer canvasUpdateTimer;
        private Thread? solverThread;

        private CancellationTokenSource? cancellationTokenSource;

        public MainWindow()
        {
            Random rng = new();
            Cities.Clear();
            for (int i = 0; i < 11; i++)
            {
                Cities.Add(new Vector2(rng.Next(0, 400), rng.Next(0, 400)));
            }

            canvasUpdateTimer = new System.Timers.Timer(FrameDelay);
            canvasUpdateTimer.Elapsed += CanvasUpdateTimer_Elapsed;

            InitializeComponent();
            canvasUpdateTimer.Start();
        }

        private void CanvasUpdateTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(UpdateCanvas);
        }

        public void UpdateCanvas()
        {
            cityCanvas.Children.Clear();

            foreach (Vector2 city in Cities)
            {
                Ellipse cityEllipse = new()
                {
                    Width = CityDiameter,
                    Height = CityDiameter,
                    Fill = Brushes.Black
                };
                _ = cityCanvas.Children.Add(cityEllipse);
                Canvas.SetLeft(cityEllipse, city.X - (CityDiameter / 2));
                Canvas.SetTop(cityEllipse, city.Y - (CityDiameter / 2));
            }

            if (tspSolver is not null && solverThread is not null)
            {
                List<int>? pathToDraw = solverThread.IsAlive ? tspSolver.LastTriedPath : tspSolver.CurrentBestPath;

                for (int i = 0; i < pathToDraw.Count - 1; i++)
                {
                    Vector2 city = tspSolver.Cities[pathToDraw[i]];
                    // Do not run if we are at the last city
                    Vector2 nextCity = tspSolver.Cities[pathToDraw[i + 1]];
                    _ = cityCanvas.Children.Add(new Line()
                    {
                        StrokeThickness = 2,
                        X1 = city.X,
                        Y1 = city.Y,
                        X2 = nextCity.X,
                        Y2 = nextCity.Y,
                        Stroke = Brushes.Black
                    });
                }
            }
        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            tspSolver = new(Cities);

            cancellationTokenSource?.Cancel();
            // Run TSP solver in background thread, with UI polling occasionally
            cancellationTokenSource = new CancellationTokenSource();
            solverThread = new(() => tspSolver.CalculateBestPath(cancellationTokenSource.Token));
            solverThread.Start();
        }
    }
}
