using System.Text;

namespace LR1_SAI
{
    public class Game
    {
        private const string savePath = "Save.json";
        private const string defaultObjName = "Холодильник";
        private const ConsoleColor mainColor = ConsoleColor.Green;
        private const ConsoleColor messageColor = ConsoleColor.Yellow;

        private readonly KnowledgeBase? knlBase;
        private readonly Dictionary<string, Action> actions;
        private readonly SaveManager saveManager;
        private readonly MessageManager messageManager;

        public Game()
        {
            saveManager = new SaveManager(savePath);
            messageManager = new MessageManager(mainColor, messageColor);

            knlBase = saveManager.ReadSave<KnowledgeBase>();
            if (knlBase == null)
            {
                messageManager.PrintMessage("К сожалению, при чтении сохранения произошла ошибка.\n" +
                    "Сейчас используется база знаний по-умолчанию.");
                knlBase = new KnowledgeBase(defaultObjName);
            }
            
            knlBase.MoveTop();
            actions = new()
            {
                { "Давай сыграем.", SearchAnswer },
                { "Я хочу узнать, что ты знаешь об одном из приборов.", PrintObjectInfo },
                { "Выведи всю базу знаний на экран.", PrintBase },
                { "Я хочу узнать, известно ли тебе о нужном мне приборе.", CheckAvailability },
            };

            AppDomain.CurrentDomain.ProcessExit += ( sender, eventArgs ) => saveManager.Save(knlBase);
        }

        public void Run()
        {
            messageManager.PrintMessage("Добро пожаловать в игру \"Угадай бытовой прибор\".");
            messageManager.PrintMessage(GetCommandSet());

            while (true)
            {
                var message = messageManager.GetMessage();

                if (actions.TryGetValue(message, out Action? action))
                {
                    action.Invoke();
                    messageManager.PrintMessage($"Чем я теперь могу быть полезна?\n{GetCommandSet()}");
                    knlBase?.MoveTop();
                }
                else
                {
                    messageManager.PrintMessage("К сожалению я не поняла, что вы имели ввиду. " +
                        "Введите один из допустимых запросов.");
                    continue;
                }
            }
        }

        private void SearchAnswer()
        {
            messageManager.PrintMessage("Давай.");
            messageManager.PrintMessage("Правила таковы:\n" +
                "1) Сначала вы загадывайте бытовой прибор;\n" +
                "2) После этого я пытаюсь с помощью вопросов его угадать.\n\n" +
                "Нажмите любую кнопку клавиатуры, если уже загадали прибор.");
            Console.ReadKey();
            messageManager.PrintMessage("Отлично. Начинаем.");

            while (true)
            {
                var programText = knlBase.CurrentType == NodeType.Object ? 
                    $"Это {knlBase.CurrentValue}?" : 
                    $"{knlBase.CurrentValue}?";

                messageManager.PrintMessage(programText);
                var answer = GetAnswer();

                if (answer && knlBase.CurrentType == NodeType.Object)
                {
                    messageManager.PrintMessage("Ура. Я выйграла.");
                    messageManager.PrintMessage("Вы хотите, чтобы я объяснила логику ответа?");

                    if(GetAnswer())
                        ExplainAnswer(knlBase.CurrentValue);

                    break;
                }

                if (!knlBase.TryMoveDown(answer))
                {
                    messageManager.PrintMessage("Сдаюсь.");
                    Train(answer);
                    break;
                } 
            }
        }

        private void PrintBase() => messageManager.PrintMessage(knlBase.ToString());
            
        private void PrintObjectInfo()
        {
            messageManager.PrintMessage("Без проблем. Назовите прибор, о котором вы хотите меня спросить.");
            var deviceName = messageManager.GetMessage();
            var objInfo = string.Empty;

            try { objInfo = knlBase.GetNodeInfo(new Node<string>(deviceName, NodeType.Object)); }
            catch (InvalidOperationException)
            {
                messageManager.PrintMessage("К сожалению, я ничего не знаю о данном приборе.");
                return;
            }

            messageManager.PrintMessage(objInfo);
            Console.WriteLine();
        }

        private void CheckAvailability()
        {
            messageManager.PrintMessage("Напишите, какой именно прибор вас интересует.");
            var deviceName = messageManager.GetMessage();

            if (!knlBase.Contains(new Node<string>(deviceName, NodeType.Object)))
            {
                messageManager.PrintMessage("Я ничего не знаю об этом приборе.");
                return;
            }

            messageManager.PrintMessage("Да, мне известно об этом приборе.");
            messageManager.PrintMessage("Хотите узнать, Что именно я о нём знаю?");
            var answer = GetAnswer();

            if (!answer)
            {
                messageManager.PrintMessage("Хорошо.");
                return;
            }

            messageManager.PrintMessage(knlBase.GetNodeInfo(new Node<string>(deviceName, NodeType.Object)));
        }

        private void ExplainAnswer(string resultAnswer)
        {
            var reasonings = knlBase
                .GetNodeInfo(new Node<string>(resultAnswer, NodeType.Object))
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Skip(1);

            var reasonongsStr = String.Join('\n', reasonings);
            messageManager.PrintMessage($"{reasonongsStr}\n\n    Следовательно, это {resultAnswer}.");
        }

        private void Train(bool lastAnswer)
        {
            messageManager.PrintMessage("Какой прибор вы загадали?");
            var deviceName = messageManager.GetMessage();

            if (knlBase.Contains(new Node<string>(deviceName, NodeType.Object)))
            {
                messageManager.PrintMessage($"Серьёзно? Я точно помню признаки прибора {deviceName}. " +
                    "Они не совпадают с тем, что мне говорили ранее.");

                return;
            }

            messageManager.PrintMessage($"Сформулируйте вопрос, который поможет распознать прибор {deviceName}.");
            var question = messageManager.GetMessage();

            while (knlBase.Contains(new Node<string>(question, NodeType.Question)))
            {
                messageManager.PrintMessage("Такой вопрос в базе знаний уже присутствует. " +
                    "Сформулируйте неизвестный мне вопрос.");

                question =  messageManager.GetMessage();
            }

            messageManager.PrintMessage("Подскажите правильный ответ на него: да или нет.");
            var answer = GetAnswer();

            knlBase.AddNode(question, NodeType.Question, lastAnswer);
            knlBase.TryMoveDown(lastAnswer);
            knlBase.AddNode(deviceName, NodeType.Object, answer);

            knlBase.MoveTop();
            messageManager.PrintMessage($"Отлично. Теперь я знаю о приборе {deviceName}. Спасибо за информацию.");
        }

        private bool GetAnswer()
        {
            while (true)
            {
                var answer = messageManager.GetMessage()?.ToLower();

                if (answer == "да") return true;
                if (answer == "нет") return false;

                messageManager.PrintMessage("Я не поняла ваш ответ. Напишите: \"да\" или \"нет\".");
            }
        }

        private string GetCommandSet()
        {
            var builder = new StringBuilder("Возможные запросы:\n");

            foreach (var command in actions)
                builder.AppendLine("   " + command.Key);

            builder.Remove(builder.Length - 1, 1);
            return builder.ToString();
        }
    }
}
