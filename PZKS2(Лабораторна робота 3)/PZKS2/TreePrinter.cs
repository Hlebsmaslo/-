using System;
using System.Collections.Generic;
using System.Linq;

namespace PZKS2
{
    static class TreePrinter
    {
        public static void Print(Node root)
        {
            PrintRec(root, 0);
        }

        private static void PrintRec(Node node, int level)
        {
            string indent = new string(' ', level * 6);

            OperatorNode on = node as OperatorNode;
            if (on != null)
            {
                foreach (var child in on.Children.AsEnumerable().Reverse())
                {
                    PrintRec(child, level + 1);
                }
                Console.WriteLine($"{indent}-> {on.Op}");
            }
            else
            {
                Console.WriteLine($"{indent}-> {node.Label}");
            }
        }
    }
}