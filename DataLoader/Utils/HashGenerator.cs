using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace DataLoader.Utils
{
    public class HashGenerator
    {
        public static string Generate(string input)
        {
            using var hashGenerator = SHA256.Create();
            var stringBuilder = new StringBuilder();

            var hash = hashGenerator.ComputeHash(Encoding.UTF8.GetBytes(input));
            hash.ToList().ForEach(x => stringBuilder.Append(x.ToString("x2")));

            return stringBuilder.ToString();
        }
    }
}