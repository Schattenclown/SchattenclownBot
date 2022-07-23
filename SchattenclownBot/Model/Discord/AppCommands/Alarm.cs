using DisCatSharp;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.Entities;
using SchattenclownBot.Model.HelpClasses;
using SchattenclownBot.Model.Objects;
using SchattenclownBot.Model.Persistence;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SchattenclownBot.Model.Discord.AppCommands
{
    internal class Alarm : ApplicationCommandsModule
    {
        /// <summary>
        ///     Set an Alarm clock per command.
        /// </summary>
        /// <param name="interactionContext">The interaction context.</param>
        /// <param name="hour">The Hour of the Alarm in the Future.</param>
        /// <param name="minute">The Minute of the Alarm in the Future.</param>
        /// <returns></returns>
        [SlashCommand("SetAlarm", "Set an alarm for a specific time!")]
        public static async Task SetAlarmAsync(InteractionContext interactionContext, [Option("HourOfDay", "0-23")] double hour, [Option("MinuteOfDay", "0-59")] double minute)
        {
            //Create a Response.
            await interactionContext.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Creating alarm..."));

            //Check if the given Time format is Valid.
            if (!TimeFormatCheck.TimeFormat(hour, minute))
            {
                //Tell the User that the Time format is not valid and return.
                await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Wrong format for hour or minute!"));
                return;
            }
            
            //Create a DateTime Variable if the Time format was Valid.
            DateTime dateTimeNow = DateTime.Now;
            DateTime alarm = new(dateTimeNow.Year, dateTimeNow.Month, dateTimeNow.Day, Convert.ToInt32(hour), Convert.ToInt32(minute), 0);

            //Check if the Alarm is a Time for Tomorrow, if it is in the Past already Today.
            if (alarm < DateTime.Now)
                alarm = alarm.AddDays(1);

            //Create an AlarmObject and add it to the Database.
            BotAlarmClock botAlarmClock = new()
            {
                ChannelId = interactionContext.Channel.Id,
                MemberId = interactionContext.Member.Id,
                NotificationTime = alarm
            };
            BotAlarmClock.Add(botAlarmClock);

            //Let the User know that the Alarm was set Successfully.
            await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Alarm set for {botAlarmClock.NotificationTime}!"));
        }

        /// <summary>
        ///     To look up what Alarm´s have been set.
        /// </summary>
        /// <param name="interactionContext"></param>
        /// <returns></returns>
        [SlashCommand("MyAlarms", "Look up your alarms!")]
        public static async Task AlarmClockLookup(InteractionContext interactionContext)
        {
            //Create an Response.
            await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            //Create a List where all Alarms will be Listed if there are any set.
            System.Collections.Generic.List<BotAlarmClock> botAlarmClockList = DB_BotAlarmClocks.ReadAll();

            //Create an Embed.
            DiscordEmbedBuilder discordEmbedBuilder = new()
            {
                Title = "Your alarms",
                Color = DiscordColor.Purple,
                Description = $"<@{interactionContext.Member.Id}>"
            };

            //Switch to check if there are any Timers at all.
            bool noTimers = true;

            //Search for any Alarms that match the Alarm creator and Requesting User.
            foreach (BotAlarmClock botAlarmClockItem in botAlarmClockList.Where(botAlarmClockItem => botAlarmClockItem.MemberId == interactionContext.Member.Id))
            {
                //Set the switch to false because at least one Alarm was found.
                noTimers = false;
                //Add an field to the Embed with the Alarm that was found.
                discordEmbedBuilder.AddField(new DiscordEmbedField($"{botAlarmClockItem.NotificationTime}", $"Alarm with ID {botAlarmClockItem.DBEntryID}"));
            }

            //Set the Title so the User knows no Alarms for him where found.
            if (noTimers)
                discordEmbedBuilder.Title = "No alarms set!";

            //Edit the Response and add the Embed.
            await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
        }
    }
}
