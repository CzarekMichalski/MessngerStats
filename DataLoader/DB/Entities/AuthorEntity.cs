using LinqToDB.Mapping;

namespace DataLoader.DB.Entities
{
    [Table(Name = "authors")]
    public class AuthorEntity
    {
        [Column(Name = "id")]
        public string Id { get; set; }
        [Column(Name = "name")]
        public string Name { get; set; }
        [Column(Name = "surname")]
        public string Surname { get; set; }
    }
}