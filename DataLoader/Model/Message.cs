using System;
using System.Collections.Generic;

namespace DataLoader.Model
{
    public class Message
    {
        private long _timestampMs;
        private DateTimeOffset _time;
        public string Content { get; set; }
        public string SenderName { get; set; }
        public long TimestampMs
        {
            get => _timestampMs;
            set
            {
                Time = DateTimeOffset.FromUnixTimeMilliseconds(value).LocalDateTime;
                _timestampMs = value;
            }
        }
        public DateTimeOffset Time
        {
            get => _time;
            set
            {
                _time = value;
                DateTimeFormat = _time.ToString("dd-MM-yyyy HH:mm");
            }
        }
        public List<Photo> Photos { get; set; }= new List<Photo>();
        public string DateTimeFormat { get; private set; }
    }
}