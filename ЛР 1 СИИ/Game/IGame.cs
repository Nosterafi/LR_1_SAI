using System.ComponentModel;

namespace LR1_SAI
{
    public interface IGame : INotifyPropertyChanged
    {
        public string[] Tips { get; }

        public void Run();
    }
}
