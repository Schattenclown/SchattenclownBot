using System.Security.Cryptography;
using System.Text;

namespace SchattenclownBot.Model.HelpClasses
{
   public class Sha256FromString
   {
      public static string ComputeSha256Hash(string rawData)
      {
         // Create a SHA256   
         using SHA256 sha256Hash = SHA256.Create();
         // ComputeHash - returns byte array
         byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

         // Convert byte array to a string
         StringBuilder builder = new();
         foreach (byte t in bytes)
         {
            builder.Append(t.ToString("x2"));
         }

         return builder.ToString();
      }
   }
}