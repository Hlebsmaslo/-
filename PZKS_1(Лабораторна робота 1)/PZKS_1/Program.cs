using System;
using System.Collections.Generic;

namespace PZKS__
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

                if (string.IsNullOrWhiteSpace(input))
                    continue;

                List<Token> tokens = Lexer.Tokenize(input);
                Parser parser = new Parser(tokens, input);
                parser.Parse();
                Console.WriteLine();
            }
        }
    }
}
