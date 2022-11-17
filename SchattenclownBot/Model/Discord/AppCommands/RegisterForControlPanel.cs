using System.Security.Cryptography;
using DisCatSharp;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.EventArgs;
using SchattenclownBot.Model.Discord.Main;
using SchattenclownBot.Model.Objects;
using System.Threading.Tasks;
using System.Text;
using System;

namespace SchattenclownBot.Model.Discord.AppCommands
{
   internal class RegisterForControlPanel : ApplicationCommandsModule
   {
      /// <summary>
      ///     Poke an User per command.
      /// </summary>
      /// <param name="interactionContext">The interactionContext</param>
      /// <returns></returns>
      [SlashCommand("Register" + Bot.isDevBot, "RegisterForControlPanel for the SchattenclownBot control panel!")]
      public static async Task Register(InteractionContext interactionContext)
      {
         DiscordInteractionModalBuilder interactionModalBuilder = new()
         {
            Title = "Register for control panel form",
            CustomId = "RegisterForm"
         };

         interactionModalBuilder.AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "Username",
            "Username", "your username", 3, 16, true, ""));
         interactionModalBuilder.AddTextComponent(new DiscordTextComponent(TextComponentStyle.Small, "Password",
            "Password", "your password", 8, 16, true, ""));

         await interactionContext.CreateModalResponseAsync(interactionModalBuilder);

         await Task.Delay(1000);
      }

      public static async Task RegisterEvent(DiscordClient sender, ComponentInteractionCreateEventArgs eventArgs)
      {
         if (eventArgs.Id == "RegisterForm")
         {
            SecretVault secretVaultRead = SecretVault.Read(eventArgs.User.Id);

            if (secretVaultRead.DiscordUserId == 0)
            {
               SecretVault secretVault = new()
               {
                  DiscordGuildId = eventArgs.Guild.Id,
                  DiscordUserId = eventArgs.User.Id,
                  Username = eventArgs.Interaction.Data.Components[0].Value,
                  SecretKey = sha256(eventArgs.Interaction.Data.Components[1].Value)
               };
               SecretVault.Register(secretVault);
               await eventArgs.User.ConvertToMember(eventArgs.Guild).Result.SendMessageAsync("You can now authenticate yourself in the SchattenclownBot control panel with your username and password.");
            }
            else
            {
               await eventArgs.User.ConvertToMember(eventArgs.Guild).Result.SendMessageAsync("You are already registered. If you forgot your password ask <@444152594898878474>.");
            }
         }
      }

      static string sha256(string randomString)
      {
         var crypt = new SHA256Managed();
         string hash = String.Empty;
         byte[] crypto = crypt.ComputeHash(Encoding.ASCII.GetBytes(randomString));
         foreach (byte theByte in crypto)
         {
            hash += theByte.ToString("x2");
         }
         return hash;
      }
   }
}

