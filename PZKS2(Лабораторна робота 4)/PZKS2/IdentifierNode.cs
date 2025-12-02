using System;
using System.Collections.Generic;

namespace PZKS2
{
    class IdentifierNode : Node
    {
        public string Name;
        public IdentifierNode(string name) { Name = name; }
        public override string Label => Name;
    }
}