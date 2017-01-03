using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageHub.Wcf.Client
{
    [Serializable]
    public class ChatMessage
    {
        public string Username { get; set; }
        public string MessageText { get; set; }

        public DateTime? Received { get; set; }

        public override string ToString()
        {
            DateTime received = Received.GetValueOrDefault(DateTime.Now);
            return $"({received}) {Username} said: {MessageText}";
        }
    }
}
