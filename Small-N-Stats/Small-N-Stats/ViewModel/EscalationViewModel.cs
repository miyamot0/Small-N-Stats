using System;
using Small_N_Stats.View;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows;
using System.Collections.Generic;
using Small_N_Stats.Mathematics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;
using Small_N_Stats.Graphics;

namespace Small_N_Stats.ViewModel
{
    class EscalationViewModel : ViewModelBase
    {
        internal MainWindowViewModel mInterface;
        internal MainWindow mWindow;
        internal EscalationWindow windowRef;

        /* Color Binds*/
        private Brush backGround = Brushes.White;
        public Brush BackGround
        {
            get { return backGround; }
            set
            {
                backGround = value;
                OnPropertyChanged("BackGround");
            }
        }

        /* String binds */
        private string rangeString = "";
        public string RangeString
        {
            get { return rangeString; }
            set
            {
                rangeString = value;
                OnPropertyChanged("RangeString");
            }
        }

        /* Bool flags */
        private bool twoLag = false;
        public bool TwoLag
        {
            get { return twoLag; }
            set
            {
                twoLag = value;
                OnPropertyChanged("TwoLag");
            }
        }

        private bool threeLag = false;
        public bool ThreeLag
        {
            get { return threeLag; }
            set
            {
                threeLag = value;
                OnPropertyChanged("ThreeLag");
            }
        }

        private bool fourLag = false;
        public bool FourLag
        {
            get { return fourLag; }
            set
            {
                fourLag = value;
                OnPropertyChanged("FourLag");
            }
        }

        /* Local Logic */

        private bool boolUserSelected = false;
        private bool boolRandomized = false;

        private const int WinWidth = 500;
        private const int WinHeight = 350;

        /* Command Logic */
        public RelayCommand GenerateDataCommand { get; set; }
        public RelayCommand LoadDataCommand { get; set; }
        public RelayCommand CalculateCommand { get; set; }

        public EscalationViewModel()
        {
            GenerateDataCommand = new RelayCommand(param => GenerateData(), param => true);
            LoadDataCommand = new RelayCommand(param => LoadData(), param => true);
            CalculateCommand = new RelayCommand(param => Calculate(), param => true);
        }

        /* Fields to be RED while picking ranges */

        private void DefaultFieldsToWhite()
        {
            BackGround = Brushes.White;
        }

        private void LoadData()
        {
            DefaultFieldsToWhite();

            BackGround = Brushes.Red;

            mWindow.spreadSheetView.PickRange((inst, range) =>
            {
                if (range.Rows > 1 && range.Cols > 1)
                {
                    BackGround = Brushes.White;
                    MessageBox.Show("Please select single row or single column selections");
                    return true;
                }

                RangeString = range.ToString();
                BackGround = Brushes.White;
                boolUserSelected = true;
                boolRandomized = false;

                return true;

            }, Cursors.Cross);
        }

        private void GenerateData()
        {
            boolUserSelected = false;
            boolRandomized = true;

            RangeString = "Randomly Generated";
        }

