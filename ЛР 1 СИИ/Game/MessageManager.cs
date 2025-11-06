using System.ComponentModel;

namespace LR1_SAI
{
    public class MessageManager : INotifyPropertyChanged
    {
        private readonly object locker = new object();

        private List<string> messages = new();
        private string? resMes;

        public string Messages => String.Join("\n\n", messages);

        public event PropertyChangedEventHandler? PropertyChanged;

        public string ReadMessage()
        {
            lock (locker)
            {
                resMes = null;

                while (resMes == null)
                {
                    Monitor.Wait(locker);
                }

                var result = resMes;
                
                resMes = null;
                return result;
            }
        }

        public void SendMessage(string senderName, string message)
        {
            lock (locker)
            {
                Monitor.Pulse(locker);
                resMes = message;
                messages.Add($"{senderName}:\n{message}\n");
                OnPropertyChanged(nameof(Messages));
            }
        }

        protected void OnPropertyChanged(string propertyName = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
