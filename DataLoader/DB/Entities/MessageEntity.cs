using System;
using LinqToDB.Mapping;

namespace DataLoader.DB.Entities
{
    [Table(Name = "messages")]
    public class MessageEntity
    {
        [Column(Name = "id")]
        public string Id { get; set; }
        [Column(Name = "author_id")]
        public string AuthorId { get; set; }
        [Column(Name = "content")]
        public string Content { get; set; }
        [Column(Name = "send_time")]
        public DateTimeOffset SendTime { get; set; }
        [Column(Name = "is_from_group")]
        public bool IsFromGroup { get; set; }
        [Column(Name = "conversation_id")]
        public string ConversationId { get; set; }
    }
}