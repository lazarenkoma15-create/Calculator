using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using calculator.Commands;
using calculator.Domain;

namespace calculator.ViewModels;

/// <summary>
/// Хранит состояние главного окна и обрабатывает команды пользователя.
/// </summary>
public sealed class MainWindowViewModel : INotifyPropertyChanged
{
    // Ограничивает количество цифр значением, которое безопасно помещается в decimal.
    private const int MaxDisplayLength = 28;

    // Выполняет арифметические вычисления независимо от интерфейса.
    private readonly CalculatorEngine _engine;

    // Хранит левый операнд до завершения бинарной операции.
    private decimal? _leftOperand;

    // Содержит операцию, ожидающую ввода правого операнда.
    private BinaryOperation? _pendingOperation;

    // Показывает, должен ли следующий символ начать новое число.
    private bool _replaceDisplayOnNextInput = true;

    // Запрещает вычисления, пока пользователь не сбросит состояние ошибки.
    private bool _hasError;

    // Текст, который отображается на экране калькулятора.
    private string _display = "0";

    /// <summary>
    /// Создаёт модель представления со стандартным вычислительным движком.
    /// </summary>
    public MainWindowViewModel()
        : this(new CalculatorEngine())
    {
    }

    /// <summary>
    /// Создаёт модель представления с указанным вычислительным движком.
    /// </summary>
    /// <param name="engine">Компонент, выполняющий арифметические операции.</param>
    public MainWindowViewModel(CalculatorEngine engine)
    {
        _engine = engine;
        InputCommand = new RelayCommand(HandleInput);
    }

