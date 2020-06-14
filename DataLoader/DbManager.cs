using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DataLoader.DB;
using DataLoader.DB.DbConnection;
using DataLoader.DB.Entities;
using DataLoader.Model;
using DataLoader.Utils;
using DbUp;
using LinqToDB;
using LinqToDB.Data;

namespace DataLoader
{
    public class DbManager
    {
        private readonly Loader _loader;
        private readonly string _ownerName;
        private readonly Dictionary<string, string> _alreadyAddedAuthors = new Dictionary<string, string>();
        private readonly Dictionary<string, string> _alreadyAddedConversations = new Dictionary<string, string>();

        public DbManager(string connectionString = null)
        {
            var postgresSettings = new PostgresSettings();

            if (connectionString != null)
            {
                postgresSettings.PostgresConnectionString = connectionString;
            }

            DataConnection.DefaultSettings = postgresSettings;

            UpgradeDatabase(postgresSettings.PostgresConnectionString);
        }

        public DbManager(string messagesPath, string ownerName, string newImagesPath = null, string connectionString = null)
        {
            var postgresSettings = new PostgresSettings();

            if (connectionString != null)
            {
                postgresSettings.PostgresConnectionString = connectionString;
            }

            DataConnection.DefaultSettings = postgresSettings;

            UpgradeDatabase(postgresSettings.PostgresConnectionString);

            _loader = new Loader(messagesPath, newImagesPath);
            _ownerName = ownerName;

            InsertOwner();
        }

        public int Run()
        {
            var conversations = _loader.LoadAll();

            var messagesCount = conversations.Sum(conversation => conversation.Messages.Count);

            InsertRange(conversations);

            if (_loader.ImagesOutputPath != null)
            {
               _loader.MoveAllImages();
            }

            return messagesCount;
        }

        private void InsertRange(IEnumerable<Conversation> conversations)
        {
            using var database = new Db();

            foreach (var conversation in conversations)
            {
                Insert(conversation, database);
            }
        }

        private void Insert(Conversation conversation, Db database = null)
        {
            if (database == null)
            {
                database = new Db();
            }

            var isGroup = conversation.ThreadType == ThreadType.RegularGroup;
            var conversationId = "";

            if (isGroup)
            {
                var conversationEntity = TypeMapper.MapGroup(conversation, _ownerName);
                conversationId = conversationEntity.Id;
                var canAdd = _alreadyAddedConversations.TryAdd(conversationEntity.Id, conversation.Title);

                if (canAdd)
                {
                    try
                    {
                        database.Insert(conversationEntity, "groups");
                    }
                    catch (Exception)
                    {
                        //Ignored
                    }
                }
            }
            else
            {
                var conversationEntity = TypeMapper.MapConversation(conversation, _ownerName);
                
                if (conversationEntity == null)
                {
                    return;
                }

                conversationId = conversationEntity.Id;
                var canAdd = _alreadyAddedConversations.TryAdd(conversationEntity.Id, conversation.Title);

                if (canAdd)
                {
                    try
                    {
                        database.Insert(conversationEntity, "conversations");
                    }
                    catch (Exception)
                    {
                        //Ignored
                    }
                }
            }
            
            AddParticipants(database, conversation.Participants);
            AddMessages(database, conversation.Messages, conversationId, isGroup);
        }

        private void AddMessages(IDataContext database, IEnumerable<Message> messages, string conversationId, bool isFromGroup)
        {
            foreach (var message in messages)
            {
                var messageEntity = TypeMapper.MapMessage(message, conversationId, isFromGroup);

                database.Insert(messageEntity, tableName: "messages");


                if (_loader.ImagesOutputPath == null)
                {
                    continue;
                }

                foreach (var photo in message.Photos)
                {
                    AddPhoto(database, photo, message.SenderName, conversationId);
                }
            }
        }

        private void AddParticipants(IDataContext database, List<Participant> participants)
        {
            foreach (var participant in participants)
            {
                var authorEntity = TypeMapper.MapAuthor(participant.Name);

                var canAdd = _alreadyAddedAuthors.TryAdd(authorEntity.Id, participant.Name);

                if (canAdd)
                {
                    try
                    {
                        database.Insert(authorEntity, tableName: "authors");

                    }
                    catch (Exception)
                    {
                        //Ignored
                    }
                }
            }
        }

        private void AddPhoto(IDataContext database, Photo photo, string authorId, string conversationId)
        {
            var photoEntity = TypeMapper.MapPhoto(photo, _loader.ImagesOutputPath, authorId, conversationId);

            database.Insert(photoEntity, tableName: "photos");
        }

