using System.Windows;
using System.Windows.Input;
using calculator.ViewModels;

namespace calculator.Views;

/// <summary>
/// Главное окно калькулятора, связывающее WPF-события с моделью представления.
/// </summary>
public partial class MainWindow : Window
{
    /// <summary>
    /// Возвращает модель представления, назначенную окну через <see cref="FrameworkElement.DataContext"/>.
    /// </summary>
    private MainWindowViewModel ViewModel => (MainWindowViewModel)DataContext;

    /// <summary>
    /// Инициализирует компоненты окна и подключает модель представления.
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
    }

    /// <summary>
    /// Передаёт текстовый ввод с клавиатуры общей команде калькулятора.
    /// </summary>
    private void Window_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        if (ViewModel.InputCommand.CanExecute(e.Text))
        {
            ViewModel.InputCommand.Execute(e.Text);
        }
    }

    /// <summary>
    /// Преобразует служебные клавиши в команды калькулятора.
    /// </summary>
    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        var input = e.Key switch
        {
            Key.Enter or Key.Return => "Enter",
            Key.Back => "Backspace",
            Key.Escape or Key.Delete => "Escape",
            _ => null
        };

        if (input is null)
        {
            return;
        }

        ViewModel.InputCommand.Execute(input);
        e.Handled = true;
    }

    /// <summary>
    /// Перемещает окно за заголовок или меняет его размер двойным щелчком.
    /// </summary>
    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Left)
        {
            return;
        }

        if (e.ClickCount == 2)
        {
            ToggleWindowState();
            return;
        }

        DragMove();
    }

    /// <summary>
    /// Сворачивает главное окно.
    /// </summary>
    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    /// <summary>
    /// Переключает окно между обычным и развёрнутым состояниями.
    /// </summary>
    private void MaximizeButton_Click(object sender, RoutedEventArgs e)
    {
        ToggleWindowState();
    }

    /// <summary>
    /// Закрывает главное окно.
    /// </summary>
    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    /// <summary>
    /// Переключает текущее состояние окна.
    /// </summary>
    private void ToggleWindowState()
    {
        WindowState = WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
    }
}
