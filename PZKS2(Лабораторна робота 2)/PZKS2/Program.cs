using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace PZKS2
{
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

                Node astRoot;
                try { astRoot = ExpressionBuilderParallel.BuildAstFromTokens(tokens); }
                catch (Exception ex) { Console.WriteLine("Помилка: " + ex.Message); continue; }

                SimplificationCounter.PrintResults();

                Console.WriteLine("\nКінцевий Результат");

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
            }
        }
    }
}