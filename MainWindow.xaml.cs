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

        private readonly Solver tspSolver;
        private readonly System.Timers.Timer canvasUpdateTimer;
        private readonly Thread solverThread;

        public MainWindow()
        {
            Random rng = new();
            List<Vector2> randomCities = new(20);
            for (int i = 0; i < 20; i++)
            {
                randomCities.Add(new Vector2(rng.Next(0, 400), rng.Next(0, 400)));
            }

            tspSolver = new(randomCities);
            canvasUpdateTimer = new System.Timers.Timer(66);
            canvasUpdateTimer.Elapsed += CanvasUpdateTimer_Elapsed;

            InitializeComponent();
            canvasUpdateTimer.Start();

            // Run TSP solver in background thread, with UI polling occasionally
            solverThread = new(() => tspSolver.CalculateBestPath());
            solverThread.Start();
        }

        private void CanvasUpdateTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(UpdateCanvas);
        }

        public void UpdateCanvas()
        {
            cityCanvas.Children.Clear();

            if (!solverThread.IsAlive)
            {
                System.Diagnostics.Debug.WriteLine("Done!");
            }

            for (int i = 0; i < tspSolver.LastTriedPath.Count - 1; i++)
            {
                Vector2 city = tspSolver.Cities[tspSolver.LastTriedPath[i]];

                Ellipse cityEllipse = new()
                {
                    Width = CityDiameter,
                    Height = CityDiameter,
                    Fill = Brushes.Black
                };
                _ = cityCanvas.Children.Add(cityEllipse);
                Canvas.SetLeft(cityEllipse, city.X - (CityDiameter / 2));
                Canvas.SetTop(cityEllipse, city.Y - (CityDiameter / 2));

                Vector2 nextCity = tspSolver.Cities[tspSolver.LastTriedPath[i + 1]];
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
}
