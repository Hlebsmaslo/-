using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PZKS__
{
    class Parser
    {
        private List<Token> tokens;
        private string input;
        private List<string> errors = new List<string>();

        public Parser(List<Token> tokens, string input)
        {
            this.tokens = tokens;
            this.input = input;
        }

        public void Parse()
        {
            errors.Clear();
            int bracketCount = 0;
            Token prev = null;
            int errorNumber = 1;

            for (int i = 0; i < tokens.Count; i++)
            {
                Token t = tokens[i];

                if (t.Type == TokenType.Unknown)
                {
                    AddError(t, $"{errorNumber++}. Некоректний токен: {t.Value}");
                    continue;
                }

                if (prev == null)
                {
                    if ((t.Type == TokenType.Operator) || t.Type == TokenType.CloseBracket)
                        AddError(t, $"{errorNumber++}. Некоректний початок виразу");

                }

                if (t.Type == TokenType.OpenBracket)
                    bracketCount++;
                else if (t.Type == TokenType.CloseBracket)
                {
                    bracketCount--;
                    if (bracketCount < 0)
                        AddError(t, $"{errorNumber++}. Закрита дужка без відкритої");
                    if (prev != null && prev.Type == TokenType.OpenBracket)
                        AddError(t, $"{errorNumber++}. Порожні дужки '()'");
                }

                if (prev != null)
                {
                    if (prev.Type == TokenType.Operator && t.Type == TokenType.Operator)
                        AddError(t, $"{errorNumber++}. Подвійна операція: {prev.Value}{t.Value}");

                    if (prev.Type == TokenType.Operator && t.Type == TokenType.CloseBracket)
                        AddError(t, $"{errorNumber++}. Оператор {prev.Value} перед закритою дужкою");

                    if (prev.Type == TokenType.OpenBracket && t.Type == TokenType.Operator)
                        AddError(t, $"{errorNumber++}. Некоректна операція {t.Value} після відкритої дужки");

                    bool prevIsFunction = prev.Type == TokenType.Identifier &&
                                          (i < tokens.Count) &&
                                          t.Type == TokenType.OpenBracket;
                    if (!prevIsFunction)
                    {
                        if ((prev.Type == TokenType.Number && t.Type == TokenType.Identifier) ||
                            (prev.Type == TokenType.Identifier && t.Type == TokenType.Identifier) ||
                            (prev.Type == TokenType.Number && t.Type == TokenType.Number))
                        {
                            AddError(t, $"{errorNumber++}. Пропущений оператор між значеннями у '{prev.Value}{t.Value}'");
                        }
                    }
                }

                if (t.Type == TokenType.Identifier)
                {
                    bool isFunctionCall = (i + 1 < tokens.Count) && tokens[i + 1].Type == TokenType.OpenBracket;
                    if (isFunctionCall)
                    {
                        int j = i + 2;
                        bool expectArgument = true;
                        int bracketLevel = 1;
                        while (j < tokens.Count && bracketLevel > 0)
                        {
                            var current = tokens[j];

                            if (current.Type == TokenType.OpenBracket)
                                bracketLevel++;
                            else if (current.Type == TokenType.CloseBracket)
                                bracketLevel--;

                            if (bracketLevel == 0)
                                break;

                            if (current.Type == TokenType.Operator && expectArgument)
                            {
                                AddError(current, $"{errorNumber++}. Некоректний аргумент у функції '{t.Value}'");
                            }

                            if (current.Type == TokenType.Number || current.Type == TokenType.Identifier)
                                expectArgument = false;

                            if (current.Type == TokenType.Operator)
                                expectArgument = true;

                            if (current.Type == TokenType.Unknown)
                                AddError(current, $"{errorNumber++}. Невідомий символ у функції '{t.Value}'");

                            j++;
                        }

                        if (bracketLevel != 0)
                            AddError(t, $"{errorNumber++}. Некоректна кількість дужок у функції '{t.Value}'");

                        if (j == i + 1)
                            AddError(t, $"{errorNumber++}. Порожні дужки функції '{t.Value}' допустимі лише для функцій без аргументів");
                    }
                    else
                    {
                        if (Regex.IsMatch(t.Value, @"[a-zA-Z]{2,}") || Regex.IsMatch(t.Value, @"[a-zA-Z]+\d+") || Regex.IsMatch(t.Value, @"\d+[a-zA-Z]+"))
                            AddError(t, $"{errorNumber++}. Пропущений оператор між значеннями у '{t.Value}'");
                    }
                }

                prev = t;
            }

            if (prev != null && prev.Type == TokenType.Operator)
                AddError(prev, $"{errorNumber++}. Некоректний кінець виразу: {prev.Value}");

            if (bracketCount > 0)
            {
                string pointerLine = new string(' ', input.Length) + "^";
                errors.Add($"{errorNumber++}. Некоректна кількість дужок: бракує {bracketCount} закритої дужки\n{input}\n{pointerLine}");
            }

            if (errors.Count == 0)
                Console.WriteLine("Вираз коректний!");
            else
                foreach (var e in errors)
                    Console.WriteLine(e);
        }

        private void AddError(Token t, string message)
        {
            string pointerLine = new string(' ', t.Position) + "^";
            errors.Add($"{input}\n{pointerLine}\n{message}");
        }
    }
}
