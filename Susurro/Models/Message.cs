using SusurroDtos;

namespace Susurro.Models
{
    public class Message(string from, string text, DateTime createdTime)
    {
        public List<string>? AllTo { get; }
        public string From { get; } = from;
        public string Text { get; } = text;
        public DateTime CreatedTime { get; } = createdTime;

        public Message(MessageDto messageDto, string plainText) : 
            this(messageDto.From, plainText, messageDto.CreateTime)
        {
            AllTo = [.. messageDto.AllTo.Split(' ')];
        }
    }
}
