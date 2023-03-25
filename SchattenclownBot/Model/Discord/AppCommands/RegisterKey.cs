﻿using System;
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

#pragma warning disable SYSLIB0021

namespace SchattenclownBot.Model.Discord.AppCommands
{
   internal class RegisterKey : ApplicationCommandsModule
   {
      /// <summary>
      ///    Poke an User per command.
      /// </summary>
      /// <param name="interactionContext">The interactionContext</param>
      /// <param name="key"></param>
      /// <returns></returns>
      [SlashCommand("RegisterKey" + Bot.isDevBot, "Add Twitch notifier!"), Obsolete("Obsolete")]
      public static async Task RegisterKeyCommand(InteractionContext interactionContext, [Option("Key", "Key.")] string key)
      {
         await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

         if (interactionContext.Member.Roles.All(x => (x.Permissions & Permissions.Administrator) == 0))
         {
            await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("unprivileged"));
            return;
         }

         await interactionContext.DeleteResponseAsync();

         DiscordComponentEmoji discordComponentEmojisPrevious = new("🔑");
         DiscordComponent[] discordComponent = new DiscordComponent[1];
         discordComponent[0] = new DiscordButtonComponent(ButtonStyle.Primary, "ClaimKey", "Claim Key!", false, discordComponentEmojisPrevious);

         string encrypted = Encrypt(key);
         await interactionContext.Channel.SendMessageAsync(new DiscordMessageBuilder().WithContent(encrypted).AddComponents(discordComponent));
      }

      private static string Encrypt(string textToEncrypt)
      {
         try
         {
            const string publicKey = "12345678";
            const string secretKey = "87654321";
            byte[] secretKeyByte = Encoding.UTF8.GetBytes(secretKey);
            byte[] publicKeyByte = Encoding.UTF8.GetBytes(publicKey);
            byte[] inputByteArray = Encoding.UTF8.GetBytes(textToEncrypt);
            using DESCryptoServiceProvider des = new();
            MemoryStream ms = new();
            CryptoStream cs = new(ms, des.CreateEncryptor(publicKeyByte, secretKeyByte), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();
            string toReturn = Convert.ToBase64String(ms.ToArray());

            return toReturn;
         }
         catch (Exception ex)
         {
            throw new Exception(ex.Message, ex.InnerException);
         }
      }

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

      private static string Decrypt(string textToDecrypt)
      {
         try
         {
            const string publicKey = "12345678";
            const string secretKey = "87654321";
            byte[] privateKeyByte = Encoding.UTF8.GetBytes(secretKey);
            byte[] publicKeyByte = Encoding.UTF8.GetBytes(publicKey);
            byte[] inputByteArray = Convert.FromBase64String(textToDecrypt.Replace(" ", "+"));
            using DESCryptoServiceProvider des = new();
            MemoryStream ms = new();
            CryptoStream cs = new(ms, des.CreateDecryptor(publicKeyByte, privateKeyByte), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();
            Encoding encoding = Encoding.UTF8;
            string toReturn = encoding.GetString(ms.ToArray());

            return toReturn;
         }
         catch (Exception ae)
         {
            throw new Exception(ae.Message, ae.InnerException);
         }
      }
   }
}