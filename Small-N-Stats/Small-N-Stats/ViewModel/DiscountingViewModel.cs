using RDotNet;
using Small_N_Stats.Graphics;
using Small_N_Stats.Interface;
using Small_N_Stats.Mathematics;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Input;
using System.Windows.Media;
using Small_N_Stats.View;
using Small_N_Stats.Model;

namespace Small_N_Stats.ViewModel
{
    class DiscountingViewModel : ViewModelBase
    {
        public MainWindow mWindow { get; set; }
        public DiscountingWindow windowRef { get; set; }

        internal OutputWindowInterface mInterface;

        private bool runAUC = false;
        public bool RunAUC
        {
            get { return runAUC; }
            set
            {
                runAUC = value;
                OnPropertyChanged("RunAUC");
            }
        }

        private bool runExp = false;
        public bool RunEXP
        {
            get { return runExp; }
            set
            {
                runExp = value;
                OnPropertyChanged("RunEXP");
            }
        }

        private bool runHyp = false;
        public bool RunHYP
        {
            get { return runHyp; }
            set
            {
                runHyp = value;
                OnPropertyChanged("RunHYP");
            }
        }

        private bool runQuasi = false;
        public bool RunQUASI
        {
            get { return runQuasi; }
            set
            {
                runQuasi = value;
                OnPropertyChanged("RunQUASI");
            }
        }

        private bool runMyerson = false;
        public bool RunMYERSON
        {
            get { return runMyerson; }
            set
            {
                runMyerson = value;
                OnPropertyChanged("RunMYERSON");
            }
        }

        private bool runRachlin = false;
        public bool RunRACHLIN
        {
            get { return runRachlin; }
            set
            {
                runRachlin = value;
                OnPropertyChanged("RunRACHLIN");
            }
        }

        private string delays = "";
        public string Delays
        {
            get { return delays; }
            set
            {
                delays = value;
                OnPropertyChanged("Delays");
            }
        }

        private string values = "";
        public string Values
        {
            get { return values; }
            set
            {
                values = value;
                OnPropertyChanged("Values");
            }
        }

        private string maxValue = "";
        public string MaxValue
        {
            get { return maxValue; }
            set
            {
                maxValue = value;
                OnPropertyChanged("MaxValue");
            }
        }

        private double maxValueA = 0;

        private Brush delaysBrush = Brushes.White;
        public Brush DelaysBrush
        {
            get { return delaysBrush; }
            set
            {
                delaysBrush = value;
                OnPropertyChanged("DelaysBrush");
            }
        }

        private Brush valuesBrush = Brushes.White;
        public Brush ValuesBrush
        {
            get { return valuesBrush; }
            set
            {
                valuesBrush = value;
                OnPropertyChanged("ValuesBrush");
            }
        }

        /* Math */

        static Modeling mDiscount;
        REngine engine;

        /* Commands */

        public RelayCommand GetDelaysRangeCommand { get; set; }
        public RelayCommand GetValuesRangeCommand { get; set; }
        public RelayCommand CalculateScoresCommand { get; set; }

        public DiscountingViewModel()
        {
            GetDelaysRangeCommand = new RelayCommand(param => GetDelaysRange(), param => true);
            GetValuesRangeCommand = new RelayCommand(param => GetValuesRange(), param => true);
            CalculateScoresCommand = new RelayCommand(param => CalculateScores(), param => true);

            mDiscount = new Modeling();

            /* Prepares R DLLs for fitting */

            REngine.SetEnvironmentVariables();
            engine = REngine.GetInstance();
            engine.Initialize();
        }

        /* Fields to be RED while picking ranges */

        private void DefaultFieldsToWhite()
        {
            DelaysBrush = Brushes.White;
            ValuesBrush = Brushes.White;
        }

        private void GetDelaysRange()
        {
            DefaultFieldsToWhite();

            DelaysBrush = Brushes.Red;
            Delays = "";

            mWindow.spreadSheetView.PickRange((inst, range) =>
            {
                if (range.Rows > 1 && range.Cols > 1)
                {
                    //baselineRange.Background = Brushes.White;
                    MessageBox.Show("Please select single row or single column selections");
                    return true;
                }

                DelaysBrush = Brushes.White;
                Delays = range.ToString();

                return true;
            }, Cursors.Cross);
        }

