namespace calculator.Domain;

/// <summary>
/// Выполняет арифметические операции над двумя десятичными числами.
/// </summary>
public sealed class CalculatorEngine
{
    /// <summary>
    /// Вычисляет результат выбранной бинарной операции.
    /// </summary>
    /// <param name="left">Левый операнд.</param>
    /// <param name="right">Правый операнд.</param>
    /// <param name="operation">Операция, которую нужно выполнить.</param>
    /// <returns>Результат вычисления.</returns>
    /// <exception cref="DivideByZeroException">
    /// Возникает при попытке деления на ноль.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Возникает, если передано неизвестное значение операции.
    /// </exception>
    public decimal Calculate(decimal left, decimal right, BinaryOperation operation)
    {
        return operation switch
        {
            BinaryOperation.Add => left + right,
            BinaryOperation.Subtract => left - right,
            BinaryOperation.Multiply => left * right,
            BinaryOperation.Divide when right == 0 => throw new DivideByZeroException(),
            BinaryOperation.Divide => left / right,
            _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null),
        };
    }
}
