using System;
using System.Collections.Generic;
using System.Linq;

namespace Small_N_Stats.Mathematics
{
    class Escalation
    {
        public const int LENGTH_VARY = 50;
        public const int BASE_LENGTH = 40;
        public int EXT_LENGTH = 0;

        // external results 
        static uint[] randrsl = new uint[256];
        static uint randcnt;
        // internal state 
        static uint[] mm = new uint[256];
        static uint aa = 0, bb = 0, cc = 0;

        static void isaac()
        {
            uint i, x, y;
            cc++;    // cc just gets incremented once per 256 results 
            bb += cc;   // then combined with bb 

            for (i = 0; i <= 255; i++)
            {
                x = mm[i];
                switch (i & 3)
                {
                    case 0: aa = aa ^ (aa << 13); break;
                    case 1: aa = aa ^ (aa >> 6); break;
                    case 2: aa = aa ^ (aa << 2); break;
                    case 3: aa = aa ^ (aa >> 16); break;
                }
                aa = mm[(i + 128) & 255] + aa;
                y = mm[(x >> 2) & 255] + aa + bb;
                mm[i] = y;
                bb = mm[(y >> 10) & 255] + x;
                randrsl[i] = bb;
            }
        }

        // if (flag==TRUE), then use the contents of randrsl[] to initialize mm[]. 
        static void mix(ref uint a, ref uint b, ref uint c, ref uint d, ref uint e, ref uint f, ref uint g, ref uint h)
        {
            a = a ^ b << 11; d += a; b += c;
            b = b ^ c >> 2; e += b; c += d;
            c = c ^ d << 8; f += c; d += e;
            d = d ^ e >> 16; g += d; e += f;
            e = e ^ f << 10; h += e; f += g;
            f = f ^ g >> 4; a += f; g += h;
            g = g ^ h << 8; b += g; h += a;
            h = h ^ a >> 9; c += h; a += b;
        }

        static void Init(bool flag)
        {
            short i; uint a, b, c, d, e, f, g, h;

            aa = 0; bb = 0; cc = 0;
            a = 0x9e3779b9; b = a; c = a; d = a;
            e = a; f = a; g = a; h = a;

            for (i = 0; i <= 3; i++)           // scramble it 
                mix(ref a, ref b, ref c, ref d, ref e, ref f, ref g, ref h);

            i = 0;
            do
            { // fill in mm[] with messy stuff  
                if (flag)
                {     // use all the information in the seed 
                    a += randrsl[i]; b += randrsl[i + 1]; c += randrsl[i + 2]; d += randrsl[i + 3];
                    e += randrsl[i + 4]; f += randrsl[i + 5]; g += randrsl[i + 6]; h += randrsl[i + 7];
                } // if flag

                mix(ref a, ref b, ref c, ref d, ref e, ref f, ref g, ref h);
                mm[i] = a; mm[i + 1] = b; mm[i + 2] = c; mm[i + 3] = d;
                mm[i + 4] = e; mm[i + 5] = f; mm[i + 6] = g; mm[i + 7] = h;
                i += 8;
            }
            while (i < 255);

            if (flag)
            {
                // do a second pass to make all of the seed affect all of mm 
                i = 0;
                do
                {
                    a += mm[i]; b += mm[i + 1]; c += mm[i + 2]; d += mm[i + 3];
                    e += mm[i + 4]; f += mm[i + 5]; g += mm[i + 6]; h += mm[i + 7];
                    mix(ref a, ref b, ref c, ref d, ref e, ref f, ref g, ref h);
                    mm[i] = a; mm[i + 1] = b; mm[i + 2] = c; mm[i + 3] = d;
                    mm[i + 4] = e; mm[i + 5] = f; mm[i + 6] = g; mm[i + 7] = h;
                    i += 8;
                }
                while (i < 255);
            }
            isaac();           // fill in the first set of results 
            randcnt = 0;       // prepare to use the first set of results 
        }

        // Seed ISAAC with a string
        static void Seed(string seed, bool flag)
        {
            for (int i = 0; i < 256; i++) mm[i] = 0;
            for (int i = 0; i < 256; i++) randrsl[i] = 0;
            int m = seed.Length;
            for (int i = 0; i < m; i++)
            {
                randrsl[i] = seed[i];
            }
            // initialize ISAAC with seed
            Init(flag);
        }

