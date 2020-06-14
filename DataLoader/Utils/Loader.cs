using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DataLoader.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DataLoader.Utils
{
    public class Loader
    {
        private const string ImagesPath = "Photos";
        public string ImagesOutputPath { get; set; }
        public string MessagesPath { get; set; }

        private readonly List<string> _subDirectories;

        public Loader(string messagesPath, string imagesOutputPath)
        {
            ImagesOutputPath = imagesOutputPath;
            MessagesPath = messagesPath;
            _subDirectories = Directory.GetDirectories(MessagesPath).ToList();
        }

        public List<Conversation> LoadAll()
        {
            var conversations = new List<Conversation>();

            foreach (var subDirectory in _subDirectories)
            {
                var files = Directory.GetFiles(subDirectory).ToList().FindAll(x => x.EndsWith(".json"));

                foreach (var file in files)
                {
                    try
                    {
                        var json = StringDecoder.ReadAndDecode(file);

                        var conversation = JsonConvert.DeserializeObject<Conversation>(json, new JsonSerializerSettings
                        {
                            ContractResolver = new DefaultContractResolver
                            {
                                NamingStrategy = new SnakeCaseNamingStrategy()
                            }
                        });

                        conversations.Add(conversation);
                    }
                    catch (Exception)
                    {
                        //Ignored
                    }
                }
            }

            return conversations;
        }

        public int MoveAllImages()
        {
            var imagesCounter = 0;

            foreach (var subDirectory in _subDirectories)
            {
                var photosPath = Path.Combine(subDirectory, ImagesPath);
                
                if (!Directory.Exists(photosPath))
                {
                    continue;
                }

                var files = Directory.GetFiles(photosPath);

                foreach (var file in files)
                {
                    var outputDirectoryPath = Path.Combine(ImagesOutputPath, subDirectory.Split(@"\")[^1]);

                    if (!Directory.Exists(outputDirectoryPath))
                    {
                        Directory.CreateDirectory(outputDirectoryPath);
                    }

                    var outputPath = Path.Combine(ImagesOutputPath, subDirectory.Split(@"\")[^1], file.Split(@"\")[^1]);

                    File.Move(file, outputPath);
                    imagesCounter++;
                }
            }

            return imagesCounter;
        }
    }
}