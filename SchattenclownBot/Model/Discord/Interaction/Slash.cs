﻿using DisCatSharp;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SchattenclownBot.Model.Objects;
using SchattenclownBot.Model.Persistence;

namespace SchattenclownBot.Model.Discord.Interaction
{
    /// <summary>
    /// The slash commands.
    /// </summary>
    internal class Slash : ApplicationCommandsModule
    {
        /// <summary>
        /// Send the help of this bot.
        /// </summary>
        /// <param name="ic">The interaction context.</param>
        [SlashCommand("help", "Schattenclown Help", true)]
        public static async Task HelpAsync(InteractionContext ic)
        {
            await ic.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            DiscordEmbedBuilder eb = new DiscordEmbedBuilder()
            {
                Title = "Help",
                Description = "This is the command help for the Schattenclown Bot",
                Color = DiscordColor.Purple
            };
            eb.AddField("/level", "Shows your level!");
            eb.AddField("/levelsystem", "Shows the levelsystem!");
            eb.AddField("/timer", "Set´s a timer!");
            eb.AddField("/mytimers", "Look up your timers!");
            eb.AddField("/alarmclock", "Set an alarm for a spesific time!");
            eb.AddField("/myalarms", "Look up your alarms!");
            eb.AddField("/invite", "Send´s an invite link!");
            eb.WithAuthor("Schattenclown help");
            eb.WithFooter("(✿◠‿◠) thanks for using me");
            eb.WithTimestamp(DateTime.Now);

            await ic.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(eb.Build()));
        }

