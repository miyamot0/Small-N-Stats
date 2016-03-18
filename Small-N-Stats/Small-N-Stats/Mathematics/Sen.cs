using Small_N_Stats.Model;
using System.Collections.Generic;

namespace Small_N_Stats.Mathematics
{
    class Sen
    {
        public List<double> BaselineObservations { get; set; }
        public List<double> InterventionObservations { get; set; }

        public Sen()
        {
            if (BaselineObservations == null)
                BaselineObservations = new List<double>();

            if (InterventionObservations == null)
                InterventionObservations = new List<double>();
        }

        public SenSlope ComputeTheilSen(List<double> data)
        {
            List<double> data1 = data;
            List<double> data2 = data;

            var N = data.Count;
            //double[] ms = new double[N * (N - 1) / 2];
            List<double> ms = new List<double>();
            var pairs = 0;

            double dx_sum = 0;
            double dy_sum = 0;
            var p = 0;
            var n = 0;
            var t = 0;

            for (int i = 0; i < N - 1; i++)
            {
                for (int j = i + 1; j < N; j++)
                {
                    var dy = (data[j] - data[i]);
                    var dx = j - i;
                    //ms[pairs] = dy / dx;
                    ms.Add(dy / dx);
                    pairs++;
                    dx_sum += dx;
                    dy_sum += dy;

                    if (dy > 0)
                        p++;
                    else if (dy < 0)
                        n++;
                    else
                        t++;
                }
            }

            ms.Sort();

            double TS = med(ms);
            double TS1 = TS;

            double OLS = dy_sum / dx_sum;
            double S = (p - n);
            double POS = p;
            double TAU = S / (pairs * 1.0);
            double SDTau = System.Math.Sqrt((4 * N + 10) * 1.0 / (9 * N * N - 9 * N));
            double Z = (TAU / SDTau);
            double PVal = get2TailedPValue(System.Math.Abs(Z));

            double SDts = (SDTau / TAU) * TS;
            double VARts = SDts * SDts;
            double PAIRS = pairs;

            return new SenSlope
            {
                Observations = data,
                Length = data.Count,
                Sen = TS,
                SenSD = (SDTau / TAU) * TS,
                SenVariance = SDts * SDts,
                LeastSquares = OLS,
                Zscore = Z,
                Tau = TAU,
                TauSD = SDTau,
                P = PVal,
                TS85 = new double[] { TS - 1.44 * SDts, TS + 1.44 * SDts },
                TS90 = new double[] { TS - 1.645 * SDts, TS + 1.645 * SDts },
                TS95 = new double[] { TS - 1.96 * SDts, TS + 1.96 * SDts }
            };
        }

        public SenSlope CompareTheilSen(SenSlope bl, SenSlope tx)
        {
            SenSlope copy = bl;
            copy.SenSD = System.Math.Sqrt(bl.SenVariance + tx.SenVariance);
            copy.Sen = bl.Sen - tx.Sen;
            copy.Zscore = bl.Sen / System.Math.Sqrt(bl.SenVariance + tx.SenVariance);
            copy.TS85 = bl.TS85;
            copy.TS90 = bl.TS90;
            copy.TS95 = bl.TS95;


            return copy;
        }

        public SenSlope GetInterceptBaseline(SenSlope current)
        {
            List<double> mx = new List<double>();
            for (int i = 0; i < current.Length; i++)
            {
                mx.Add(i + 1);
            }

            List<double> my = new List<double>(current.Observations);

            mx.Sort();
            my.Sort();

            current.MedianX = med(mx);
            current.MedianY = med(my);
            current.ConoverIntercept = current.MedianY - current.Sen * current.MedianX;

            return current;
        }

        public SenSlope GetInterceptTreatment(SenSlope current, int prevLength)
        {
            List<double> mx = new List<double>();
            for (int i = prevLength; i < current.Length + prevLength; i++)
            {
                mx.Add(i + 1);
            }

            List<double> my = new List<double>(current.Observations);

            mx.Sort();
            my.Sort();

            current.MedianX = med(mx);
            current.MedianY = med(my);
            current.ConoverIntercept = current.MedianY - current.Sen * current.MedianX;

            return current;
        }

        private double med(List<double> data)
        {
            List<double> copy = new List<double>(data);

            int mid = (int)System.Math.Floor(copy.Count / 2.0);
            return (copy[mid] + copy[mid - (1 - copy.Count % 2)]) / 2.0;
        }

        public double get2TailedPValue(double z)
        {
            z = System.Math.Abs(z);
            var p2 = (((((.000005383 * z + .0000488906) * z + .0000380036) * z + .0032776263) * z + .0211410061) * z + .049867347) * z + 1;
            p2 = System.Math.Pow(p2, -16);
            return p2;
        }
    }
}