        // Get a random 32-bit value 
        static uint Random()
        {
            uint result = randrsl[randcnt];
            randcnt++;
            if (randcnt > 255)
            {
                isaac(); randcnt = 0;
            }
            return result;
        }

        private List<int> getRandomSequence()
        {
            List<int> learningSequence = new List<int>();

            uint randomBase = Random() % LENGTH_VARY;
            uint count = randomBase + BASE_LENGTH;

            int i = 0;

            for (i = 0; i < count; i++)
            {

                if (Random() % 2 != 0)
                {
                    learningSequence.Add(1);
                }
                else
                {
                    learningSequence.Add(0);
                }

            }

            return learningSequence;
        }

        private List<int> _learnedSequence;

        public Escalation()
        {
            string key = RandomString(256);
            Seed(key, true);


            if (_learnedSequence == null)
                _learnedSequence = new List<int>();
        }

        /*
        Optional key re-seeding
        */
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public void RandomizeLearningSequence()
        {
            _learnedSequence.Clear();
            _learnedSequence = getRandomSequence();
        }

        public void SetCustomLearningSequence(List<int> newList)
        {
            _learnedSequence = new List<int>(newList);
        }

        public void InjectLosingSequence(int length)
        {
            for (int i = 0; i < length; i++)
            {
                _learnedSequence.Add(0);
            }

            EXT_LENGTH = length;
        }

        public List<double> RunTwoLag()
        {
            List<double> predictions = new List<double>();

            int finalCountdown, finalAbove = 0, lastIterator = 0;

            int finalLength = _learnedSequence.Count;

            double preAve = 0.0, extAve = 0.0;
            List<int> comparison = new List<int>();

            for (finalCountdown = 2; finalCountdown <= finalLength; finalCountdown++)
            {
                comparison.Clear();

                for (int cnt = 0; cnt < finalCountdown; cnt++)
                {
                    comparison.Add(_learnedSequence[cnt]);
                }

                predictions.Add(getTwoLaggedPrediction(comparison, finalCountdown));

                lastIterator++;
            }

            preAve = preAve / (finalLength - EXT_LENGTH - 1);

            double gCount = 0.0, bCount = 0.0;

            int mCount;

            for (mCount = 0; mCount < _learnedSequence.Count; mCount++)
            {
                if (_learnedSequence[mCount] == 1)
                {
                    gCount += 1.0;
                }
                else if (_learnedSequence[mCount] == 0)
                {
                    bCount += 1.0;
                }
            }

            return predictions;
        }

        double getTwoLaggedPrediction(List<int> mGoltz, int sizeOfArray)
        {
            int[] mString = new int[2];

            int gg = 0, bb = 0, gb = 0, bg = 0;
            int ggWin = 0, bbWin = 0, gbWin = 0, bgWin = 0;
            int ggLoss = 0, bbLoss = 0, gbLoss = 0, bgLoss = 0;

            int i = 0;

            for (i = 2; i < sizeOfArray; i++)
            {
                mString[0] = mGoltz[i - 2];
                mString[1] = mGoltz[i - 1];

                if (mString[0] == 1 && mString[1] == 1)
                {
                    gg++;
                    if (mGoltz[i] == 1)
                    {
                        ggWin++;
                    }
                    else if (mGoltz[i] == 0)
                    {
                        ggLoss++;
                    }
                }
                else if (mString[0] == 0 && mString[1] == 0)
                {
                    bb++;
                    if (mGoltz[i] == 1)
                    {
                        bbWin++;
                    }
                    else if (mGoltz[i] == 0)
                    {
                        bbLoss++;
                    }
                }
                else if (mString[0] == 0 && mString[1] == 1)
                {
                    bg++;
                    if (mGoltz[i] == 1)
                    {
                        bgWin++;
                    }
                    else if (mGoltz[i] == 0)
                    {
                        bgLoss++;
                    }
                }
                else if (mString[0] == 1 && mString[1] == 0)
                {
                    gb++;
                    if (mGoltz[i] == 1)
                    {
                        gbWin++;
                    }
                    else if (mGoltz[i] == 0)
                    {
                        gbLoss++;
                    }

                }

            }

            double prediction = 0.0;

            if (mGoltz[sizeOfArray - 2] == 1 && mGoltz[sizeOfArray - 1] == 1)
            {
                prediction = (((double)ggWin + 1) / ((double)gg + 2));
            }
            else if (mGoltz[sizeOfArray - 2] == 0 && mGoltz[sizeOfArray - 1] == 0)
            {
                prediction = (((double)bbWin + 1) / ((double)bb + 2));
            }
            else if (mGoltz[sizeOfArray - 2] == 1 && mGoltz[sizeOfArray - 1] == 0)
            {
                prediction = (((double)gbWin + 1) / ((double)gb + 2));
            }
            else if (mGoltz[sizeOfArray - 2] == 0 && mGoltz[sizeOfArray - 1] == 1)
            {
                prediction = (((double)bgWin + 1) / ((double)bg + 2));
            }

            return prediction;
        }

