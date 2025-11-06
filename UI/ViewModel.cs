using LR1_SAI;
using System.ComponentModel;

namespace UI
{
    public class ViewModel : INotifyPropertyChanged
    {
        private readonly IGame game;
        private readonly MessageManager chat;
        
        public string Messages => chat.Messages;

        public string[] Tips => game.Tips;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ViewModel()
        {
            chat = new MessageManager();
            game = new Game(chat);

            chat.PropertyChanged += (a, b) => OnPropertyChanged(nameof(Messages));
            game.PropertyChanged += (a, b) => OnPropertyChanged(nameof(Tips));

            Task.Run(() => game.Run());
        }

        public void SendMessage(string message) =>
            chat.SendMessage("Игрок", message);

        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
