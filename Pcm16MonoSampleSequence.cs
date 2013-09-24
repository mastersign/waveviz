using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace de.mastersign.waveviz
{
    class Pcm16MonoSampleSequence : ISampleSequence
    {
        private readonly long offset;
        private readonly int count;
        private readonly BinaryReader reader;

        public Pcm16MonoSampleSequence(Stream s, long offset, long length)
        {
            this.offset = offset;
            this.count = (int)(length / 2);
            reader = new BinaryReader(s);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<float> GetEnumerator()
        {
            for (var i = 0; i < count; i++)
            {
                yield return this[i];
            }
        }

        public int Count { get { return count; } }

        public float this[int index]
        {
            get
            {
                reader.BaseStream.Position = offset + index * 2;
                return reader.ReadInt16() / 32768f;
            }
        }

        public float[] GetBlock(int start, int length)
        {
            var block = new float[length];
            reader.BaseStream.Position = offset + start * 2;
            for (var i = 0; i < length; i++)
            {
                block[i] = reader.ReadInt16() / 32768f;
            }
            return block;
        }
    }
}
