using calculator.Domain;

namespace calculator.Tests;

/// <summary>
/// Содержит автономные проверки вычислительного движка без внешнего тестового фреймворка.
/// </summary>
internal static class CalculatorEngineTests
{
    /// <summary>
    /// Один экземпляр движка используется во всех независимых проверках.
    /// </summary>
    private static readonly CalculatorEngine Engine = new();

    /// <summary>
    /// Запускает тестовые сценарии и возвращает код успешного завершения.
    /// </summary>
    public static int Main()
    {
        Run(
            "Сложение",
            () => AssertEqual(5m, Engine.Calculate(2m, 3m, BinaryOperation.Add)));

        Run(
            "Вычитание",
            () => AssertEqual(-1m, Engine.Calculate(2m, 3m, BinaryOperation.Subtract)));

        Run(
            "Умножение",
            () => AssertEqual(6m, Engine.Calculate(2m, 3m, BinaryOperation.Multiply)));

        Run(
            "Деление",
            () => AssertEqual(2.5m, Engine.Calculate(5m, 2m, BinaryOperation.Divide)));

        Run("Деление на ноль", AssertDivideByZero);

        Console.WriteLine("Все тесты CalculatorEngine пройдены.");
        return 0;
    }

    /// <summary>
    /// Выполняет одну проверку и выводит её название после успешного завершения.
    /// </summary>
    private static void Run(string name, Action test)
    {
        test();
        Console.WriteLine($"[OK] {name}");
    }

    /// <summary>
    /// Сравнивает ожидаемый и фактический десятичные результаты.
    /// </summary>
    private static void AssertEqual(decimal expected, decimal actual)
    {
        if (expected != actual)
        {
            throw new InvalidOperationException(
                $"Ожидалось {expected}, получено {actual}.");
        }
    }

    /// <summary>
    /// Проверяет, что движок явно сообщает о попытке деления на ноль.
    /// </summary>
    private static void AssertDivideByZero()
    {
        try
        {
            Engine.Calculate(1m, 0m, BinaryOperation.Divide);
        }
        catch (DivideByZeroException)
        {
            return;
        }

        throw new InvalidOperationException(
            "Ожидалось исключение DivideByZeroException.");
    }
}