        public List<MessageEntity> GetMessages(bool fromGroup, string conversationName, DateTime? from = null, DateTime? to = null, string keyword = null)
        {
            using var database = new Db();
            IQueryable<MessageEntity> query;

            if (to == null)
            {
                to = DateTime.Today;
            }

            if (fromGroup)
            {
                query = from m in database.Messages
                    join g in database.Groups on m.ConversationId equals g.Id
                    where g.Name == conversationName
                    where m.SendTime < to
                    select m;
            }
            else
            {
                query = from m in database.Messages
                    join c in database.Conversations on m.ConversationId equals c.Id
                    where c.ParticipantId == conversationName
                    where m.SendTime < to
                    select m;
            }

            var messages = query.ToList();

            if (from != null)
            {
                messages = messages.Where(x => x.SendTime >= from).ToList();
            }

            if (keyword != null)
            {
                messages = messages.FindAll(x => x.Content != null).FindAll(x => x.Content.Contains(keyword));
            }

            return messages;
        }

        public List<string> GetAllConversations()
        {
            var conversations = new List<string>();

            conversations.AddRange(GetGroups());
            conversations.AddRange(GetPrivateConversations());

            return conversations;
        }

        public List<string> GetGroups()
        {
            var conversations = new List<string>();

            using var database = new Db();

            var groupsQuery = from g in database.Groups
                select g.Name;

            conversations.AddRange(groupsQuery.ToList());

            return conversations;
        }

        public List<string> GetPrivateConversations()
        {
            var conversations = new List<string>();

            using var database = new Db();

            var privateConversationsQuery = from c in database.Conversations
                select c.ParticipantId;

            conversations.AddRange(privateConversationsQuery.ToList());

            return conversations;
        }

        public void DeleteAllMessages()
        {
            using var database = new Db();

            database.Groups.Delete();
            database.Photos.Delete();
            database.Authors.Delete();
            database.Messages.Delete();
            database.Conversations.Delete();
        }

        public List<Message> GetAllMessages(DateTime? from = null, DateTime? to = null, string keyword = null)
        {
            using var database = new Db();

            if (to == null)
            {
                to = DateTime.Today;
            }

            var query = from m in database.Messages
                where m.SendTime < to
                select m;

            var messageEntities = query.ToList();
            var messages = new List<Message>();

            if (keyword != null)
            {
                messages = messages.FindAll(x => x.Content != null).FindAll(x => x.Content.Contains(keyword));
            }

            foreach (var messageEntity in messageEntities)
            {
                var authorName = database.Authors.FirstOrDefault(x => x.Id == messageEntity.AuthorId);

                var message = new Message
                {
                    Content = messageEntity.Content,
                    Time = messageEntity.SendTime,
                    SenderName = $"{authorName?.Name} {authorName?.Surname}"
                };

                messages.Add(message);
            }

            return messages;
        }

        public List<Message> MessageEntityToMessage(List<MessageEntity> messageEntities)
        {
            using var database = new Db();
            var messages = new List<Message>();

            foreach (var messageEntity in messageEntities)
            {
                var authorName = database.Authors.FirstOrDefault(x => x.Id == messageEntity.AuthorId);

                var message = new Message
                {
                    Content = messageEntity.Content,
                    Time = messageEntity.SendTime,
                    SenderName = $"{authorName?.Name} {authorName?.Surname}"
                };

                messages.Add(message);
            }

            return messages;
        }

        public (DateTime, DateTime) GetDateRange()
        {
            using var database = new Db();

            var from = database.Messages.Min(x => x.SendTime);
            var to = database.Messages.Max(x => x.SendTime);

            return (from.LocalDateTime, to.LocalDateTime);
        }

        private void InsertOwner()
        {
            using var db = new Db();
            var authorEntity = new AuthorEntity{
                Name = _ownerName.Split(" ")[0],
                Surname = _ownerName.Split(" ")[1],
                Id = HashGenerator.Generate(_ownerName)
            };

            try
            {
                db.Insert(authorEntity, "authors");
            }
            catch (Exception)
            {
                //Ignored
            }
        }

        private void UpgradeDatabase(string connectionString)
        {
            EnsureDatabase.For.PostgresqlDatabase(connectionString);

            var upgradeEngine = DeployChanges.To.PostgresqlDatabase(connectionString)
                .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
                .Build();

            upgradeEngine.PerformUpgrade();
        }
    }
}