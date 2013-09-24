using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace de.mastersign.riff
{
    public static class RiffParser
    {
        public static RiffChunk ReadChunk(Stream s)
        {
            var p = s.Position;
            var chunk = new RiffChunk(s);
            if (chunk.FourCc == "RIFF" ||
                chunk.FourCc == "FORM" ||
                chunk.FourCc == "LIST" ||
                chunk.FourCc == "CAT ")
            {
                s.Position = p;
                return new ContainerChunk(s);
            }
            return chunk;
        }
    }

    public class RiffChunk : IDisposable
    {
        protected BinaryReader Reader;

        public string FourCc { get; private set; }

        public uint DataLength { get; protected set; }

        public long DataPosition { get; protected set; }

        protected long GetOffsetWithPadding(long offset)
        {
            return offset % 2 == 0 ? offset : offset + 1;
        }

        internal RiffChunk(Stream s)
        {
            Reader = new BinaryReader(s, Encoding.ASCII);
            FourCc = new string(Reader.ReadChars(4));
            DataLength = Reader.ReadUInt32();
            DataPosition = Reader.BaseStream.Position;
            JumpToEnd();
        }

        public virtual void JumpToData()
        {
            Reader.BaseStream.Position = DataPosition;
        }

        public void JumpToEnd()
        {
            Reader.BaseStream.Position = GetOffsetWithPadding(DataPosition + DataLength);
        }

        public byte[] ReadAllBytes()
        {
            JumpToData();
            return Reader.ReadBytes((int)DataLength);
        }

        public Stream GetDataStream()
        {
            return new MemoryStream(ReadAllBytes());
        }

        public string ReadContentAsString()
        {
            JumpToData();
            return new string(Reader.ReadChars((int)DataLength));
        }

        private const string INDENT = "- ";

        protected string GetIndent(int indent)
        {
            if (indent == 0) return "";
            if (indent == 1) return INDENT;
            var sb = new StringBuilder();
            for (var i = 0; i < indent; i++) sb.Append(INDENT);
            return sb.ToString();
        }

        public virtual string ToString(int indent)
        {
            return string.Format("{0}Chunk [{1}] ({2} .. {3}) {4}",
                GetIndent(indent), FourCc, DataPosition, DataPosition + DataLength, DataLength);
        }

        public override string ToString()
        {
            return ToString(0);
        }

        public bool IsDisposed
        {
            get { return Reader == null; }
        }

        public virtual void Dispose()
        {
            if (IsDisposed) return;
            Reader = null;
        }

        ~RiffChunk()
        {
            Dispose();
        }
    }

    public class ContainerChunk : RiffChunk, IEnumerable<RiffChunk>
    {
        public string ContainerFourCc { get; private set; }

        private readonly RiffChunk[] subChunks;

        public int Count { get { return subChunks.Length; } }

        public RiffChunk FindFirst(string fourCc)
        {
            return subChunks.FirstOrDefault(sc => sc.FourCc == fourCc);
        }

        public IEnumerable<RiffChunk> FindAll(string fourCc)
        {
            return subChunks.Where(sc => sc.FourCc == fourCc);
        }

        internal ContainerChunk(Stream s)
            : base(s)
        {
            Reader.BaseStream.Position = DataPosition;
            ContainerFourCc = new string(Reader.ReadChars(4));
            DataPosition = DataPosition + 4;
            DataLength = DataLength - 4;
            subChunks = ReadSubChunks().ToArray();
            JumpToEnd();
        }

        private IEnumerable<RiffChunk> ReadSubChunks()
        {
            Reader.BaseStream.Position = DataPosition;
            while (Reader.BaseStream.Position < DataPosition + DataLength)
            {
                yield return RiffParser.ReadChunk(Reader.BaseStream);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<RiffChunk> GetEnumerator()
        {
            return ((IEnumerable<RiffChunk>)subChunks).GetEnumerator();
        }

        public override string ToString(int indent)
        {
            var sb = new StringBuilder();
            sb.AppendLine(string.Format("{0}Container [{1}:{2}] ({3} .. {4}) {5}:",
                GetIndent(indent), FourCc, ContainerFourCc,
                DataPosition, DataPosition + DataLength, DataLength));
            foreach (var chunk in ReadSubChunks())
            {
                sb.AppendLine(chunk.ToString(indent + 1));
            }
            return sb.ToString(0, sb.Length - Environment.NewLine.Length);
        }

        public override string ToString()
        {
            return ToString(0);
        }

        public override void Dispose()
        {
            if (IsDisposed) return;
            base.Dispose();
            foreach (var subChunk in subChunks)
            {
                subChunk.Dispose();
            }
        }
    }

}
