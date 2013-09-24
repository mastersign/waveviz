using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using de.mastersign.riff;

namespace de.mastersign.waveviz
{
    class Program
    {
        private const string ARGUMENT_INFO = @"Syntax: waveviz <wave-file> <png-file> [w h] [bg f1 f2 li]
  w  = image width in pixels
  h  = image height in pixels
  bg = background color
  f1 = foreground color 1
  f2 = foreground color 2
  li = line color
The colors are specified as RGBA hex values. (e.g. #E080D0FF)";

        static int Main(string[] args)
        {
            if (args.Length != 2 && args.Length != 4 && args.Length != 6 && args.Length != 8)
            {
                Console.Error.WriteLine("Invalid command line arguments.");
                Console.WriteLine(ARGUMENT_INFO);
                return -1;
            }

            var srcPath = args[0];
            var trgPath = args[1];
            
            var vizProps = new WaveVisualizingProperties();

            if (args.Length == 4)
            {
                vizProps.Width = int.Parse(args[2]);
                vizProps.Height = int.Parse(args[3]);
            }
            if (args.Length == 6)
            {
                vizProps.BackgroundColor = ParseColor(args[2]);
                vizProps.ForegroundColor1 = ParseColor(args[3]);
                vizProps.ForegroundColor2 = ParseColor(args[4]);
                vizProps.LineColor = ParseColor(args[5]);
            }
            if (args.Length == 8)
            {
                vizProps.Width = int.Parse(args[2]);
                vizProps.Height = int.Parse(args[3]);
                vizProps.BackgroundColor = ParseColor(args[4]);
                vizProps.ForegroundColor1 = ParseColor(args[5]);
                vizProps.ForegroundColor2 = ParseColor(args[6]);
                vizProps.LineColor = ParseColor(args[7]);
            }

            Debug.WriteLine("Source: " + srcPath);
            Debug.WriteLine("Target: " + trgPath);
            Debug.WriteLine("Width: " + vizProps.Width);
            Debug.WriteLine("Height: " + vizProps.Height);
            Debug.WriteLine("Background: " + vizProps.BackgroundColor);
            Debug.WriteLine("Foreground 1: " + vizProps.ForegroundColor1);
            Debug.WriteLine("Foreground 2: " + vizProps.ForegroundColor2);
            Debug.WriteLine("Line: " + vizProps.LineColor);

            using (var s = File.OpenRead(srcPath))
            using (var riff = (ContainerChunk)RiffParser.ReadChunk(s))
            {
                Debug.WriteLine(riff.ToString());

                var format = riff.FindFirst("fmt ");
                using (var fs = format.GetDataStream())
                {
                    var fr = new BinaryReader(fs);
                    var tag = fr.ReadUInt16();
                    var chs = fr.ReadUInt16();
                    var smplsPerSec = fr.ReadUInt32();
                    var avgBytes = fr.ReadUInt32();
                    var blockAlgn = fr.ReadUInt16();
                    var bitsPerSmpl = fr.ReadUInt16();

                    Debug.WriteLine("Format: 0x{0:X4}, {1} ch, {2} Hz, {5} bits/Smpl, {4} Bytes/block, {3} Bytes/s",
                        tag, chs, smplsPerSec, avgBytes, blockAlgn, bitsPerSmpl);

                    if (tag != 0x0001 || chs != 1 || smplsPerSec != 16000 || bitsPerSmpl != 16)
                    {
                        Console.Error.WriteLine("Unsupported format.");
                        return -1;
                    }
                }
                var data = riff.FindFirst("data");
                var samples = new Pcm16MonoSampleSequence(s, data.DataPosition, data.DataLength);
                var viz = new WaveVisualizer(vizProps);
                var bmp = viz.CreateImage(samples);
                bmp.Save(Path.ChangeExtension(trgPath, "png"), ImageFormat.Png);
                return 0;
            }
        }

        static Color ParseColor(string value)
        {
            if (value == null) throw new ArgumentNullException();
            if (!value.StartsWith("#") || value.Length != 9)
            {
                throw new ArgumentException(string.Format("The given value '{0}' is not a valid color.", value));
            }
            return Color.FromArgb(
                int.Parse(value.Substring(7, 2), NumberStyles.AllowHexSpecifier),
                int.Parse(value.Substring(1, 2), NumberStyles.AllowHexSpecifier),
                int.Parse(value.Substring(3, 2), NumberStyles.AllowHexSpecifier),
                int.Parse(value.Substring(5, 2), NumberStyles.AllowHexSpecifier));
        }
    }
}
