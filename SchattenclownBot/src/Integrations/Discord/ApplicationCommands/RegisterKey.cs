using System;
using System.Collections.Generic;
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

#pragma warning disable SYSLIB0021

namespace SchattenclownBot.Integrations.Discord.ApplicationCommands
{
    public class RegisterKey : ApplicationCommandsModule
    {
        /// <summary>
        ///     Poke an User per command.
        /// </summary>
        /// <param name="interactionContext">The interactionContext</param>
        /// <param name="info"></param>
        /// <param name="platform"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        [SlashCommand("RegisterKey", "Add Twitch notifier!")]
        public async Task RegisterKeyCommand(InteractionContext interactionContext, [Option("Info", "Information about the Key.")] string info, [Option("Platform", "Platform the Key.")] string platform, [Option("Key", "Key.")] string key)
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

            DiscordEmbedBuilder discordEmbedBuilder = new();

            discordEmbedBuilder.WithTitle($"Key for : '{info}'");
            discordEmbedBuilder.WithDescription("Click on the button to Claim a Key");
            discordEmbedBuilder.WithColor(DiscordColor.HotPink);
            discordEmbedBuilder.AddField(new DiscordEmbedField("encrypted key", encrypted));
            discordEmbedBuilder.AddField(new DiscordEmbedField("Platform", platform ?? "hmm"));

            await interactionContext.Channel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(discordEmbedBuilder.Build()).AddComponents(discordComponent));
        }

        public async Task ButtonPressEvent(DiscordClient client, ComponentInteractionCreateEventArgs eventArgs)
        {
            switch (eventArgs.Id)
            {
                case "ClaimKey":
                {
                    IReadOnlyList<DiscordEmbedField> discordEmbedFields = eventArgs.Message.Embeds.FirstOrDefault()?.Fields;
                    if (discordEmbedFields != null)
                    {
                        string decrypted = Decrypt(discordEmbedFields.FirstOrDefault()?.Value);

                        DiscordComponentEmoji discordComponentEmojisPrevious = new("🔑");
                        DiscordComponent[] discordComponent = new DiscordComponent[1];
                        discordComponent[0] = new DiscordButtonComponent(ButtonStyle.Primary, "ClaimKey", "Claim Key!", true, discordComponentEmojisPrevious);

                        await eventArgs.Message.ModifyAsync(x => x.WithContent("Claimed").AddComponents(discordComponent));

                        await eventArgs.User.SendMessageAsync(new DiscordMessageBuilder().WithContent(decrypted));
                    }

                    break;
                }
            }
        }

        public string Encrypt(string textToEncrypt)
        {
            try
            {
                //this is not save but no one will look at this so no one will decode the keys
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

        public string Decrypt(string textToDecrypt)
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