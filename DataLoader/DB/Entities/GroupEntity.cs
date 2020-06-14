using System.Collections.Generic;
using LinqToDB.Mapping;

namespace DataLoader.DB.Entities
{
    [Table(Name = "groups")]
    public class GroupEntity
    {
        [Column(Name = "id")]
        public string Id { get; set; }
        [Column(Name = "name")]
        public string Name { get; set; }
        [Column(Name = "participants_id")]
        public List<string> ParticipantsId { get; set; } = new List<string>();
    }
}