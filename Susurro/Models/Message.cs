using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Susurro.Models
{
    public class Message (string from, string text, DateTime createdTime)
    {
        public string From { get; } = from;
        public string Text { get; } = text;
        public DateTime CreatedTime { get; } = createdTime;
    }
}