        [SlashCommand("alarmclock", "Set an alarm for a spesific time!", true)]
        public static async Task AlarmClock(InteractionContext ic, [Option("hourofday", "0-23")] double hour, [Option("minuteofday", "0-59")] double minute)
        {
            await ic.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Creating alarm..."));

            if (!TimeFormat(hour, minute))
            {
                await ic.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Wrong format for hour or minute!"));
                return;
            }

            DateTime dateTimeNow = DateTime.Now;
            DateTime alarm = new DateTime(dateTimeNow.Year, dateTimeNow.Month, dateTimeNow.Day, Convert.ToInt32(hour), Convert.ToInt32(minute), 0);

            if (alarm < DateTime.Now)
                alarm = alarm.AddDays(1);

            ScAlarmClock scAlarmClock = new ScAlarmClock
            {
                ChannelId = ic.Channel.Id,
                MemberId = ic.Member.Id,
                NotificationTime = alarm
            };
            ScAlarmClock.Add(scAlarmClock);
            await ic.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Alarm set for {scAlarmClock.NotificationTime}!"));
        }
        [SlashCommand("myalarms", "Look up your alarms!", true)]
        public static async Task AlarmClockLookup(InteractionContext ic)
        {
            List<ScAlarmClock> lstScAlarmClocks = DB_ScAlarmClocks.ReadAll();
            DiscordEmbedBuilder eb = new DiscordEmbedBuilder
            {
                Title = "Your alarms",
                Color = DiscordColor.Purple,
                Description = $"<@{ic.Member.Id}>"
            };
            bool noTimers = true;
            foreach (var scAlarmClock in lstScAlarmClocks)
            {
                if (scAlarmClock.MemberId == ic.Member.Id)
                {
                    noTimers = false;
                    eb.AddField($"{scAlarmClock.NotificationTime}", $"Alarm with ID {scAlarmClock.DBEntryID}");
                }
            }
            if (noTimers)
                eb.Title = "No alarms set!";

            await ic.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(eb.Build()));
        }

        [SlashCommand("timer", "Set a timer!", true)]
        public static async Task Timer(InteractionContext ic, [Option("hours", "0-23")] double hour, [Option("minutes", "0-59")] double minute)
        {
            await ic.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Creating timer..."));

            if (!TimeFormat(hour, minute))
            {
                await ic.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Wrong format for hour or minute!"));
                return;
            }

            DateTime dateTimeNow = DateTime.Now;
            ScTimer scTimer = new ScTimer
            {
                ChannelId = ic.Channel.Id,
                MemberId = ic.Member.Id,
                NotificationTime = dateTimeNow.AddHours(hour).AddMinutes(minute)
            };
            ScTimer.Add(scTimer);

            await ic.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Timer set for {scTimer.NotificationTime}!"));
        }

        [SlashCommand("mytimers", "Look up your timers!", true)]
        public static async Task TimerLookup(InteractionContext ic)
        {
            List<ScTimer> lstScTimers = DB_ScTimers.ReadAll();
            DiscordEmbedBuilder eb = new DiscordEmbedBuilder
            {
                Title = "Your timers",
                Color = DiscordColor.Purple,
                Description = $"<@{ic.Member.Id}>"
            };
            bool noTimers = true;
            foreach (var scTimer in lstScTimers)
            {
                if (scTimer.MemberId == ic.Member.Id)
                {
                    noTimers = false;
                    eb.AddField($"{scTimer.NotificationTime}", $"Timer with ID {scTimer.DBEntryID}");
                }
            }
            if (noTimers)
                eb.Title = "No timers set!";

            await ic.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(eb.Build()));
        }

        [SlashCommand("LevelSystem", "Look up the LevelSystem!", true)]
        public static async Task LevelSystem(InteractionContext interactionContext)
        {
            List<DcUserLevelSystem> dcUserLevelSystemList = DcUserLevelSystem.Read(interactionContext.Guild.Id);

            List<DcUserLevelSystem> dcUserLevelSystemListSorted = dcUserLevelSystemList.OrderBy(x => x.OnlineTicks).ToList();
            dcUserLevelSystemListSorted.Reverse();

            string liststring = "```css\n" +
                                "{365.24:60}[Username]\n\n";
            foreach (var dcLevelSystem in dcUserLevelSystemListSorted)
            {
                var discordUser = await Discord.DiscordBot.Client.GetUserAsync(dcLevelSystem.MemberId);

                DateTime date1 = new DateTime(1969, 4, 20, 4, 20, 0);
                DateTime date2 = new DateTime(1969, 4, 20, 4, 20, 0).AddMinutes(dcLevelSystem.OnlineTicks);
                TimeSpan timeSpan = date2 - date1;

                liststring += "{" + $"{timeSpan,9:ddd\\.hh\\:mm}" + "}" + $"[{discordUser.Username}]\n";
            }
            liststring += "\n```";
            DiscordEmbedBuilder discordEmbedBuilder = new DiscordEmbedBuilder();
            discordEmbedBuilder.Title = "LevelSystem";
            discordEmbedBuilder.Description = liststring;
            discordEmbedBuilder.Color = DiscordColor.Purple;

            await interactionContext.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(discordEmbedBuilder.Build()));
        }

        [SlashCommand("Level", "Look up your level!", true)]
        public static async Task Level(InteractionContext interactionContext)
        {
            List<DcUserLevelSystem> dcUserLevelSystemList = DcUserLevelSystem.Read(interactionContext.Guild.Id);
            List<DcUserLevelSystem> dcUserLevelSystemListSorted = dcUserLevelSystemList.OrderBy(x => x.OnlineTicks).ToList();
            dcUserLevelSystemListSorted.Reverse();

            //https://quickchart.io/chart/render/zm-61647abc-3b66-47ef-8823-dbef4d19642f?title=titlet&labels=Q1,Q2,Q3,Q4&data1=50,40,30,20
            string uriString = "https://quickchart.io/chart/render/zm-61647abc-3b66-47ef-8823-dbef4d19642f?title=";

            int totalXp = 0;
            int totalLevel, modXp;
            int rank = 1;
            string level, xp;

            var discordUser = await Discord.DiscordBot.Client.GetUserAsync(interactionContext.Member.Id);
            string username = discordUser.Username;

            foreach (var dcUserLevelSystemItem in dcUserLevelSystemListSorted)
            {
                if(dcUserLevelSystemItem.MemberId == interactionContext.Member.Id)
                {
                    rank = dcUserLevelSystemList.IndexOf(dcUserLevelSystemItem);
                    totalXp = dcUserLevelSystemItem.OnlineTicks * 125 / 60;
                    break;
                }
            }

            if (totalXp > 0)
            {
                totalLevel = totalXp / 1000;
                modXp = totalXp % 1000;
                level = $"Level {totalLevel}  ";
                xp = $"{modXp}xp/1000xp&data1={modXp}";
            }
            else
            {
                totalLevel = 0;
                modXp = 0;
                level = $"Level {totalLevel}  ";
                xp = $"{modXp}xp/1000xp&data1={modXp}";
            }

            uriString += $"{level} {username} {xp,50}";

            Uri uri = new Uri(uriString);
            DiscordEmbedBuilder discordEmbedBuilder = new DiscordEmbedBuilder();
            discordEmbedBuilder.WithFooter($"Rank #{rank}");
            discordEmbedBuilder.WithImageUrl(uri.AbsoluteUri);
            discordEmbedBuilder.Color = DiscordColor.Purple;

            await interactionContext.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(discordEmbedBuilder.Build()));
        }

        /// <summary>
        /// Checks if the given hour and minute are usable to make a datetime object out of them.
        /// Returns true if the given arguments are usable.
        /// Returns false if the hour or the minute are not usable.
        /// </summary>
        /// <param name="hour">The hour.</param>
        /// <param name="minute">The minute.</param>
        /// <returns>A bool.</returns>
        public static bool TimeFormat(double hour, double minute)
        {
            bool hourformatisright = false;
            bool minuteformatisright = false;

            for (int i = 0; i < 24; i++)
            {
                if (hour == i)
                    hourformatisright = true;
            }
            if (!hourformatisright)
                return false;

            for (int i = 0; i < 60; i++)
            {
                if (minute == i)
                    minuteformatisright = true;
            }
            if (!minuteformatisright)
                return false;

            return true;
        }

        /// <summary>
        /// Generates an Invite link.
        /// </summary>
        /// <param name="ic">The ic.</param>
        /// <returns>A Task.</returns>
        [SlashCommand("invite", "Invite ListforgeNotify", true)]
        public static async Task InviteAsync(InteractionContext ic)
        {
            await ic.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            var bot_invite = ic.Client.GetInAppOAuth(Permissions.Administrator);

            await ic.EditResponseAsync(new DiscordWebhookBuilder().WithContent(bot_invite.AbsoluteUri));
        }

        /// <summary>
        /// Gets the user's avatar & banner.
        /// </summary>
        /// <param name="ctx">The contextmenu context.</param>
        [ContextMenu(ApplicationCommandType.User, "Get avatar & banner")]
        public static async Task GetUserBannerAsync(ContextMenuContext ctx)
        {
            var user = await ctx.Client.GetUserAsync(ctx.TargetUser.Id, true);

            var eb = new DiscordEmbedBuilder
            {
                Title = $"Avatar & Banner of {user.Username}",
                ImageUrl = user.BannerHash != null ? user.BannerUrl : null
            }.
            WithThumbnail(user.AvatarUrl).
            WithColor(user.BannerColor ?? DiscordColor.Purple).
            WithFooter($"Requested by {ctx.Member.DisplayName}", ctx.Member.AvatarUrl).
            WithAuthor($"{user.Username}", user.AvatarUrl, user.AvatarUrl);
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(eb.Build()));
        }
    }
}
