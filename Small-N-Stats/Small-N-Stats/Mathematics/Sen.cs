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

        public SenSlopeModel ComputeTheilSen(List<double> data)
        {
            List<double> data1 = new List<double>(data);

            List<double> senSlopes = new List<double>();

            var N = data.Count;
            var senPairs = 0;

            double xDeltaTotal = 0.0, yDeltaTotal = 0.0;
            int p = 0, n = 0, t = 0;

            for (int i = 0; i < N - 1; i++)
            {
                for (int j = i + 1; j < N; j++)
                {
                    var yDelta = (data[j] - data[i]);
                    var xDelta = j - i;

                    senSlopes.Add(yDelta / xDelta);

                    xDeltaTotal += xDelta;
                    yDeltaTotal += yDelta;

                    senPairs++;

                    if (yDelta > 0)
                    {
                        p++;
                    }
                    else if (yDelta < 0)
                    {
                        n++;
                    }
                    else
                    {
                        t++;
                    }
                }
            }

            senSlopes.Sort();

            double medianSenSlope = GetMedian(senSlopes);
            double S = (p - n);
            double POS = p;
            double TAU = S / ((double) senPairs);

            /* Approximation of normal distribution (mean zero, no variance) */
            double senVariance = (2.0 * (2.0 * N + 5)) / ((9 * N) * (N - 1));
            double SDTau = System.Math.Sqrt(senVariance);

            double senZScore = (TAU / SDTau);

            double PVal = GetPValueFromUDistribution(System.Math.Abs(senZScore), true);

            double SDts = (SDTau / TAU) * medianSenSlope;
            double VARts = SDts * SDts;
            double PAIRS = senPairs;

            return new SenSlopeModel
            {
                Observations = data,
                Length = data.Count,
                Sen = medianSenSlope,
                SenSD = (SDTau / TAU) * medianSenSlope,
                SenVariance = SDts * SDts,
                Zscore = senZScore,
                Tau = TAU,
                TauSD = SDTau,
                P = PVal,
                TS85 = new double[] { medianSenSlope - 1.44 * SDts, medianSenSlope + 1.44 * SDts },
                TS90 = new double[] { medianSenSlope - 1.645 * SDts, medianSenSlope + 1.645 * SDts },
                TS95 = new double[] { medianSenSlope - 1.96 * SDts, medianSenSlope + 1.96 * SDts }
            };
        }

        public SenSlopeModel CompareTheilSen(SenSlopeModel bl, SenSlopeModel tx)
        {
            SenSlopeModel senClone = bl;
            senClone.SenSD = System.Math.Sqrt(bl.SenVariance + tx.SenVariance);
            senClone.Sen = bl.Sen - tx.Sen;
            senClone.Zscore = bl.Sen / System.Math.Sqrt(bl.SenVariance + tx.SenVariance);
            senClone.TS85 = bl.TS85;
            senClone.TS90 = bl.TS90;
            senClone.TS95 = bl.TS95;

            return senClone;
        }

        public SenSlopeModel GetInterceptBaseline(SenSlopeModel current)
        {
            /* Intercept derived based upon Conover's theorum */
            /* Median(y) = SenSlope * Median(x) + Intercept */
            /* Intercept = Median(y) - (SenSlope * Median(x)) */

            List<double> mx = new List<double>();
            for (int i = 0; i < current.Length; i++)
            {
                mx.Add(i + 1);
            }

            List<double> my = new List<double>(current.Observations);

            mx.Sort();
            my.Sort();

            current.MedianX = GetMedian(mx);
            current.MedianY = GetMedian(my);
            current.ConoverIntercept = current.MedianY - current.Sen * current.MedianX;

            return current;
        }

        public SenSlopeModel GetInterceptTreatment(SenSlopeModel current, int prevLength)
        {
            /* Intercept derived based upon Conover's theorum */
            /* Median(y) = SenSlope * Median(x) + Intercept */
            /* Intercept = Median(y) - (SenSlope * Median(x)) */

            List<double> mx = new List<double>();

            for (int i = prevLength; i < current.Length + prevLength; i++)
            {
                mx.Add(i + 1);
            }

            List<double> my = new List<double>(current.Observations);

            mx.Sort();
            my.Sort();

            current.MedianX = GetMedian(mx);
            current.MedianY = GetMedian(my);
            current.ConoverIntercept = current.MedianY - current.Sen * current.MedianX;

            return current;
        }

        private double GetMedian(List<double> data)
        {
            List<double> listClone = new List<double>(data);
            int midItemRoundedDown = (int) System.Math.Floor(listClone.Count / (double) 2);
            return (listClone[midItemRoundedDown] + listClone[midItemRoundedDown - (1 - listClone.Count % 2)]) / (double) 2;
        }

        public double GetPValueFromUDistribution(double x, bool isTwoTailed)
        {
            /* ORIGINAL AUTHOR
            * 
            * Ben Tilly <btilly@gmail.com>
            * 
            * Originl Perl version by Michael Kospach <mike.perl@gmx.at>
            * 
            * Nice formating, simplification and bug repair by Matthias Trautner Kromann
            * <mtk@id.cbs.dk>
            * 
            * COPYRIGHT 
            * 
            * Copyright 2008 Ben Tilly.
            * 
            * This library is free software; you can redistribute it and/or modify it
            * under the same terms as Perl itself.  This means under either the Perl
            * Artistic License or the GPL v1 or later.
            */

            double p = 0;
            var absx = System.Math.Abs(x);

            if (absx < 1.9)
            {
                p = System.Math.Pow((1 + absx * (.049867347 + absx * (.0211410061 + absx * (.0032776263 + absx * (.0000380036 + absx * (.0000488906 + absx * .000005383)))))), -16) / (double)2;
            }
            else if (absx <= 100)
            {
                for (int i = 18; i >= 1; i--)
                {
                    p = i / (absx + p);
                }

                p = System.Math.Exp(-.5 * absx * absx) / System.Math.Sqrt(2 * System.Math.PI) / (absx + p);
            }

            if (x < 0)
            {
                p = 1 - p;
            }

            if (isTwoTailed)
            {
                p = p * 2;
            }

            return p;
        }
    }
}
