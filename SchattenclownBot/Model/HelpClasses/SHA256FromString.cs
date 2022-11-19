using System;
using System.Security.Cryptography;
using System.Text;

namespace SchattenclownBot.Model.HelpClasses;

public class SHA256FromString
{
   public static string SHA256(string randomString)
   {
      SHA256Managed crypt = new();
      string hash = string.Empty;
      byte[] crypto = crypt.ComputeHash(Encoding.ASCII.GetBytes(randomString));
      foreach (byte theByte in crypto)
      {
         hash += theByte.ToString("x2");
      }

      return hash;
   }
}