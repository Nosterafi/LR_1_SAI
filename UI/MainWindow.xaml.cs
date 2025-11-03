using System.Windows;

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
            //messageComboBox.ItemsSource = viewModel.Tips;
            Console.WriteLine(viewModel.Chat.Messages);
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            ((ViewModel)DataContext).Chat.SendMessage("Игрок", messageComboBox.Text);
            messageComboBox.ItemsSource = viewModel.Tips;
            
            Console.Clear();
            Console.WriteLine(viewModel.Chat.Messages);
        }
    }
}