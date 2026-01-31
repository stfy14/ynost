using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Ynost.View;  // <-- здесь находится TeacherMonitoringWindow
using Ynost.ViewModels;

namespace Ynost
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _vm;
        private readonly string _logPath;
        private readonly object _logLock = new();   // защита от одновременной записи

        public MainWindow()
        {
            InitializeComponent();

            _logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ynost.log");

            // глобальные перехватчики необработанных исключений
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Application.Current.DispatcherUnhandledException += Dispatcher_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            // View‑model
            _vm = new MainViewModel();   // ← без параметров; берёт App.Db
            _vm.PropertyChanged += Vm_PropertyChanged; // пример: если нужно отслеживать ошибки внутри VM
            DataContext = _vm;

            // события окна
            Loaded += MainWindow_Loaded;
        }
        private void OnTeachersButton_Click(object sender, RoutedEventArgs e)
        {
            // Проверяем, авторизован ли пользователь
            if (!_vm.IsLoggedIn)
            {
                // Если нет — показываем информационное окно
                MessageBox.Show(
                    "Сначала войдите в аккаунт",
                    "Требуется авторизация",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            else
            {
                // Если да — создаём и показываем окно мониторинга
                var win = new TeacherMonitoringWindow
                {
                    Owner = this // Чтобы окно было дочерним
                };
                win.Show();
            }
        }
        // ------------------------------------------------------------------
        #region Глобальные обработчики ошибок
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Log($"UNHANDLED AppDomain EXCEPTION: {e.ExceptionObject}", true);
        }

        private void Dispatcher_UnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Log($"UNHANDLED Dispatcher EXCEPTION: {e.Exception}", true);
            e.Handled = true; // чтобы приложение не падало мгновенно
            MessageBox.Show("Возникла необработанная ошибка в UI. Подробности записаны в журнал.",
                            "Ynost — Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            Log($"UNHANDLED TaskScheduler EXCEPTION: {e.Exception}", true);
            e.SetObserved();
        }
        #endregion

        // ------------------------------------------------------------------
        #region Жизненный цикл окна
        private async void MainWindow_Loaded(object? sender, RoutedEventArgs e)
        {
            Log("=== Начало загрузки данных при старте окна (MainWindow_Loaded) ===");
            var sw = Stopwatch.StartNew();
            try
            {
                // MainViewModel сам решит, нужно ли грузить данные (в зависимости от IsLoggedIn)
                await _vm.LoadDataAsync();

                sw.Stop();
                Log($"Операция MainWindow_Loaded завершена за {sw.ElapsedMilliseconds} ms. Статус ViewModel: {_vm.ConnectionStatusText}");
            }
            catch (Exception ex)
            {
                sw.Stop();
                Log($"КРИТИЧЕСКАЯ ОШИБКА при вызове LoadDataAsync из MainWindow_Loaded: {ex}", true);
                MessageBox.Show($"Критическая ошибка при инициализации загрузки:\n{ex.Message}",
                                "Ynost — Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Log("=== Окончание обработки MainWindow_Loaded ===\n");
            }
        }
        #endregion

        // ------------------------------------------------------------------
        #region Поиск / фильтр
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (_vm?.Teachers == null) return;
                var view = CollectionViewSource.GetDefaultView(_vm.Teachers);
                if (view == null) return;

                string q = SearchBox.Text.Trim();
                if (string.IsNullOrWhiteSpace(q))
                {
                    view.Filter = null;
                }
                else
                {
                    view.Filter = o => o is TeacherViewModel t &&
                                        t.FullName.Contains(q, StringComparison.OrdinalIgnoreCase);
                }
                Log($"Фильтр применён: \"{q}\" (осталось {view.Cast<object>().Count()} записей)");
            }
            catch (Exception ex)
            {
                Log($"Ошибка в SearchBox_TextChanged: {ex}", true);
            }
        }
        #endregion

        // ------------------------------------------------------------------
        #region Вспомогательные методы
        private void Log(string message, bool isError = false)
        {
            try
            {
                lock (_logLock)
                {
                    File.AppendAllText(_logPath,
                        $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} {(isError ? "[ERR] " : string.Empty)}{message}{Environment.NewLine}");
                }
            }
            catch
            {
                // Подавляем ошибки логирования, чтобы не зациклить крэш
            }
        }
        #endregion

        // ------------------------------------------------------------------
        #region Обработчики прокрутки и редактирования DataGrid
        private void DataGrid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            try
            {
                if (!e.Handled && sender is UIElement && DetailsScrollViewer != null)
                {
                    DetailsScrollViewer.ScrollToVerticalOffset(DetailsScrollViewer.VerticalOffset - e.Delta);
                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                Log($"Ошибка в DataGrid_PreviewMouseWheel: {ex}", true);
            }
        }

        private void EditingTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key != Key.Enter) return;
                if (sender is not TextBox textBox) return;

                if ((Keyboard.Modifiers & ModifierKeys.Shift) != ModifierKeys.Shift)
                {
                    e.Handled = true;

                    DependencyObject parent = VisualTreeHelper.GetParent(textBox);
                    DataGrid? grid = null;
                    while (parent != null && grid == null)
                    {
                        grid = parent as DataGrid;
                        parent = VisualTreeHelper.GetParent(parent);
                    }
                    grid?.CommitEdit(DataGridEditingUnit.Row, true);
                }
            }
            catch (Exception ex)
            {
                Log($"Ошибка в EditingTextBox_PreviewKeyDown: {ex}", true);
            }
        }

        private void DataGrid_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {
            if (e.EditingElement is TextBox textBox)
            {
                textBox.PreviewKeyDown -= EditingTextBox_PreviewKeyDown;
                textBox.PreviewKeyDown += EditingTextBox_PreviewKeyDown;
            }
        }

        private void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditingElement is TextBox textBox)
            {
                textBox.PreviewKeyDown -= EditingTextBox_PreviewKeyDown;
            }
        }
        #endregion

        // ------------------------------------------------------------------
        #region Прочие заглушки (оставлены пустыми, но с try/catch для логов)
        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Существующий метод (если он был пуст, заполните его)
            if (sender is DataGrid grid && grid.SelectedItem != null)
            {
                grid.ScrollIntoView(grid.SelectedItem);
            }
        }

        private void DataGrid_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            try { /* реализация при необходимости */ }
            catch (Exception ex) { Log($"Ошибка в DataGrid_SelectionChanged_1: {ex}", true); }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try { /* реализация при необходимости */ }
            catch (Exception ex) { Log($"Ошибка в Button_Click: {ex}", true); }
        }
        #endregion

        // ------------------------------------------------------------------
        #region VM helper
        private void Vm_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // пример: если ViewModel будет публиковать ErrorOccurred, можно тут реагировать
        }
        #endregion

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {

        }
    }
}