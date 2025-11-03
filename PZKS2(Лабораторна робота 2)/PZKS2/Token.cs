using System;

namespace PZKS2
{
    class Token
    {
        public string Value;
        public TokenType Type;
        public int Position;
        public Token(string value, TokenType type, int position) { Value = value; Type = type; Position = position; }
    }
}