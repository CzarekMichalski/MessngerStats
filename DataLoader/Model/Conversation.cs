using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DataLoader.Model
{
    public class Conversation
    {
        public string Title { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public ThreadType ThreadType { get; set; }
        public List<Participant> Participants { get; set; } = new List<Participant>();
        public List<Message> Messages { get; set; } = new List<Message>();
    }
}