using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PZKS__
{
    class Token
    {
        public string Value { get; set; }
        public TokenType Type { get; set; }
        public int Position { get; set; }

        public Token(string value, TokenType type, int position)
        {
            Value = value;
            Type = type;
            Position = position;
        }
    }
}
