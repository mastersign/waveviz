using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace de.mastersign.waveviz
{
    public class WaveVisualizer
    {
        private const float ANTI_ALIAS_FACTOR_X = 3f;
        private const float ANTI_ALIAS_FACTOR_Y = 2f;

        private readonly WaveVisualizingProperties props;

        public WaveVisualizer(WaveVisualizingProperties props)
        {
            this.props = props;
        }

        private float[] GetSampleBlock(int x, ISampleSequence samples)
        {
            var w = props.Width * ANTI_ALIAS_FACTOR_X;
            var incStart = (double)x / w;
            var excEnd = (double)++x / w;
            var i0 = (int)Math.Floor(incStart * samples.Count);
            var cnt = (int)Math.Floor(excEnd * samples.Count) - 1 - i0;
            return samples.GetBlock(i0, cnt);
        }

        private void PaintBar(Graphics g, int x, BarInfo barInfo, float scale)
        {
            var w = props.Width * ANTI_ALIAS_FACTOR_X;
            var h = props.Height * ANTI_ALIAS_FACTOR_Y;
            var yc = h / 2f;
            var y11 = yc - barInfo.Max * scale * 0.5f * h;
            var y12 = yc - barInfo.Min * scale * 0.5f * h;
            var y21 = yc - barInfo.PosMean * scale * 0.5f * h + 1;
            var y22 = yc - barInfo.NegMean * scale * 0.5f * h + 1;
            var brush = new SolidBrush(props.ForegroundColor1);
            var posBrush = (int)y11 == (int)y21
                ? brush : (Brush)new LinearGradientBrush(new Point(0, (int)y21 + 1), new Point(0, (int)y11 - 1),
                                    props.ForegroundColor1, props.ForegroundColor2);
            var negBrush = (int)y12 == (int)y22
                ? brush : (Brush)new LinearGradientBrush(new Point(0, (int)y22 - 1), new Point(0, (int)y12 + 1),
                                    props.ForegroundColor1, props.ForegroundColor2);
            var linePen = new Pen(props.LineColor, ANTI_ALIAS_FACTOR_Y);

            g.SmoothingMode = SmoothingMode.HighSpeed;
            g.FillRectangle(posBrush, x, y11, 1, y21 - y11);
            g.FillRectangle(negBrush, x, y22, 1, y12 - y22);
            g.FillRectangle(brush, x, y21, 1, y22 - y21);

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.DrawLine(linePen, 0f, yc, w, yc);

            posBrush.Dispose();
            negBrush.Dispose();
            brush.Dispose();
        }

        public Bitmap CreateImage(ISampleSequence samples)
        {
            var bmp = new Bitmap(props.Width, props.Height, PixelFormat.Format32bppArgb);
            var fullWidth = (int)(props.Width * ANTI_ALIAS_FACTOR_X);
            var fullHeigth = (int)(props.Height * ANTI_ALIAS_FACTOR_Y);
            using (var fullBmp = new Bitmap(fullWidth, fullHeigth, PixelFormat.Format32bppArgb))
            {
                var barInfos = new BarInfo[fullWidth];
                for (var x = 0; x < fullWidth; x++)
                {
                    barInfos[x] = new BarInfo(GetSampleBlock(x, samples));
                }
                Parallel.For(0, fullWidth, x => barInfos[x].Compute());
                var min = barInfos.Select(bi => bi.Min).Min();
                var max = barInfos.Select(bi => bi.Max).Max();
                var scale = 1f / Math.Max(max, -min);
                using (var g = Graphics.FromImage(fullBmp))
                {
                    g.Clear(props.BackgroundColor);
                    for (var x = 0; x < fullWidth; x++)
                    {
                        PaintBar(g, x, barInfos[x], scale);
                    }
                }
                using (var g = Graphics.FromImage(bmp))
                {
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.DrawImage(fullBmp,
                        new Rectangle(0, 0, bmp.Width, bmp.Height),
                        new Rectangle(0, 0, fullBmp.Width, fullBmp.Height),
                        GraphicsUnit.Pixel);
                }
            }
            return bmp;
        }
    }
}
