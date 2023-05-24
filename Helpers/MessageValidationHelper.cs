using Telegram.Bot.Types;

namespace BettingBot.Helpers
{
    public static class MessageValidationHelper
    {
        /// <summary>
        /// Валидация входящего сообщения.
        /// Объект сообщения сейчас не используется,
        /// но если надо будет валидировать по id отправителя, то будет удобнее внедрить.
        /// </summary>
        /// <param name="message">Объект сообщения telegram API.</param>
        public static bool ValidateIputMessage(Message message)
        {
            if (string.IsNullOrEmpty(message.Text) || string.IsNullOrWhiteSpace(message.Text))
            {
                return false;
            }
            return ValidateInputString(message.Text);
        }

        /// <summary>
        /// Валидация текста сообщения.
        /// </summary>
        /// <param name="message">Текст входящего сообщения.</param>
        /// <returns></returns>
        private static bool ValidateInputString(string message)
        {
            var splittedString = message.Split("\n");
            if (splittedString.Length != 6)
            {
                return false;
            }
            if (splittedString[1].Split(')').Length < 2)
            {
                return false;
            }

            if (splittedString[3].Split('/').Length != 4)
            {
                return false;
            }

            if (splittedString[4].Split('/').Length != 4)
            {
                return false;
            }

            return true;
        }
    }
}
