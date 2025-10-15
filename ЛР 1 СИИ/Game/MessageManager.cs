namespace LR1_SAI
{
    public class MessageManager(ConsoleColor mainColor, ConsoleColor messageColor)
    {
        private readonly ConsoleColor mainColor = mainColor;
        private readonly ConsoleColor messageColor = messageColor;

        public void PrintMessage(string message)
        {
            var origColor = Console.ForegroundColor;

            Console.ForegroundColor = mainColor;
            Console.WriteLine("Программа:");

            if (message.Length > 0)
            {
                Console.ForegroundColor = messageColor;
                Console.WriteLine(message + "\n");
            }
            
            Console.ForegroundColor = origColor;
        }

        public string GetMessage()
        {
            var origColor = Console.ForegroundColor;
            
            while (true)
            {
                Console.ForegroundColor = mainColor;
                Console.WriteLine("Игрок:");

                Console.ForegroundColor = messageColor;
                var result = Console.ReadLine();

                if (!String.IsNullOrWhiteSpace(result))
                {
                    Console.WriteLine();
                    Console.ForegroundColor = origColor;
                    return result;
                }
                else
                    PrintMessage("Похоже, что вы ввели пустую строку. Повторите ввод.");
            }
        }
    }
}
