using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

        public List<Vector2> Cities { get; set; } = new();

        private Solver? tspSolver;
        private readonly System.Timers.Timer canvasUpdateTimer;
        private Thread? solverThread;

        private CancellationTokenSource? cancellationTokenSource;

        private int permutationCount = 0;

        private Stopwatch stopwatch = Stopwatch.StartNew();

        public MainWindow()
        {
            canvasUpdateTimer = new System.Timers.Timer();
            canvasUpdateTimer.Elapsed += CanvasUpdateTimer_Elapsed;

            InitializeComponent();

            canvasUpdateTimer.Start();
        }

        private void CanvasUpdateTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            stopwatch.Stop();
            Dispatcher.Invoke(() =>
            {
                canvasUpdateTimer.Interval = frameDelaySlider.Value;
                UpdateCanvas();
                UpdateStats();
            });
            stopwatch = Stopwatch.StartNew();
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
                for (int i = 0; i < tspSolver.CurrentBestPath.Count - 1; i++)
                {
                    Vector2 city = tspSolver.Cities[tspSolver.CurrentBestPath[i]];
                    // Do not run if we are at the last city
                    Vector2 nextCity = tspSolver.Cities[tspSolver.CurrentBestPath[i + 1]];
                    _ = cityCanvas.Children.Add(new Line()
                    {
                        StrokeThickness = 4,
                        X1 = city.X,
                        Y1 = city.Y,
                        X2 = nextCity.X,
                        Y2 = nextCity.Y,
                        Stroke = solverThread.IsAlive ? Brushes.Red : Brushes.Green
                    });
                }

                if (solverThread.IsAlive)
                {
                    for (int i = 0; i < tspSolver.LastTriedPath.Count - 1; i++)
                    {
                        Vector2 city = tspSolver.Cities[tspSolver.LastTriedPath[i]];
                        // Do not run if we are at the last city
                        Vector2 nextCity = tspSolver.Cities[tspSolver.LastTriedPath[i + 1]];
                        _ = cityCanvas.Children.Add(new Line()
                        {
                            StrokeThickness = 2,
                            X1 = city.X,
                            Y1 = city.Y,
                            X2 = nextCity.X,
                            Y2 = nextCity.Y,
                            Stroke = new SolidColorBrush(Colors.Black)
                        });
                    }
                }
            }
        }

        public void UpdateStats()
        {
            int triedPaths = tspSolver?.TriedPaths ?? 0;
            double averagePathsPerSecond = triedPaths * 1000d / tspSolver?.IterationStopwatch.ElapsedMilliseconds ?? 0;

            statsLabel.Text = $"Tried Paths: {triedPaths}/{permutationCount} ({(float)triedPaths / permutationCount * 100:0.0}%)" +
                $"\nImprovements: {tspSolver?.Improvements ?? 0}" +
                $"\nDisplay Frame Delay: {stopwatch.ElapsedMilliseconds}ms ({1000d / stopwatch.ElapsedMilliseconds:0.0} FPS)" +
                $"\nAverage Paths Per Second: {averagePathsPerSecond:0.00}" +
                $"\nEstimated Remaining Seconds: {(permutationCount - triedPaths) / averagePathsPerSecond:0.0}";
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

        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            cancellationTokenSource?.Cancel();
        }

        private void cityCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (solverThread is not null && solverThread.IsAlive)
            {
                return;
            }

            Point pos = Mouse.GetPosition(cityCanvas);
            Cities.Add(new Vector2((float)pos.X, (float)pos.Y));
            permutationCount = Permutations.PermutationsCount(Cities.Count);
        }

        private void clearButton_Click(object sender, RoutedEventArgs e)
        {
            cancellationTokenSource?.Cancel();
            tspSolver = null;
            solverThread = null;
            Cities.Clear();
            permutationCount = 0;
        }
    }
}
