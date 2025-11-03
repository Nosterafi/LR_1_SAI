using LR1_SAI;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace UI
{
    class ViewModel
    {
        private readonly IGame game;

        public readonly MessageManager Chat;

        public ViewModel()
        {
            Chat = new MessageManager();
            game = new Game(Chat);

            Task.Run(() => game.Run());
        }

        //TODO
        public string[] GetTips()
        {
            var messages = Chat.Messages;

            if (messages[] == "Давай сыграем." || lastAiMessage == "Вы хотите, чтобы я объяснила логику ответа?")
            {
                return ["да", "нет"];
            }
            else if (lastAiMessage == "Я хочу узнать, что ты знаешь об одном из приборов." ||
                lastAiMessage == "Я хочу узнать, известно ли тебе о нужном мне приборе.")
            {
                return game.ObjectsNames;
            }
            else if (lastAiMessage == "Какой прибор вы загадали?")
            {
                return [];
            }
            else if (lastAiMessage == "Чем я теперь могу быть полезна?")
            {
                return game.InitialRequests;
            }
            else 
        }
    }
}
