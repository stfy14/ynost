// File: TeacherMonitoringWindow.xaml.cs
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Ynost.Models;
using Ynost.ViewModels;
using System.Threading.Tasks;

namespace Ynost
{
    public partial class TeacherMonitoringWindow : Window
    {
        private readonly TeacherMonitoringViewModel _vm;

        public TeacherMonitoringWindow()
        {
            InitializeComponent();

            // 1) создаём VM один раз и вешаем на DataContext
            _vm = new TeacherMonitoringViewModel(App.Db);
            DataContext = _vm;
        }

        /* ------------------------  ЗАГРУЗКА ОКНА  ------------------------ */

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // подтягиваем список преподавателей
            _vm.LoadCommand.Execute(null);
        }

        /* ----------------------  ФИЛЬТР СПИСКА ФИО  ---------------------- */

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var view = CollectionViewSource.GetDefaultView(_vm.TeachList);
            if (view == null) return;

            string q = ((TextBox)sender).Text.Trim();

            view.Filter = string.IsNullOrWhiteSpace(q)
                ? null
                : o => o is Teach t &&
                       !string.IsNullOrEmpty(t.FullName) &&
                       t.FullName.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0;

            view.Refresh();
        }

        /* --------------  ПЕРЕКЛЮЧЕНИЕ МЕЖДУ ПРЕПОДАВАТЕЛЯМИ -------------- 
        увы не робит TO-DO
        */

        private void TeachList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TeachList.SelectedItem is Teach t)
                _vm.SelectTeacherCommand.Execute(t);
        }

        private async void TeachList_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction != DataGridEditAction.Commit || !(e.Row.Item is Teach editedTeach))
            {
                return;
            }

            var textBox = e.EditingElement as TextBox;
            if (textBox == null) return;
            string newFullName = textBox.Text;

            // Вызываем метод ViewModel для сохранения, если имя действительно изменилось
            if (editedTeach.FullName != newFullName)
            {
                // Используем Dispatcher, чтобы избежать проблем с потоками при ожидании
                await Dispatcher.InvokeAsync(async () =>
                {
                    await _vm.UpdateTeachNameAsync(editedTeach.Id, newFullName);
                });
            }
        }

        private void MonitoringGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction != DataGridEditAction.Commit) return;

            _vm.SaveCommand.Execute(null);          // сохраняем
            _vm.ReloadMonitoringCommand.Execute(null); // и сразу перечитываем
        }


        /* ----------  Заглушки, если понадобятся ваши дополнительные кнопки  ---------- */

        private void DataGrid_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e) { }
        private void DataGrid_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e) { }
        private void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e) { }
        private void Button_Click_1(object sender, RoutedEventArgs e) { }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
