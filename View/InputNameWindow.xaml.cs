using System.Windows;

namespace Ynost.View
{
    public partial class InputNameWindow : Window
    {
        public string EnteredName { get; private set; } = string.Empty;

        public InputNameWindow()
        {
            InitializeComponent();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            EnteredName = NameBox.Text.Trim();
            if (!string.IsNullOrEmpty(EnteredName))
            {
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Пожалуйста, введите имя.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}