using Small_N_Stats.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Small_N_Stats.Mathematics
{
    class Modeling
    {
        public List<double> xArray { get; set; }
        public List<double> yArray { get; set; }

        public double maxValue { get; set; }

        public double GetAreaUnderCurve()
        {
            double tempHold = 0.0;
            double adder = 0.0;
            double delayTemp = 0.0;
            double holder = 0.0;

            double totalArea = maxValue * xArray.Max();

            for (int i = 0; i < xArray.Count; i++)
            {
                if (i == 0)
                {
                    delayTemp = xArray[i];
                    holder = yArray[i] / 2.0;
                    adder = (delayTemp * holder);
                    tempHold = tempHold + adder;
                }
                else
                {
                    delayTemp = xArray[i] - xArray[i - 1];
                    double a = yArray[i];
                    double b = yArray[i - 1];
                    holder = (a + b) / 2.0;
                    adder = (delayTemp * holder);
                    tempHold = tempHold + adder;
                }
            }

            return (tempHold / totalArea);
        }

        public ExponentialModel GetExponentialModel()
        {
            List<double> distancesSquared = new List<double>();
            double k = 0.000001;

            double tempHolder = 0.0;
            double residualSquared = 0.0;
            double holdup = 1000000000;
            double holderBig = 0.0;

            bool cancelLoop = false;

            while (!cancelLoop)
            {
                for (int iterations = 0; iterations < xArray.Count; iterations++)
                {
                    tempHolder = (yArray[iterations] - (maxValue * System.Math.Exp((-1 * k * xArray[iterations]))));
                    residualSquared = tempHolder * tempHolder;
                    holderBig += residualSquared;
                }

                if (holderBig < holdup)
                {
                    cancelLoop = false;
                    holdup = holderBig;
                    k += 0.000001;
                    holderBig = 0;
                }
                else
                {
                    cancelLoop = true;
                }
            }

            double valueHolder = (yArray.Sum()) / yArray.Count;
            double tempDistance = 0.0;

            for (int i = 0; i < yArray.Count; i++)
            {
                tempDistance = yArray[i] - valueHolder;
                distancesSquared.Add(tempDistance * tempDistance);
            }

            double r2 = (1 - (holdup / distancesSquared.Sum()));

            return new ExponentialModel { Constant = k, rSquared = r2 };
        }

        public double getExponentialR2(double k)
        {
            double tempHolder = 0.0;
            double residualSquared = 0.0;
            double holderBig = 0.0;

            List<double> distancesSquared = new List<double>();

            for (int iterations = 0; iterations < xArray.Count; iterations++)
            {
                tempHolder = (yArray[iterations] - (maxValue * System.Math.Exp((-1 * k * xArray[iterations]))));
                residualSquared = tempHolder * tempHolder;
                holderBig += residualSquared;
            }

            double valueHolder = (yArray.Sum()) / yArray.Count;
            double tempDistance = 0.0;

            for (int i = 0; i < yArray.Count; i++)
            {
                tempDistance = yArray[i] - valueHolder;
                distancesSquared.Add(tempDistance * tempDistance);
            }

            return (1 - (holderBig / distancesSquared.Sum()));
        }

        public double getExponentialED50(double k)
        {
            double halfValue = maxValue / 2.0;
            halfValue /= maxValue;
            halfValue = System.Math.Log(halfValue);
            halfValue /= k;
            return -halfValue;
        }

        public HyperbolicModel GetHyperbolicModel()
        {
            List<double> distancesSquared = new List<double>();
            double k = 0.000001;

            double tempHolder = 0.0;
            double residualSquared = 0.0;
            double holdup = 1000000000;
            double holderBig = 0.0;

            bool cancelLoop = false;

            while (!cancelLoop)
            {
                for (int iterations = 0; iterations < xArray.Count; iterations++)
                {
                    tempHolder = (yArray[iterations] - ((maxValue / (1 + (k * xArray[iterations])))));
                    residualSquared = tempHolder * tempHolder;
                    holderBig += residualSquared;
                }

                if (holderBig < holdup)
                {
                    cancelLoop = false;
                    holdup = holderBig;
                    k += 0.000001;
                    holderBig = 0;
                }
                else
                {
                    cancelLoop = true;
                }
            }

            double valueHolder = (yArray.Sum()) / yArray.Count;
            double tempDistance = 0.0;

            for (int i = 0; i < yArray.Count; i++)
            {
                tempDistance = yArray[i] - valueHolder;
                distancesSquared.Add(tempDistance * tempDistance);
            }

            double r2 = (1 - (holdup / distancesSquared.Sum()));

            return new HyperbolicModel { Constant = k, rSquared = r2 };
        }

        public double getHyperbolicR2(double k)
        {
            double tempHolder = 0.0;
            double residualSquared = 0.0;
            double holderBig = 0.0;

            List<double> distancesSquared = new List<double>();

            for (int iterations = 0; iterations < xArray.Count; iterations++)
            {
                tempHolder = (yArray[iterations] - ((maxValue / (1 + (k * xArray[iterations])))));
                residualSquared = tempHolder * tempHolder;
                holderBig += residualSquared;
            }

            double valueHolder = (yArray.Sum()) / yArray.Count;
            double tempDistance = 0.0;

            for (int i = 0; i < yArray.Count; i++)
            {
                tempDistance = yArray[i] - valueHolder;
                distancesSquared.Add(tempDistance * tempDistance);
            }

            return (1 - (holderBig / distancesSquared.Sum()));
        }

        public double getAinslieED50(double k)
        {
            return (1 / k);
        }

        public QuasiHyperbolicModel GetQuasiHyperbolicModel()
        {
            double comparer = 9999999999999.0;
            double sum = 0.0;
            double resid = 0.0;

            List<double> distanceSquared = new List<double>();

            double fitI = 0;
            double fitJ = 0;
            double fitSS = 0;

            double predicted;
            double distances;

            for (double i = 0.999; i > 0.70; i = i - 0.001)
            {
                for (double j = 0.000001; j < 0.0004; j = j + 0.000001)
                {
                    sum = 0;

                    for (int count = 0; count < xArray.Count; count++)
                    {
                        predicted = maxValue * i * System.Math.Exp(-xArray[count] * j);
                        resid = yArray[count] - predicted;
                        sum = sum + (resid * resid);
                    }

                    if (sum < comparer)
                    {
                        fitI = i;
                        fitJ = j;
                        fitSS = sum;
                        comparer = sum;
                    }
                }
            }

            double valueHolder = yArray.Sum() / yArray.Count;

            for (int i = 0; i < xArray.Count; i++)
            {
                distances = yArray[i] - valueHolder;
                distanceSquared.Add(distances * distances);
            }

            double r2 = (1 - (fitSS / distanceSquared.Sum()));

            return new QuasiHyperbolicModel { Beta = fitI, Constant = fitJ, rSquared = r2 };
        }

        public double getQuasiHyperbolicR2(double b, double k)
        {
            double tempHolder = 0.0;
            double residualSquared = 0.0;
            double holderBig = 0.0;

            List<double> distancesSquared = new List<double>();

            for (int iterations = 0; iterations < xArray.Count; iterations++)
            {
                tempHolder = yArray[iterations] - (maxValue * b * System.Math.Exp(-xArray[iterations] * k));
                residualSquared = tempHolder * tempHolder;
                holderBig += residualSquared;
            }

            double valueHolder = (yArray.Sum()) / yArray.Count;
            double tempDistance = 0.0;

            for (int i = 0; i < yArray.Count; i++)
            {
                tempDistance = yArray[i] - valueHolder;
                distancesSquared.Add(tempDistance * tempDistance);
            }

            return (1 - (holderBig / distancesSquared.Sum()));
        }

        public double getQuasiED50(double beta, double k, double max)
        {
            double target = max / 2.0;
            bool running = true;
            double delay = 1;

            while (running)
            {
                double currentValue = maxValue * beta * System.Math.Exp(-delay * k);

                if (currentValue <= target)
                {
                    running = false;
                }
                else
                {
                    delay = delay + 0.01;
                }
            }

            return delay;
        }

        public HyperboloidMyersonModel GetHyperboloidMyerson()
        {
            List<double> distanceSquared = new List<double>();

            double tempComparison = 10000000;
            double bestK = 0;
            double bestS = 0;
            double tempHold = 0;

            double holder;
            double residualSquared;

            double mDelay = 0;
            double mSubj = 0;

            for (double j = 0.001; j < 0.75; j = j + 0.001)
            {
                for (double i = 0.001; i < 0.2; i = i + 0.001)
                {
                    holder = 0;
                    residualSquared = 0;
                    tempHold = 0.0;


                    for (int iterations = 0; iterations < xArray.Count; iterations++)
                    {
                        mDelay = xArray[iterations];
                        mSubj = yArray[iterations];
                        holder = mSubj - LookupWithS(i, maxValue, mDelay, j);
                        residualSquared = (holder * holder);
                        tempHold += residualSquared;
                    }

                    if (tempHold < tempComparison)
                    {
                        tempComparison = tempHold;
                        bestK = i;
                        bestS = j;
                    }
                }
            }

            double valueHolder = yArray.Sum() / yArray.Count;
            double distances;

            for (int i = 0; i < xArray.Count; i++)
            {
                distances = yArray[i] - valueHolder;
                distanceSquared.Add(distances * distances);
            }

            double r2 = (1 - (tempComparison / distanceSquared.Sum()));

            return new HyperboloidMyersonModel { Constant = bestK, Scaling = bestS, rSquared = r2 };
        }

        public double getHyperboloidMyersonR2(double k, double s)
        {
            double tempHolder = 0.0;
            double residualSquared = 0.0;
            double holderBig = 0.0;

            List<double> distancesSquared = new List<double>();

            for (int iterations = 0; iterations < xArray.Count; iterations++)
            {
                tempHolder = yArray[iterations] - LookupWithS(k, maxValue, xArray[iterations], s);
                residualSquared = tempHolder * tempHolder;
                holderBig += residualSquared;
            }

            double valueHolder = (yArray.Sum()) / yArray.Count;
            double tempDistance = 0.0;

            for (int i = 0; i < yArray.Count; i++)
            {
                tempDistance = yArray[i] - valueHolder;
                distancesSquared.Add(tempDistance * tempDistance);
            }

            return (1 - (holderBig / distancesSquared.Sum()));
        }

        public double getMyersonED50(double k, double s)
        {
            return ((System.Math.Pow(2, (1 / s)) - 1) / k);
        }

        public double LookupWithS(double k, double values, double delay, double s)
        {
            double look = (values / System.Math.Pow((1 + (k * delay)), s));
            return look;
        }

        public HyperboloidRachlinModel GetHyperboloidRachlin()
        {
            List<double> distanceSquared = new List<double>();

            double tempComparison = 10000000;
            double bestK = 0;
            double bestS = 0;
            double tempHold = 0;

            double holder;
            double residualSquared;

            double mDelay = 0;
            double mSubj = 0;

            for (double j = 0.001; j < 0.75; j = j + 0.001)
            {
                for (double i = 0.0001; i < 0.1; i = i + 0.0001)
                {
                    holder = 0;
                    residualSquared = 0;
                    tempHold = 0.0;

                    for (int iterations = 0; iterations < xArray.Count; iterations++)
                    {
                        mDelay = xArray[iterations];
                        mSubj = yArray[iterations];
                        holder = mSubj - lookUpWithSInside(i, maxValue, mDelay, j);
                        residualSquared = (holder * holder);
                        tempHold += residualSquared;
                    }

                    if (tempHold < tempComparison)
                    {
                        tempComparison = tempHold;
                        bestK = i;
                        bestS = j;
                    }
                }
            }

            double valueHolder = yArray.Sum() / yArray.Count;
            double distances;

            for (int i = 0; i < xArray.Count; i++)
            {
                distances = yArray[i] - valueHolder;
                distanceSquared.Add(distances * distances);
            }

            double r2 = (1 - (tempComparison / distanceSquared.Sum()));

            return new HyperboloidRachlinModel { Constant = bestK, Scaling = bestS, rSquared = r2 };
        }

        public double getHyperboloidRachlinR2(double k, double s)
        {
            double tempHolder = 0.0;
            double residualSquared = 0.0;
            double holderBig = 0.0;

            List<double> distancesSquared = new List<double>();

            for (int iterations = 0; iterations < xArray.Count; iterations++)
            {
                tempHolder = yArray[iterations] - lookUpWithSInside(k, maxValue, xArray[iterations], s);
                residualSquared = tempHolder * tempHolder;
                holderBig += residualSquared;
            }

            double valueHolder = (yArray.Sum()) / yArray.Count;
            double tempDistance = 0.0;

            for (int i = 0; i < yArray.Count; i++)
            {
                tempDistance = yArray[i] - valueHolder;
                distancesSquared.Add(tempDistance * tempDistance);
            }

            return (1 - (holderBig / distancesSquared.Sum()));
        }

        public double getRachlinED50(double k, double s, double max)
        {
            double target = max / 2.0;
            bool running = true;
            double delay = 1;

            while (running)
            {
                double currentValue = lookUpWithSInside(k, max, delay, s);

                if (currentValue <= target)
                {
                    running = false;
                }
                else
                {
                    delay = delay + 0.01;
                }
            }

            return delay;
        }

        public double lookUpWithSInside(double k, double values, double delay, double s)
        {
            double look = (values / (1 + System.Math.Pow((k * delay), s)));
            return look;
        }

        public bool getJohnsonBickelCriteria()
        {
            bool checkOne = true;
            bool checkTwo = true;

            bool answer = false;

            for (int counter = 0; counter < xArray.Count - 1; counter++)
            {

                if (yArray[counter + 1] / yArray[counter] >= 1.2)
                {
                    checkOne = false;
                }
            }

            if (yArray[xArray.Count - 1] / yArray[0] >= 0.9)
            {
                checkTwo = false;
            }
            if (checkOne && checkTwo)
            {
                answer = true;
            }

            return answer;
        }
    }
}
