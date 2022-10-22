namespace Calc.Models;

public static class OperatorChar
{
    public const char Add = '+';
    public const char Subtract = '-';
    public const char Multiply = '*';
    public const char Divide = '/';
    
    public static readonly char[] Operators = { Add, Subtract, Multiply, Divide };
    public static readonly char[] PrecedentOperators = { Multiply, Divide };
    public static readonly char[] NonPrecedentOperators = { Add, Subtract };

    public static bool IsAnOperator(char character)
    {
        foreach (var c in Operators)
        {
            if (c == character)
            {
                return true;
            }
        }

        return false;
    }
}