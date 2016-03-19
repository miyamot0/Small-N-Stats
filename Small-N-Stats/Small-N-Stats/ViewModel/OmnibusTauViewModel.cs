using Small_N_Stats.Interface;
using Small_N_Stats.Mathematics;
using Small_N_Stats.Model;
using Small_N_Stats.View;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Small_N_Stats.ViewModel
{
    class OmnibusTauViewModel : ViewModelBase
    {
        public MainWindow mWindow { get; set; }
        public OmnibusTauWindow windowRef { get; set; }

        internal OutputWindowInterface mInterface;

        /* Brushes */

        private Brush baselineBackGround = Brushes.White;
        public Brush BaselineBackGround
        {
            get { return baselineBackGround; }
            set
            {
                baselineBackGround = value;
                OnPropertyChanged("BaselineBackGround");
            }
        }

        private Brush interventionBackGround = Brushes.White;
        public Brush InterventionBackGround
        {
            get { return interventionBackGround; }
            set
            {
                interventionBackGround = value;
                OnPropertyChanged("InterventionBackGround");
            }
        }

        private ObservableCollection<TAUU> tauUHolder;
        public ObservableCollection<TAUU> TauUHolder
        {
            get { return tauUHolder; }
            set
            {
                tauUHolder = value;
                OnPropertyChanged("TauUHolder");
            }
        }

        /* String binds */

        private string baselineRangeString = "";
        public string BaselineRangeString
        {
            get { return baselineRangeString; }
            set
            {
                baselineRangeString = value;
                OnPropertyChanged("BaselineRangeString");
            }
        }

        private string interventionRangeString = "";
        public string InterventionRangeString
        {
            get { return interventionRangeString; }
            set
            {
                interventionRangeString = value;
                OnPropertyChanged("InterventionRangeString");
            }
        }

        /* Bool flags */

        private bool correctBaseline = false;
        public bool CorrectBaseline
        {
            get { return correctBaseline; }
            set
            {
                correctBaseline = value;
                OnPropertyChanged("CorrectBaseline");
            }
        }

        /* Command Logic */

        //public RelayCommand RemoveItemCommand { get; set; }

        public RelayCommand ViewLoadedCommand { get; set; }
        public RelayCommand LoadBaselineDataCommand { get; set; }
        public RelayCommand LoadInterventionDataCommand { get; set; }
        public RelayCommand CheckBaselineCommand { get; set; }
        public RelayCommand CompareBaselineCommand { get; set; }
        public RelayCommand CalculateOmnibusCommand { get; set; }

        private ICommand _removeItemCommand;
        public ICommand RemoveItemCommand
        {
            get { return _removeItemCommand ?? (_removeItemCommand = new RelayCommand(p => RemoveItem((TAUU)p))); }
        }

        /* Math */

        TauU mTauMethods;

        public OmnibusTauViewModel()
        {
            //RemoveItemCommand = new RelayCommand(param => RemoveItem(this.RemoveItemCommand), param => true);

            ViewLoadedCommand = new RelayCommand(param => ViewLoaded(), param => true);
            LoadBaselineDataCommand = new RelayCommand(param => LoadBaselineData(), param => true);
            LoadInterventionDataCommand = new RelayCommand(param => LoadInterventionData(), param => true);

            CheckBaselineCommand = new RelayCommand(param => CheckBaseline(), param => true);
            CompareBaselineCommand = new RelayCommand(param => CompareBaseline(), param => true);
            CalculateOmnibusCommand = new RelayCommand(param => CalculateOmnibus(), param => true);
        }

        private void ViewLoaded()
        {
            mInterface.SendMessageToOutput("---------------------------------------------------");
            mInterface.SendMessageToOutput("Loading Tau-U modules...");

            mTauMethods = new TauU();

            TauUHolder = new ObservableCollection<TAUU>();

            mInterface.SendMessageToOutput("Tau-U modules loaded.");
        }

        private void RemoveItem(TAUU item)
        {
            if (item != null)
                TauUHolder.Remove(item);

        }

        private void LoadBaselineData()
        {
            DefaultFieldsToWhite();

            BaselineBackGround = Brushes.Red;

            mWindow.spreadSheetView.PickRange((inst, range) =>
            {
                if (range.Rows > 1 && range.Cols > 1)
                {
                    BaselineBackGround = Brushes.White;
                    MessageBox.Show("Please select single row or single column selections");
                    return true;
                }

                BaselineRangeString = range.ToString();
                BaselineBackGround = Brushes.White;

                return true;

            }, Cursors.Cross);
        }

        private void LoadInterventionData()
        {
            DefaultFieldsToWhite();

            InterventionBackGround = Brushes.Red;

            mWindow.spreadSheetView.PickRange((inst, range) =>
            {
                if (range.Rows > 1 && range.Cols > 1)
                {
                    InterventionBackGround = Brushes.White;
                    MessageBox.Show("Please select single row or single column selections");
                    return true;
                }

                InterventionRangeString = range.ToString();
                InterventionBackGround = Brushes.White;
                return true;

            }, Cursors.Cross);
        }

        private void CheckBaseline()
        {
            List<double> phase1 = mWindow.ParseRange(baselineRangeString);
            TAUU mTau = mTauMethods.BaselineTrend(phase1, phase1, false);
            mTau.Name = "Baseline: " + baselineRangeString;
            mTau.Range = baselineRangeString;
            mTau.IsChecked = false;

            mInterface.SendMessageToOutput("---------------------------------------------------");
            mInterface.SendMessageToOutput("Scoring baseline range { " + baselineRangeString + " }");
            mInterface.SendMessageToOutput("Baseline Tau score was " + mTau.TAU);
        }

        private void CompareBaseline()
        {
            List<double> phase1 = mWindow.ParseRange(BaselineRangeString);
            List<double> phase2 = mWindow.ParseRange(InterventionRangeString);

            mInterface.SendMessageToOutput("---------------------------------------------------");

            if (phase1.Count < 3 || phase2.Count < 3)
            {
                mInterface.SendMessageToOutput("Arrays appear short ( less than 3 items in 1 or more arrays ).");
                MessageBox.Show("Too few items were found.  Please review the selected arrays.");
                return;
            }

            TAUU mTau = mTauMethods.BaselineTrend(phase1, phase2, CorrectBaseline);

            mTau.Name = "Comparisons of {" + BaselineRangeString + "} and {" + InterventionRangeString + "} Corrected: " + CorrectBaseline;
            mTau.IsCorrected = CorrectBaseline;
            mTau.Range = BaselineRangeString;
            mTau.IsChecked = false;

            mInterface.SendMessageToOutput("Comparisons of {" + BaselineRangeString + "} and {" + InterventionRangeString + "} Corrected: " + CorrectBaseline);

            string mOut = string.Format("S: {0}, Pairs: {1}, Tau: {2}, TauB: {3}, P: {4}",
                mTau.S,
                mTau.Pairs,
                mTau.TAU,
                mTau.TAUB,
                mTau.PValue);

            mInterface.SendMessageToOutput(mOut);

            mOut = string.Format("@ 85% CI ({0} - {1})",
                mTau.CI_85[0],
                mTau.CI_85[1]);

            mInterface.SendMessageToOutput(mOut);

            mOut = string.Format("@ 90% CI ({0} - {1})",
                mTau.CI_90[0],
                mTau.CI_90[1]);

            mInterface.SendMessageToOutput(mOut);

            mOut = string.Format("@ 95% CI ({0} - {1})",
                mTau.CI_95[0],
                mTau.CI_95[1]);

            mInterface.SendMessageToOutput(mOut);

            TauUHolder.Add(mTau);

            CorrectBaseline = false;

            BaselineRangeString = "";
            InterventionRangeString = "";
        }

        private void CalculateOmnibus()
        {
            ObservableCollection<TAUU> tempHolder = new ObservableCollection<TAUU>(TauUHolder);

            if (tempHolder.Count < 2)
            {
                mInterface.SendMessageToOutput("At least two Tau-U objects must be selected to construct an omnibus");
                MessageBox.Show("Please select at least two Tau-U objects in the list view (i.e., CTRL+click)");
                return;
            }

            var globalTau = 0.0;
            var globalSETau = 0.0;
            var wsum = 0.0;

            for (var i = 0; i < tempHolder.Count; i++)
            {
                var tau = tempHolder[i].TAU;
                var sdtau = tempHolder[i].SDtau;

                var w = 1.0 / sdtau;

                wsum += w;
                globalTau += tau * w;
                globalSETau += sdtau * sdtau;

            }

            globalTau = globalTau / wsum;
            globalSETau = System.Math.Sqrt(globalSETau) / tempHolder.Count;

            var z = globalTau / globalSETau;

            var pval = mTauMethods.get2TailedPValue(z);

            double tot_w = 0.0;
            double tot_tau = 0.0;
            double pairs = 0;
            double ties = 0;
            double S = 0;

            for (var i = 0; i < tempHolder.Count; i++)
            {

                TAUU res = tempHolder[i];
                var w = 1.0 / (res.SDtau * res.SDtau);

                tot_w += w;

                tot_tau += res.TAU * w;

                pairs += res.Pairs;
                ties += res.Ties;
                S += res.S;

            }

            TAUU omniTau = new TAUU
            {
                S = S,
                Pairs = pairs,
                TAU = tot_tau / tot_w,
                TAUB = (S / (pairs * 1.0 - ties * 0.5)),
                VARs = (1.0 / tot_w),
                SDtau = System.Math.Sqrt(1.0 / tot_w),
                Z = z,
                PValue = pval,
                CI_85 = new double[] { ((tot_tau / tot_w) - 1.44 * globalSETau), ((tot_tau / tot_w) + 1.44 * globalSETau) },
                CI_90 = new double[] { ((tot_tau / tot_w) - 1.645 * globalSETau), ((tot_tau / tot_w) + 1.645 * globalSETau) },
                CI_95 = new double[] { ((tot_tau / tot_w) - 1.96 * globalSETau), ((tot_tau / tot_w) + 1.96 * globalSETau) },
                IsChecked = false,
                IsCorrected = false
            };

            mInterface.SendMessageToOutput("---------------------------------------------------");
            mInterface.SendMessageToOutput("Omnibus comparison created: ");

            string mOut = string.Format("Omnibus ES --- S: {0}, Pairs: {1}, Tau: {2}, TauB: {3}, P: {4}",
                omniTau.S,
                omniTau.Pairs,
                omniTau.TAU,
                omniTau.TAUB,
                omniTau.PValue);

            mInterface.SendMessageToOutput(mOut);

            mOut = string.Format("@ 85% CI ({0} - {1})",
                omniTau.CI_85[0],
                omniTau.CI_85[1]);

            mInterface.SendMessageToOutput(mOut);

            mOut = string.Format("@ 90% CI ({0} - {1})",
                omniTau.CI_90[0],
                omniTau.CI_90[1]);

            mInterface.SendMessageToOutput(mOut);

            mOut = string.Format("@ 95% CI ({0} - {1})",
                omniTau.CI_95[0],
                omniTau.CI_95[1]);

            mInterface.SendMessageToOutput(mOut);

            CorrectBaseline = false;

            BaselineRangeString = "";
            InterventionRangeString = "";
        }

        /* For clarify sake, so no multiple red fields */

        private void DefaultFieldsToWhite()
        {
            BaselineBackGround = Brushes.White;
            InterventionBackGround = Brushes.White;
        }

    }
}
