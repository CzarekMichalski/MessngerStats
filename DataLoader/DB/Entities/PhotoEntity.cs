using System;
using LinqToDB.Mapping;

namespace DataLoader.DB.Entities
{
    [Table(Name = "photos")]
    public class PhotoEntity
    {
        [Column(Name = "id")]
        public string Id { get; set; }
        [Column(Name = "author_id")]
        public string AuthorId { get; set; }
        [Column(Name = "send_time")]
        public DateTimeOffset SendTime { get; set; }
        [Column(Name = "local_path")]
        public string LocalPath { get; set; }
        [Column(Name = "conversation_id")]
        public string ConversationId { get; set; }
    }
}