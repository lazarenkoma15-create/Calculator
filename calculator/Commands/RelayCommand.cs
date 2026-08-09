using System.Windows.Input;

namespace calculator.Commands;

/// <summary>
/// Связывает команду интерфейса с переданными действиями выполнения и проверки доступности.
/// </summary>
public sealed class RelayCommand : ICommand
{
    /// <summary>
    /// Действие, которое выполняется при вызове команды.
    /// </summary>
    private readonly Action<object?> _execute;

    /// <summary>
    /// Необязательное условие, определяющее доступность команды.
    /// </summary>
    private readonly Predicate<object?>? _canExecute;

    /// <summary>
    /// Инициализирует команду указанными обработчиками.
    /// </summary>
    /// <param name="execute">Действие, выполняемое командой.</param>
    /// <param name="canExecute">Условие доступности команды.</param>
    public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    /// <summary>
    /// Возникает, когда WPF повторно проверяет доступность команды.
    /// </summary>
    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    /// <summary>
    /// Проверяет, можно ли выполнить команду с указанным параметром.
    /// </summary>
    /// <param name="parameter">Параметр команды.</param>
    /// <returns><see langword="true"/>, если команда доступна.</returns>
    public bool CanExecute(object? parameter)
    {
        return _canExecute?.Invoke(parameter) ?? true;
    }

    /// <summary>
    /// Выполняет связанное с командой действие.
    /// </summary>
    /// <param name="parameter">Параметр команды.</param>
    public void Execute(object? parameter)
    {
        _execute(parameter);
    }
}
