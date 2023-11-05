using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using SchattenclownBot.Integrations.Discord.Services;
using SchattenclownBot.Utils;

namespace SchattenclownBot.Integrations.Discord.ApplicationCommands
{
    internal class RegisterTwitch : ApplicationCommandsModule
    {
        /// <summary>
        ///     Poke an User per command.
        /// </summary>
        /// <param name="interactionContext">The interactionContext</param>
        /// <param name="discordTargetChannel"></param>
        /// <param name="discordTargetRole"></param>
        /// <param name="twitchThing"></param>
        /// <returns></returns>
        [SlashCommand("TwitchRegister", "Add Twitch notifier!")]
        public static async Task TwitchRegister(InteractionContext interactionContext, [Option("Channel", "#..."), ChannelTypes(ChannelType.Text)] DiscordChannel discordTargetChannel, [Option("Role", "@...")] DiscordRole discordTargetRole, [Option("Twitch", "TwitchChannelUrl or TwitchUserName")] string twitchThing)
        {
            await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            if (interactionContext.Member.Roles.All(x => (x.Permissions & Permissions.Administrator) == 0))
            {
                await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("unprivileged"));
                return;
            }

            if (twitchThing.Contains("https://"))
            {
                twitchThing = StringCutter.RemoveUntil(twitchThing, "https://www.twitch.tv/", "https://www.twitch.tv/".Length);
            }

            List<TwitchNotifier> twitchNotifiers = TwitchNotifier.Read(interactionContext.Guild.Id);

            if (twitchNotifiers.Any(x => x.DiscordGuildId == interactionContext.Guild.Id && x.TwitchChannelUrl.ToLower() == twitchThing && x.DiscordRoleId == discordTargetRole.Id && x.DiscordChannelId == discordTargetChannel.Id))
            {
                await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Already registered"));
            }
            else
            {
                TwitchNotifier twitchNotifierObj = new()
                {
                            DiscordGuildId = interactionContext.Guild.Id,
                            DiscordMemberId = interactionContext.Member.Id,
                            DiscordChannelId = discordTargetChannel.Id,
                            DiscordRoleId = discordTargetRole.Id
                };

                try
                {
                    if (Convert.ToUInt64(twitchThing) > 0)
                    {
                        twitchNotifierObj.TwitchUserId = Convert.ToUInt64(twitchThing);
                    }
                }
                catch
                {
                    //ignore
                }
                finally
                {
                    twitchNotifierObj.TwitchChannelUrl = twitchThing;
                }

                TwitchNotifier.Add(twitchNotifierObj);
                TwitchNotifier.SetMonitoring();
                await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Added\n" + twitchNotifierObj));
            }
        }
    }
}