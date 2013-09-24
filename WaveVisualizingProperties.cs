using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace de.mastersign.waveviz
{
    public class WaveVisualizingProperties
    {
        public int Width { get; set; }
        public int Height { get; set; }

        public Color BackgroundColor { get; set; }
        public Color ForegroundColor1 { get; set; }
        public Color ForegroundColor2 { get; set; }
        public Color LineColor { get; set; }

        public WaveVisualizingProperties()
        {
            Width = 1000;
            Height = 100;
            BackgroundColor = Color.White;
            ForegroundColor1 = Color.Green;
            ForegroundColor2 = Color.Yellow;
            LineColor = Color.DarkGreen;
        }
    }
}
