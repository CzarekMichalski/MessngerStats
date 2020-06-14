using System;

namespace DataLoader.Model
{
    public class Photo
    {
        public string Uri { get; set; }
        public long CreationTimestamp
        {
            set => Time = DateTimeOffset.FromUnixTimeMilliseconds(value).LocalDateTime;
        }
        public DateTimeOffset Time { private set; get; }
    }
}