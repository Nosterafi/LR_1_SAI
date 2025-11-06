using System.Windows;
using System.Windows.Input;

namespace UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ViewModel viewModel = new();

        public MainWindow()
        {
            DataContext = viewModel;
            InitializeComponent();
        }

        private void SendButton_Click(object sender, RoutedEventArgs e) =>
            viewModel.SendMessage(messageComboBox.Text);

        private void ChatTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) =>
            chatTextBox.ScrollToEnd();

        private void messageComboBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                viewModel.SendMessage(messageComboBox.Text);
        } 
    }
}