using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace de.mastersign.waveviz
{
    class BarInfo
    {
        public float Min;
        public float Max;
        public int PosCnt;
        public int NegCnt;
        public double PosSum;
        public double NegSum;

        public float PosMean { get { return PosCnt > 0 ? (float)(PosSum / PosCnt) : 0f; } }
        public float NegMean { get { return NegCnt > 0 ? (float)(NegSum / NegCnt) : 0f; } }

        public readonly float[] Samples;

        public BarInfo(float[] samples)
        {
            Samples = samples;
        }

        public void Compute()
        {
            foreach (var sample in Samples)
            {
                AddSample(sample);
            }
        }

        public void AddSample(float v)
        {
            if (v >= 0f)
            {
                Max = Math.Max(Max, v);
                PosCnt++;
                PosSum += v;
            }
            else
            {
                Min = Math.Min(Min, v);
                NegCnt++;
                NegSum += v;
            }
        }
    }
}
