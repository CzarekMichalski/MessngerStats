using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace DataLoader.Utils
{
    public class StringDecoder
    {
        public static string ReadAndDecode(string path)
        {
            using var streamReader = new StreamReader(path);
            var text = streamReader.ReadToEnd();
            
            foreach (var (key, value) in ReplaceCharsetDictionary)
            {
                text = text.Replace(key, value);
            }

            text = Regex.Replace(text, @"\\u00[a-f|\d][a-f|\d]", string.Empty);

            return text;
        }

        private static readonly Dictionary<string, string> ReplaceCharsetDictionary = new Dictionary<string, string>
        {
            {@"\u00c4\u0085", "ą"},
            {@"\u00c4\u0087", "ć"},
            {@"\u00c4\u0099", "ę"},
            {@"\u00c5\u0082", "ł"},
            {@"\u00c5\u0084", "ń"},
            {@"\u00c3\u00b3", "ó"},
            {@"\u00c5\u009b", "ś"},
            {@"\u00c5\u00ba", "ź"},
            {@"\u00c5\u00bc", "ż"},
            {@"\u00c4\u0084", "Ą"},
            {@"\u00c4\u0086", "Ć"},
            {@"\u00c4\u0098", "Ę"},
            {@"\u00c5\u0081", "Ł"},
            {@"\u00c5\u0083", "Ń"},
            {@"\u00c3\u00b2", "Ó"},
            {@"\u00c5\u009a", "Ś"},
            {@"\u00c5\u00b9", "Ź"},
            {@"\u00c5\u00bb", "Ż"},
        };
    }
}