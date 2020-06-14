using System;
using System.IO;
using DataLoader.DB.Entities;
using DataLoader.Model;

namespace DataLoader.Utils
{
    public class TypeMapper
    {
        public static AuthorEntity MapAuthor(string name)
        {
            var author = new AuthorEntity();
            var fullName = name.Split(" ");

            author.Name = fullName.Length > 0 ? fullName[0] : "";
            author.Surname = fullName.Length > 1 ? fullName[1] : "";
            author.Id = HashGenerator.Generate(name);

            return author;
        }

        public static ConversationEntity MapConversation(Conversation conversation, string ignoreName)
        {
            var conversationEntity = new ConversationEntity();
            
            conversation.Participants.RemoveAll(x => x.Name == ignoreName);

            if (conversation.Participants.Count == 0)
            {
                return null;
            }

            conversationEntity.ParticipantId = conversation.Participants[0].Name;
            conversationEntity.Id = HashGenerator.Generate($"{conversation.Title} chat");

            return conversationEntity;
        }

        public static GroupEntity MapGroup(Conversation conversation, string ignoreName)
        {
            var group = new GroupEntity();

            conversation.Participants.RemoveAll(x => x.Name == ignoreName);
            group.Name = conversation.Title;

            foreach (var participant in conversation.Participants)
            {
                group.ParticipantsId.Add(HashGenerator.Generate(participant.Name));
            }

            group.Id = HashGenerator.Generate(group.Name);

            return group;
        }

        public static MessageEntity MapMessage(Message message, string conversationId, bool isFromGroup)
        {
            var messageEntity = new MessageEntity
            {
                Id = Guid.NewGuid().ToString("N"),
                AuthorId = HashGenerator.Generate(message.SenderName),
                Content = message.Content,
                IsFromGroup = isFromGroup,
                ConversationId = conversationId,
                SendTime = message.Time
            };

            return messageEntity;
        }

        public static PhotoEntity MapPhoto(Photo photo, string newBasePath, string authorId, string conversationId)
        {
            var photoEntity = new PhotoEntity
            {
                Id = Guid.NewGuid().ToString("N"),
                AuthorId = authorId,
                ConversationId = conversationId,
                SendTime = photo.Time,
                LocalPath = Path.Combine(newBasePath, photo.Uri.Split(@"\")[^1])
            };

            return photoEntity;
        }
    }
}