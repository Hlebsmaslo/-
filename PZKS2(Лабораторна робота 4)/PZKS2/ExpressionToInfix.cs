using System;
using System.Collections.Generic;
using System.Linq;

namespace PZKS2
{
    static class ExpressionToInfix
    {
        private static readonly Dictionary<string, (int prec, bool rightAssoc)> opInfo =
            new Dictionary<string, (int, bool)>()
            {
                { "+", (1, false) }, { "-", (1, false) },
                { "*", (2, false) }, { "/", (2, false) }
            };

        public static string Convert(Node root)
        {
            if (root == null) return string.Empty;
            return ConvertRec(root, 0, false);
        }

        private static string ConvertRec(Node node, int parentPrecedence, bool isRightChild)
        {
            if (node is NumberNode numberNode)
            {
                return numberNode.Label;
            }
            else if (node is IdentifierNode identifierNode)
            {
                return identifierNode.Label;
            }
            else if (node is OperatorNode opNode)
            {
                if (opNode.Children.Count < 2)
                {
                    return $"[ERROR: {opNode.Op} with {opNode.Children.Count} children]";
                }

                string op = opNode.Op;
                var children = opNode.Children;

                if (!opInfo.ContainsKey(op)) return $"[UNKNOWN OP: {op}]";

                int currentPrecedence = opInfo[op].prec;
                bool isRightAssociative = opInfo[op].rightAssoc;

                if (children.Count == 2)
                {
                    string leftExpr = ConvertRec(children[0], currentPrecedence, false);
                    string rightExpr = ConvertRec(children[1], currentPrecedence, true);

                    string result = $"{leftExpr} {op} {rightExpr}";

                    if (currentPrecedence < parentPrecedence)
                    {
                        return $"({result})";
                    }
                    if (currentPrecedence == parentPrecedence)
                    {
                        if (!isRightAssociative && isRightChild) return $"({result})";
                        if (isRightAssociative && !isRightChild) return $"({result})";
                    }
                    return result;
                }

                string resultExpr = string.Join($" {op} ", children.Select(c => ConvertRec(c, currentPrecedence, false)));

                if (currentPrecedence < parentPrecedence)
                {
                    return $"({resultExpr})";
                }

                return resultExpr;
            }
            return "?";
        }
    }
}