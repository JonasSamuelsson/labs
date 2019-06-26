using System;
using System.Security.Cryptography;

namespace CryptoKeyGenerator
{
   public class Program
   {
      public static void Main()
      {
         foreach (var keySize in new[] { 16, 32, 64 })
         {
            using (var random = new RNGCryptoServiceProvider())
            {
               var bytes = new byte[keySize];

               random.GetBytes(bytes);

               Console.WriteLine($"{keySize} bytes ({keySize * 8}-bit): {Convert.ToBase64String(bytes)}");
            }
         }
      }
   }
}