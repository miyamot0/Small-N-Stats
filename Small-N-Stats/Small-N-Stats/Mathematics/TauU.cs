using Small_N_Stats.Model;
using System.Collections.Generic;
using System.Linq;

namespace Small_N_Stats.Mathematics
{
    class TauU
    {
        public List<double> BaselineObservations { get; set; }
        public List<double> InterventionObservations { get; set; }

        public TauU()
        {
            if (BaselineObservations == null)
                BaselineObservations = new List<double>();

            if (InterventionObservations == null)
                InterventionObservations = new List<double>();
        }

        public void OutputMetrics()
        {
            List<double> blCopy = new List<double>(BaselineObservations);
            List<double> txCopy = new List<double>(InterventionObservations);

            int over = 0, under = 0, same = 0;

            double comparer;

            for (int i = 0; i < blCopy.Count; i++)
            {
                for (int j = 0; j < txCopy.Count; j++)
                {
                    comparer = txCopy[j] - blCopy[i];

                    if (comparer > 0)
                    {
                        over++;
                    }
                    else if (comparer == 0 || (txCopy[j].ToString() == blCopy[i].ToString()))
                    {
                        /* Hackish comparison to permit more reliably floating point value comparisons */
                        same++;
                    }
                    else
                    {
                        under++;
                    }
                }
            }

            double tau = ((double)over - (double)under) / (double)(blCopy.Count * txCopy.Count);
            double variance = (double)(blCopy.Count * txCopy.Count) * (double)(blCopy.Count + txCopy.Count + 1) / 3.0;
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
                 p = System.Math.Pow((1 + absx * (.049867347 + absx * (.0211410061 + absx * (.0032776263 + absx * (.0000380036 + absx * (.0000488906 + absx * .000005383)))))), -16) / (double) 2;
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

        public TauUModel BaselineTrend(List<double> phase1, List<double> phase2, bool lessBaselineTrend)
        {
            List<double> blCopy = new List<double>(phase1);
            List<double> txCopy = new List<double>(phase2);

            var blNotTx = blCopy.Except(txCopy).ToList();

            if (blNotTx.Count < 1)
            {
                /* BlNotTx is LINQ except list.  If n = 0, both supplied are identical */
                // Both Phases supplied have same elements!

                int pos = 0, neg = 0, ties = 0;
                var pairs = 0;

                var inc = 0;

                int trend_off = 1;

                for (var i = 0; i < blCopy.Count - trend_off; i++)
                {
                    for (var j = trend_off + inc; j < txCopy.Count; j++)
                    {
                        System.Console.WriteLine(string.Format("bl counter {0}, bl length {1}, tx counter {2}, tx length {3}",
                            i,
                            blCopy.Count,
                            j,
                            txCopy.Count));

                        var diff = (txCopy[j] - blCopy[i]);

                        if (diff == 0 || (txCopy[j].ToString() == blCopy[i].ToString()))
                        {
                            /* Hackish workaround for odd floating point comparisons */
                            ties++;
                        }
                        else if (diff > 0)
                        {
                            pos++;
                        }
                        else
                        {
                            neg++;
                        }

                        pairs++;
                    }

                    inc++;
                }

                var S = pos - neg;

                /* Variance as Defined by two-tailed Mann-Kendall test */
                double Vars = blCopy.Count * (blCopy.Count - 1.0) * (2.0 * blCopy.Count + 5.0) / 18.0;

                return new TauUModel
                {
                    Reflective = true,
                    S = S,
                    Pairs = pairs,
                    Ties = ties,
                    TAU = (double)S / (double)pairs,
                    TAUB = (S / (pairs * 1.0 - ties * 0.5)),
                    VARs = Vars,
                    SD = System.Math.Sqrt(Vars),
                    SDtau = (System.Math.Sqrt(Vars) / pairs),
                    Z = ((S / (double)pairs) / (System.Math.Sqrt(Vars) / (double)pairs)),
                    PValue = GetPValueFromUDistribution(System.Math.Abs(((S / (double)pairs) / (System.Math.Sqrt(Vars) / (double)pairs))), true),
                    CI_85 = new double[] { ((double)S / (double)pairs - 1.44 * (System.Math.Sqrt(Vars) / (double)pairs)), ((double)S / (double)pairs + 1.44 * (System.Math.Sqrt(Vars) / (double)pairs)) },
                    CI_90 = new double[] { ((double)S / (double)pairs - 1.645 * (System.Math.Sqrt(Vars) / (double)pairs)), ((double)S / (double)pairs + 1.645 * (System.Math.Sqrt(Vars) / (double)pairs)) },
                    CI_95 = new double[] { ((double)S / (double)pairs - 1.96 * (System.Math.Sqrt(Vars) / (double)pairs)), ((double)S / (double)pairs + 1.96 * (System.Math.Sqrt(Vars) / (double)pairs)) }
                };
            }
            else
            {
                // Phases have differing elements
                var pos = 0;
                var neg = 0;
                var ties = 0;
                var pairs = 0;

                var istrend = false;
                var trend_off = 0;
                var inc = 0;
                var n = blCopy.Count;
                var m = txCopy.Count;

                for (var i = 0; i < n - trend_off; i++)
                {
                    for (var j = trend_off + inc; j < m; j++)
                    {
                        var diff = (txCopy[j] - blCopy[i]);

                        if (diff == 0 || (txCopy[j].ToString() == blCopy[i].ToString()))
                        {
                            ties++;
                        }
                        else if (diff > 0)
                        {
                            pos++;
                        }
                        else
                        {
                            neg++;
                        }

                        pairs++;
                    }

                    if (istrend)
                    {
                        inc++;
                    }
                }

                var S = pos - neg;
                var Vars = 0.0;

                if (istrend)
                {
                    /* Variance as Defined by two-tailed Mann-Kendall test */
                    Vars = n * (n - 1.0) * (2.0 * n + 5.0) / 18.0;
                }
                else
                {
                    Vars = n * m * (n + m + 1) / 3.0;
                }

                if (lessBaselineTrend)
                {
                    TauUModel mBl = BaselineTrend(phase1, phase1, false);
                    S -= (int) mBl.S;
                }

                return new TauUModel
                {
                    Reflective = true,
                    S = S,
                    SD = System.Math.Sqrt(Vars),
                    TAU = (double)S / (double)pairs,
                    Pairs = pairs,
                    Ties = ties,
                    TAUB = (S / (pairs * 1.0 - ties * 0.5)),
                    VARs = Vars,
                    SDtau = (System.Math.Sqrt(Vars) / pairs),
                    Z = ((S / (double)pairs) / (System.Math.Sqrt(Vars) / (double)pairs)),
                    PValue = GetPValueFromUDistribution(System.Math.Abs(((S / (double)pairs) / (System.Math.Sqrt(Vars) / (double)pairs))), true),
                    CI_85 = new double[] { ((double)S / (double)pairs - 1.44 * (System.Math.Sqrt(Vars) / (double)pairs)), ((double)S / (double)pairs + 1.44 * (System.Math.Sqrt(Vars) / (double)pairs)) },
                    CI_90 = new double[] { ((double)S / (double)pairs - 1.645 * (System.Math.Sqrt(Vars) / (double)pairs)), ((double)S / (double)pairs + 1.645 * (System.Math.Sqrt(Vars) / (double)pairs)) },
                    CI_95 = new double[] { ((double)S / (double)pairs - 1.96 * (System.Math.Sqrt(Vars) / (double)pairs)), ((double)S / (double)pairs + 1.96 * (System.Math.Sqrt(Vars) / (double)pairs)) }
                };
            }
        }
    }
}
