using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PatternChanger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.Loaded += MainWindow_Loaded;
        }

        private Dictionary<string, Color> _Palette = new Dictionary<string, Color>();

        private Color _renderMC = Colors.White;

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //_renderedImage.Source = ProcessDirectory(@"C:\Users\bdeldri\Documents\visual studio 2012\Projects\PatternChanger\pony\", 240, 215);
            //_renderedImage.Source = ProcessDirectory(@"C:\Users\bdeldri\Documents\visual studio 2012\Projects\PatternChanger\derpy\", 50, 50);
            //_renderedImage.Source = ProcessDirectory(@"C:\Users\bdeldri\Documents\visual studio 2012\Projects\PatternChanger\derpy_with_fan\", 100, 50);
            _renderedImage.Source = ProcessDirectory(@"C:\Users\bdeldri\Documents\visual studio 2012\Projects\PatternChanger\kyle\", 200, 150);
        }

        private ImageSource ProcessDirectory(string p_dir, int p_width, int p_height)
        {
            LoadPalette(System.IO.Path.Combine(p_dir, "palette.txt"));
            var colorList = BuildPatternFromImage(System.IO.Path.Combine(p_dir,  "grid_source.png"), p_width, p_height);

            var colorAliases = BuildColorAliases(colorList, p_width, p_height);

            List<string> outputFileLines = new List<string>();

            outputFileLines.Add("--- ALIAS LIST ---");
            foreach (var a in colorAliases)
            {
                outputFileLines.Add(a.Value.PadRight(2) + ": " + a.Key);
            }

            var counts = GetBobbinCounts(colorList, p_width, p_height, colorAliases);
            outputFileLines.Add("--- BOBBIN COUNTS ---");
            foreach (var c in counts)
            {
                outputFileLines.Add(c.Key.PadRight(2) + ": " + c.Value);
            }

            outputFileLines.Add("--- ROW LISTS (STARTING AT BOTTOM RIGHT CORNER, ALTERNATING DIRECTION) ---");
            for (int row = 0; row < p_height; row++)
            {
                var ri = GetRowInstructions(colorList, row, p_width, p_height, colorAliases);
                outputFileLines.Add("Row " + (row + 1).ToString().PadLeft(3) + ": " + String.Join(" ", ri.Select(p_item => p_item.Name + "(" + p_item.Count + ")")));
            }

            System.IO.File.WriteAllLines(System.IO.Path.Combine(p_dir, "pattern.txt"), outputFileLines);

            return RenderPattern(colorList, p_width, p_height, 5, 5);
        }

        private void LoadPalette(string p_file)
        {
            _Palette.Clear();

            string[] lines = System.IO.File.ReadAllLines(p_file);

            foreach (var line in lines)
            {
                string[] data = line.Split(new char[] { ',' });

                if (data.Length == 4)
                {
                    _Palette.Add(data[0], Color.FromRgb(byte.Parse(data[1]), byte.Parse(data[2]), byte.Parse(data[3])));
                }
            }
        }

        private List<string> _aliases = new List<string> {
            "MC",
            "A",
            "B",
            "C",
            "D",
            "E",
            "F",
            "G",
            "H",
            "I",
            "J",
            "K",
            "L",
            "M",
            "N",
            "O",
            "P",
            "Q",
            "R",
            "S",
            "T",
            "U",
            "V",
            "W",
            "X",
            "Y",
            "Z",
            "AA",
            "BB",
            "CC",
            "DD",
            "EE",
        };

        private Dictionary<string, string> BuildColorAliases(List<string> p_colors, int p_gridWidth, int p_gridHeight)
        {
            Dictionary<string, string> aliasList = new Dictionary<string,string>();

            int nextAlias = 0;

            for(int row = 0; row < p_gridHeight; row++) {

                var rowColors = GetRow(p_colors, row, p_gridWidth, p_gridHeight);

                foreach(var color in rowColors) 
                {
                    if(!aliasList.ContainsKey(color)) {
                        aliasList.Add(color, _aliases[nextAlias++]);
                    }
                }
            }

            return aliasList;
        }

        void RGBtoHSV(double r, double g, double b, out double h, out double s, out double v)
        {
            double min, max, delta;
            min = Math.Min(r, Math.Min(g, b));
            max = Math.Max(r, Math.Max(g, b));
            v = max;				// v
            delta = max - min;
            if (max != 0)
                s = delta / max;		// s
            else
            {
                // r = g = b = 0		// s = 0, v is undefined
                s = 0;
                h = -1;
                return;
            }
            if (r == max)
                h = (g - b) / delta;		// between yellow & magenta
            else if (g == max)
                h = 2 + (b - r) / delta;	// between cyan & yellow
            else
                h = 4 + (r - g) / delta;	// between magenta & cyan
            h *= 60;				// degrees
            if (h < 0)
                h += 360;
        }


        private string GetClosestColor(Color p_color)
        {
            double minDist = double.MaxValue;
            string minColor = null;

            foreach (var color in _Palette)
            {
                /*
                double h, s, v;
                double h2, s2, v2;
                RGBtoHSV(r, g, b, out h, out s, out v);
                RGBtoHSV(color.Value.R, color.Value.G, color.Value.B, out h2, out s2, out v2);
                 

                var d = Math.Pow(h - h2, 2) +
                    Math.Pow(s - s2, 2) +
                    Math.Pow(v - v2, 2);
                */
                
                var d = Math.Pow(p_color.R - color.Value.R, 2) +
                    Math.Pow(p_color.G - color.Value.G, 2) +
                    Math.Pow(p_color.B - color.Value.B, 2);
                

                if (d < minDist)
                {
                    minDist = d;
                    minColor = color.Key;
                }
            }

            return minColor;
        }

        private class ColorCount
        {
            public string Name;
            public int Count;

            public ColorCount(string p_name, int p_count)
            {
                this.Name = p_name;
                this.Count = p_count;
            }
        }

        private List<string> GetRow(List<string> p_colors, int p_rowNum, int p_gridWidth, int p_gridHeight) 
        {
            // Rows are numbered from the bottom

            int startIndex = (p_gridHeight - p_rowNum - 1) * p_gridWidth;
            int step = 1;

            // even numbered rows start on the right and count backwards, odd ones on the left
            if (p_rowNum % 2 == 0)
            {
                step = -1;
                startIndex += p_gridWidth - 1;
            }

            List<string> row = new List<string>();

            int currentIndex = startIndex;
            for (int i = 0; i < p_gridWidth; i++)
            {
                row.Add(p_colors[currentIndex]);

                currentIndex += step;
            }

            return row;
        }

        private List<ColorCount> GetRowInstructions(List<string> p_colors, int p_rowNum, int p_gridWidth, int p_gridHeight, Dictionary<string, string> p_aliasList)
        {
            // Rows are numbered from the bottom

            var row = GetRow(p_colors, p_rowNum, p_gridWidth, p_gridHeight);

            List<ColorCount> rowInstruction = new List<ColorCount>();

            foreach(var color in row)
            {
                var alias = p_aliasList[color];

                if (!rowInstruction.Any() || alias != rowInstruction.Last().Name)
                {
                    rowInstruction.Add(new ColorCount(alias, 1));
                }
                else
                {
                    rowInstruction.Last().Count++;
                }
            }

            return rowInstruction;
        }

        private Dictionary<string, int> GetBobbinCounts(List<string> p_colors, int p_gridWidth, int p_gridHeight, Dictionary<string, string> p_aliasList)
        {
            Dictionary<string, int> counts = new Dictionary<string, int>();

            List<List<ColorCount>> riList = new List<List<ColorCount>>();

            for (int row = 0; row < p_gridHeight; row++)
            {
                riList.Add(GetRowInstructions(p_colors, row, p_gridWidth, p_gridHeight, p_aliasList));
            }

            foreach (var alias in p_aliasList)
            {
                // find the row with the maximum number
                int maxCount = int.MinValue;
                foreach (var ri in riList)
                {
                    int count = 0;
                    foreach (var cc in ri)
                    {
                        if (cc.Name == alias.Value) count++;
                    }

                    if (count > maxCount)
                    {
                        maxCount = count;
                    }
                }

                counts.Add(alias.Value, maxCount);
            }

            return counts;
        }


        private List<string> BuildPatternFromImage(string p_imageFileName, int p_gridWidth, int p_gridHeight)
        {
            BitmapSource bmp = new BitmapImage(new Uri(p_imageFileName));

            byte[] rawPixels = new byte[bmp.PixelWidth * bmp.PixelHeight * 4];

            bmp.CopyPixels(rawPixels, bmp.PixelWidth * 4, 0);

            double cellWidth = (double)bmp.PixelWidth / p_gridWidth;
            double cellHeight = (double)bmp.PixelHeight / p_gridHeight;

            Console.WriteLine("Width: " + cellWidth + ", Height: " + cellHeight);

            List<string> ret = new List<string>();

            Dictionary<string, int> votes = new Dictionary<string, int>();

            // Pull out a list of the average colors
            for(double j = 0; j < bmp.PixelHeight; j += cellHeight) 
            {
                Console.WriteLine("Processing line " + Math.Round(j) + ", " + (j / cellHeight));
                for(double i = 0; i < bmp.PixelWidth; i += cellWidth) 
                {
                    //TODO: average may not be the best idea here, maybe a voting scheme?
                    double average_r = 0;
                    double average_g = 0;
                    double average_b = 0;

                    //Debug.WriteLine("=== Sampling cell " + i + ", " + j);

                    double pixelCount = 0;

                    votes.Clear();
                    foreach (var c in _Palette)
                    {
                        votes[c.Key] = 0;
                    }

                    double cellStep = 1;

                    //Console.WriteLine("Processing region at " + (i) + ", " + (j));

                    for (double cellj = cellStep; cellj < cellHeight - cellStep; cellj += cellStep)
                    {
                        for (double celli = cellStep; celli < cellWidth - cellStep; celli += cellStep)
                        {
                            var index = (int)((j + cellj) * bmp.PixelWidth + (i + celli)) * 4;

                            //votes[GetClosestColor(Interpolate(rawPixels, i + celli, j + cellj, bmp.PixelWidth))]++;

                            votes[GetClosestColor(Color.FromRgb(rawPixels[index+2], rawPixels[index+1], rawPixels[index]))]++;
                        }
                    }

                    if (votes.Any(p_v => p_v.Value > 0))
                    {
                        ret.Add(votes.OrderBy(p_v => p_v.Value).Last().Key);

                        //Debug.WriteLine("=== Done sampling cell, color = " + color );
                    }
                }
            }

            //Debug.Assert(ret.Count == p_gridHeight * p_gridWidth);

            return ret;
        }

        public static Color Interpolate(byte[] rawPixels, double x, double y, int p_pixelWidth)
        {
            // Get the nearest integer pixel coords (xi;yi).
            int xi = (int)Math.Floor(x);
            int yi = (int)Math.Floor(y);

            double k1 = x - xi; // Coefficients for interpolation formula.
            double k2 = y - yi;

            double v11r = 0;
            double v11g = 0;
            double v11b = 0;
            int vIndex11 = (yi * p_pixelWidth + xi) * 4;
            if (vIndex11 > 0 && vIndex11 + 2 < rawPixels.Length)
            {
                v11b = rawPixels[vIndex11];
                v11g = rawPixels[vIndex11+1];
                v11r = rawPixels[vIndex11+2];
            }

            double v12r = 0;
            double v12g = 0;
            double v12b = 0;
            int vIndex12 = (yi * p_pixelWidth + (xi + 1)) * 4;
            if (vIndex12 > 0 && vIndex12 + 2 < rawPixels.Length)
            {
                v12b = rawPixels[vIndex12];
                v12g = rawPixels[vIndex12 + 1];
                v12r = rawPixels[vIndex12 + 2];
            }

            double v21r = 0;
            double v21g = 0;
            double v21b = 0;
            int vIndex21 = ((yi + 1) * p_pixelWidth + xi) * 4;
            if (vIndex21 > 0 && vIndex21 + 2 < rawPixels.Length)
            {
                v21b = rawPixels[vIndex21];
                v21g = rawPixels[vIndex21 + 1];
                v21r = rawPixels[vIndex21 + 2];
            }

            double v22r = 0;
            double v22g = 0;
            double v22b = 0;
            int vIndex22 = ((yi + 1) * p_pixelWidth + (xi + 1)) * 4;
            if (vIndex22 > 0 && vIndex22 + 2 < rawPixels.Length)
            {
                v22b = rawPixels[vIndex22];
                v22g = rawPixels[vIndex22 + 1];
                v22r = rawPixels[vIndex22 + 2];
            }

            // Interpolate pixel intensity.
            double interpolatedValuer =
                        (1.0f - k1) * (1.0f - k2) * v11r +
                        (k1 * (1.0f - k2) * v12r) +
                        ((1.0f - k1) * k2 * v21r) +
                        (k1 * k2 * v22r);

            double interpolatedValueg =
            (1.0f - k1) * (1.0f - k2) * v11g +
            (k1 * (1.0f - k2) * v12g) +
            ((1.0f - k1) * k2 * v21g) +
            (k1 * k2 * v22g);

            double interpolatedValueb =
            (1.0f - k1) * (1.0f - k2) * v11b +
            (k1 * (1.0f - k2) * v12b) +
            ((1.0f - k1) * k2 * v21b) +
            (k1 * k2 * v22b);

            return Color.FromRgb((byte)interpolatedValuer, (byte)interpolatedValueg, (byte)interpolatedValueb);
        }


        private ImageSource RenderPattern(List<string> p_colorList, int p_gridWidth, int p_gridHeight, int p_gridCellWidth, int p_gridCellHeight)
        {
            WriteableBitmap bmp = new WriteableBitmap(p_gridWidth * p_gridCellWidth, p_gridHeight * p_gridCellHeight, 96, 96, PixelFormats.Pbgra32, null);

            //Debug.WriteLine("Rendering image, width=" + bmp.PixelWidth + ", height=" + bmp.PixelHeight);

            int cellCount = 0;

            foreach (var colorName in p_colorList)
            {
                var color = _Palette[colorName];

                if (colorName == "MC")
                {
                    // render MC as another color
                    color = _renderMC;
                }


                byte[] cell = new byte[p_gridCellWidth * p_gridCellHeight * 4];

                for (int i = 0; i < p_gridCellWidth * p_gridCellHeight; i++)
                {
                    cell[i * 4] = color.B;
                    cell[i * 4 + 1] = color.G;
                    cell[i * 4 + 2] = color.R;
                    cell[i * 4 + 3] = color.A;
                }

                int xOffset = (cellCount % p_gridWidth) * p_gridCellWidth;
                int yOffset = ((cellCount - (cellCount % p_gridWidth)) / p_gridWidth) * p_gridCellHeight;

                //Debug.WriteLine("Rendering cell at " + xOffset + ", " + yOffset + ", color = " + color);
                if (xOffset + p_gridCellWidth <= bmp.PixelWidth &&
                    yOffset + p_gridCellHeight <= bmp.PixelHeight)
                {
                    Int32Rect rect = new Int32Rect(xOffset, yOffset, p_gridCellWidth, p_gridCellHeight);
                    bmp.WritePixels(rect, cell, p_gridCellWidth * 4, 0);
                }
                cellCount++;
            }

            return bmp;
        }
    }
}