        private void GetValuesRange()
        {
            DefaultFieldsToWhite();

            ValuesBrush = Brushes.Red;
            Values = "";

            mWindow.spreadSheetView.PickRange((inst, range) =>
            {
                if (range.Rows > 1 && range.Cols > 1)
                {
                    //baselineRange.Background = Brushes.White;
                    MessageBox.Show("Please select single row or single column selections");
                    return true;
                }

                ValuesBrush = Brushes.White;
                Values = range.ToString();

                return true;
            }, Cursors.Cross);
        }

        private async void CalculateScores()
        {
            List<double> xRange = mWindow.ParseRange(Delays);
            List<double> yRange = mWindow.ParseRange(Values);

            if (!double.TryParse(MaxValue, out maxValueA) || (xRange.Count != yRange.Count))
            {
                mInterface.SendMessageToOutput("Error in computing ranges, inputs must be equal in length.");
                mInterface.SendMessageToOutput("Counts for Delays and Values were " + xRange.Count + " and " + yRange.Count + " respectively.");
                mInterface.SendMessageToOutput("Max value was: " + maxValueA);
                MessageBox.Show("Hmm, check your ranges.  These don't seem paired up");
                return;
            }

            mInterface.SendMessageToOutput("---------------------------------------------------");

            mDiscount.maxValue = maxValueA;
            mDiscount.xArray = xRange;
            mDiscount.yArray = yRange;

            mInterface.SendMessageToOutput("Results of Johnson & Bickel criteria: " + mDiscount.getJohnsonBickelCriteria());

            var chartWin = new ChartingWindow();
            Chart chart = chartWin.FindName("MyWinformChart") as Chart;
            chart.Series.Clear();
            // Style chart areas
            chart.ChartAreas[0].AxisX.MajorGrid.Enabled = false;
            chart.ChartAreas[0].AxisX.Minimum = 0;
            chart.ChartAreas[0].AxisX.Title = "Delays";
            chart.ChartAreas[0].AxisY.MajorGrid.Enabled = false;
            chart.ChartAreas[0].AxisY.Minimum = 0;
            chart.ChartAreas[0].AxisY.Title = "Values";

            int seriesCount = 0;

            if (runAUC)
            {
                mInterface.SendMessageToOutput("---------------------------------------------------");
                mInterface.SendMessageToOutput("Calculating Area Under Curve...");

                double AUC = await GetAreaUnderCurve();

                mInterface.SendMessageToOutput("Results of AUC were " + AUC.ToString("0.0000"));

                var series = new Series
                {
                    Name = "AUC",
                    Color = System.Drawing.Color.Blue,
                    IsVisibleInLegend = true,
                    IsXValueIndexed = false,
                    ChartType = SeriesChartType.Point
                };

                chart.Series.Add(series);

                for (int i = 0; i < xRange.Count; i++)
                {
                    chart.Series[seriesCount].Points.AddXY(xRange[i], yRange[i]);
                }

                chart.Legends.Add(series.Name);

                seriesCount++;

            }
            if (runExp)
            {
                mInterface.SendMessageToOutput("---------------------------------------------------");
                mInterface.SendMessageToOutput("Fitting Exponential Model...");

                double kVal = 0;

                try
                {
                    NumericVector delays = engine.CreateNumericVector(xRange.ToArray());
                    engine.SetSymbol("delays", delays);

                    NumericVector values = engine.CreateNumericVector(yRange.ToArray());
                    engine.SetSymbol("values", values);

                    GenericVector testResult = engine.Evaluate("nls(values ~ " + maxValueA + " * exp(-a*delays), start = list(a = 0))").AsList();

                    engine.SetSymbol("exponentialModel", testResult);
                    kVal = engine.Evaluate("coef(exponentialModel)").AsNumeric().First();

                }
                catch (EvaluationException evalExc)
                {
                    System.Console.WriteLine(evalExc.ToString());
                    mInterface.SendMessageToOutput("R methods failed to achieve convergence.  Defaulting to SnS backups...");

                    /*
                        Fall-back method if convergence issues found
                    */

                    ExponentialModel exponentialModel = await GetExponentialModel();
                    kVal = exponentialModel.Constant;
                }

                double exponentialED50 = mDiscount.getExponentialED50(kVal);

                double expR2 = mDiscount.getExponentialR2(kVal);

                mInterface.SendMessageToOutput("Results of Exponential fit were k: " + kVal.ToString("0.0000") + ", r2: " + expR2.ToString("0.0000"));
                mInterface.SendMessageToOutput("Results of exponential ED50: " + exponentialED50.ToString("0.0000"));

                var series = new Series
                {
                    Name = "Exponential Model",
                    Color = System.Drawing.Color.Purple,
                    IsVisibleInLegend = true,
                    IsXValueIndexed = false,
                    ChartType = SeriesChartType.Line
                };

                chart.Series.Add(series);

                double projected;

                for (int i = (int)xRange[0]; i < xRange[xRange.Count - 1]; i++)
                {
                    projected = maxValueA * System.Math.Exp(-kVal * i);

                    chart.Series[seriesCount].Points.AddXY(i, projected);
                }

                chart.Legends.Add(series.Name);

                seriesCount++;

            }
            if (runHyp)
            {
                mInterface.SendMessageToOutput("---------------------------------------------------");
                mInterface.SendMessageToOutput("Fitting hyperbolic Model...");

                double kVal = 0;

                try
                {
                    NumericVector delays = engine.CreateNumericVector(xRange.ToArray());
                    engine.SetSymbol("delays", delays);

                    NumericVector values = engine.CreateNumericVector(yRange.ToArray());
                    engine.SetSymbol("values", values);

                    GenericVector testResult = engine.Evaluate("nls(values ~ " + maxValueA + " / (1 + a*delays), start = list(a = 0))").AsList();
                    engine.SetSymbol("hyperbolicModel", testResult);
                    kVal = engine.Evaluate("coef(hyperbolicModel)").AsNumeric().First();

                }
                catch (EvaluationException evalExc)
                {
                    /*
                        Fall-back method if convergence issues found
                    */

                    System.Console.WriteLine(evalExc.ToString());
                    mInterface.SendMessageToOutput("R methods failed to achieve convergence.  Defaulting to SnS backups...");

                    HyperbolicModel hyperbolicModel = await GetHyperbolicModel();
                    kVal = hyperbolicModel.Constant;
                }

                double hyperbolicED50 = mDiscount.getAinslieED50(kVal);

                double hypR2 = mDiscount.getHyperbolicR2(kVal);

                mInterface.SendMessageToOutput("Results of hyperbolic fit were k: " + kVal.ToString("0.0000") + ", r2: " + hypR2.ToString("0.0000"));
                mInterface.SendMessageToOutput("Results of hyperbolic ED50: " + hyperbolicED50.ToString("0.0000"));

                var series = new Series
                {
                    Name = "Hyperbolic Model",
                    Color = System.Drawing.Color.LimeGreen,
                    IsVisibleInLegend = true,
                    IsXValueIndexed = false,
                    ChartType = SeriesChartType.Line
                };

                chart.Series.Add(series);

                double projected;

                for (int i = (int)xRange[0]; i < xRange[xRange.Count - 1]; i++)
                {
                    projected = maxValueA / (1 + kVal * i);
                    chart.Series[seriesCount].Points.AddXY(i, projected);
                }

                chart.Legends.Add(series.Name);

                seriesCount++;
            }
            if (runQuasi)
            {
                mInterface.SendMessageToOutput("---------------------------------------------------");
                mInterface.SendMessageToOutput("Fitting quasi-hyperbolic Model...");

                double[] kVal = new double[2];

                try
                {
                    NumericVector delays = engine.CreateNumericVector(xRange.ToArray());
                    engine.SetSymbol("delays", delays);

                    NumericVector values = engine.CreateNumericVector(yRange.ToArray());
                    engine.SetSymbol("values", values);

                    GenericVector testResult = engine.Evaluate("nls(values ~ " + maxValueA + " * a * exp(-b*delays), start = list(a = 0.5, b = 0.001))").AsList();

                    engine.SetSymbol("quasiModel", testResult);
                    kVal = engine.Evaluate("coef(quasiModel)").AsNumeric().ToArray();

                }
                catch (EvaluationException evalExc)
                {
                    System.Console.WriteLine(evalExc.ToString());

                    mInterface.SendMessageToOutput("R methods failed to achieve convergence.  Defaulting to SnS backups...");

                    /*
                    Backup methods
                    */

                    QuasiHyperbolicModel quasiHyperbolicModel = await GetQuasiHyperbolicModel();

                    kVal = new double[] { quasiHyperbolicModel.Beta, quasiHyperbolicModel.Constant };
                }

                double quasiED50 = mDiscount.getQuasiED50(kVal[0], kVal[1], maxValueA);

                double quasiR2 = mDiscount.getQuasiHyperbolicR2(kVal[0], kVal[1]);

                mInterface.SendMessageToOutput("Results of quasi-hyperbolic fit were beta: " + kVal[0].ToString("0.0000") + " k: " + kVal[1].ToString("0.0000") + ", r2: " + quasiR2.ToString("0.0000"));
                mInterface.SendMessageToOutput("Results of quasi-hyperbolic ED50: " + quasiED50.ToString("0.0000"));

                var series = new Series
                {
                    Name = "Quasi-Hyperbolic Model",
                    Color = System.Drawing.Color.Magenta,
                    IsVisibleInLegend = true,
                    IsXValueIndexed = false,
                    ChartType = SeriesChartType.Line
                };

                chart.Series.Add(series);

                double projected;

                for (int i = (int)xRange[0]; i < xRange[xRange.Count - 1]; i++)
                {
                    projected = maxValueA * kVal[0] * System.Math.Exp(-kVal[1] * i);
                    chart.Series[seriesCount].Points.AddXY(i, projected);
                }

                chart.Legends.Add(series.Name);

                seriesCount++;
            }
            if (runMyerson)
            {
                mInterface.SendMessageToOutput("---------------------------------------------------");
                mInterface.SendMessageToOutput("Fitting hyperboloid (Myerson's) Model...");

                double[] kVal = new double[2];

                try
                {
                    NumericVector delays = engine.CreateNumericVector(xRange.ToArray());
                    engine.SetSymbol("delays", delays);

                    NumericVector values = engine.CreateNumericVector(yRange.ToArray());
                    engine.SetSymbol("values", values);

                    GenericVector testResult = engine.Evaluate("nls(values ~ " + maxValueA + " / ((1 + a*delays)^b), start = list(a = 0.001, b = 0.2))").AsList();

                    engine.SetSymbol("myersonModel", testResult);
                    kVal = engine.Evaluate("coef(myersonModel)").AsNumeric().ToArray();
                }
                catch (EvaluationException evalExc)
                {
                    System.Console.WriteLine(evalExc.ToString());
                    mInterface.SendMessageToOutput("R methods failed to achieve convergence.  Defaulting to SnS backups...");

                    /*
                    Backup methods
                    */

                    HyperboloidMyerson hyperboloidModelMyerson = mDiscount.GetHyperboloidMyerson();
                    kVal = new double[] { hyperboloidModelMyerson.Constant, hyperboloidModelMyerson.Scaling };
                }

                double hyperboloidMyersonED50 = mDiscount.getMyersonED50(kVal[0], kVal[1]);

                double myersonR2 = mDiscount.getHyperboloidMyersonR2(kVal[0], kVal[1]);

                mInterface.SendMessageToOutput("Results of hyperboloid fit (Myerson) were k: " + kVal[0].ToString("0.0000") + " s: " + kVal[1].ToString("0.0000") + ", r2: " + myersonR2.ToString("0.0000"));
                mInterface.SendMessageToOutput("Results of hyperboloid (Myerson) ED50: " + hyperboloidMyersonED50.ToString("0.0000"));

                var series = new Series
                {
                    Name = "Hyperboloid (M) Model",
                    Color = System.Drawing.Color.DarkCyan,
                    IsVisibleInLegend = true,
                    IsXValueIndexed = false,
                    ChartType = SeriesChartType.Line
                };

                chart.Series.Add(series);

                double projected;

                for (int i = (int)xRange[0]; i < xRange[xRange.Count - 1]; i++)
                {
                    projected = mDiscount.LookupWithS(kVal[0], maxValueA, i, kVal[1]);
                    chart.Series[seriesCount].Points.AddXY(i, projected);
                }

                chart.Legends.Add(series.Name);

                seriesCount++;
            }
            if (runRachlin)
            {
                mInterface.SendMessageToOutput("---------------------------------------------------");
                mInterface.SendMessageToOutput("Fitting hyperboloid (Rachlin's) Model...");

                /*
                Nuke after verifying with R
                */

                double[] kVal = new double[2];

                try
                {
                    NumericVector delays = engine.CreateNumericVector(xRange.ToArray());
                    engine.SetSymbol("delays", delays);

                    NumericVector values = engine.CreateNumericVector(yRange.ToArray());
                    engine.SetSymbol("values", values);

                    GenericVector mod = engine.Evaluate("nls.control(maxiter = 1000)").AsList();

                    GenericVector testResult = engine.Evaluate("nls(values ~ " + maxValueA + " / (1 + (a*delays)^b), start = list(a = 0.001, b = 0.2))").AsList();

                    engine.SetSymbol("rachlinModel", testResult);

                    kVal = engine.Evaluate("coef(rachlinModel)").AsNumeric().ToArray();
                }
                catch (EvaluationException evalExc)
                {
                    System.Console.WriteLine(evalExc.ToString());
                    mInterface.SendMessageToOutput("R methods failed to achieve convergence.  Defaulting to SnS backups...");

                    /*
                    Backup methods
                    */

                    HyperboloidRachlin hyperboloidModelRachlin = await GetHyperboloidRachlin();

                    kVal = new double[] { hyperboloidModelRachlin.Constant, hyperboloidModelRachlin.Scaling };
                }

                double hyperboloidRachlinED50 = mDiscount.getRachlinED50(kVal[0], kVal[1], maxValueA);

                double rachlinR2 = mDiscount.getHyperboloidRachlinR2(kVal[0], kVal[1]);

                mInterface.SendMessageToOutput("Results of hyperboloid fit (Rachlin) were k: " + kVal[0].ToString("0.0000") + " s: " + kVal[1].ToString("0.0000") + ", r2: " + rachlinR2.ToString("0.0000"));
                mInterface.SendMessageToOutput("Results of hyperboloid (Rachlin) ED50: " + hyperboloidRachlinED50.ToString("0.0000"));

                var series = new Series
                {
                    Name = "Hyperboloid (R) Model",
                    Color = System.Drawing.Color.Crimson,
                    IsVisibleInLegend = true,
                    IsXValueIndexed = false,
                    ChartType = SeriesChartType.Line
                };

                chart.Series.Add(series);

                double projected;

                for (int i = (int)xRange[0]; i < xRange[xRange.Count - 1]; i++)
                {
                    projected = mDiscount.lookUpWithSInside(kVal[0], maxValueA, i, kVal[1]);
                    chart.Series[seriesCount].Points.AddXY(i, projected);
                }

                chart.Legends.Add(series.Name);

                seriesCount++;
            }

            chart.Legends[0].IsDockedInsideChartArea = true;
            chartWin.Owner = windowRef;
            chartWin.Show();
        }

        public static async Task<double> GetAreaUnderCurve()
        {
            return await Task.Factory.StartNew(() => mDiscount.GetAreaUnderCurve());
        }

        public static async Task<ExponentialModel> GetExponentialModel()
        {
            return await Task.Factory.StartNew(() => mDiscount.GetExponentialModel());
        }

        public static async Task<HyperbolicModel> GetHyperbolicModel()
        {
            return await Task.Factory.StartNew(() => mDiscount.GetHyperbolicModel());
        }

        public static async Task<QuasiHyperbolicModel> GetQuasiHyperbolicModel()
        {
            return await Task.Factory.StartNew(() => mDiscount.GetQuasiHyperbolicModel());
        }

        public static async Task<HyperboloidMyerson> GetHyperboloidMyerson()
        {
            return await Task.Factory.StartNew(() => mDiscount.GetHyperboloidMyerson());
        }

        public static async Task<HyperboloidRachlin> GetHyperboloidRachlin()
        {
            return await Task.Factory.StartNew(() => mDiscount.GetHyperboloidRachlin());
        }

    }
}
