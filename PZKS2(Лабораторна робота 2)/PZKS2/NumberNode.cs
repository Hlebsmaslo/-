using System;
using System.Collections.Generic;

namespace PZKS2
{
    class NumberNode : Node
    {
        public double Value;
        public NumberNode(double v) { Value = v; }
        public override string Label => Value.ToString();
    }
}