using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightNlp.Demo
{
    class SparseItemInt
    {
        public int Label { get; set; }

        public Dictionary<int, double> Features { get; set; }
    }
}