        private async void Calculate()
        {
            if (RangeString.Length < 1)
            {
                mInterface.SendMessageToOutput("Error in determining range.");
                MessageBox.Show("Hmm, we need a range of 0/1's supplied or to load a generated sequence.");
                return;
            }
            else
            {
                mInterface.SendMessageToOutput("---------------------------------------------------");
                Escalation mBayesian;
                List<double> learnedSequence = new List<double>();
                int extLength = 0;

                if (RangeString.Length > 1 && boolUserSelected && (TwoLag || ThreeLag || FourLag))
                {
                    mBayesian = new Escalation();
                    List<int> integers = mWindow.ParseRange(RangeString).Select(i => (int)i).ToList();
                    mBayesian.SetCustomLearningSequence(integers);

                    mInterface.SendMessageToOutput("Using user-supplied data for bayesian agent.");

                }
                else if (boolRandomized && (TwoLag || ThreeLag || FourLag))
                {
                    mBayesian = new Escalation();
                    mBayesian.RandomizeLearningSequence();

                    mInterface.SendMessageToOutput("Using randomly-generated data for bayesian agent.");
                }
                else
                {
                    return;
                }

                extLength = 12;
                mBayesian.InjectLosingSequence(extLength);

                if (TwoLag)
                {
                    /*  Two Lag  */

                    List<double> twoLagged = await Task.Factory.StartNew(
                        () => mBayesian.RunTwoLag()
                    );

                    mInterface.SendMessageToOutput("Two-step Markov chains completed.");
                    mInterface.SendMessageToOutput("Charting Two-step Markov model.");

                    var chartWin = new ChartingWindow();
                    Chart chart = chartWin.FindName("MyWinformChart") as Chart;
                    chart.Series.Clear();

                    // Style chart areas
                    chart.ChartAreas[0].AxisX.MajorGrid.Enabled = false;
                    chart.ChartAreas[0].AxisX.Minimum = 0;
                    chart.ChartAreas[0].AxisX.Title = "Trial";
                    chart.ChartAreas[0].AxisY.MajorGrid.Enabled = false;
                    chart.ChartAreas[0].AxisY.Minimum = 0;
                    chart.ChartAreas[0].AxisY.Title = "Warranted Probability";

                    var series = new Series
                    {
                        Name = "Two-Lag Acquisition",
                        Color = System.Drawing.Color.Blue,
                        IsVisibleInLegend = true,
                        IsXValueIndexed = false,
                        ChartType = SeriesChartType.Line
                    };

                    chart.Series.Add(series);

                    for (int i = 0; i < twoLagged.Count - extLength - 1; i++)
                    {
                        chart.Series[0].Points.AddXY(i, twoLagged[i]);
                    }

                    chart.Legends.Add(series.Name);

                    var series2 = new Series
                    {
                        Name = "Two-Lag Extinction",
                        Color = System.Drawing.Color.Black,
                        IsVisibleInLegend = true,
                        IsXValueIndexed = false,
                        ChartType = SeriesChartType.Line
                    };

                    chart.Series.Add(series2);

                    for (int i = twoLagged.Count - extLength - 1; i < twoLagged.Count; i++)
                    {
                        chart.Series[1].Points.AddXY(i, twoLagged[i]);
                    }

                    chart.Legends.Add(series2.Name);

                    chart.Legends[0].IsDockedInsideChartArea = true;
                    chartWin.Title = "Two-Lagged Bayesian Escalation Model";
                    chartWin.Width = WinWidth;
                    chartWin.Height = WinHeight;
                    chartWin.Owner = windowRef;
                    chartWin.Show();

                    mInterface.SendMessageToOutput("Two-step Markov chains charted.");
                }

                if (ThreeLag)
                {
                    /*  THREE LAG  */

                    List<double> threeLagged = await Task.Factory.StartNew(
                        () => mBayesian.RunThreeLag()
                    );

                    mInterface.SendMessageToOutput("Three-step Markov chains completed.");
                    mInterface.SendMessageToOutput("Charting Three-step Markov model.");

                    var chartWin2 = new ChartingWindow();
                    Chart chart2 = chartWin2.FindName("MyWinformChart") as Chart;
                    chart2.Series.Clear();

                    // Style chart areas
                    chart2.ChartAreas[0].AxisX.MajorGrid.Enabled = false;
                    chart2.ChartAreas[0].AxisX.Minimum = 0;
                    chart2.ChartAreas[0].AxisX.Title = "Trial";
                    chart2.ChartAreas[0].AxisY.MajorGrid.Enabled = false;
                    chart2.ChartAreas[0].AxisY.Minimum = 0;
                    chart2.ChartAreas[0].AxisY.Title = "Warranted Probability";

                    var seriesTwo = new Series
                    {
                        Name = "Three-Lag Acquisition",
                        Color = System.Drawing.Color.Blue,
                        IsVisibleInLegend = true,
                        IsXValueIndexed = false,
                        ChartType = SeriesChartType.Line
                    };

                    chart2.Series.Add(seriesTwo);

                    for (int i = 0; i < threeLagged.Count - extLength - 1; i++)
                    {
                        chart2.Series[0].Points.AddXY(i, threeLagged[i]);
                    }

                    chart2.Legends.Add(seriesTwo.Name);

                    var seriesTwo2 = new Series
                    {
                        Name = "Three-Lag Extinction",
                        Color = System.Drawing.Color.Black,
                        IsVisibleInLegend = true,
                        IsXValueIndexed = false,
                        ChartType = SeriesChartType.Line
                    };

                    chart2.Series.Add(seriesTwo2);

                    for (int i = threeLagged.Count - extLength - 1; i < threeLagged.Count; i++)
                    {
                        chart2.Series[1].Points.AddXY(i, threeLagged[i]);
                    }

                    chart2.Legends.Add(seriesTwo2.Name);

                    chart2.Legends[0].IsDockedInsideChartArea = true;
                    chartWin2.Title = "Three-Lagged Bayesian Escalation Model";
                    chartWin2.Width = WinWidth;
                    chartWin2.Height = WinHeight;
                    chartWin2.Owner = windowRef;
                    chartWin2.Show();
                }

                if (FourLag)
                {
                    /*  Four LAG  */

                    List<double> fourLagged = await Task.Factory.StartNew(
                        () => mBayesian.RunFourLag()
                    );

                    mInterface.SendMessageToOutput("Four-step Markov chains completed.");
                    mInterface.SendMessageToOutput("Charting Four-step Markov model.");

                    var chartWin3 = new ChartingWindow();
                    Chart chart3 = chartWin3.FindName("MyWinformChart") as Chart;
                    chart3.Series.Clear();

                    // Style chart areas
                    chart3.ChartAreas[0].AxisX.MajorGrid.Enabled = false;
                    chart3.ChartAreas[0].AxisX.Minimum = 0;
                    chart3.ChartAreas[0].AxisX.Title = "Trial";
                    chart3.ChartAreas[0].AxisY.MajorGrid.Enabled = false;
                    chart3.ChartAreas[0].AxisY.Minimum = 0;
                    chart3.ChartAreas[0].AxisY.Title = "Warranted Probability";

                    var seriesThree = new Series
                    {
                        Name = "Four-Lag Acquisition",
                        Color = System.Drawing.Color.Blue,
                        IsVisibleInLegend = true,
                        IsXValueIndexed = false,
                        ChartType = SeriesChartType.Line
                    };

                    chart3.Series.Add(seriesThree);

                    for (int i = 0; i < fourLagged.Count - extLength - 1; i++)
                    {
                        chart3.Series[0].Points.AddXY(i, fourLagged[i]);
                    }

                    chart3.Legends.Add(seriesThree.Name);

                    var seriesThree2 = new Series
                    {
                        Name = "Four-Lag Extinction",
                        Color = System.Drawing.Color.Black,
                        IsVisibleInLegend = true,
                        IsXValueIndexed = false,
                        ChartType = SeriesChartType.Line
                    };

                    chart3.Series.Add(seriesThree2);

                    for (int i = fourLagged.Count - extLength - 1; i < fourLagged.Count; i++)
                    {
                        chart3.Series[1].Points.AddXY(i, fourLagged[i]);
                    }

                    chart3.Legends.Add(seriesThree2.Name);

                    chart3.Legends[0].IsDockedInsideChartArea = true;
                    chartWin3.Title = "Four-Lagged Bayesian Escalation Model";
                    chartWin3.Width = WinWidth;
                    chartWin3.Height = WinHeight;
                    chartWin3.Owner = windowRef;
                    chartWin3.Show();
                }
            }
        }
    }
}
