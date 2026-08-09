namespace calculator.Domain;

/// <summary>
/// Определяет поддерживаемые бинарные арифметические операции.
/// </summary>
public enum BinaryOperation
{
    /// <summary>
    /// Сложение двух чисел.
    /// </summary>
    Add,

    /// <summary>
    /// Вычитание правого числа из левого.
    /// </summary>
    Subtract,

    /// <summary>
    /// Умножение двух чисел.
    /// </summary>
    Multiply,

    /// <summary>
    /// Деление левого числа на правое.
    /// </summary>
    Divide
}