        public List<double> RunThreeLag()
        {
            List<double> predictions = new List<double>();

            int finalCountdown, finalAbove = 0, lastIterator = 0;

            int finalLength = _learnedSequence.Count;

            double preAve = 0.0, extAve = 0.0;

            List<int> comparison = new List<int>();

            for (finalCountdown = 3; finalCountdown <= finalLength; finalCountdown++)
            {
                comparison.Clear();
                for (int cnt = 0; cnt < finalCountdown; cnt++)
                {
                    comparison.Add(_learnedSequence[cnt]);
                }

                predictions.Add(getThreeLaggedPrediction(comparison, finalCountdown));

                lastIterator++;
            }

            double gCount = 0.0, bCount = 0.0;

            int mCount;

            for (mCount = 0; mCount < _learnedSequence.Count; mCount++)
            {
                if (_learnedSequence[mCount] == 1)
                {
                    gCount += 1.0;
                }
                else if (_learnedSequence[mCount] == 0)
                {
                    bCount += 1.0;
                }
            }

            return predictions;
        }

        double getThreeLaggedPrediction(List<int> mGoltz, int sizeOfArray)
        {
            int[] mString = new int[3];

            int ggg = 0, ggb = 0, gbg = 0, gbb = 0, bgg = 0, bgb = 0, bbg = 0, bbb = 0;
            int gggWin = 0, ggbWin = 0, gbgWin = 0, gbbWin = 0, bggWin = 0, bgbWin = 0, bbgWin = 0, bbbWin = 0;
            int gggLoss = 0, ggbLoss = 0, gbgLoss = 0, gbbLoss = 0, bggLoss = 0, bgbLoss = 0, bbgLoss = 0, bbbLoss = 0;

            int i = 0;

            for (i = 3; i < sizeOfArray; i++)
            {
                mString[0] = mGoltz[i - 3];
                mString[1] = mGoltz[i - 2];
                mString[2] = mGoltz[i - 1];

                if (mString[0] == 1 && mString[1] == 1 && mString[2] == 1)
                {
                    ggg++;
                    if (mGoltz[i] == 1)
                    {
                        gggWin++;
                    }
                    else if (mGoltz[i] == 0)
                    {
                        gggLoss++;
                    }
                }
                else if (mString[0] == 1 && mString[1] == 0 && mString[2] == 1)
                {
                    gbg++;
                    if (mGoltz[i] == 1)
                    {
                        gbgWin++;
                    }
                    else if (mGoltz[i] == 0)
                    {
                        gbgLoss++;
                    }
                }
                else if (mString[0] == 1 && mString[1] == 1 && mString[2] == 0)
                {
                    ggb++;
                    if (mGoltz[i] == 1)
                    {
                        ggbWin++;
                    }
                    else if (mGoltz[i] == 0)
                    {
                        ggbLoss++;
                    }
                }
                else if (mString[0] == 1 && mString[1] == 0 && mString[2] == 0)
                {
                    gbb++;
                    if (mGoltz[i] == 1)
                    {
                        gbbWin++;
                    }
                    else if (mGoltz[i] == 0)
                    {
                        gbbLoss++;
                    }
                }
                else if (mString[0] == 0 && mString[1] == 1 && mString[2] == 1)
                {
                    bgg++;
                    if (mGoltz[i] == 1)
                    {
                        bggWin++;
                    }
                    else if (mGoltz[i] == 0)
                    {
                        bggLoss++;
                    }
                }
                else if (mString[0] == 0 && mString[1] == 1 && mString[2] == 0)
                {
                    bgb++;
                    if (mGoltz[i] == 1)
                    {
                        bgbWin++;
                    }
                    else if (mGoltz[i] == 0)
                    {
                        bgbLoss++;
                    }
                }
                else if (mString[0] == 0 && mString[1] == 0 && mString[2] == 1)
                {
                    bbg++;
                    if (mGoltz[i] == 1)
                    {
                        bbgWin++;
                    }
                    else if (mGoltz[i] == 0)
                    {
                        bbgLoss++;
                    }
                }
                else if (mString[0] == 0 && mString[1] == 0 && mString[2] == 0)
                {
                    bbb++;
                    if (mGoltz[i] == 1)
                    {
                        bbbWin++;
                    }
                    else if (mGoltz[i] == 0)
                    {
                        bbbLoss++;
                    }
                }

            }

            double prediction = 0.0;

            if (mGoltz[sizeOfArray - 3] == 1 && mGoltz[sizeOfArray - 2] == 1 && mGoltz[sizeOfArray - 1] == 1)
            {
                prediction = (((double)gggWin + 1) / ((double)ggg + 2));
            }
            else if (mGoltz[sizeOfArray - 3] == 1 && mGoltz[sizeOfArray - 2] == 1 && mGoltz[sizeOfArray - 1] == 0)
            {
                prediction = (((double)ggbWin + 1) / ((double)ggb + 2));
            }
            else if (mGoltz[sizeOfArray - 3] == 1 && mGoltz[sizeOfArray - 2] == 0 && mGoltz[sizeOfArray - 1] == 1)
            {
                prediction = (((double)gbgWin + 1) / ((double)gbg + 2));
            }
            else if (mGoltz[sizeOfArray - 3] == 1 && mGoltz[sizeOfArray - 2] == 0 && mGoltz[sizeOfArray - 1] == 0)
            {
                prediction = (((double)gbbWin + 1) / ((double)gbb + 2));
            }
            else if (mGoltz[sizeOfArray - 3] == 0 && mGoltz[sizeOfArray - 2] == 1 && mGoltz[sizeOfArray - 1] == 1)
            {
                prediction = (((double)bggWin + 1) / ((double)bgg + 2));
            }
            else if (mGoltz[sizeOfArray - 3] == 0 && mGoltz[sizeOfArray - 2] == 1 && mGoltz[sizeOfArray - 1] == 0)
            {
                prediction = (((double)bgbWin + 1) / ((double)bgb + 2));
            }
            else if (mGoltz[sizeOfArray - 3] == 0 && mGoltz[sizeOfArray - 2] == 0 && mGoltz[sizeOfArray - 1] == 1)
            {
                prediction = (((double)bbgWin + 1) / ((double)bbg + 2));
            }
            else if (mGoltz[sizeOfArray - 3] == 0 && mGoltz[sizeOfArray - 2] == 0 && mGoltz[sizeOfArray - 1] == 0)
            {
                prediction = (((double)bbbWin + 1) / ((double)bbb + 2));
            }

            return prediction;
        }

