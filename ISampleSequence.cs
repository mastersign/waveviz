using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace de.mastersign.waveviz
{
    public interface ISampleSequence : IReadOnlyList<float>
    {
        float[] GetBlock(int start, int length);
    }
}
