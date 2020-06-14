using LinqToDB.Mapping;

namespace DataLoader.DB.Entities
{
    [Table(Name = "conversations")]
    public class ConversationEntity
    {
        [Column(Name = "id")]
        public string Id { get; set; }
        [Column(Name = "participant_id")]
        public string ParticipantId { get; set; }
    }
}