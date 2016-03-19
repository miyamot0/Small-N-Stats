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

            int blN = blCopy.Count;
            int txN = txCopy.Count;

            int n = blN + txN;

            int pairs = blN * txN;
            int over = 0;
            int under = 0;
            int same = 0;

            double comparer;

            for (int i = 0; i < blN; i++)
            {
                for (int j = 0; j < txN; j++)
                {
                    comparer = txCopy[j] - blCopy[i];

                    if (comparer > 0)
                    {
                        over++;
                    }
                    else if (comparer == 0)
                    {
                        same++;
                    }
                    else
                    {
                        under++;
                    }
                }
            }

            double tau = ((double)over - (double)under) / (double)pairs;
            double variance = (double)pairs * (double)(blN + txN + 1.0) / 3.0;
        }

        public double get2TailedPValue(double z)
        {
            z = System.Math.Abs(z);
            var p2 = (((((.000005383 * z + .0000488906) * z + .0000380036) * z + .0032776263) * z + .0211410061) * z + .049867347) * z + 1;
            p2 = System.Math.Pow(p2, -16);
            return p2;
        }

        public TAUU BaselineTrend(List<double> phase1, List<double> phase2, bool lessBaselineTrend)
        {
            List<double> blCopy = new List<double>(phase1);
            List<double> txCopy = new List<double>(phase2);

            var blNotTx = blCopy.Except(txCopy).ToList();

            if (blNotTx.Count < 1)
            {
                // Both Phases supplied have same elements!
                var pos = 0;
                var neg = 0;
                var ties = 0;
                var pairs = 0;

                var istrend = false;
                var trend_off = 0;
                var inc = 0;
                var n = blCopy.Count;
                var m = txCopy.Count;

                istrend = true;
                trend_off = 1;

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
                    Vars = n * (n - 1.0) * (2.0 * n + 5.0) / 18.0;
                }

                return new TAUU
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
                    PValue = get2TailedPValue(System.Math.Abs(((S / (double)pairs) / (System.Math.Sqrt(Vars) / (double)pairs)))),
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
                    Vars = n * (n - 1.0) * (2.0 * n + 5.0) / 18.0;
                }
                else
                {
                    Vars = n * m * (n + m + 1) / 3.0;
                }

                if (lessBaselineTrend)
                {
                    TAUU mBl = BaselineTrend(phase1, phase1, false);
                    S -= (int)mBl.S;
                }

                return new TAUU
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
                    PValue = get2TailedPValue(System.Math.Abs(((S / (double)pairs) / (System.Math.Sqrt(Vars) / (double)pairs)))),
                    CI_85 = new double[] { ((double)S / (double)pairs - 1.44 * (System.Math.Sqrt(Vars) / (double)pairs)), ((double)S / (double)pairs + 1.44 * (System.Math.Sqrt(Vars) / (double)pairs)) },
                    CI_90 = new double[] { ((double)S / (double)pairs - 1.645 * (System.Math.Sqrt(Vars) / (double)pairs)), ((double)S / (double)pairs + 1.645 * (System.Math.Sqrt(Vars) / (double)pairs)) },
                    CI_95 = new double[] { ((double)S / (double)pairs - 1.96 * (System.Math.Sqrt(Vars) / (double)pairs)), ((double)S / (double)pairs + 1.96 * (System.Math.Sqrt(Vars) / (double)pairs)) }
                };
            }
        }
    }
}
