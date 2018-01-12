using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatternChanger
{
    public class GridCell
    {
        public int StartX { get; set; }
        public int StartY { get; set; }

        public int Width { get; set; }
        public int Height { get; set; }

        public string Tag { get; set; }
    }
}
