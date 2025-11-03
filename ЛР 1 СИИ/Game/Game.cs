using System.Text;

namespace LR1_SAI
{
    public class Game : IGame
    {
        private const string savePath = "Save.json";
        private const string defaultObjName = "Холодильник";

        private readonly KnowledgeBase? knlBase;
        private readonly Dictionary<string, Action> actions;
        private readonly MessageManager messageManager;
        private readonly SaveManager saveManager = new(savePath);

        public string[] InitialRequests => [.. actions.Keys];

        public string[] ObjectsNames => knlBase.ObjectsNames;

        public Game(MessageManager messageManager)
        {
            this.messageManager = messageManager;
            saveManager = new SaveManager(savePath);
            knlBase = saveManager.ReadSave<KnowledgeBase>();

            if (knlBase == null)
            {
                SendMessage("К сожалению, при чтении сохранения произошла ошибка.\n" +
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
            SendMessage("Добро пожаловать в игру \"Угадай бытовой прибор\".");
            SendMessage(GetCommandSet());

            while (true)
            {
                var message = messageManager.ReadMessage();

                if (actions.TryGetValue(message, out Action? action))
                {
                    action.Invoke();
                    SendMessage($"Чем я теперь могу быть полезна?\n{GetCommandSet()}");
                    knlBase?.MoveTop();
                }
                else
                {
                    SendMessage("К сожалению я не поняла, что вы имели ввиду. " +
                        "Введите один из допустимых запросов.");
                    continue;
                }
            }
        }

        private void SearchAnswer()
        {
            SendMessage("Давай.");
            SendMessage("Правила таковы:\n" +
                "1) Сначала вы загадывайте бытовой прибор;\n" +
                "2) После этого я пытаюсь с помощью вопросов его угадать.\n\n" +
                "НАЧИНАЕМ!!!");

            while (true)
            {
                var programText = knlBase.CurrentType == NodeType.Object ? 
                    $"Это {knlBase.CurrentValue}?" : 
                    $"{knlBase.CurrentValue}?";

                SendMessage(programText);
                var answer = GetAnswer();

                if (answer && knlBase.CurrentType == NodeType.Object)
                {
                    SendMessage("Ура. Я выйграла.");
                    SendMessage("Вы хотите, чтобы я объяснила логику ответа?");

                    if(GetAnswer())
                        ExplainAnswer(knlBase.CurrentValue);

                    break;
                }

                if (!knlBase.TryMoveDown(answer))
                {
                    SendMessage("Сдаюсь.");
                    Train(answer);
                    break;
                } 
            }
        }

        private void PrintBase() => SendMessage(knlBase.ToString());
            
        private void PrintObjectInfo()
        {
            SendMessage("Без проблем. Назовите прибор, о котором вы хотите меня спросить.");
            var isSucces = TryGetObjectName(out string? deviceName);

            if (!isSucces)
            {
                SendMessage("К сожалению, мне ничего не известно об этом приборе.");
                return;
            }

            var objInfo = knlBase.GetNodeInfo(new Node<string>(deviceName, NodeType.Object));
            SendMessage(objInfo);
        }

        private void CheckAvailability()
        {
            SendMessage("Напишите, какой именно прибор вас интересует.");
            var isSucces = TryGetObjectName(out string? deviceName);

            if (!isSucces)
            {
                SendMessage("К сожалению, мне ничего не известно об этом приборе.");
                return;
            }

            SendMessage("Да, мне известно об этом приборе.");
            SendMessage("Хотите узнать, Что именно я о нём знаю?");

            var answer = GetAnswer();

            if (!answer)
            {
                SendMessage("Хорошо.");
                return;
            }

            SendMessage(knlBase.GetNodeInfo(new Node<string>(deviceName, NodeType.Object)));
        }

        private void ExplainAnswer(string resultAnswer)
        {
            var reasonings = knlBase
                .GetNodeInfo(new Node<string>(resultAnswer, NodeType.Object))
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Skip(1);

            var reasonongsStr = String.Join('\n', reasonings);
            SendMessage($"{reasonongsStr}\n\nСледовательно, это {resultAnswer}.");
        }

        private void Train(bool lastAnswer)
        {
            SendMessage("Какой прибор вы загадали?");
            var deviceName = messageManager.ReadMessage();

            if (knlBase.Contains(new Node<string>(deviceName, NodeType.Object)))
            {
                SendMessage($"Серьёзно? Я точно помню признаки прибора {deviceName}. " +
                    "Они не совпадают с тем, что мне говорили ранее.");

                return;
            }

            SendMessage($"Сформулируйте вопрос, который поможет распознать прибор {deviceName}.");
            var question = messageManager.ReadMessage();

            while (knlBase.Contains(new Node<string>(question, NodeType.Question)))
            {
                SendMessage("Такой вопрос в базе знаний уже присутствует. " +
                    "Сформулируйте неизвестный мне вопрос.");

                question =  messageManager.ReadMessage();
            }

            SendMessage("Подскажите правильный ответ на него: да или нет.");
            var answer = GetAnswer();

            knlBase.AddNode(question, NodeType.Question, lastAnswer);
            knlBase.TryMoveDown(lastAnswer);
            knlBase.AddNode(deviceName, NodeType.Object, answer);

            knlBase.MoveTop();
            SendMessage($"Отлично. Теперь я знаю о приборе {deviceName}. Спасибо за информацию.");
        }

        private void SendMessage(string message) =>
            messageManager.SendMessage("Программа", message);

        private bool GetAnswer()
        {
            while (true)
            {
                var answer = messageManager.ReadMessage()?.ToLower();

                if (answer != "да" && answer != "нет")
                    SendMessage("Я не поняла ваш ответ. Напишите: \"да\" или \"нет\".");
                else
                {
                    return answer == "да";
                }
            }
        }

        private bool TryGetObjectName(out string? deviceName)
        {
            var result = messageManager.ReadMessage();

            if (knlBase.Contains(new Node<string>(result, NodeType.Object)))
            {
                deviceName = result;
                return true;
            }
            else
            {
                deviceName = null;
                return false;
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
