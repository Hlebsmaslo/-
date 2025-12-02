using System;
using System.Collections.Generic;

namespace PZKS2
{
    class OperatorNode : Node
    {
        public string Op;
        public List<Node> Children = new List<Node>();
        public OperatorNode(string op) { Op = op; }
        public override string Label => Op;
    }
}