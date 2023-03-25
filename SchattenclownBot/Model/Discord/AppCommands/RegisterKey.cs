using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DisCatSharp;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.EventArgs;
using SchattenclownBot.Model.Discord.Main;

namespace SchattenclownBot.Model.Discord.AppCommands
{
   internal class RegisterKey : ApplicationCommandsModule
   {
      /// <summary>
      ///    Poke an User per command.
      /// </summary>
      /// <param name="interactionContext">The interactionContext</param>
      /// <returns></returns>
      [SlashCommand("RegisterKey" + Bot.isDevBot, "Add Twitch notifier!"), Obsolete("Obsolete")]
      public static async Task RegisterKeyCommand(InteractionContext interactionContext, [Option("Key", "Key.")] string key)
      {
         await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

         if (interactionContext.Member.Roles.All(x => (x.Permissions & Permissions.Administrator) == 0))
         {
            await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("unprvlegiert"));
            return;
         }

         await interactionContext.DeleteResponseAsync();

         DiscordComponentEmoji discordComponentEmojisPrevious = new("🔑");
         DiscordComponent[] discordComponent = new DiscordComponent[1];
         discordComponent[0] = new DiscordButtonComponent(ButtonStyle.Primary, "ClaimKey", "Claim Key!", false, discordComponentEmojisPrevious);

         string encrypted = Encrypt(key);
         await interactionContext.Channel.SendMessageAsync(new DiscordMessageBuilder().WithContent(encrypted).AddComponents(discordComponent));
      }

      [Obsolete("Obsolete")]
      private static string Encrypt(string textToEncrypt)
      {
         try
         {
            string ToReturn = "";
            string publickey = "12345678";
            string secretkey = "87654321";
            byte[] secretkeyByte =
            {
            };
            secretkeyByte = Encoding.UTF8.GetBytes(secretkey);
            byte[] publickeybyte =
            {
            };
            publickeybyte = Encoding.UTF8.GetBytes(publickey);
            MemoryStream ms = null;
            CryptoStream cs = null;
            byte[] inputbyteArray = Encoding.UTF8.GetBytes(textToEncrypt);
            using (DESCryptoServiceProvider des = new())
            {
               ms = new MemoryStream();
               cs = new CryptoStream(ms, des.CreateEncryptor(publickeybyte, secretkeyByte), CryptoStreamMode.Write);
               cs.Write(inputbyteArray, 0, inputbyteArray.Length);
               cs.FlushFinalBlock();
               ToReturn = Convert.ToBase64String(ms.ToArray());
            }

            return ToReturn;
         }
         catch (Exception ex)
         {
            throw new Exception(ex.Message, ex.InnerException);
         }
      }

      [Obsolete("Obsolete")]
      internal static async Task ButtonPressEvent(DiscordClient client, ComponentInteractionCreateEventArgs eventArgs)
      {
         switch (eventArgs.Id)
         {
            case "ClaimKey":
            {
               string decrypted = Decrypt(eventArgs.Message.Content);

               DiscordComponentEmoji discordComponentEmojisPrevious = new("🔑");
               DiscordComponent[] discordComponent = new DiscordComponent[1];
               discordComponent[0] = new DiscordButtonComponent(ButtonStyle.Primary, "ClaimKey", "Claim Key!", true, discordComponentEmojisPrevious);

               await eventArgs.Message.ModifyAsync(x => x.WithContent("Claimed").AddComponents(discordComponent));

               await eventArgs.User.SendMessageAsync(new DiscordMessageBuilder().WithContent(decrypted));

               break;
            }
         }
      }

      [Obsolete("Obsolete")]
      private static string Decrypt(string textToDecrypt)
      {
         try
         {
            string ToReturn = "";
            string publickey = "12345678";
            string secretkey = "87654321";
            byte[] privatekeyByte =
            {
            };
            privatekeyByte = Encoding.UTF8.GetBytes(secretkey);
            byte[] publickeybyte =
            {
            };
            publickeybyte = Encoding.UTF8.GetBytes(publickey);
            MemoryStream ms = null;
            CryptoStream cs = null;
            byte[] inputbyteArray = new byte[textToDecrypt.Replace(" ", "+").Length];
            inputbyteArray = Convert.FromBase64String(textToDecrypt.Replace(" ", "+"));
            using (DESCryptoServiceProvider des = new())
            {
               ms = new MemoryStream();
               cs = new CryptoStream(ms, des.CreateDecryptor(publickeybyte, privatekeyByte), CryptoStreamMode.Write);
               cs.Write(inputbyteArray, 0, inputbyteArray.Length);
               cs.FlushFinalBlock();
               Encoding encoding = Encoding.UTF8;
               ToReturn = encoding.GetString(ms.ToArray());
            }

            return ToReturn;
         }
         catch (Exception ae)
         {
            throw new Exception(ae.Message, ae.InnerException);
         }
      }
   }
}