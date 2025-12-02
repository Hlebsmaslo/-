using System;

namespace PZKS2
{
    class Token
    {
        public string Value;
        public TokenType Type;
        public int Position;
        public int Length;

        public Token(string value, TokenType type, int position)
        {
            Value = value;
            Type = type;
            Position = position;
            Length = value.Length;
        }
    }
}