        public List<double> RunFourLag()
        {
            List<double> predictions = new List<double>();

            int finalCountdown, finalAbove = 0, lastIterator = 0;

            int finalLength = _learnedSequence.Count;

            double preAve = 0.0, extAve = 0.0;
            List<int> comparison = new List<int>();

            for (finalCountdown = 4; finalCountdown <= finalLength; finalCountdown++)
            {
                comparison.Clear();
                for (int cnt = 0; cnt < finalCountdown; cnt++)
                {
                    comparison.Add(_learnedSequence[cnt]);
                }

                predictions.Add(getFourLaggedPrediction(comparison, finalCountdown));

                lastIterator++;
            }

            double gCount = 0.0, bCount = 0.0;

            int mCount;

            for (mCount = 0; mCount < _learnedSequence.Count; mCount++)
            {
                if (_learnedSequence[mCount] == 1)
                {
                    gCount += 1.0;
                }
                else if (_learnedSequence[mCount] == 0)
                {
                    bCount += 1.0;
                }
            }

            return predictions;
        }

        double getFourLaggedPrediction(List<int> mGoltz, int sizeOfArray)
        {
            int[] mString = new int[4];

            int bbbb = 0, bbbg = 0, bbgb = 0, bbgg = 0, bgbb = 0, bgbg = 0, bggb = 0,
                    bggg = 0, gbbb = 0, gbbg = 0, gbgb = 0, gbgg = 0, ggbb = 0,
                    ggbg = 0, gggb = 0, gggg = 0;
            int bbbbWin = 0, bbbgWin = 0, bbgbWin = 0, bbggWin = 0, bgbbWin = 0,
                    bgbgWin = 0, bggbWin = 0, bgggWin = 0, gbbbWin = 0, gbbgWin = 0,
                    gbgbWin = 0, gbggWin = 0, ggbbWin = 0, ggbgWin = 0, gggbWin = 0,
                    ggggWin = 0;
            int bbbbLoss = 0, bbbgLoss = 0, bbgbLoss = 0, bbggLoss = 0, bgbbLoss = 0,
                    bgbgLoss = 0, bggbLoss = 0, bgggLoss = 0, gbbbLoss = 0,
                    gbbgLoss = 0, gbgbLoss = 0, gbggLoss = 0, ggbbLoss = 0,
                    ggbgLoss = 0, gggbLoss = 0, ggggLoss = 0;

            int i = 0;

            for (i = 4; i < sizeOfArray; i++)
            {
                mString[0] = mGoltz[i - 4];
                mString[1] = mGoltz[i - 3];
                mString[2] = mGoltz[i - 2];
                mString[3] = mGoltz[i - 1];

                if (mString[0] == 0 && mString[1] == 0 && mString[2] == 0 && mString[3] == 0)
                {
                    bbbb++;
                    if (mGoltz[i] == 1)
                    {
                        bbbbWin++;
                    }
                    else if (mGoltz[i] == 0)
                    {
                        bbbbLoss++;
                    }
                }
                else if (mString[0] == 0 && mString[1] == 0 && mString[2] == 0 && mString[3] == 1)
                {
                    bbbg++;
                    if (mGoltz[i] == 1)
                    {
                        bbbgWin++;
                    }
                    else if (mGoltz[i] == 0)
                    {
                        bbbgLoss++;
                    }
                }
                else if (mString[0] == 0 && mString[1] == 0 && mString[2] == 1 && mString[3] == 0)
                {
                    bbgb++;
                    if (mGoltz[i] == 1)
                    {
                        bbgbWin++;
                    }
                    else if (mGoltz[i] == 0)
                    {
                        bbgbLoss++;
                    }
                }
                else if (mString[0] == 0 && mString[1] == 0 && mString[2] == 1 && mString[3] == 1)
                {
                    bbgg++;
                    if (mGoltz[i] == 1)
                    {
                        bbggWin++;
                    }
                    else if (mGoltz[i] == 0)
                    {
                        bbggLoss++;
                    }
                }
                else if (mString[0] == 0 && mString[1] == 1 && mString[2] == 0 && mString[3] == 0)
                {
                    bgbb++;
                    if (mGoltz[i] == 1)
                    {
                        bgbbWin++;
                    }
                    else if (mGoltz[i] == 0)
                    {
                        bgbbLoss++;
                    }
                }
                else if (mString[0] == 0 && mString[1] == 1 && mString[2] == 0 && mString[3] == 1)
                {
                    bgbg++;
                    if (mGoltz[i] == 1)
                    {
                        bgbgWin++;
                    }
                    else if (mGoltz[i] == 0)
                    {
                        bgbgLoss++;
                    }
                }
                else if (mString[0] == 0 && mString[1] == 1 && mString[2] == 1 && mString[3] == 0)
                {
                    bggb++;
                    if (mGoltz[i] == 1)
                    {
                        bggbWin++;
                    }
                    else if (mGoltz[i] == 0)
                    {
                        bggbLoss++;
                    }
                }
                else if (mString[0] == 0 && mString[1] == 1 && mString[2] == 1 && mString[3] == 1)
                {
                    bggg++;
                    if (mGoltz[i] == 1)
                    {
                        bgggWin++;
                    }
                    else if (mGoltz[i] == 0)
                    {
                        bgggLoss++;
                    }
                }
                else if (mString[0] == 1 && mString[1] == 0 && mString[2] == 0 && mString[3] == 0)
                {
                    gbbb++;
                    if (mGoltz[i] == 1)
                    {
                        gbbbWin++;
                    }
                    else if (mGoltz[i] == 0)
                    {
                        gbbbLoss++;
                    }
                }
                else if (mString[0] == 1 && mString[1] == 0 && mString[2] == 0 && mString[3] == 1)
                {
                    gbbg++;
                    if (mGoltz[i] == 1)
                    {
                        gbbgWin++;
                    }
                    else if (mGoltz[i] == 0)
                    {
                        gbbgLoss++;
                    }
                }
                else if (mString[0] == 1 && mString[1] == 0 && mString[2] == 1 && mString[3] == 0)
                {
                    gbgb++;
                    if (mGoltz[i] == 1)
                    {
                        gbgbWin++;
                    }
                    else if (mGoltz[i] == 0)
                    {
                        gbgbLoss++;
                    }
                }
                else if (mString[0] == 1 && mString[1] == 0 && mString[2] == 1 && mString[3] == 1)
                {
                    gbgg++;
                    if (mGoltz[i] == 1)
                    {
                        gbggWin++;
                    }
                    else if (mGoltz[i] == 0)
                    {
                        gbggLoss++;
                    } 
                }
                else if (mString[0] == 1 && mString[1] == 1 && mString[2] == 0 && mString[3] == 0)
                {
                    ggbb++;
                    if (mGoltz[i] == 1)
                    {
                        ggbbWin++;
                    }
                    else if (mGoltz[i] == 0)
                    {
                        ggbbLoss++;
                    }
                }
                else if (mString[0] == 1 && mString[1] == 1 && mString[2] == 0 && mString[3] == 1)
                {
                    ggbg++;
                    if (mGoltz[i] == 1)
                    {
                        ggbgWin++;
                    }
                    else if (mGoltz[i] == 0)
                    {
                        ggbgLoss++;
                    }
                }
                else if (mString[0] == 1 && mString[1] == 1 && mString[2] == 1 && mString[3] == 0)
                {
                    gggb++;
                    if (mGoltz[i] == 1)
                    {
                        gggbWin++;
                    }
                    else if (mGoltz[i] == 0)
                    {
                        gggbLoss++;
                    }
                }
                else if (mString[0] == 1 && mString[1] == 1 && mString[2] == 1 && mString[3] == 1)
                {
                    gggg++;
                    if (mGoltz[i] == 1)
                    {
                        ggggWin++;
                    }
                    else if (mGoltz[i] == 0)
                    {
                        ggggLoss++;
                    }
                }

            }

            double prediction = 0.0;

            if (mGoltz[sizeOfArray - 4] == 1 && mGoltz[sizeOfArray - 3] == 1 && mGoltz[sizeOfArray - 2] == 1 && mGoltz[sizeOfArray - 1] == 1)
            {
                prediction = (((double)ggggWin + 1) / ((double)gggg + 2));
            }
            else if (mGoltz[sizeOfArray - 4] == 1 && mGoltz[sizeOfArray - 3] == 1 && mGoltz[sizeOfArray - 2] == 1 && mGoltz[sizeOfArray - 1] == 0)
            {
                prediction = (((double)gggbWin + 1) / ((double)gggb + 2));
            }
            else if (mGoltz[sizeOfArray - 4] == 1 && mGoltz[sizeOfArray - 3] == 1 && mGoltz[sizeOfArray - 2] == 0 && mGoltz[sizeOfArray - 1] == 1)
            {
                prediction = (((double)ggbgWin + 1) / ((double)ggbg + 2));
            }
            else if (mGoltz[sizeOfArray - 4] == 1 && mGoltz[sizeOfArray - 3] == 1 && mGoltz[sizeOfArray - 2] == 0 && mGoltz[sizeOfArray - 1] == 0)
            {
                prediction = (((double)ggbbWin + 1) / ((double)ggbb + 2));
            }
            else if (mGoltz[sizeOfArray - 4] == 1 && mGoltz[sizeOfArray - 3] == 0 && mGoltz[sizeOfArray - 2] == 1 && mGoltz[sizeOfArray - 1] == 1)
            {
                prediction = (((double)gbggWin + 1) / ((double)gbgg + 2));
            }
            else if (mGoltz[sizeOfArray - 4] == 1 && mGoltz[sizeOfArray - 3] == 0 && mGoltz[sizeOfArray - 2] == 1 && mGoltz[sizeOfArray - 1] == 0)
            {
                prediction = (((double)gbgbWin + 1) / ((double)gbgb + 2));
            }
            else if (mGoltz[sizeOfArray - 4] == 1 && mGoltz[sizeOfArray - 3] == 0 && mGoltz[sizeOfArray - 2] == 0 && mGoltz[sizeOfArray - 1] == 1)
            {
                prediction = (((double)gbbgWin + 1) / ((double)gbbg + 2));
            }
            else if (mGoltz[sizeOfArray - 4] == 1 && mGoltz[sizeOfArray - 3] == 0 && mGoltz[sizeOfArray - 2] == 0 && mGoltz[sizeOfArray - 1] == 0)
            {
                prediction = (((double)gbbbWin + 1) / ((double)gbbb + 2));
            }
            else if (mGoltz[sizeOfArray - 4] == 0 && mGoltz[sizeOfArray - 3] == 1 && mGoltz[sizeOfArray - 2] == 1 && mGoltz[sizeOfArray - 1] == 1)
            {
                prediction = (((double)bgggWin + 1) / ((double)bggg + 2));
            }
            else if (mGoltz[sizeOfArray - 4] == 0 && mGoltz[sizeOfArray - 3] == 1 && mGoltz[sizeOfArray - 2] == 1 && mGoltz[sizeOfArray - 1] == 0)
            {
                prediction = (((double)bggbWin + 1) / ((double)bggb + 2));
            }
            else if (mGoltz[sizeOfArray - 4] == 0 && mGoltz[sizeOfArray - 3] == 1 && mGoltz[sizeOfArray - 2] == 0 && mGoltz[sizeOfArray - 1] == 1)
            {
                prediction = (((double)bgbgWin + 1) / ((double)bgbg + 2));
            }
            else if (mGoltz[sizeOfArray - 4] == 0 && mGoltz[sizeOfArray - 3] == 1 && mGoltz[sizeOfArray - 2] == 0 && mGoltz[sizeOfArray - 1] == 0)
            {
                prediction = (((double)bgbbWin + 1) / ((double)bgbb + 2));
            }
            else if (mGoltz[sizeOfArray - 4] == 0 && mGoltz[sizeOfArray - 3] == 0 && mGoltz[sizeOfArray - 2] == 1 && mGoltz[sizeOfArray - 1] == 1)
            {
                prediction = (((double)bbggWin + 1) / ((double)bbgg + 2));
            }
            else if (mGoltz[sizeOfArray - 4] == 0 && mGoltz[sizeOfArray - 3] == 0 && mGoltz[sizeOfArray - 2] == 1 && mGoltz[sizeOfArray - 1] == 0)
            {
                prediction = (((double)bbgbWin + 1) / ((double)bbgb + 2));
            }
            else if (mGoltz[sizeOfArray - 4] == 0 && mGoltz[sizeOfArray - 3] == 0 && mGoltz[sizeOfArray - 2] == 0 && mGoltz[sizeOfArray - 1] == 1)
            {
                prediction = (((double)bbbgWin + 1) / ((double)bbbg + 2));
            }
            else if (mGoltz[sizeOfArray - 4] == 0 && mGoltz[sizeOfArray - 3] == 0 && mGoltz[sizeOfArray - 2] == 0 && mGoltz[sizeOfArray - 1] == 0)
            {
                prediction = (((double)bbbbWin + 1) / ((double)bbbb + 2));
            }

            return prediction;
        }
    }
}
