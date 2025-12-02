using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace PZKS2
{
    class ErrorDetail
    {
        public string Message { get; }
        public int Position { get; }
        public int Length { get; }

        public ErrorDetail(string message, int position, int length)
        {
            Message = message;
            Position = position;
            Length = length;
        }
    }

    class Program
    {
        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.InputEncoding = System.Text.Encoding.UTF8;

            while (true)
            {
                Console.WriteLine("Введіть арифметичний вираз:");
                string input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input)) continue;

                SimplificationCounter.Reset();

                var tokens = Lexer.Tokenize(input);
                var errors = new List<ErrorDetail>();
                for (int i = 0; i < tokens.Count - 1; i++)
                {
                    if (tokens[i].Value == "/" && tokens[i + 1].Value == "0")
                    {
                        errors.Add(new ErrorDetail("Помилка: Виявлено ділення на нуль!", tokens[i].Position, tokens[i].Length));
                    }
                }

                Node astRoot = null;
                try
                {
                    astRoot = ExpressionBuilderParallel.BuildAstFromTokens(tokens, errors);
                }
                catch (Exception ex)
                {
                    errors.Add(new ErrorDetail(
                        ex.Message,
                        tokens.LastOrDefault()?.Position ?? input.Length - 1,
                        tokens.LastOrDefault()?.Length ?? 1));
                }

                if (errors.Any())
                {
                    Console.WriteLine(input);

                    var distinctErrors = errors.GroupBy(e => new { e.Message, e.Position }).Select(g => g.First()).ToList();

                    for (int i = 0; i < distinctErrors.Count; i++)
                    {
                        var error = distinctErrors[i];
                        string indicator = new string(' ', error.Position) + new string('^', Math.Max(1, error.Length));

                        Console.WriteLine(indicator);
                        Console.WriteLine($"{i + 1}. {error.Message}");
                        if (i < distinctErrors.Count - 1)
                        {
                            Console.WriteLine(input);
                        }
                    }
                    Console.WriteLine();
                    continue;
                }

                if (astRoot == null) continue;
                SimplificationCounter.PrintResults();
                Console.WriteLine("\nКінцевий Результат");
                string commutedExpression = ExpressionBuilderParallel.NodeToExpression(astRoot);
                Console.WriteLine($"Змінений вираз (комутативний закон): {commutedExpression}");

                NumberNode resultNode = astRoot as NumberNode;
                if (resultNode != null)
                {
                    Console.WriteLine($"-> Кінцеве значення: {resultNode.Value}");
                }
                else
                {
                    Console.WriteLine("Дерево:");
                    TreePrinter.Print(astRoot);
                }
                Console.WriteLine(); 
            }
        }
    }
}