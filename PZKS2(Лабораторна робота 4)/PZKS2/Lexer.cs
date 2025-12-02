using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace PZKS2
{
    class Lexer
    {
        private static readonly string pattern = @"\s*(\d+(\.\d+)?|[a-zA-Z_][a-zA-Z0-9_]*|[\+\-\*/()]|;|\S)\s*";

        public static List<Token> Tokenize(string expression)
        {
            var tokens = new List<Token>();
            foreach (Match match in Regex.Matches(expression, pattern))
            {
                string val = match.Groups[1].Value;
                if (string.IsNullOrWhiteSpace(val)) continue;

                int tokenStartAbsolute = match.Index + match.Value.IndexOf(val);

                TokenType type;
                if (Regex.IsMatch(val, @"^\d+(\.\d+)?$")) type = TokenType.Number;
                else if (Regex.IsMatch(val, @"^[a-zA-Z_][a-zA-Z0-9_]*$")) type = TokenType.Identifier;
                else if ("+-*/".Contains(val)) type = TokenType.Operator;
                else if (val == "(") type = TokenType.OpenBracket;
                else if (val == ")") type = TokenType.CloseBracket;
                else if (val == ";") type = TokenType.Semicolon;
                else type = TokenType.Unknown;

                tokens.Add(new Token(val, type, tokenStartAbsolute));
            }
            return tokens;
        }
    }
}