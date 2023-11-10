using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Common;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using SchattenclownBot.DataAccess.MySQL.Services;
using SchattenclownBot.Models;

// ReSharper disable UnusedMember.Global

namespace SchattenclownBot.Integrations.Discord.ApplicationCommands
{
    public class Timer : ApplicationCommandsModule
    {
        /// <summary>
        ///     Set an Timer per Command.
        /// </summary>
        /// <param name="interactionContext">The interactionContext</param>
        /// <param name="hour">The Hour of the Alarm in the Future.</param>
        /// <param name="minute">The Minute of the Alarm in the Future.</param>
        /// <returns></returns>
        [SlashCommand("SetTimer", "Set a timer!")]
        public async Task SetTimerAsync(InteractionContext interactionContext, [Option("hours", "0-23")] int hour, [Option("minutes", "0-59")] int minute)
        {
            //Create a Response.
            await interactionContext.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Creating timer..."));

            //RunAsync if the Give Time format is Valid.
            //Create an TimerObject and add it to the Database.
            if (!(hour.IsInRange(0, 24) && minute.IsInRange(0, 59)))
            {
                await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Wrong format for hour or minute!"));
                return;
            }

            DateTime dateTimeNow = DateTime.Now;
            BotTimer botTimer = new()
            {
                        ChannelId = interactionContext.Channel.Id,
                        MemberId = interactionContext.Member.Id,
                        NotificationTime = dateTimeNow.AddHours(hour).AddMinutes(minute)
            };
            new BotTimer().Add(botTimer);

            //Edit the Response and add the Embed.
            await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Timer set for {botTimer.NotificationTime}!"));
        }

        /// <summary>
        ///     To look up what Timers have been set.
        /// </summary>
        /// <param name="interactionContext"></param>
        /// <returns></returns>
        [SlashCommand("MyTimers", "Look up your timers!")]
        public async Task TimerLookup(InteractionContext interactionContext)
        {
            //Create a Response.
            await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            //Create an List with all Timers that where found in the Database.
            List<BotTimer> botTimerList = new DbBotTimer().ReadAll();

            //Create an Embed.
            DiscordEmbedBuilder discordEmbedBuilder = new()
            {
                        Title = "Your timers",
                        Color = DiscordColor.Purple,
                        Description = $"<@{interactionContext.Member.Id}>"
            };

            //Switch to check if any Timers where set at all.
            bool noTimers = true;

            //Search for any Timers that match the Timer creator and Requesting User.
            foreach (BotTimer botTimerItem in botTimerList.Where(botTimerItem => botTimerItem.MemberId == interactionContext.Member.Id))
            {
                //Set the switch to false because at least one Timer was found.
                noTimers = false;
                //Add an field to the Embed with the Timer that was found.
                discordEmbedBuilder.AddField(new DiscordEmbedField($"{botTimerItem.NotificationTime}", $"Timer with ID {botTimerItem.DbEntryId}"));
            }

            //Set the Title so the User knows no Timers for him where found.
            if (noTimers)
            {
                discordEmbedBuilder.Title = "No timers set!";
            }

            //Edit the Response and add the Embed.
            await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
        }
    }
}