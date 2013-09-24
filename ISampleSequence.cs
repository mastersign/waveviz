using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace de.mastersign.waveviz
{
    public interface ISampleSequence : IReadOnlyList<float>
    {
        float[] GetBlock(int start, int length);
    }
}
