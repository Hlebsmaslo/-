using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PZKS__
{
    class Lexer
    {
        private static readonly string pattern =
            @"\s*(\d+(\.\d+)?|[a-zA-Z_][a-zA-Z0-9_]*|[\+\-\*/]|\(|\)|;|\S)\s*";

        public static List<Token> Tokenize(string expression)
        {
            List<Token> tokens = new List<Token>();
            var matches = Regex.Matches(expression, pattern);

            foreach (Match match in matches)
            {
                string val = match.Groups[1].Value;
                if (string.IsNullOrWhiteSpace(val))
                    continue;

                TokenType type;

                if (Regex.IsMatch(val, @"^\d+(\.\d+)?$"))
                    type = TokenType.Number;
                else if (Regex.IsMatch(val, @"^[a-zA-Z_][a-zA-Z0-9_]*$"))
                    type = TokenType.Identifier;
                else if ("+-*/".Contains(val))
                    type = TokenType.Operator;
                else if (val == "(")
                    type = TokenType.OpenBracket;
                else if (val == ")")
                    type = TokenType.CloseBracket;
                else if (val == ";")
                    type = TokenType.Semicolon;
                else
                    type = TokenType.Unknown;

                tokens.Add(new Token(val, type, match.Index));
            }

            return tokens;
        }
    }
}
