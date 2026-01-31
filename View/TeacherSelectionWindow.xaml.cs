using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace Ynost.View
{
    public partial class TeacherSelectionWindow : Window
    {
        public string SelectedName { get; private set; } = string.Empty;

        public TeacherSelectionWindow(IEnumerable<string> candidates)
        {
            InitializeComponent();
            TeachersList.ItemsSource = candidates;
            TeachersList.SelectedIndex = 0; // Выбираем первого по умолчанию
        }

        private void Select_Click(object sender, RoutedEventArgs e)
        {
            ConfirmSelection();
        }

        private void TeachersList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ConfirmSelection();
        }

        private void ConfirmSelection()
        {
            if (TeachersList.SelectedItem is string name)
            {
                SelectedName = name;
                DialogResult = true;
                Close();
            }
        }
    }
}