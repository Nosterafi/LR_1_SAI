namespace LR1_SAI
{
    public interface IGame
    {
        public string[] InitialRequests { get; }

        public string[] ObjectsNames { get; }

        public void Run();
    }
}
