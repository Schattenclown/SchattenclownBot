﻿using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using SchattenclownBot.Model.Discord.Main;
using SchattenclownBot.Model.HelpClasses;
using SchattenclownBot.Model.Objects;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SchattenclownBot.Model.AsyncFunction;

namespace SchattenclownBot.Model.Discord.AppCommands
{
   internal class RegisterTwitch : ApplicationCommandsModule
   {
      /// <summary>
      ///    Poke an User per command.
      /// </summary>
      /// <param name="interactionContext">The interactionContext</param>
      /// <returns></returns>
      [SlashCommand("TwitchRegister" + Bot.isDevBot, "Add Twitch notifier!")]
      public static async Task TwitchRegister(InteractionContext interactionContext, [Option("Channel", "#...")][ChannelTypes(ChannelType.Text)] DiscordChannel discordTargetChannel, [Option("Role", "@...")] DiscordRole discordTargetRole, [Option("Twitch", "TwitchUserId or TwitchChannelUrl")] string twitchThing)
      {
         await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

         List<TwitchNotifier> something = TwitchNotifier.Read(interactionContext.Guild.Id);

         /*if (something.Count == 0)
         {*/
            var twitchNotifierObj = new TwitchNotifier
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
               if (twitchThing.Contains("https://"))
                  twitchThing = StringCutter.RmUntil(twitchThing, "https://www.twitch.tv/", "https://www.twitch.tv/".Length);

               twitchNotifierObj.TwitchChannelUrl = twitchThing;
            }
            TwitchNotifier.Add(twitchNotifierObj);
            TwitchNotifier.SetMonitoring();
            await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Maybe" + twitchNotifierObj));
         /*}*/
      }
   }
}
