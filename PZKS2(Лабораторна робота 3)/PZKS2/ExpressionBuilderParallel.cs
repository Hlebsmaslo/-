using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace PZKS2
{
    static class ExpressionBuilderParallel
    {
        private static readonly Dictionary<string, (int prec, bool rightAssoc)> opInfo =
            new Dictionary<string, (int, bool)>()
            {
                { "+", (1, false) }, { "-", (1, false) },
                { "*", (2, false) }, { "/", (2, false) }
            };

        public static Node BuildAstFromTokens(List<Token> tokens, List<ErrorDetail> errors)
        {
            var output = new Stack<Node>();
            var ops = new Stack<Token>();
            Token prev = null;

            for (int i = 0; i < tokens.Count; i++)
            {
                var t = tokens[i];

                if (prev != null)
                {
                    bool leftOperand = prev.Type == TokenType.Number || prev.Type == TokenType.Identifier || prev.Type == TokenType.CloseBracket;
                    bool rightOperand = t.Type == TokenType.Number || t.Type == TokenType.Identifier || t.Type == TokenType.OpenBracket;

                    if (leftOperand && rightOperand && t.Type == TokenType.OpenBracket)
                    {
                        errors.Add(new ErrorDetail(
                            "Синтаксична помилка: відсутній бінарний оператор (+, -, *, /) між операндом і дужкою.",
                            t.Position,
                            t.Length));
                        prev = t;
                        continue;
                    }
                    else if (leftOperand && rightOperand && prev.Type == TokenType.CloseBracket)
                    {
                        errors.Add(new ErrorDetail(
                           "Синтаксична помилка: відсутній бінарний оператор (+, -, *, /) між дужкою і операндом.",
                           t.Position,
                           t.Length));
                        prev = t;
                        continue;
                    }
                }

                if (t.Type == TokenType.Number)
                {
                    output.Push(new NumberNode(double.Parse(t.Value)));
                }
                else if (t.Type == TokenType.Identifier)
                {
                    output.Push(new IdentifierNode(t.Value));
                }
                else if (t.Type == TokenType.Operator)
                {
                    bool isUnary = (t.Value == "-" || t.Value == "+") &&
                                 (prev == null || prev.Type == TokenType.Operator || prev.Type == TokenType.OpenBracket);

                    if (isUnary && t.Value == "-")
                    {
                        output.Push(new NumberNode(0));
                    }
                    else if (isUnary && t.Value == "+")
                    {
                        prev = t;
                        continue;
                    }

                    if (!isUnary && prev != null && prev.Type == TokenType.Operator)
                    {
                        errors.Add(new ErrorDetail(
                            $"Синтаксична помилка: невірне розміщення оператора {t.Value} (очікувався операнд).",
                            t.Position,
                            t.Length));
                        prev = t;
                        continue;
                    }

                    while (ops.Count > 0 && ops.Peek().Type == TokenType.Operator)
                    {
                        var top = ops.Peek();
                        if (!opInfo.ContainsKey(t.Value) || !opInfo.ContainsKey(top.Value)) break;

                        var curInfo = opInfo[t.Value];
                        var topInfo = opInfo[top.Value];

                        if ((!curInfo.rightAssoc && curInfo.prec <= topInfo.prec) ||
                            (curInfo.rightAssoc && curInfo.prec < topInfo.prec))
                        {
                            try
                            {
                                ApplyOperatorForParallelism(output, ops.Pop().Value);
                            }
                            catch (Exception ex)
                            {
                                errors.Add(new ErrorDetail(ex.Message, t.Position, t.Length));
                                ops.Clear();
                                break;
                            }
                        }
                        else break;
                    }
                    ops.Push(t);
                }
                else if (t.Type == TokenType.OpenBracket)
                {
                    if (prev != null && (prev.Type == TokenType.Number || prev.Type == TokenType.Identifier || prev.Type == TokenType.CloseBracket))
                    {
                        errors.Add(new ErrorDetail(
                            "Синтаксична помилка: відсутній бінарний оператор (+, -, *, /) перед відкритою дужкою.",
                            t.Position,
                            t.Length));
                    }
                    ops.Push(t);
                }
                else if (t.Type == TokenType.CloseBracket)
                {
                    while (ops.Count > 0 && ops.Peek().Type != TokenType.OpenBracket)
                    {
                        try
                        {
                            ApplyOperatorForParallelism(output, ops.Pop().Value);
                        }
                        catch (Exception ex)
                        {
                            errors.Add(new ErrorDetail(ex.Message, t.Position, t.Length));
                            ops.Clear();
                            break;
                        }
                    }

                    if (ops.Count > 0 && ops.Peek().Type == TokenType.OpenBracket)
                    {
                        ops.Pop();
                    }
                    else
                    {
                        errors.Add(new ErrorDetail("Неправильна кількість дужок: відсутня відкриваюча дужка.", t.Position, t.Length));
                    }
                }
                else if (t.Type == TokenType.Unknown)
                {
                    errors.Add(new ErrorDetail($"Невідомий символ: {t.Value}", t.Position, t.Length));
                }

                prev = t;
            }

            if (prev != null && (prev.Type == TokenType.Operator || prev.Type == TokenType.OpenBracket))
            {
                errors.Add(new ErrorDetail($"Некоректний кінець виразу: очікувався операнд/дужка, знайдено {prev.Value}.", prev.Position, prev.Length));
            }

            while (ops.Count > 0)
            {
                var opT = ops.Pop();
                if (opT.Type == TokenType.OpenBracket)
                {
                    errors.Add(new ErrorDetail("Некоректна кількість дужок: бракує 1 закритої дужки.", opT.Position, opT.Length));
                }
                else
                {
                    try
                    {
                        ApplyOperatorForParallelism(output, opT.Value);
                    }
                    catch (Exception ex)
                    {
                        errors.Add(new ErrorDetail(ex.Message, opT.Position, opT.Length));
                    }
                }
            }

            if (errors.Any()) return null;

            if (output.Count != 1)
            {
                errors.Add(new ErrorDetail("Помилка побудови дерева. Очікувався один кінцевий результат.",
                    0,
                    tokens.LastOrDefault()?.Position + tokens.LastOrDefault()?.Length ?? 1));
                return null;
            }

            Node root = output.Pop();

            try
            {
                root = ConstantFoldingAndSimplification(root);
                root = ApplyCommutativeSorting(root); 
                root = OptimizeParallelism(root);
            }
            catch (Exception)
            {
                return null;
            }

            return root;
        }

        private static void ApplyOperatorForParallelism(Stack<Node> output, string op)
        {
            if (output.Count < 2) throw new Exception($"Відсутні операнди для оператора {op}.");

            var right = output.Pop();
            var left = output.Pop();

            if (op == "+" || op == "*")
            {
                var nodes = new List<Node>();
                OperatorNode onLeft = left as OperatorNode;
                if (onLeft != null && onLeft.Op == op)
                    nodes.AddRange(onLeft.Children);
                else
                    nodes.Add(left);
                OperatorNode onRight = right as OperatorNode;
                if (onRight != null && onRight.Op == op)
                    nodes.AddRange(onRight.Children);
                else
                    nodes.Add(right);

                var newNode = new OperatorNode(op);
                newNode.Children.AddRange(nodes);
                output.Push(newNode);
            }
            else if (op == "-" || op == "/")
            {
                var node = new OperatorNode(op);
                node.Children.Add(left);
                node.Children.Add(right);
                output.Push(node);
            }
            else
            {
                var node = new OperatorNode(op);
                node.Children.Add(left);
                node.Children.Add(right);
                output.Push(node);
            }
        }

        public static Node ConstantFoldingAndSimplification(Node root)
        {
            OperatorNode opNode = root as OperatorNode;
            if (opNode == null) return root;
            for (int i = 0; i < opNode.Children.Count; i++)
            {
                opNode.Children[i] = ConstantFoldingAndSimplification(opNode.Children[i]);
            }

            if (opNode.Op == "+" || opNode.Op == "*")
            {
                double constantValue = (opNode.Op == "+") ? 0.0 : 1.0;
                var nonConstantChildren = new List<Node>();
                var constantChildren = new List<NumberNode>();

                foreach (var child in opNode.Children)
                {
                    NumberNode num = child as NumberNode;
                    if (num != null)
                    {
                        if (opNode.Op == "*")
                        {
                            if (Math.Abs(num.Value - 1.0) < 1e-9)
                            {
                                SimplificationCounter.RemovedMulOne++;
                                continue;
                            }
                            constantValue *= num.Value;
                        }
                        else if (opNode.Op == "+")
                        {
                            if (Math.Abs(num.Value - 0.0) < 1e-9)
                            {
                                SimplificationCounter.RemovedAddZero++;
                                continue;
                            }
                            constantValue += num.Value;
                        }
                        constantChildren.Add(num);
                    }
                    else
                    {
                        nonConstantChildren.Add(child);
                    }
                }

                if (constantChildren.Count > 1)
                {
                    string oldExpr = string.Join(opNode.Op, constantChildren.Select(n => n.Value.ToString()));
                    if ((opNode.Op == "+" && Math.Abs(constantValue - 0.0) > 1e-9) || (opNode.Op == "*" && Math.Abs(constantValue - 1.0) > 1e-9))
                    {
                        SimplificationCounter.ConstantFoldedDetails.Add($"  -> Згортання ({opNode.Op}): {oldExpr} = {constantValue}");
                    }
                }

                if (opNode.Op == "*" && Math.Abs(constantValue - 0.0) < 1e-9)
                {
                    SimplificationCounter.RemovedMulZero++;
                    return new NumberNode(0);
                }

                if (!nonConstantChildren.Any())
                    return new NumberNode(constantValue);

                bool constantAdded = false;
                if (opNode.Op == "+" && Math.Abs(constantValue - 0.0) > 1e-9)
                {
                    nonConstantChildren.Add(new NumberNode(constantValue));
                    constantAdded = true;
                }
                else if (opNode.Op == "*" && Math.Abs(constantValue - 1.0) > 1e-9)
                {
                    nonConstantChildren.Add(new NumberNode(constantValue));
                    constantAdded = true;
                }

                if (nonConstantChildren.Count == 1) return nonConstantChildren.First();

                opNode.Children = nonConstantChildren;
                return opNode;
            }
            if (opNode.Children.Count == 2)
            {
                var left = opNode.Children[0];
                var right = opNode.Children[1];

                NumberNode leftNum = left as NumberNode;
                NumberNode rightNum = right as NumberNode;

                if (opNode.Op == "/" && rightNum != null && rightNum.Value == 0.0)
                {
                    return new NumberNode(0);
                }

                if (leftNum != null && rightNum != null)
                {
                    double result;
                    switch (opNode.Op)
                    {
                        case "+": result = leftNum.Value + rightNum.Value; break;
                        case "-": result = leftNum.Value - rightNum.Value; break;
                        case "*": result = leftNum.Value * rightNum.Value; break;
                        case "/": result = leftNum.Value / rightNum.Value; break;
                        default: throw new InvalidOperationException($"Невідомий оператор: {opNode.Op}");
                    }
                    SimplificationCounter.ConstantFoldedDetails.Add($"  -> Згортання ({opNode.Op}): {leftNum.Value} {opNode.Op} {rightNum.Value} = {result}");
                    return new NumberNode(result);
                }
                if (opNode.Op == "/" && rightNum != null && rightNum.Value == 1)
                {
                    SimplificationCounter.RemovedDivOne++;
                    return left;
                }
                if (opNode.Op == "*")
                {
                    if (leftNum != null && leftNum.Value == 1) { SimplificationCounter.RemovedMulOne++; return right; }
                    if (rightNum != null && rightNum.Value == 1) { SimplificationCounter.RemovedMulOne++; return left; }
                    if (leftNum != null && leftNum.Value == 0) { SimplificationCounter.RemovedMulZero++; return new NumberNode(0); }
                    if (rightNum != null && rightNum.Value == 0) { SimplificationCounter.RemovedMulZero++; return new NumberNode(0); }
                }

                if (opNode.Op == "+")
                {
                    if (leftNum != null && leftNum.Value == 0) { SimplificationCounter.RemovedAddZero++; return right; }
                    if (rightNum != null && rightNum.Value == 0) { SimplificationCounter.RemovedAddZero++; return left; }
                }

                if (opNode.Op == "-" && rightNum != null && rightNum.Value == 0)
                {
                    SimplificationCounter.RemovedSubZero++;
                    return left;
                }
            }

            return opNode;
        }

        public static Node OptimizeParallelism(Node root)
        {
            OperatorNode opNode = root as OperatorNode;
            if (opNode == null) return root;

            for (int i = 0; i < opNode.Children.Count; i++)
            {
                opNode.Children[i] = OptimizeParallelism(opNode.Children[i]);
            }

            if (opNode.Children.Count > 2)
            {
                if (opNode.Op == "+" || opNode.Op == "*")
                {
                    return BuildParallelTree(opNode.Op, opNode.Children);
                }
                else if (opNode.Op == "-")
                {
                    var firstOperand = opNode.Children[0];
                    var remainingOperands = opNode.Children.Skip(1).ToList();
                    var sumNode = BuildParallelTree("+", remainingOperands);
                    var minusRoot = new OperatorNode("-");
                    minusRoot.Children.Add(firstOperand);
                    minusRoot.Children.Add(sumNode);
                    return minusRoot;
                }
                else if (opNode.Op == "/")
                {
                    var firstOperand = opNode.Children[0];
                    var remainingOperands = opNode.Children.Skip(1).ToList();
                    var prodNode = BuildParallelTree("*", remainingOperands);
                    var divRoot = new OperatorNode("/");
                    divRoot.Children.Add(firstOperand);
                    divRoot.Children.Add(prodNode);
                    return divRoot;
                }
            }
            return opNode;
        }

        private static Node BuildParallelTree(string op, List<Node> operands)
        {
            if (operands.Count <= 1) return operands.FirstOrDefault();

            if (operands.Count == 2)
            {
                var node = new OperatorNode(op);
                node.Children.Add(operands[0]);
                node.Children.Add(operands[1]);
                return node;
            }

            int half = operands.Count / 2;
            var leftGroup = operands.Take(half).ToList();
            var rightGroup = operands.Skip(half).ToList();

            var leftSubtree = BuildParallelTree(op, leftGroup);
            var rightSubtree = BuildParallelTree(op, rightGroup);

            var root = new OperatorNode(op);
            root.Children.Add(leftSubtree);
            root.Children.Add(rightSubtree);

            return root;
        }

        private static int GetNodeComplexity(Node node)
        {
            if (node is NumberNode) return 1;
            if (node is IdentifierNode) return 2;
            if (node is OperatorNode opNode)
            {
                return 3 + opNode.Children.Sum(GetNodeComplexity);
            }
            return 100; 
        }

        public static Node ApplyCommutativeSorting(Node root)
        {
            OperatorNode opNode = root as OperatorNode;
            if (opNode == null) return root;

            for (int i = 0; i < opNode.Children.Count; i++)
            {
                opNode.Children[i] = ApplyCommutativeSorting(opNode.Children[i]);
            }

            if (opNode.Op == "+" || opNode.Op == "*")
            {
                opNode.Children.Sort((n1, n2) => GetNodeComplexity(n1).CompareTo(GetNodeComplexity(n2)));
            }

            return opNode;
        }
        public static string NodeToExpression(Node node)
        {
            if (node is NumberNode numNode) return numNode.Value.ToString();
            if (node is IdentifierNode idNode) return idNode.Name;
            if (node is OperatorNode opNode)
            {
                if (opNode.Children.Count == 0) return opNode.Op;
                if (opNode.Op == "+" || opNode.Op == "*")
                {

                    var parts = opNode.Children.Select(child =>
                    {
                        string childExpr = NodeToExpression(child);
                        if (child is OperatorNode childOpNode && childOpNode.Children.Count > 1)
                        {
                            var childPrec = opInfo.ContainsKey(childOpNode.Op) ? opInfo[childOpNode.Op].prec : 0;
                            var currentPrec = opInfo.ContainsKey(opNode.Op) ? opInfo[opNode.Op].prec : 0;
                            if (childPrec < currentPrec || (childPrec == currentPrec && childOpNode.Op != opNode.Op))
                            {
                                return $"({childExpr})";
                            }
                        }
                        return childExpr;
                    });

                    return string.Join($" {opNode.Op} ", parts);
                }
                if (opNode.Children.Count == 2)
                {
                    var left = opNode.Children[0];
                    var right = opNode.Children[1];

                    string leftExpr = NodeToExpression(left);
                    string rightExpr = NodeToExpression(right);
                    if (left is OperatorNode leftOpNode && leftOpNode.Children.Count > 1)
                    {
                        var leftPrec = opInfo.ContainsKey(leftOpNode.Op) ? opInfo[leftOpNode.Op].prec : 0;
                        var currentPrec = opInfo.ContainsKey(opNode.Op) ? opInfo[opNode.Op].prec : 0;
                        if (leftPrec < currentPrec)
                        {
                            leftExpr = $"({leftExpr})";
                        }
                    }

                    if (right is OperatorNode rightOpNode && rightOpNode.Children.Count > 1)
                    {
                        var rightPrec = opInfo.ContainsKey(rightOpNode.Op) ? opInfo[rightOpNode.Op].prec : 0;
                        var currentPrec = opInfo.ContainsKey(opNode.Op) ? opInfo[opNode.Op].prec : 0;
                        if (rightPrec <= currentPrec)
                        {
                            rightExpr = $"({rightExpr})";
                        }
                    }

                    return $"{leftExpr} {opNode.Op} {rightExpr}";
                }
            }
            return node.Label;
        }
    }
}