    /// <summary>
    /// Уведомляет интерфейс об изменении свойства модели представления.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Возвращает текст, отображаемый на экране калькулятора.
    /// </summary>
    public string Display
    {
        get => _display;
        private set
        {
            if (_display == value)
            {
                return;
            }

            _display = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Обрабатывает ввод с кнопок интерфейса и клавиатуры.
    /// </summary>
    public ICommand InputCommand { get; }

    /// <summary>
    /// Определяет тип введённого значения и направляет его нужному обработчику.
    /// </summary>
    private void HandleInput(object? parameter)
    {
        if (parameter is not string input || string.IsNullOrWhiteSpace(input))
        {
            return;
        }

        if (input.Length == 1 && char.IsDigit(input[0]))
        {
            EnterDigit(input[0]);
            return;
        }

        switch (input)
        {
            case ".":
            case ",":
                EnterDecimalSeparator();
                break;
            case "+":
                SetOperation(BinaryOperation.Add);
                break;
            case "-":
            case "−":
                SetOperation(BinaryOperation.Subtract);
                break;
            case "*":
            case "×":
                SetOperation(BinaryOperation.Multiply);
                break;
            case "/":
            case "÷":
                SetOperation(BinaryOperation.Divide);
                break;
            case "=":
            case "Enter":
                Evaluate();
                break;
            case "%":
                ApplyPercent();
                break;
            case "+/−":
            case "+/-":
                ToggleSign();
                break;
            case "Backspace":
                Backspace();
                break;
            case "AC":
            case "Escape":
                Clear();
                break;
        }
    }

    /// <summary>
    /// Добавляет цифру к текущему числу на экране.
    /// </summary>
    private void EnterDigit(char digit)
    {
        RecoverFromError();

        if (_replaceDisplayOnNextInput || Display == "0")
        {
            Display = digit.ToString();
        }
        else if (Display.Length < MaxDisplayLength)
        {
            Display += digit;
        }

        _replaceDisplayOnNextInput = false;
    }

    /// <summary>
    /// Добавляет десятичный разделитель с учётом текущей культуры.
    /// </summary>
    private void EnterDecimalSeparator()
    {
        RecoverFromError();
        var separator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;

        if (_replaceDisplayOnNextInput)
        {
            Display = $"0{separator}";
            _replaceDisplayOnNextInput = false;
        }
        else if (!Display.Contains(separator, StringComparison.Ordinal))
        {
            Display += separator;
        }
    }

    /// <summary>
    /// Запоминает выбранную операцию и подготавливает ввод правого операнда.
    /// </summary>
    private void SetOperation(BinaryOperation operation)
    {
        if (_hasError)
        {
            return;
        }

        if (_pendingOperation.HasValue && !_replaceDisplayOnNextInput)
        {
            if (!TryEvaluate())
            {
                return;
            }
        }
        else
        {
            _leftOperand = ParseDisplay();
        }

        _pendingOperation = operation;
        _replaceDisplayOnNextInput = true;
    }

    /// <summary>
    /// Завершает ожидающую операцию и очищает сохранённые операнды.
    /// </summary>
    private void Evaluate()
    {
        if (_hasError || !_pendingOperation.HasValue || !_leftOperand.HasValue)
        {
            return;
        }

        TryEvaluate();
        _pendingOperation = null;
        _leftOperand = null;
        _replaceDisplayOnNextInput = true;
    }

    /// <summary>
    /// Пытается вычислить результат и переводит калькулятор в состояние ошибки при сбое.
    /// </summary>
    /// <returns><see langword="true"/>, если вычисление выполнено успешно.</returns>
    private bool TryEvaluate()
    {
        try
        {
            var result = _engine.Calculate(
                _leftOperand!.Value,
                ParseDisplay(),
                _pendingOperation!.Value);

            Display = Format(result);
            _leftOperand = result;

            return true;
        }
        catch (DivideByZeroException)
        {
            SetError("Ошибка: деление на ноль");
            return false;
        }
        catch (OverflowException)
        {
            SetError("Ошибка переполнения");
            return false;
        }
    }

    /// <summary>
    /// Преобразует отображаемое число в процентное значение.
    /// </summary>
    private void ApplyPercent()
    {
        if (_hasError)
        {
            return;
        }

        Display = Format(ParseDisplay() / 100m);
        _replaceDisplayOnNextInput = true;
    }

    /// <summary>
    /// Меняет знак отображаемого числа на противоположный.
    /// </summary>
    private void ToggleSign()
    {
        if (_hasError)
        {
            return;
        }

        Display = Format(-ParseDisplay());
    }

    /// <summary>
    /// Удаляет последний введённый символ или сбрасывает состояние ошибки.
    /// </summary>
    private void Backspace()
    {
        if (_hasError)
        {
            Clear();
            return;
        }

        if (_replaceDisplayOnNextInput)
        {
            return;
        }

        Display = Display.Length > 1 ? Display[..^1] : "0";

        if (Display is "-" or "−")
        {
            Display = "0";
        }
    }

    /// <summary>
    /// Возвращает калькулятор в исходное состояние.
    /// </summary>
    private void Clear()
    {
        Display = "0";
        _leftOperand = null;
        _pendingOperation = null;
        _replaceDisplayOnNextInput = true;
        _hasError = false;
    }

    /// <summary>
    /// Очищает сообщение об ошибке перед вводом нового числа.
    /// </summary>
    private void RecoverFromError()
    {
        if (_hasError)
        {
            Clear();
        }
    }

    /// <summary>
    /// Показывает сообщение об ошибке и сбрасывает незавершённую операцию.
    /// </summary>
    private void SetError(string message)
    {
        Display = message;
        _hasError = true;
        _pendingOperation = null;
        _leftOperand = null;
        _replaceDisplayOnNextInput = true;
    }

    /// <summary>
    /// Преобразует отображаемый текст в число с учётом текущей культуры.
    /// </summary>
    private decimal ParseDisplay()
    {
        return decimal.Parse(Display, NumberStyles.Number, CultureInfo.CurrentCulture);
    }

    /// <summary>
    /// Форматирует число без лишних завершающих нулей.
    /// </summary>
    private static string Format(decimal value)
    {
        return value.ToString("G29", CultureInfo.CurrentCulture);
    }

    /// <summary>
    /// Отправляет уведомление об изменении указанного свойства.
    /// </summary>
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
