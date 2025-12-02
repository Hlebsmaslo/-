using System;
using System.Collections.Generic;
using System.Linq;

namespace PZKS2
{
    static class SimplificationCounter
    {
        public static int RemovedAddZero = 0;
        public static int RemovedSubZero = 0;
        public static int RemovedMulOne = 0;
        public static int RemovedDivOne = 0;
        public static int RemovedMulZero = 0;
        public static List<string> ConstantFoldedDetails = new List<string>();

        public static void Reset()
        {
            RemovedAddZero = 0;
            RemovedSubZero = 0;
            RemovedMulOne = 0;
            RemovedDivOne = 0;
            RemovedMulZero = 0;
            ConstantFoldedDetails.Clear();
        }

        public static void PrintResults()
        {
            bool anyImprovement = false;

            if (ConstantFoldedDetails.Any())
            {
                foreach (var detail in ConstantFoldedDetails)
                {
                    Console.WriteLine(detail);
                }
                anyImprovement = true;
            }

            Console.WriteLine("Видалення нейтральних/поглинаючих елементів.");

            if (RemovedMulZero > 0)
            {
                Console.WriteLine($"Множення на 0 застосовано: {RemovedMulZero} раз(и)");
                anyImprovement = true;
            }
            if (RemovedMulOne > 0)
            {
                Console.WriteLine($"Множення на 1 видалено: {RemovedMulOne} раз(и)");
                anyImprovement = true;
            }
            if (RemovedDivOne > 0)
            {
                Console.WriteLine($"Ділення на 1 видалено: {RemovedDivOne} раз(и)");
                anyImprovement = true;
            }
            if (RemovedAddZero > 0)
            {
                Console.WriteLine($"Додавання 0 видалено: {RemovedAddZero} раз(и)");
                anyImprovement = true;
            }
            if (RemovedSubZero > 0)
            {
                Console.WriteLine($"Віднімання 0 видалено: {RemovedSubZero} раз(и)");
                anyImprovement = true;
            }

            if (!anyImprovement && !ConstantFoldedDetails.Any())
            {
                Console.WriteLine("Спрощення не застосовувались.");
            }
        }
    }
}