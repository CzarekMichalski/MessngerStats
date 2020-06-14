using DataLoader.DB.Entities;
using LinqToDB;
using LinqToDB.Data;

namespace DataLoader.DB
{
    public class Db : DataConnection
    {
        public Db() : base("fb_database") { }

        public ITable<AuthorEntity> Authors => GetTable<AuthorEntity>();
        public ITable<ConversationEntity> Conversations => GetTable<ConversationEntity>();
        public ITable<GroupEntity> Groups => GetTable<GroupEntity>();
        public ITable<MessageEntity> Messages => GetTable<MessageEntity>();
        public ITable<PhotoEntity> Photos => GetTable<PhotoEntity>();

    }
}